using QdtCqts.Domain;

namespace QdtCqts.Tests.Topology;

public sealed class TopologyTests
{
    [Fact]
    public void LinearQdtCircuitIsValidAndDeterministic()
    {
        var model = CreateModel(
            new[] { new Node("root", "TR", "circuit", null, null, true), new Node("n1", "P1", "circuit", null, null, false), new Node("n2", "P2", "circuit", null, null, false) },
            new[] { Edge("e1", "root", "n1"), Edge("e2", "n1", "n2") });
        var validator = new TopologyValidator();
        var first = validator.Validate(model, "circuit");
        var second = validator.Validate(model, "circuit");
        Assert.True(first.IsValid);
        Assert.Equal(first.OrderedNodeIds, second.OrderedNodeIds);
        Assert.Equal(new[] { "root", "n1", "n2" }, first.OrderedNodeIds);
    }

    [Fact]
    public void BranchingCqtsCircuitIsValid()
    {
        var model = CreateModel(
            new[] { new Node("root", "TR", "circuit", null, null, true), new Node("left", "P1", "circuit", null, null, false), new Node("right", "P2", "circuit", null, null, false) },
            new[] { Edge("e1", "root", "left"), Edge("e2", "root", "right") });
        Assert.True(new TopologyValidator().Validate(model, "circuit").IsValid);
    }

    [Fact]
    public void CycleIsRejected()
    {
        var model = CreateModel(
            new[] { new Node("root", "TR", "circuit", null, null, true), new Node("n1", "P1", "circuit", null, null, false) },
            new[] { Edge("e1", "root", "n1"), Edge("e2", "n1", "root") });
        Assert.Contains(new TopologyValidator().Validate(model, "circuit").Diagnostics, item => item.Code == TopologyDiagnosticCode.Cycle);
    }

    [Fact]
    public void MultipleParentsAreRejected()
    {
        var model = CreateModel(
            new[] { new Node("root", "TR", "circuit", null, null, true), new Node("a", "A", "circuit", null, null, false), new Node("b", "B", "circuit", null, null, false), new Node("child", "C", "circuit", null, null, false) },
            new[] { Edge("e1", "root", "a"), Edge("e2", "root", "b"), Edge("e3", "a", "child"), Edge("e4", "b", "child") });
        Assert.Contains(new TopologyValidator().Validate(model, "circuit").Diagnostics, item => item.Code == TopologyDiagnosticCode.MultipleParents);
    }

    [Fact]
    public void MissingRootAndOrphanAreRejected()
    {
        var model = CreateModel(new[] { new Node("n1", "P1", "circuit", null, null, false) }, Array.Empty<Edge>());
        var result = new TopologyValidator().Validate(model, "circuit");
        Assert.Contains(result.Diagnostics, item => item.Code == TopologyDiagnosticCode.MissingRoot);
        Assert.Contains(result.Diagnostics, item => item.Code == TopologyDiagnosticCode.Orphan);
    }

    private static Edge Edge(string id, string from, string to) => new(id, id, "circuit", from, to, new UnitValue(1, UnitCode.Meter), null, null, null);

    private static NetworkModel CreateModel(IReadOnlyList<Node> nodes, IReadOnlyList<Edge> edges) => new("model", "version", Array.Empty<Transformer>(), new[] { new Circuit("circuit", "C", "transformer", 1, CalculationMode.Cqts) }, nodes, edges, Array.Empty<Branch>(), Array.Empty<Load>(), Array.Empty<Conductor>(), Array.Empty<ElectricalParameter>());
}
