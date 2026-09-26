using QdtCqts.Calculation.Abstractions;
using QdtCqts.Calculation.Cqts;
using QdtCqts.Calculation.Qdt;
using QdtCqts.Domain;

namespace QdtCqts.Tests.Domain;

public sealed class EngineContractTests
{
    [Theory]
    [InlineData(CalculationMode.Qdt)]
    [InlineData(CalculationMode.Cqts)]
    public void ProductionEnginesRemainBlocked(CalculationMode mode)
    {
        var model = new NetworkModel("model", "version", Array.Empty<Transformer>(), Array.Empty<Circuit>(), Array.Empty<Node>(), Array.Empty<Edge>(), Array.Empty<Branch>(), Array.Empty<Load>(), Array.Empty<Conductor>(), Array.Empty<ElectricalParameter>());
        var version = new ProjectVersion("version", "project", "TEST", model, "source", true);
        var request = new CalculationRequest(version, mode, "foundation-0", "input-hash");
        var result = mode == CalculationMode.Qdt ? new QdtCalculationEngine().Calculate(request) : new CqtsCalculationEngine().Calculate(request);
        Assert.Equal(CalculationStatus.Blocked, result.Status);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "ENGINE_NOT_READY");
    }
}
