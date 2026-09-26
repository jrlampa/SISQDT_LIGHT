using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Qdt;

public sealed class CandidateRamalImpedanceRule : IIsolatedRule
{
    public const string FormulaId = "QDT.RAMAL.CANDIDATE_RX_COMBINATION.FORMULA";
    public const string EvidenceId = "F15-CANDIDATE-RAMAIS-C13";
    public string RuleId => "QDT.RAMAL.CANDIDATE_RX_COMBINATION";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var resistance = inputs.SingleOrDefault(input => input.Name == "C11");
        var reactance = inputs.SingleOrDefault(input => input.Name == "C12");
        if (resistance is null || reactance is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "C11 and C12 are required.");
        }

        if (resistance.Unit != UnitCode.Ohm || reactance.Unit != UnitCode.Ohm)
        {
            return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, null, UnitCode.Unknown, CalculationStatus.Unknown, Array.Empty<RuleTrace>(), "C11 and C12 must be ohms in the candidate context.");
        }

        if (resistance.Value is not double resistanceValue || reactance.Value is not double reactanceValue)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "C11 and C12 must be numeric.");
        }

        var output = resistanceValue * 0.85 + reactanceValue * 0.5268;
        var trace = new[]
        {
            new RuleTrace("C11", resistanceValue, UnitCode.Ohm, "C11 * 0.85", "C13", output, UnitCode.Ohm),
            new RuleTrace("C12", reactanceValue, UnitCode.Ohm, "C12 * 0.5268", "C13", output, UnitCode.Ohm)
        };
        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, output, UnitCode.Ohm, CalculationStatus.Pass, trace, null);
    }
}