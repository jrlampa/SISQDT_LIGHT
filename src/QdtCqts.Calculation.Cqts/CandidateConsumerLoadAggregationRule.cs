using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Regra candidata para agregação de cargas individuais de consumidores conectados diretamente a um ponto/poste.
/// Identificador: CQTS.REAL_PROJECT.CONSUMER_LOAD_AGGREGATION
/// Fórmula: S_local = SUM(N_c * S_unit_c)
/// </summary>
public sealed class CandidateConsumerLoadAggregationRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.CONSUMER_LOAD_AGGREGATION.FORMULA";
    public const string EvidenceId = "F21-CQTS-CONSUMER-LOADS-EVIDENCE";
    public string RuleId => "CQTS.REAL_PROJECT.CONSUMER_LOAD_AGGREGATION";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var consumerGroupsInput = inputs.SingleOrDefault(input => input.Name == "ConsumerGroups");
        if (consumerGroupsInput is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "ConsumerGroups is required.");
        }

        if (consumerGroupsInput.Value is not IEnumerable<(int Count, double UnitLoadKva)> groups)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "ConsumerGroups must be an IEnumerable of (int Count, double UnitLoadKva).");
        }

        double totalLoadKva = 0.0;
        int totalConsumers = 0;
        var traces = new List<RuleTrace>();

        int index = 0;
        foreach (var (count, unitLoad) in groups)
        {
            index++;
            if (count < 0 || unitLoad < 0)
            {
                return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Consumer count and unit load must be non-negative.");
            }

            double subtotal = count * unitLoad;
            totalLoadKva += subtotal;
            totalConsumers += count;

            traces.Add(new($"Group_{index}", $"{count} x {unitLoad} kVA", UnitCode.Kva, "Subtotal of consumer group", "TotalLoadKva", subtotal, UnitCode.Kva));
        }

        traces.Add(new("TotalConsumers", totalConsumers, UnitCode.Unknown, "Total local consumers", "TotalConsumers", totalConsumers, UnitCode.Unknown));

        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, totalLoadKva, UnitCode.Kva, CalculationStatus.Pass, traces.ToArray(), null);
    }
}
