using QdtCqts.Domain;

namespace QdtCqts.Calculation.Abstractions;

public sealed record RuleExecutionContext(string CalculationRunId, string? GoldenCaseId);
public sealed record RuleInput(string Name, object? Value, UnitCode Unit);
public sealed record RuleTrace(string InputPath, object? InputValue, UnitCode InputUnit, string Operation, string OutputPath, object? OutputValue, UnitCode OutputUnit);
public sealed record RuleExecutionResult(string RuleId, string FormulaId, string EvidenceId, string? GoldenCaseId, object? OutputValue, UnitCode OutputUnit, CalculationStatus Status, IReadOnlyList<RuleTrace> Trace, string? Reason)
{
    public static RuleExecutionResult Blocked(string ruleId, string formulaId, string evidenceId, RuleExecutionContext context, string reason) =>
        new(ruleId, formulaId, evidenceId, context.GoldenCaseId, null, UnitCode.Unknown, CalculationStatus.Blocked, Array.Empty<RuleTrace>(), reason);
}

public interface IIsolatedRule
{
    string RuleId { get; }
    RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context);
}
