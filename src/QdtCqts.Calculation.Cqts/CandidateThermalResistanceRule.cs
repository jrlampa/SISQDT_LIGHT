using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Regra candidata para cálculo da resistência de corrente alternada corrigida pela temperatura de regime do condutor (coluna BI).
/// Identificador: CQTS.REAL_PROJECT.THERMAL_RESISTANCE
/// Fórmula Excel comprovada (LADO 1!BI13):
///   Rca(T) = Rcc_20 * (1 + alpha_20 * (T_cabo - 20)) * K_star
///   onde:
///     Rcc_20 = Resistência CC a 20°C em Ohm/km (coluna AT)
///     alpha_20 = Coeficiente de variação térmica da resistência (0.00403 para Alumínio, 0.00393 para Cobre) (coluna AW)
///     T_cabo = Temperatura de regime contínuo do condutor em °C (coluna BX, vinda de P13)
///     K_star = Fator de efeito pelicular/proximidade K* = Rca_90 / Rcc_90 (coluna BE)
/// </summary>
public sealed class CandidateThermalResistanceRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.THERMAL_RESISTANCE.FORMULA";
    public const string EvidenceId = "F22.1-CQTS-PROJ7-LADO1-BI13";
    public string RuleId => "CQTS.REAL_PROJECT.THERMAL_RESISTANCE";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var rcc20Input = inputs.SingleOrDefault(i => i.Name == "Rcc20" || i.Name == "AT" || i.Name == "ResistanceDc20");
        var alphaInput = inputs.SingleOrDefault(i => i.Name == "Alpha20" || i.Name == "AW" || i.Name == "Alpha");
        var tempInput = inputs.SingleOrDefault(i => i.Name == "Temperature" || i.Name == "BX" || i.Name == "CableTemperature");
        var kStarInput = inputs.SingleOrDefault(i => i.Name == "KStar" || i.Name == "BE" || i.Name == "AcDcRatio");

        if (rcc20Input is null || tempInput is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Rcc20 and Temperature are required.");
        }

        if (rcc20Input.Value is not double rcc20 ||
            tempInput.Value is not double temp)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Inputs must be numeric.");
        }

        double alpha = 0.00403; // Default Alumínio
        if (alphaInput?.Value is double aVal && aVal > 0)
        {
            alpha = aVal;
        }

        double kStar = 1.0;
        if (kStarInput?.Value is double kVal && kVal > 0)
        {
            kStar = kVal;
        }

        double rcaT = rcc20 * (1.0 + alpha * (temp - 20.0)) * kStar;

        var traces = new[]
        {
            new RuleTrace("Rcc20", rcc20, UnitCode.Ohm, "DC resistance at 20°C [Ohm/km] (AT)", "RcaT", rcaT, UnitCode.Ohm),
            new RuleTrace("Alpha20", alpha, UnitCode.Unknown, "Thermal coefficient [1/°C] (AW)", "RcaT", rcaT, UnitCode.Ohm),
            new RuleTrace("CableTemperature", temp, UnitCode.Celsius, "Operating temperature [°C] (BX)", "RcaT", rcaT, UnitCode.Ohm),
            new RuleTrace("KStar", kStar, UnitCode.Unknown, "AC/DC skin effect ratio (BE)", "RcaT", rcaT, UnitCode.Ohm)
        };

        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, rcaT, UnitCode.Ohm, CalculationStatus.Pass, traces, null);
    }
}
