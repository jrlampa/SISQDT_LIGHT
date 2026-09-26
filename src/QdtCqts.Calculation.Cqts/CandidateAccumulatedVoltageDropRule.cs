using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Regra candidata para acumulação da queda de tensão ao longo de um caminho na árvore radial (coluna CA).
/// Identificador: CQTS.REAL_PROJECT.ACCUMULATED_VOLTAGE_DROP
/// Fórmula:
///   No nó raiz (LID): DeltaV%_acumulado = DeltaV%_MT + DeltaV%_trafo + DeltaV%_trecho_inicial
///   Nos nós a jusante: DeltaV%_acumulado(v) = DeltaV%_acumulado(pai) + DeltaV%_trecho(pai -> v)
/// </summary>
public sealed class CandidateAccumulatedVoltageDropRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.ACCUMULATED_VOLTAGE_DROP.FORMULA";
    public const string EvidenceId = "F22-CQTS-PROJ7-LADO1-CA13";
    public string RuleId => "CQTS.REAL_PROJECT.ACCUMULATED_VOLTAGE_DROP";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var segmentDropInput = inputs.SingleOrDefault(i => i.Name == "SegmentDropPercent" || i.Name == "BZ");
        var upstreamDropInput = inputs.SingleOrDefault(i => i.Name == "UpstreamDropPercent" || i.Name == "CA_Parent");
        var trafoDropInput = inputs.SingleOrDefault(i => i.Name == "TrafoDropPercent" || i.Name == "BV4");
        var mtDropInput = inputs.SingleOrDefault(i => i.Name == "MtDropPercent" || i.Name == "CV105");

        if (segmentDropInput is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "SegmentDropPercent is required.");
        }

        if (segmentDropInput.Value is not double segmentDrop)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "SegmentDropPercent must be numeric.");
        }

        double upstreamAccumulated = 0.0;
        var traces = new List<RuleTrace>
        {
            new("SegmentDropPercent", segmentDrop, UnitCode.Percent, "Segment voltage drop DeltaV%", "AccumulatedDropPercent", 0.0, UnitCode.Percent)
        };

        if (upstreamDropInput?.Value is double parentDrop)
        {
            upstreamAccumulated = parentDrop;
            traces.Add(new("UpstreamDropPercent", parentDrop, UnitCode.Percent, "Parent node accumulated voltage drop", "AccumulatedDropPercent", 0.0, UnitCode.Percent));
        }
        else
        {
            // Nó raiz / inicial: inclui componente MT e transformador
            double trafoDrop = (trafoDropInput?.Value as double?) ?? 0.0;
            double mtDrop = (mtDropInput?.Value as double?) ?? 0.0;
            upstreamAccumulated = trafoDrop + mtDrop;

            traces.Add(new("TrafoDropPercent", trafoDrop, UnitCode.Percent, "Transformer internal voltage drop (BV4)", "AccumulatedDropPercent", 0.0, UnitCode.Percent));
            traces.Add(new("MtDropPercent", mtDrop, UnitCode.Percent, "Medium voltage upstream drop reflected to BT (CV105)", "AccumulatedDropPercent", 0.0, UnitCode.Percent));
        }

        double totalAccumulated = upstreamAccumulated + segmentDrop;

        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, totalAccumulated, UnitCode.Percent, CalculationStatus.Pass, traces.ToArray(), null);
    }
}
