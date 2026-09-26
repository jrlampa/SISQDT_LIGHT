using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

public sealed class CqtsCalculationEngine : ICalculationEngine
{
    public CalculationMode Mode => CalculationMode.Cqts;

    public CalculationResult Calculate(CalculationRequest request) =>
        CalculationResult.Blocked(request, "ENGINE_NOT_READY", "CQTS production calculation is blocked until its rules are fully evidenced.");
}
