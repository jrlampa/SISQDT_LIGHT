using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

public sealed class CandidateEndLoadSelectionRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.END_LOAD_SELECTION.FORMULA";
    public const string EvidenceId = "F20-CQTS-PROJ7-LADO1-M13";
    public string RuleId => "CQTS.REAL_PROJECT.END_LOAD_SELECTION";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var consumersInput = inputs.SingleOrDefault(input => input.Name == "D13" || input.Name == "Consumers");
        var loadInput = inputs.SingleOrDefault(input => input.Name == "E13" || input.Name == "AccumulatedLoad");
        var diversityInput = inputs.SingleOrDefault(input => input.Name == "G13" || input.Name == "DiversityFactor");
        var applyFloorInput = inputs.SingleOrDefault(input => input.Name == "CH5" || input.Name == "ApplyConsumerFloor");

        if (consumersInput is null || loadInput is null || diversityInput is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Consumers (D), AccumulatedLoad (E) and DiversityFactor (G) are required.");
        }

        if (loadInput.Unit != UnitCode.Kva)
        {
            return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, null, UnitCode.Unknown, CalculationStatus.Unknown, Array.Empty<RuleTrace>(), "Accumulated load must be in kVA.");
        }

        if (consumersInput.Value is not double consumers || loadInput.Value is not double loadKva || diversityInput.Value is not double diversityFactor)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Inputs D, E and G must be numeric.");
        }

        if (consumers < 1 || diversityFactor <= 0)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Consumers must be >= 1 and diversity factor must be > 0.");
        }

        bool applyFloor = true;
        if (applyFloorInput is not null)
        {
            if (applyFloorInput.Value is string s)
            {
                applyFloor = string.Equals(s, "SIM", StringComparison.OrdinalIgnoreCase);
            }
            else if (applyFloorInput.Value is bool b)
            {
                applyFloor = b;
            }
        }

        double output;
        if (applyFloor)
        {
            if (consumers > 2)
            {
                output = loadKva * diversityFactor;
            }
            else if (Math.Abs(consumers - 2.0) < 0.001)
            {
                output = 8.0 * diversityFactor;
            }
            else
            {
                output = 4.0 * diversityFactor;
            }
        }
        else
        {
            output = loadKva * diversityFactor;
        }

        var trace = new[]
        {
            new RuleTrace("Consumers", consumers, UnitCode.Unknown, "Accumulated consumer count (D)", "M13", output, UnitCode.Kva),
            new RuleTrace("AccumulatedLoad", loadKva, UnitCode.Kva, "Accumulated segment load in kVA (E)", "M13", output, UnitCode.Kva),
            new RuleTrace("DiversityFactor", diversityFactor, UnitCode.Unknown, "Diversity factor FDIV (G)", "M13", output, UnitCode.Kva)
        };

        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, output, UnitCode.Kva, CalculationStatus.Pass, trace, null);
    }
}
