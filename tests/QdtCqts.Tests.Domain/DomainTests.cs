using QdtCqts.Domain;

namespace QdtCqts.Tests.Domain;

public sealed class DomainTests
{
    [Fact]
    public void SnapshotHashIsDeterministic()
    {
        Assert.Equal(SnapshotHash.Sha256("abc"), SnapshotHash.Sha256("abc"));
        Assert.NotEqual(SnapshotHash.Sha256("abc"), SnapshotHash.Sha256("abd"));
    }

    [Fact]
    public void UnknownUnitRemainsUnknown()
    {
        var value = UnitValue.Unknown();
        Assert.Equal(UnitCode.Unknown, value.Unit);
    }

    [Fact]
    public void NetworkModelPreservesUnknownParameters()
    {
        var parameter = new ElectricalParameter("ETA", null, null, UnitCode.Unknown, false);
        var model = new NetworkModel("model", "version", Array.Empty<Transformer>(), Array.Empty<Circuit>(), Array.Empty<Node>(), Array.Empty<Edge>(), Array.Empty<Branch>(), Array.Empty<Load>(), Array.Empty<Conductor>(), new[] { parameter });
        Assert.False(model.Parameters.Single().IsKnown);
        Assert.Equal(UnitCode.Unknown, model.Parameters.Single().Unit);
    }
}
