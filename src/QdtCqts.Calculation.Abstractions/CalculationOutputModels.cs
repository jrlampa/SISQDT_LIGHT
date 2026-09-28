using System.Collections.Generic;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Abstractions;

/// <summary>
/// Resultado detalhado de cálculo para um trecho (edge/segmento) da rede.
/// </summary>
public sealed record SegmentCalculationResult(
    string EdgeId,
    string FromNodeId,
    string ToNodeId,
    string? ConductorKey,
    int PhaseCount,
    double ParallelCables,
    double PhysicalLengthMeters,
    double EquivalentLengthMeters,
    double DownstreamAccumulatedConsumers,
    double DownstreamAccumulatedLoadKva,
    double DiversityFactor,
    double EndLoadSelectedKva,
    double OperatingCurrentAmperes,
    double RatedAmpacityAmperes,
    bool IsOverloaded,
    double OperatingTemperatureCelsius,
    double ResistanceCaOhmPerKm,
    double ReactanceOhmPerKm,
    double ImpedanceOhmPerKm,
    double FactorBj,
    double PhaseFactor,
    double SegmentVoltageDropPercent,
    double ShortCircuit3PhaseAmperes = 0.0,
    double ShortCircuit1PhaseAmperes = 0.0,
    double UpstreamResistanceOhm = 0.0,
    double UpstreamReactanceOhm = 0.0);

/// <summary>
/// Resultado de cálculo para um nó da rede (poste, caixa, derivação, carga).
/// </summary>
public sealed record NodeCalculationResult(
    string NodeId,
    string ExternalKey,
    int LocalConsumers,
    double LocalLoadKva,
    double AccumulatedVoltageDropPercent);

/// <summary>
/// Resultado de cálculo para o transformador de alimentação e média tensão.
/// </summary>
public sealed record TransformerCalculationResult(
    string TransformerId,
    string ExternalKey,
    double NominalPowerKva,
    double OperatingLoadKva,
    double ImpedancePercent,
    double TrafoVoltageDropPercent,
    double MtVoltageDropPercent,
    double TotalOriginVoltageDropPercent);

/// <summary>
/// Resultado de avaliação de proteção e suportabilidade térmica do circuito.
/// </summary>
/// <remarks>
/// Nomenclatura canônica alinhada com <see cref="QdtCqts.Domain.ProtectionAssessment"/>.
/// Fase 25 — WS-B: eliminação do contract mismatch F24.1-A.
/// </remarks>
public sealed record ProtectionCalculationResult(
    double ProjectCurrentAmperes,
    double RecommendedFuseCurrentAmperes,
    double MinSinglePhaseShortCircuitAmperes,
    string CriticalConductorKey,
    double CriticalConductorSectionMm2,
    double CriticalOperatingTemperatureCelsius,
    double MaxAdmissibleTimeSeconds,
    double FuseMeltingTimeSeconds,
    bool IsRatedCurrentAdequate,
    bool IsThermalWithstandAdequate,
    string StatusMessage);

/// <summary>
/// Relatório consolidado e imutável de cálculo da rede elétrica para um circuito ou rede completa.
/// </summary>
public sealed record NetworkCalculationReport(
    string RunId,
    CalculationMode Mode,
    string AlgorithmVersion,
    string InputHash,
    string OutputHash,
    IReadOnlyList<TransformerCalculationResult> Transformers,
    IReadOnlyList<SegmentCalculationResult> Segments,
    IReadOnlyList<NodeCalculationResult> Nodes,
    int TotalCircuitsCalculated,
    double ExecutionDurationMs,
    ProtectionCalculationResult? Protection = null);

