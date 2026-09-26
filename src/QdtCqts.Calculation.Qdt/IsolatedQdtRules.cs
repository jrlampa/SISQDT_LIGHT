using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Qdt;

public sealed class SelectKlToM13Rule : IIsolatedRule
{
    public const string FormulaId = "QDT.LADO1.M13.FORMULA";
    public const string EvidenceId = "QDT.LADO1.M13.EVIDENCE";
    public string RuleId => "QDT.LADO1.SELECT_KL_TO_M13";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var flag = inputs.SingleOrDefault(input => input.Name == "CH5")?.Value as string;
        var k = inputs.SingleOrDefault(input => input.Name == "K13")?.Value as double?;
        var l = inputs.SingleOrDefault(input => input.Name == "L13")?.Value as double?;
        if (k is null || l is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "K13 and L13 are required for the isolated rule.");
        }

        var output = string.Equals(flag, "SIM", StringComparison.Ordinal) ? k.Value : l.Value;
        var trace = new[]
        {
            new RuleTrace("CH5", flag, UnitCode.ConductorKey, "IF CH5 == SIM", "M13", output, UnitCode.Kva),
            new RuleTrace("K13", k, UnitCode.Kva, "selected branch", "M13", output, UnitCode.Kva),
            new RuleTrace("L13", l, UnitCode.Kva, "selected branch", "M13", output, UnitCode.Kva)
        };
        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, output, UnitCode.Kva, CalculationStatus.Pass, trace, null);
    }
}

public sealed class ValidationI13Rule : IIsolatedRule
{
    public const string FormulaId = "QDT.LADO1.I13.FORMULA";
    public const string EvidenceId = "QDT.LADO1.I13.EVIDENCE";
    public string RuleId => "QDT.LADO1.VALIDATION_I13";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var d13 = Number(inputs, "D13");
        var h13 = Number(inputs, "H13");
        if (d13 is null || h13 is null)
        {
            return TextResult(context, string.Empty, inputs);
        }

        var d14 = Number(inputs, "D14");
        var h9 = Number(inputs, "H9");
        if (d14 is null || h9 is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "D14 and H9 are required when D13 and H13 are present.");
        }

        var output = d13 == 0 || h13 == 0 || !(h13 >= h9 && d14 <= d13) ? "Erro 02" : "OK !";
        return TextResult(context, output, inputs);
    }

    private static RuleExecutionResult TextResult(RuleExecutionContext context, string output, IReadOnlyList<RuleInput> inputs)
    {
        var traces = inputs.Select(input => new RuleTrace(input.Name, input.Value, input.Unit, "IF/OR/AND", "I13", output, UnitCode.ConductorKey)).ToArray();
        return new("QDT.LADO1.VALIDATION_I13", FormulaId, EvidenceId, context.GoldenCaseId, output, UnitCode.ConductorKey, CalculationStatus.Pass, traces, null);
    }

    private static double? Number(IReadOnlyList<RuleInput> inputs, string name) =>
        inputs.SingleOrDefault(input => input.Name == name)?.Value as double?;
}
