using System;
using System.Collections.Generic;
using System.Linq;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Construtor determinístico de projeção de apresentação unifilar (Fase 27B).
/// Funde NetworkModel + TopologyValidationResult + NetworkCalculationReport
/// em UnifilarDiagramViewModel sem recalcular grandezas elétricas.
/// </summary>
public sealed class UnifilarPresentationBuilder
{
    private readonly IUnifilarLayoutEngine _layoutEngine;

    public UnifilarPresentationBuilder(IUnifilarLayoutEngine? layoutEngine = null)
    {
        _layoutEngine = layoutEngine ?? new HierarchicalTreeLayoutEngine();
    }

    public UnifilarDiagramViewModel BuildDiagram(
        NetworkModel model,
        TopologyValidationResult topology,
        NetworkCalculationReport report,
        CalculationUiState uiState = CalculationUiState.Success)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(report);

        // 1. Calcula posições geométricas independentes de grandezas elétricas
        var positions = _layoutEngine.ComputeLayout(model, topology);

        var outgoingEdges = model.Edges.ToLookup(e => e.FromNodeId, StringComparer.Ordinal);
        var terminalSet = new HashSet<string>(topology.TerminalNodeIds, StringComparer.Ordinal);
        var rootSet = new HashSet<string>(topology.RootNodeIds, StringComparer.Ordinal);

        var nodeResultsById = report.Nodes.ToDictionary(n => n.NodeId, StringComparer.Ordinal);
        var segmentResultsById = report.Segments.ToDictionary(s => s.EdgeId, StringComparer.Ordinal);
        var trafoResultsById = report.Transformers.ToDictionary(t => t.TransformerId, StringComparer.Ordinal);

        var nodeVms = new List<UnifilarNodeViewModel>();
        var edgeVms = new List<UnifilarEdgeViewModel>();
        var trafoVms = new List<UnifilarTransformerViewModel>();

        // 2. Constrói projeção de Nós
        foreach (var node in model.Nodes)
        {
            var (x, y) = positions.TryGetValue(node.Id, out var pos) ? pos : (0.0, 0.0);
            nodeResultsById.TryGetValue(node.Id, out var nodeResult);

            var nodeType = DetermineNodeType(node, rootSet, terminalSet, outgoingEdges[node.Id].Count());

            nodeVms.Add(new UnifilarNodeViewModel
            {
                NodeId = node.Id,
                ExternalKey = node.ExternalKey,
                CircuitId = node.CircuitId,
                NodeType = nodeType,
                X = x,
                Y = y,
                LocalConsumers = nodeResult?.LocalConsumers ?? 0,
                LocalLoadKva = nodeResult?.LocalLoadKva ?? 0.0,
                AccumulatedVoltageDropPercent = nodeResult?.AccumulatedVoltageDropPercent ?? 0.0,
                VoltageV127 = nodeResult?.VoltageV127 ?? 127.0,
                VoltageV220 = nodeResult?.VoltageV220 ?? 220.0,
                RunId = report.RunId
            });
        }

        // 3. Constrói projeção de Trechos (Arestas)
        foreach (var edge in model.Edges)
        {
            var (x1, y1) = positions.TryGetValue(edge.FromNodeId, out var p1) ? p1 : (0.0, 0.0);
            var (x2, y2) = positions.TryGetValue(edge.ToNodeId, out var p2) ? p2 : (0.0, 0.0);

            segmentResultsById.TryGetValue(edge.Id, out var segResult);

            edgeVms.Add(new UnifilarEdgeViewModel
            {
                EdgeId = edge.Id,
                CircuitId = edge.CircuitId,
                FromNodeId = edge.FromNodeId,
                ToNodeId = edge.ToNodeId,
                ConductorKey = segResult?.ConductorKey ?? edge.ConductorId,
                PhaseCount = segResult?.PhaseCount ?? (int.TryParse(edge.Phase, out var p) ? p : 3),
                ParallelCables = segResult?.ParallelCables ?? (double.TryParse(edge.InstallationMethod, out var ap) ? ap : 1.0),
                PhysicalLengthMeters = segResult?.PhysicalLengthMeters ?? edge.Length.Magnitude,
                EquivalentLengthMeters = segResult?.EquivalentLengthMeters ?? edge.Length.Magnitude,
                DownstreamAccumulatedConsumers = segResult?.DownstreamAccumulatedConsumers ?? 0.0,
                DownstreamAccumulatedLoadKva = segResult?.DownstreamAccumulatedLoadKva ?? 0.0,
                DiversityFactor = segResult?.DiversityFactor ?? 1.0,
                EndLoadSelectedKva = segResult?.EndLoadSelectedKva ?? 0.0,
                OperatingCurrentAmperes = segResult?.OperatingCurrentAmperes ?? 0.0,
                RatedAmpacityAmperes = segResult?.RatedAmpacityAmperes ?? 0.0,
                IsOverloaded = segResult?.IsOverloaded ?? false, // Consumo direto do cálculo do backend
                OperatingTemperatureCelsius = segResult?.OperatingTemperatureCelsius ?? 0.0,
                ResistanceCaOhmPerKm = segResult?.ResistanceCaOhmPerKm ?? 0.0,
                ReactanceOhmPerKm = segResult?.ReactanceOhmPerKm ?? 0.0,
                ImpedanceOhmPerKm = segResult?.ImpedanceOhmPerKm ?? 0.0,
                FactorBj = segResult?.FactorBj ?? 0.0,
                PhaseFactor = segResult?.PhaseFactor ?? 1.0,
                SegmentVoltageDropPercent = segResult?.SegmentVoltageDropPercent ?? 0.0,
                ShortCircuit3PhaseAmperes = segResult?.ShortCircuit3PhaseAmperes ?? 0.0,
                ShortCircuit1PhaseAmperes = segResult?.ShortCircuit1PhaseAmperes ?? 0.0,
                UpstreamResistanceOhm = segResult?.UpstreamResistanceOhm ?? 0.0,
                UpstreamReactanceOhm = segResult?.UpstreamReactanceOhm ?? 0.0,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                RunId = report.RunId
            });
        }

        // 4. Constrói projeção de Transformadores
        foreach (var trafo in model.Transformers)
        {
            trafoResultsById.TryGetValue(trafo.Id, out var trafoResult);

            // Posiciona o transformador visualmente ligeiramente acima do nó raiz do circuito correspondente
            var circuit = model.Circuits.FirstOrDefault(c => c.TransformerId == trafo.Id);
            var rootNode = model.Nodes.FirstOrDefault(n => n.CircuitId == circuit?.Id && n.IsSource);
            var (rx, ry) = (rootNode != null && positions.TryGetValue(rootNode.Id, out var rp)) ? rp : (40.0, 40.0);

            trafoVms.Add(new UnifilarTransformerViewModel
            {
                TransformerId = trafo.Id,
                ExternalKey = trafo.ExternalKey,
                NominalPowerKva = trafoResult?.NominalPowerKva ?? trafo.Power.Magnitude,
                OperatingLoadKva = trafoResult?.OperatingLoadKva ?? trafo.Demand.Magnitude,
                ImpedancePercent = trafoResult?.ImpedancePercent ?? trafo.Impedance.Magnitude,
                TrafoVoltageDropPercent = trafoResult?.TrafoVoltageDropPercent ?? 0.0,
                MtVoltageDropPercent = trafoResult?.MtVoltageDropPercent ?? 0.0,
                TotalOriginVoltageDropPercent = trafoResult?.TotalOriginVoltageDropPercent ?? 0.0,
                X = rx - 50.0,
                Y = ry,
                RunId = report.RunId
            });
        }

        return new UnifilarDiagramViewModel(
            model,
            topology,
            report,
            nodeVms,
            edgeVms,
            trafoVms,
            uiState);
    }

    private static UnifilarNodeType DetermineNodeType(
        Node node,
        ISet<string> rootSet,
        ISet<string> terminalSet,
        int outgoingEdgeCount)
    {
        if (rootSet.Contains(node.Id) || node.IsSource)
        {
            return UnifilarNodeType.Source;
        }

        if (terminalSet.Contains(node.Id) || outgoingEdgeCount == 0)
        {
            return UnifilarNodeType.Terminal;
        }

        if (outgoingEdgeCount > 1)
        {
            return UnifilarNodeType.Branch;
        }

        return UnifilarNodeType.PassThrough;
    }
}
