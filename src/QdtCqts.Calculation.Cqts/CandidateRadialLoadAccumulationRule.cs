using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Regra candidata para cálculo e acumulação radial a montante de cargas (coluna E) no CQTS.
/// Identificador: CQTS.REAL_PROJECT.RADIAL_LOAD_ACCUMULATION
/// Fórmula: E(trecho) = CargaLocal(no_jusante) + SUM(E(ramos_filhos))
/// </summary>
public sealed class CandidateRadialLoadAccumulationRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.RADIAL_LOAD_ACCUMULATION.FORMULA";
    public const string EvidenceId = "F21-CQTS-PROJ7-LADO1-E13";
    public string RuleId => "CQTS.REAL_PROJECT.RADIAL_LOAD_ACCUMULATION";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var localLoadInput = inputs.SingleOrDefault(input => input.Name == "LocalLoadKva" || input.Name == "LocalLoad");
        var childBranchesInput = inputs.SingleOrDefault(input => input.Name == "DownstreamBranchesLoadKva" || input.Name == "DownstreamLoad");
        var localConsumersInput = inputs.SingleOrDefault(input => input.Name == "LocalConsumers");
        var downstreamConsumersInput = inputs.SingleOrDefault(input => input.Name == "DownstreamConsumers");

        if (localLoadInput is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "LocalLoadKva is required.");
        }

        if (localLoadInput.Unit != UnitCode.Kva)
        {
            return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, null, UnitCode.Unknown, CalculationStatus.Unknown, Array.Empty<RuleTrace>(), "Local load must be in kVA.");
        }

        if (localLoadInput.Value is not double localLoad)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "LocalLoadKva must be numeric.");
        }

        double downstreamLoad = 0.0;
        if (childBranchesInput is not null)
        {
            if (childBranchesInput.Unit != UnitCode.Kva)
            {
                return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, null, UnitCode.Unknown, CalculationStatus.Unknown, Array.Empty<RuleTrace>(), "Downstream branches load must be in kVA.");
            }

            if (childBranchesInput.Value is double dLoad)
            {
                downstreamLoad = dLoad;
            }
            else if (childBranchesInput.Value is IEnumerable<double> branchLoads)
            {
                downstreamLoad = branchLoads.Sum();
            }
            else
            {
                return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Downstream branches load must be numeric or a list of doubles.");
            }
        }

        double accumulatedLoadKva = localLoad + downstreamLoad;

        var traces = new List<RuleTrace>
        {
            new("LocalLoadKva", localLoad, UnitCode.Kva, "Local load at downstream node", "AccumulatedLoadKva", accumulatedLoadKva, UnitCode.Kva),
            new("DownstreamLoadKva", downstreamLoad, UnitCode.Kva, "Sum of downstream child branches loads", "AccumulatedLoadKva", accumulatedLoadKva, UnitCode.Kva)
        };

        if (localConsumersInput?.Value is double localConsumers)
        {
            double downstreamConsumers = (downstreamConsumersInput?.Value as double?) ?? 0.0;
            double totalConsumers = localConsumers + downstreamConsumers;
            traces.Add(new("TotalConsumers", totalConsumers, UnitCode.Unknown, "Total accumulated consumers (D)", "AccumulatedLoadKva", accumulatedLoadKva, UnitCode.Kva));
        }

        return new(RuleId, FormulaId, EvidenceId, context.GoldenCaseId, accumulatedLoadKva, UnitCode.Kva, CalculationStatus.Pass, traces.ToArray(), null);
    }
}
