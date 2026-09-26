using QdtCqts.Domain;

namespace QdtCqts.Calculation.Abstractions;

public enum RuleStatus
{
    Official,
    RealProjectConfirmed,
    Candidate,
    Blocked
}

public enum TraceMode
{
    Normal,
    Diagnostic
}

public sealed record RuleExecutionContext(
    string CalculationRunId,
    string? GoldenCaseId = null,
    string? CorrelationId = null,
    TraceMode Mode = TraceMode.Normal)
{
    public string EffectiveCorrelationId => CorrelationId ?? CorrelationContext.Current.CorrelationId;
}

public sealed record RuleInput(string Name, object? Value, UnitCode Unit);

public sealed record RuleTrace(string InputPath, object? InputValue, UnitCode InputUnit, string Operation, string OutputPath, object? OutputValue, UnitCode OutputUnit);

public sealed record RuleExecutionResult(
    string RuleId,
    string FormulaId,
    string EvidenceId,
    string? GoldenCaseId,
    object? OutputValue,
    UnitCode OutputUnit,
    CalculationStatus Status,
    IReadOnlyList<RuleTrace> Trace,
    string? Reason,
    string RuleVersion = "1.0.0",
    RuleStatus RuleStatus = RuleStatus.Candidate,
    string? InputHash = null,
    string? OutputHash = null)
{
    public static RuleExecutionResult Blocked(string ruleId, string formulaId, string evidenceId, RuleExecutionContext context, string reason, string ruleVersion = "1.0.0") =>
        new(ruleId, formulaId, evidenceId, context.GoldenCaseId, null, UnitCode.Unknown, CalculationStatus.Blocked, Array.Empty<RuleTrace>(), reason, ruleVersion, RuleStatus.Blocked);
}

public interface IIsolatedRule
{
    string RuleId { get; }
    string RuleVersion => "1.0.0";
    RuleStatus Status => RuleStatus.Candidate;
    RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context);
}
