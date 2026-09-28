using System;
using System.Collections.Generic;
using System.Linq;
using QdtCqts.Domain;

namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Motor de layout determinístico em árvore hierárquica ortogonal para unifilar (Fase 27B).
/// Distribui a rede radial horizontalmente (da fonte para as pontas) com
/// espalhamento vertical das ramificações, sem cruzamento de linhas.
/// </summary>
public sealed class HierarchicalTreeLayoutEngine : IUnifilarLayoutEngine
{
    public IReadOnlyDictionary<string, (double X, double Y)> ComputeLayout(
        NetworkModel model,
        TopologyValidationResult topology,
        double startX = 80,
        double startY = 80,
        double horizontalSpacing = 160,
        double verticalSpacing = 90)
    {
        var positions = new Dictionary<string, (double X, double Y)>(StringComparer.Ordinal);
        if (model.Nodes.Count == 0) return positions;

        // Se houver coordenadas 2D ricas e não colineares (variando em X e Y),
        // preservamos as proporções relativas escalonadas.
        if (HasRichPlanarCoordinates(model.Nodes))
        {
            return ProjectPlanarCoordinates(model.Nodes, startX, startY, horizontalSpacing, verticalSpacing);
        }

        // Caso contrário, gera layout esquemático radial ortogonal canônico
        var outgoingEdges = model.Edges.ToLookup(e => e.FromNodeId, StringComparer.Ordinal);
        var roots = topology.RootNodeIds.Count > 0
            ? topology.RootNodeIds
            : model.Nodes.Where(n => n.IsSource).Select(n => n.Id).ToArray();

        if (roots.Count == 0 && model.Nodes.Count > 0)
        {
            roots = new[] { model.Nodes[0].Id };
        }

        int currentLeafSlot = 0;

        foreach (var rootId in roots)
        {
            LayoutSubtree(rootId, 0, outgoingEdges, positions, ref currentLeafSlot, startX, startY, horizontalSpacing, verticalSpacing);
        }

        // Garante que qualquer nó não alcançado (caso haja) receba posição
        foreach (var node in model.Nodes)
        {
            if (!positions.ContainsKey(node.Id))
            {
                positions[node.Id] = (startX, startY + (currentLeafSlot++ * verticalSpacing));
            }
        }

        return positions;
    }

    private static double LayoutSubtree(
        string nodeId,
        int depth,
        ILookup<string, Edge> outgoingEdges,
        IDictionary<string, (double X, double Y)> positions,
        ref int currentLeafSlot,
        double startX,
        double startY,
        double horizontalSpacing,
        double verticalSpacing)
    {
        var children = outgoingEdges[nodeId].Select(e => e.ToNodeId).OrderBy(id => id, StringComparer.Ordinal).ToList();
        double myX = startX + (depth * horizontalSpacing);

        if (children.Count == 0)
        {
            // Nó folha ocupa um novo slot vertical
            double myY = startY + (currentLeafSlot * verticalSpacing);
            currentLeafSlot++;
            positions[nodeId] = (myX, myY);
            return myY;
        }

        // Nó com filhos: calcula recursivamente a posição dos filhos
        var childYPositions = new List<double>();
        foreach (var childId in children)
        {
            double childY = LayoutSubtree(childId, depth + 1, outgoingEdges, positions, ref currentLeafSlot, startX, startY, horizontalSpacing, verticalSpacing);
            childYPositions.Add(childY);
        }

        // Nó pai fica centralizado verticalmente entre seus filhos
        double centerChildY = childYPositions.Average();
        positions[nodeId] = (myX, centerChildY);
        return centerChildY;
    }

    private static bool HasRichPlanarCoordinates(IReadOnlyList<Node> nodes)
    {
        var valid = nodes.Where(n => n.X.HasValue && n.Y.HasValue).ToList();
        if (valid.Count < 3) return false;

        double minX = valid.Min(n => n.X!.Value);
        double maxX = valid.Max(n => n.X!.Value);
        double minY = valid.Min(n => n.Y!.Value);
        double maxY = valid.Max(n => n.Y!.Value);

        // Se Y for constante (como 0, 0 em fixtures) ou delta muito pequeno, não é uma planta 2D
        return (maxX - minX) > 10.0 && (maxY - minY) > 10.0;
    }

    private static IReadOnlyDictionary<string, (double X, double Y)> ProjectPlanarCoordinates(
        IReadOnlyList<Node> nodes,
        double startX,
        double startY,
        double horizontalSpacing,
        double verticalSpacing)
    {
        var result = new Dictionary<string, (double X, double Y)>(StringComparer.Ordinal);
        var valid = nodes.Where(n => n.X.HasValue && n.Y.HasValue).ToList();

        double minX = valid.Min(n => n.X!.Value);
        double maxX = valid.Max(n => n.X!.Value);
        double minY = valid.Min(n => n.Y!.Value);
        double maxY = valid.Max(n => n.Y!.Value);

        double spanX = Math.Max(1.0, maxX - minX);
        double spanY = Math.Max(1.0, maxY - minY);

        double targetWidth = Math.Max(400.0, nodes.Count * horizontalSpacing * 0.7);
        double targetHeight = Math.Max(300.0, nodes.Count * verticalSpacing * 0.7);

        foreach (var node in nodes)
        {
            if (node.X.HasValue && node.Y.HasValue)
            {
                double normX = (node.X.Value - minX) / spanX;
                double normY = (node.Y.Value - minY) / spanY;
                result[node.Id] = (startX + (normX * targetWidth), startY + (normY * targetHeight));
            }
            else
            {
                result[node.Id] = (startX, startY);
            }
        }

        return result;
    }
}
