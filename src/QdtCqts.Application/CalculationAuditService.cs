using System.Diagnostics;
using Microsoft.Extensions.Logging;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Application;

public interface ICalculationAuditService
{
    RuleExecutionResult ExecuteRuleWithAudit(
        IIsolatedRule rule,
        IReadOnlyList<RuleInput> inputs,
        string? goldenCaseId = null,
        TraceMode mode = TraceMode.Normal);

    void RecordParity(
        string ruleId,
        string caseId,
        string source,
        string target,
        double expectedValue,
        double actualValue,
        double delta,
        double tolerance,
        string status);

    void RecordGolden(
        string goldenCaseId,
        string goldenVersion,
        string sourceHash,
        double expectedValue,
        double actualValue,
        double delta,
        double tolerance,
        string status);
}

public sealed class CalculationAuditService : ICalculationAuditService
{
    private readonly ILogger<CalculationAuditService> _logger;
    private readonly ICalculationTraceRecorder _recorder;

    public CalculationAuditService(ILogger<CalculationAuditService> logger, ICalculationTraceRecorder recorder)
    {
        _logger = logger;
        _recorder = recorder;
    }

    public RuleExecutionResult ExecuteRuleWithAudit(
        IIsolatedRule rule,
        IReadOnlyList<RuleInput> inputs,
        string? goldenCaseId = null,
        TraceMode mode = TraceMode.Normal)
    {
        var correlationId = CorrelationContext.Current.CorrelationId;
        var calculationId = CorrelationContext.Current.CalculationId ?? CorrelationContext.GenerateCalculationId();

        using var scope = CorrelationContext.BeginScope(correlationId, calculationId);

        var inputHash = DeterministicHashing.ComputeInputHash(inputs);

        _logger.LogInformation(CalculationEventIds.CalcStarted,
            "Calculation started. CalculationId={CalculationId}, CorrelationId={CorrelationId}, RuleId={RuleId}, RuleVersion={RuleVersion}, InputHash={InputHash}",
            calculationId, correlationId, rule.RuleId, rule.RuleVersion, inputHash);

        var context = new RuleExecutionContext(calculationId, goldenCaseId, correlationId, mode);

        var stopwatch = Stopwatch.StartNew();
        var result = rule.Execute(inputs, context);
        stopwatch.Stop();

        var outputHash = DeterministicHashing.ComputeOutputHash(result.OutputValue, result.OutputUnit);

        var enrichedResult = result with
        {
            RuleVersion = rule.RuleVersion,
            RuleStatus = rule.Status,
            InputHash = inputHash,
            OutputHash = outputHash
        };

        if (rule.Status == RuleStatus.Candidate)
        {
            _logger.LogWarning(CalculationEventIds.RuleCandidateWarning,
                "Rule executed with CANDIDATE status. CalculationId={CalculationId}, RuleId={RuleId}, Version={RuleVersion}",
                calculationId, rule.RuleId, rule.RuleVersion);
        }

        if (result.Status == CalculationStatus.Pass)
        {
            _logger.LogInformation(CalculationEventIds.RuleExecuted,
                "Rule executed successfully. CalculationId={CalculationId}, RuleId={RuleId}, OutputValue={OutputValue}, Unit={OutputUnit}, OutputHash={OutputHash}, DurationMs={DurationMs}",
                calculationId, rule.RuleId, result.OutputValue, result.OutputUnit, outputHash, stopwatch.Elapsed.TotalMilliseconds);

            _logger.LogInformation(CalculationEventIds.CalcCompleted,
                "Calculation completed. CalculationId={CalculationId}, Status=PASS, DurationMs={DurationMs}",
                calculationId, stopwatch.Elapsed.TotalMilliseconds);
        }
        else
        {
            _logger.LogError(CalculationEventIds.RuleFailed,
                "Rule execution failed or blocked. CalculationId={CalculationId}, RuleId={RuleId}, Reason={Reason}",
                calculationId, rule.RuleId, result.Reason);

            _logger.LogError(CalculationEventIds.CalcBlocked,
                "Calculation blocked. CalculationId={CalculationId}, Reason={Reason}",
                calculationId, result.Reason);
        }

        var traceDetail = new CalculationTraceDetail(
            CalculationId: calculationId,
            CorrelationId: correlationId,
            RuleId: rule.RuleId,
            RuleVersion: rule.RuleVersion,
            RuleStatus: rule.Status,
            Mode: mode,
            InputHash: inputHash,
            OutputHash: outputHash,
            Inputs: inputs,
            Steps: result.Trace,
            OutputValue: result.OutputValue,
            OutputUnit: result.OutputUnit,
            Status: result.Status,
            FormulaString: result.FormulaId,
            EvidenceSource: result.EvidenceId,
            DurationMs: stopwatch.Elapsed.TotalMilliseconds,
            Timestamp: DateTimeOffset.UtcNow);

        _recorder.RecordTrace(traceDetail);

        return enrichedResult;
    }

    public void RecordParity(
        string ruleId,
        string caseId,
        string source,
        string target,
        double expectedValue,
        double actualValue,
        double delta,
        double tolerance,
        string status)
    {
        var correlationId = CorrelationContext.Current.CorrelationId;
        var calculationId = CorrelationContext.Current.CalculationId ?? CorrelationContext.GenerateCalculationId();

        var parityRecord = new ParityTraceRecord(
            CalculationId: calculationId,
            CorrelationId: correlationId,
            RuleId: ruleId,
            CaseId: caseId,
            Source: source,
            Target: target,
            ExpectedValue: expectedValue,
            ActualValue: actualValue,
            Delta: delta,
            Tolerance: tolerance,
            Status: status,
            Timestamp: DateTimeOffset.UtcNow);

        _recorder.RecordParity(parityRecord);

        if (status.Equals("MATCH", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(CalculationEventIds.ParityMatch,
                "Parity check passed. RuleId={RuleId}, CaseId={CaseId}, Expected={Expected}, Actual={Actual}, Delta={Delta}, Tolerance={Tolerance}",
                ruleId, caseId, expectedValue, actualValue, delta, tolerance);
        }
        else
        {
            _logger.LogError(CalculationEventIds.ParityMismatch,
                "Parity check failed. RuleId={RuleId}, CaseId={CaseId}, Expected={Expected}, Actual={Actual}, Delta={Delta}, Tolerance={Tolerance}",
                ruleId, caseId, expectedValue, actualValue, delta, tolerance);
        }
    }

    public void RecordGolden(
        string goldenCaseId,
        string goldenVersion,
        string sourceHash,
        double expectedValue,
        double actualValue,
        double delta,
        double tolerance,
        string status)
    {
        var correlationId = CorrelationContext.Current.CorrelationId;
        var calculationId = CorrelationContext.Current.CalculationId ?? CorrelationContext.GenerateCalculationId();

        var goldenRecord = new GoldenTraceRecord(
            GoldenCaseId: goldenCaseId,
            GoldenVersion: goldenVersion,
            CalculationId: calculationId,
            CorrelationId: correlationId,
            SourceHash: sourceHash,
            ExpectedValue: expectedValue,
            ActualValue: actualValue,
            Delta: delta,
            Tolerance: tolerance,
            Status: status,
            Timestamp: DateTimeOffset.UtcNow);

        _recorder.RecordGolden(goldenRecord);

        if (status.Equals("PASS", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(CalculationEventIds.GoldenPass,
                "Golden Case passed. GoldenCaseId={GoldenCaseId}, Expected={Expected}, Actual={Actual}, Delta={Delta}",
                goldenCaseId, expectedValue, actualValue, delta);
        }
        else
        {
            _logger.LogError(CalculationEventIds.GoldenMismatch,
                "Golden Case mismatch. GoldenCaseId={GoldenCaseId}, Expected={Expected}, Actual={Actual}, Delta={Delta}",
                goldenCaseId, expectedValue, actualValue, delta);
        }
    }
}
