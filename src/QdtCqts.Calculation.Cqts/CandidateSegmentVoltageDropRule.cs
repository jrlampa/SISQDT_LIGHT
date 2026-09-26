using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Regra candidata para cálculo da queda de tensão percentual no trecho individual (coluna BZ).
/// Identificador: CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP
/// Fórmula:
///   DeltaV% = M * (Z / (V^2 / 100)) * Lequiv * FaseFactor
///   onde FaseFactor = 1 (3 fases), 2 (2 fases), 6 (1 fase).
/// </summary>
public sealed class CandidateSegmentVoltageDropRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP.FORMULA";
    public const string EvidenceId = "F22-CQTS-PROJ7-LADO1-BZ13";
    public string RuleId => "CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var loadInput = inputs.SingleOrDefault(i => i.Name == "M" || i.Name == "LoadKva");
        var resistanceInput = inputs.SingleOrDefault(i => i.Name == "R" || i.Name == "ResistanceOhmPerKm");
        var reactanceInput = inputs.SingleOrDefault(i => i.Name == "X" || i.Name == "ReactanceOhmPerKm");
        var lengthInput = inputs.SingleOrDefault(i => i.Name == "L" || i.Name == "LengthMeters");
        var parallelCablesInput = inputs.SingleOrDefault(i => i.Name == "ParallelCables" || i.Name == "AP");
        var voltageInput = inputs.SingleOrDefault(i => i.Name == "V" || i.Name == "VoltageNominal");
        var phaseCountInput = inputs.SingleOrDefault(i => i.Name == "H" || i.Name == "PhaseCount");

        if (loadInput is null || resistanceInput is null || reactanceInput is null || lengthInput is null || voltageInput is null || phaseCountInput is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "M, R, X, L, V and PhaseCount are required.");
        }

        if (loadInput.Value is not double loadKva ||
            resistanceInput.Value is not double r ||
            reactanceInput.Value is not double x ||
            lengthInput.Value is not double lengthMeters ||
            voltageInput.Value is not double vNominal ||
            phaseCountInput.Value is not double phaseCount)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Inputs must be numeric.");
        }

        if (vNominal <= 0 || lengthMeters < 0 || loadKva < 0)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "V must be > 0, Length and Load must be >= 0.");
        }

        double parallelCables = 1.0;
        if (parallelCablesInput?.Value is double pCables && pCables > 0)
        {
            parallelCables = pCables;
        }

        double lEquiv = lengthMeters / parallelCables;
        double z = Math.Sqrt((r * r) + (x * x));
        double factorBj = z / ((vNominal * vNominal) / 100.0);

        int phases = (int)Math.Round(phaseCount);
        double phaseFactor = phases switch
        {
            3 => 1.0,
            2 => 2.0,
            1 => 6.0,
            _ => 1.0
        };

        double deltaVPercent = loadKva * factorBj * lEquiv * phaseFactor;

        var traces = new[]
        {
            new RuleTrace("LoadKva", loadKva, UnitCode.Kva, "Load in kVA (M)", "DeltaVPercent", deltaVPercent, UnitCode.Percent),
            new RuleTrace("ImpedanceOhmPerKm", z, UnitCode.Ohm, "Z = sqrt(R^2 + X^2) in Ohm/km", "DeltaVPercent", deltaVPercent, UnitCode.Percent),
            new RuleTrace("FactorBJ", factorBj, UnitCode.Unknown, "BJ = Z / (V^2 / 100)", "DeltaVPercent", deltaVPercent, UnitCode.Percent),
            new RuleTrace("EquivalentLengthMeters", lEquiv, UnitCode.Meter, "Lequiv = L / parallelCables", "DeltaVPercent", deltaVPercent, UnitCode.Percent),
            new RuleTrace("PhaseFactor", phaseFactor, UnitCode.Unknown, "Multiplier based on phase count (1, 2 or 6)", "DeltaVPercent", deltaVPercent, UnitCode.Percent)
        };

        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, deltaVPercent, UnitCode.Percent, CalculationStatus.Pass, traces, null);
    }
}
