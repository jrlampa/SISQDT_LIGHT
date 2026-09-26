using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

public sealed class CandidateCableTemperatureRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.CABLE_TEMPERATURE.FORMULA";
    public const string EvidenceId = "F17-CQTS-PROJ7-LADO1-P13";
    public string RuleId => "CQTS.REAL_PROJECT.CABLE_TEMPERATURE";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var load = inputs.SingleOrDefault(input => input.Name == "M13");
        var voltage = inputs.SingleOrDefault(input => input.Name == "BX6");
        var phases = inputs.SingleOrDefault(input => input.Name == "H13");
        var ampacity = inputs.SingleOrDefault(input => input.Name == "AN13");
        var length = inputs.SingleOrDefault(input => input.Name == "AP13");
        if (load is null || voltage is null || phases is null || ampacity is null || length is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "M13, BX6, H13, AN13 and AP13 are required.");
        }

        if (load.Unit != UnitCode.Kva || voltage.Unit != UnitCode.V || ampacity.Unit != UnitCode.Ampere || length.Unit != UnitCode.Meter)
        {
            return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, null, UnitCode.Unknown, CalculationStatus.Unknown, Array.Empty<RuleTrace>(), "Candidate units are incompatible.");
        }

        if (load.Value is not double loadValue || voltage.Value is not double voltageValue || phases.Value is not double phaseCount || ampacity.Value is not double ampacityValue || length.Value is not double lengthValue)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Candidate inputs must be numeric.");
        }

        if (phaseCount != 3 || voltageValue == 0 || ampacityValue == 0 || lengthValue == 0)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "This candidate branch is only validated for three-phase non-zero inputs.");
        }

        var output = loadValue / (voltageValue * Math.Sqrt(3) / 1000d) / ((ampacityValue * lengthValue) / (90d - 30d)) + 30d;
        var trace = new[]
        {
            new RuleTrace("M13", loadValue, UnitCode.Kva, "M13 / (BX6 * SQRT(3) / 1000)", "P13/BX13", output, UnitCode.Celsius),
            new RuleTrace("BX6", voltageValue, UnitCode.V, "three-phase voltage branch", "P13/BX13", output, UnitCode.Celsius),
            new RuleTrace("AN13*AP13", ampacityValue * lengthValue, UnitCode.Unknown, "(AN13 * AP13) / (90 - 30)", "P13/BX13", output, UnitCode.Celsius)
        };
        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, output, UnitCode.Celsius, CalculationStatus.Pass, trace, null);
    }
}