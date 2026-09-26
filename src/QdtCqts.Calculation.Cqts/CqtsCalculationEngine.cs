using Microsoft.Extensions.Logging;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Motor de cálculo CQTS oficial integrado ao sisQDT_LIGHT.
/// Executa a cadeia completa de distribuição de energia com suporte a topologias radiais ricas,
/// ramificações, subderivações e cálculo térmico.
/// </summary>
public sealed class CqtsCalculationEngine : ICalculationEngine
{
    private readonly UnifiedCalculationPipeline _pipeline;

    public CqtsCalculationEngine(ILogger<CqtsCalculationEngine>? logger = null)
    {
        _pipeline = new UnifiedCalculationPipeline(logger);
    }

    public CalculationMode Mode => CalculationMode.Cqts;

    public CalculationResult Calculate(CalculationRequest request)
    {
        return _pipeline.Execute(request);
    }
}
