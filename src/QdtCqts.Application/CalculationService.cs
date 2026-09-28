using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Calculation.Cqts;
using QdtCqts.Calculation.Qdt;
using QdtCqts.Domain;

namespace QdtCqts.Application;

/// <summary>
/// Serviço de aplicação para execução e auditoria de cálculos elétricos unificados (sisQDT_LIGHT).
/// Gerencia CorrelationId, CalculationId, observabilidade estruturada, traces e auditoria.
/// </summary>
public sealed class CalculationService
{
    private readonly ILogger<CalculationService> _logger;
    private readonly ICalculationTraceRecorder _traceRecorder;
    private readonly ICalculationAuditService _auditService;
    private readonly CqtsCalculationEngine _cqtsEngine;
    private readonly QdtCalculationEngine _qdtEngine;

    public CalculationService(
        ILogger<CalculationService>? logger = null,
        ICalculationTraceRecorder? traceRecorder = null,
        ICalculationAuditService? auditService = null,
        CqtsCalculationEngine? cqtsEngine = null,
        QdtCalculationEngine? qdtEngine = null)
    {
        _logger = logger ?? NullLogger<CalculationService>.Instance;
        _traceRecorder = traceRecorder ?? new CalculationTraceRecorder();
        _auditService = auditService ?? new CalculationAuditService(NullLogger<CalculationAuditService>.Instance, _traceRecorder);
        _cqtsEngine = cqtsEngine ?? new CqtsCalculationEngine();
        _qdtEngine = qdtEngine ?? new QdtCalculationEngine();
    }

    public CalculationResult ExecuteCalculation(
        ProjectVersion projectVersion,
        CalculationMode mode,
        string algorithmVersion = "25.0.0",
        string? existingCorrelationId = null)
    {
        var correlationId = existingCorrelationId ?? CorrelationContext.Current?.CorrelationId ?? Guid.NewGuid().ToString("N");
        var calculationId = $"CALC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";

        using var scope = CorrelationContext.BeginScope(correlationId, calculationId);
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            CalculationEventIds.CalcStarted,
            "Iniciando execução da cadeia de cálculo {CalculationId} para o projeto {ProjectId} (Versão {VersionLabel}) no modo {Mode} com algoritmo {AlgorithmVersion}.",
            calculationId,
            projectVersion.ProjectId,
            projectVersion.Label,
            mode,
            algorithmVersion);

        // Gera hash determinístico das entradas do modelo
        var inputValues = new List<RuleInput>
        {
            new("PROJECT_ID", projectVersion.ProjectId, UnitCode.Unknown),
            new("SOURCE_HASH", projectVersion.SourceHash, UnitCode.Unknown),
            new("CIRCUITS_COUNT", projectVersion.NetworkModel.Circuits.Count, UnitCode.Unknown),
            new("NODES_COUNT", projectVersion.NetworkModel.Nodes.Count, UnitCode.Unknown),
            new("EDGES_COUNT", projectVersion.NetworkModel.Edges.Count, UnitCode.Unknown),
            new("LOADS_COUNT", projectVersion.NetworkModel.Loads.Count, UnitCode.Unknown)
        };
        string inputHash = DeterministicHashing.ComputeInputHash(inputValues);

        var request = new CalculationRequest(projectVersion, mode, algorithmVersion, inputHash);

        try
        {
            ICalculationEngine engine = mode switch
            {
                CalculationMode.Cqts => _cqtsEngine,
                CalculationMode.Qdt => _qdtEngine,
                _ => _cqtsEngine
            };

            var result = engine.Calculate(request);
            stopwatch.Stop();

            if (result.Status == CalculationStatus.Pass)
            {
                _logger.LogInformation(
                    CalculationEventIds.CalcCompleted,
                    "Cálculo {CalculationId} finalizado com SUCESSO em {DurationMs:F2} ms. OutputHash={OutputHash}.",
                    calculationId,
                    stopwatch.Elapsed.TotalMilliseconds,
                    result.Report?.OutputHash ?? "<none>");

                // Registra traces na auditoria se o relatório estiver presente
                if (result.Report != null)
                {
                    foreach (var seg in result.Report.Segments)
                    {
                        var traceDetail = new CalculationTraceDetail(
                            calculationId,
                            correlationId,
                            "CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP",
                            algorithmVersion,
                            RuleStatus.RealProjectConfirmed,
                            TraceMode.Normal,
                            inputHash,
                            DeterministicHashing.ComputeOutputHash(seg.SegmentVoltageDropPercent, UnitCode.Percent),
                            new[]
                            {
                                new RuleInput("M", seg.EndLoadSelectedKva, UnitCode.Kva),
                                new RuleInput("Z", seg.ImpedanceOhmPerKm, UnitCode.Ohm),
                                new RuleInput("L", seg.PhysicalLengthMeters, UnitCode.Meter),
                                new RuleInput("AP", seg.ParallelCables, UnitCode.Unknown),
                                new RuleInput("H", (double)seg.PhaseCount, UnitCode.ConductorKey)
                            },
                            new[]
                            {
                                new RuleTrace("Ib", seg.OperatingCurrentAmperes, UnitCode.Ampere, "Corrente de projeto do trecho", "DeltaVPercent", seg.SegmentVoltageDropPercent, UnitCode.Percent),
                                new RuleTrace("T", seg.OperatingTemperatureCelsius, UnitCode.Celsius, "Temperatura de regime contínuo do condutor", "DeltaVPercent", seg.SegmentVoltageDropPercent, UnitCode.Percent),
                                new RuleTrace("Rca", seg.ResistanceCaOhmPerKm, UnitCode.Ohm, "Resistência CA corrigida termicamente", "DeltaVPercent", seg.SegmentVoltageDropPercent, UnitCode.Percent),
                                new RuleTrace("BJ", seg.FactorBj, UnitCode.Unknown, "Fator BJ da fórmula de queda", "DeltaVPercent", seg.SegmentVoltageDropPercent, UnitCode.Percent)
                            },
                            seg.SegmentVoltageDropPercent,
                            UnitCode.Percent,
                            CalculationStatus.Pass,
                            "DeltaV% = M * BJ * (L / AP) * kFase",
                            seg.EdgeId,
                            stopwatch.Elapsed.TotalMilliseconds,
                            DateTimeOffset.UtcNow);

                        _traceRecorder.RecordTrace(traceDetail);
                    }
                }
            }
            else
            {
                var diag = result.Diagnostics.FirstOrDefault();
                _logger.LogWarning(
                    CalculationEventIds.CalcBlocked,
                    "Cálculo {CalculationId} BLOQUEADO: [{Code}] {Message}",
                    calculationId,
                    diag?.Code ?? "UNKNOWN",
                    diag?.Message ?? "Sem mensagem detalhada");
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                CalculationEventIds.RuleFailed,
                ex,
                "FALHA INESPERADA no cálculo {CalculationId} após {DurationMs:F2} ms: {ExceptionMessage}",
                calculationId,
                stopwatch.Elapsed.TotalMilliseconds,
                ex.Message);

            return CalculationResult.Blocked(request, "UNHANDLED_EXCEPTION", ex.Message);
        }
    }
}
