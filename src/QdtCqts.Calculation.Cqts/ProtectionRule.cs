using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

public sealed class IbInIzProtectionRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.PROTECTION.IB_IN_IZ.FORMULA";
    public const string EvidenceId = "CQTS.PROTECTION.IB_IN_IZ.EVIDENCE";
    public string RuleId => "CQTS.PROTECTION.IB_IN_IZ";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var ib = inputs.SingleOrDefault(input => input.Name == "Ib");
        var @in = inputs.SingleOrDefault(input => input.Name == "In");
        var iz = inputs.SingleOrDefault(input => input.Name == "Iz");
        if (ib is null || @in is null || iz is null) return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Ib, In and Iz are required.");
        if (ib.Unit != UnitCode.Ampere || @in.Unit != UnitCode.Ampere || iz.Unit != UnitCode.Ampere) return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, null, UnitCode.Unknown, CalculationStatus.Unknown, Array.Empty<RuleTrace>(), "Ib, In and Iz must be expressed in ampere.");
        if (ib.Value is not double ibValue || @in.Value is not double inValue || iz.Value is not double izValue) return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Ib, In and Iz must be numeric.");

        var output = ibValue <= inValue && inValue <= izValue ? "OK" : "VERIFICAR";
        var trace = new[]
        {
            new RuleTrace("Ib", ibValue, UnitCode.Ampere, "Ib <= In <= Iz", "PROTECAO", output, UnitCode.ConductorKey),
            new RuleTrace("In", inValue, UnitCode.Ampere, "Ib <= In <= Iz", "PROTECAO", output, UnitCode.ConductorKey),
            new RuleTrace("Iz", izValue, UnitCode.Ampere, "Ib <= In <= Iz", "PROTECAO", output, UnitCode.ConductorKey)
        };
        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, output, UnitCode.ConductorKey, CalculationStatus.Pass, trace, null);
    }
}
