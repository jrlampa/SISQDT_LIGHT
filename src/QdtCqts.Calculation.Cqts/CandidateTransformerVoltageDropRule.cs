using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Regra candidata para cálculo da queda de tensão percentual interna no transformador de distribuição (célula BV4).
/// Identificador: CQTS.REAL_PROJECT.TRANSFORMER_VOLTAGE_DROP
/// Fórmula Excel comprovada (LADO 1!BV4):
///   DeltaV%_trafo = (M13 / AS6) * BW6
///   onde:
///     M13 = Carga total do transformador em kVA (BW13)
///     AS6 = Potência nominal do transformador em kVA (ex: 112.5 kVA)
///     BW6 = Impedância percentual de curto-circuito do transformador Z% (ex: 3.5%)
/// </summary>
public sealed class CandidateTransformerVoltageDropRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.TRANSFORMER_VOLTAGE_DROP.FORMULA";
    public const string EvidenceId = "F22.1-CQTS-PROJ7-LADO1-BV4";
    public string RuleId => "CQTS.REAL_PROJECT.TRANSFORMER_VOLTAGE_DROP";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var loadInput = inputs.SingleOrDefault(i => i.Name == "TrafoLoadKva" || i.Name == "M13" || i.Name == "M");
        var nominalPowerInput = inputs.SingleOrDefault(i => i.Name == "TrafoNominalKva" || i.Name == "AS6" || i.Name == "S_trafo");
        var impedancePercentInput = inputs.SingleOrDefault(i => i.Name == "TrafoImpedancePercent" || i.Name == "BW6" || i.Name == "Z_percent");

        if (loadInput is null || nominalPowerInput is null || impedancePercentInput is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "TrafoLoadKva, TrafoNominalKva and TrafoImpedancePercent are required.");
        }

        if (loadInput.Value is not double loadKva ||
            nominalPowerInput.Value is not double sTrafo ||
            impedancePercentInput.Value is not double zPercent)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Inputs must be numeric.");
        }

        if (sTrafo <= 0)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Transformer nominal power must be > 0.");
        }

        double deltaVPercent = (loadKva / sTrafo) * zPercent;

        var traces = new[]
        {
            new RuleTrace("TrafoLoadKva", loadKva, UnitCode.Kva, "Total transformer load (M13)", "TrafoDropPercent", deltaVPercent, UnitCode.Percent),
            new RuleTrace("TrafoNominalKva", sTrafo, UnitCode.Kva, "Nominal transformer capacity (AS6)", "TrafoDropPercent", deltaVPercent, UnitCode.Percent),
            new RuleTrace("TrafoImpedancePercent", zPercent, UnitCode.Percent, "Transformer impedance Z% (BW6)", "TrafoDropPercent", deltaVPercent, UnitCode.Percent)
        };

        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, deltaVPercent, UnitCode.Percent, CalculationStatus.Pass, traces, null);
    }
}
