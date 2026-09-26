namespace QdtCqts.Domain;

public sealed record TopologyDiagnostic(TopologyDiagnosticCode Code, string Message, string? EntityId = null);

public sealed record TopologyValidationResult(
    CalculationStatus Status,
    IReadOnlyList<string> OrderedNodeIds,
    IReadOnlyList<string> OrderedEdgeIds,
    IReadOnlyList<string> RootNodeIds,
    IReadOnlyList<string> TerminalNodeIds,
    IReadOnlyList<TopologyDiagnostic> Diagnostics)
{
    public bool IsValid => Status == CalculationStatus.Pass;
}

public sealed class TopologyValidator
{
    public TopologyValidationResult Validate(NetworkModel model, string circuitId)
    {
        var circuit = model.Circuits.SingleOrDefault(item => item.Id == circuitId);
        if (circuit is null)
        {
            return Failure(new TopologyDiagnostic(TopologyDiagnosticCode.Orphan, "Circuit does not exist.", circuitId));
        }

        var nodes = model.Nodes.Where(item => item.CircuitId == circuitId).ToDictionary(item => item.Id);
        var edges = model.Edges.Where(item => item.CircuitId == circuitId).ToArray();
        var diagnostics = new List<TopologyDiagnostic>();

        foreach (var edge in edges)
        {
            if (edge.FromNodeId == edge.ToNodeId)
            {
                diagnostics.Add(new(TopologyDiagnosticCode.SelfLoop, "Edge cannot connect a node to itself.", edge.Id));
            }

            if (!nodes.ContainsKey(edge.FromNodeId) || !nodes.ContainsKey(edge.ToNodeId))
            {
                diagnostics.Add(new(TopologyDiagnosticCode.MissingEndpoint, "Edge endpoint does not exist in the circuit.", edge.Id));
            }

            if (edge.Length.Unit == UnitCode.Meter && edge.Length.Magnitude < 0)
            {
                diagnostics.Add(new(TopologyDiagnosticCode.NegativeLength, "Edge length cannot be negative.", edge.Id));
            }
        }

        var roots = nodes.Values.Where(node => node.IsSource).Select(node => node.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray();
        if (roots.Length == 0)
        {
            diagnostics.Add(new(TopologyDiagnosticCode.MissingRoot, "Circuit must have one source node."));
        }
        else if (roots.Length > 1)
        {
            diagnostics.Add(new(TopologyDiagnosticCode.MultipleRoots, "Circuit cannot have multiple source nodes."));
        }

        var incoming = edges.GroupBy(edge => edge.ToNodeId).ToDictionary(group => group.Key, group => group.Count());
        foreach (var item in incoming.Where(pair => pair.Value > 1))
        {
            diagnostics.Add(new(TopologyDiagnosticCode.MultipleParents, "Node has multiple incoming edges.", item.Key));
        }

        var reachable = new HashSet<string>(StringComparer.Ordinal);
        if (roots.Length == 1)
        {
            var queue = new Queue<string>();
            queue.Enqueue(roots[0]);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!reachable.Add(current))
                {
                    continue;
                }

                foreach (var child in edges.Where(edge => edge.FromNodeId == current).OrderBy(edge => edge.ToNodeId, StringComparer.Ordinal))
                {
                    queue.Enqueue(child.ToNodeId);
                }
            }
        }

        foreach (var node in nodes.Values.Where(node => !reachable.Contains(node.Id)))
        {
            diagnostics.Add(new(TopologyDiagnosticCode.Orphan, "Node is not reachable from the circuit root.", node.Id));
        }

        var orderedNodeIds = TopologicalOrder(nodes.Keys, edges, diagnostics);
        var orderedEdgeIds = edges.OrderBy(edge => orderedNodeIds.IndexOf(edge.FromNodeId)).ThenBy(edge => edge.ToNodeId, StringComparer.Ordinal).Select(edge => edge.Id).ToArray();
        var terminalNodeIds = nodes.Values.Where(node => edges.All(edge => edge.FromNodeId != node.Id)).Select(node => node.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray();

        return diagnostics.Count == 0
            ? new(CalculationStatus.Pass, orderedNodeIds, orderedEdgeIds, roots, terminalNodeIds, diagnostics)
            : new(CalculationStatus.Fail, orderedNodeIds, orderedEdgeIds, roots, terminalNodeIds, diagnostics);
    }

    private static List<string> TopologicalOrder(IEnumerable<string> nodeIds, IReadOnlyList<Edge> edges, ICollection<TopologyDiagnostic> diagnostics)
    {
        var indegree = nodeIds.ToDictionary(id => id, _ => 0, StringComparer.Ordinal);
        var children = nodeIds.ToDictionary(id => id, _ => new List<string>(), StringComparer.Ordinal);
        foreach (var edge in edges)
        {
            if (!indegree.ContainsKey(edge.FromNodeId) || !indegree.ContainsKey(edge.ToNodeId))
            {
                continue;
            }

            indegree[edge.ToNodeId]++;
            children[edge.FromNodeId].Add(edge.ToNodeId);
        }

        var queue = new PriorityQueue<string, string>(StringComparer.Ordinal);
        foreach (var item in indegree.Where(pair => pair.Value == 0).Select(pair => pair.Key))
        {
            queue.Enqueue(item, item);
        }

        var result = new List<string>();
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);
            foreach (var child in children[current].OrderBy(id => id, StringComparer.Ordinal))
            {
                indegree[child]--;
                if (indegree[child] == 0)
                {
                    queue.Enqueue(child, child);
                }
            }
        }

        if (result.Count != indegree.Count)
        {
            diagnostics.Add(new(TopologyDiagnosticCode.Cycle, "Circuit contains a directed cycle."));
        }

        return result;
    }

    private static TopologyValidationResult Failure(TopologyDiagnostic diagnostic) =>
        new(CalculationStatus.Fail, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), new[] { diagnostic });
}
