using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Qdt;

public sealed class QdtCalculationEngine : ICalculationEngine
{
    public CalculationMode Mode => CalculationMode.Qdt;

    public CalculationResult Calculate(CalculationRequest request) =>
        CalculationResult.Blocked(request, "ENGINE_NOT_READY", "QDT calculation is blocked until critical formulas and golden cases are resolved.");
}
