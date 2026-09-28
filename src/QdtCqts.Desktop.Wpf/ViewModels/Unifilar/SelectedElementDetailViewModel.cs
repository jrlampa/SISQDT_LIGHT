using System.Collections.Generic;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

public sealed record DetailItem(string Label, string Value, string? Unit = null);

public sealed record DetailCategoryGroup(string CategoryName, IReadOnlyList<DetailItem> Items);

/// <summary>
/// Modelo de apresentação para o Painel de Detalhes do elemento selecionado (Fase 27B).
/// Reúne dados físicos e elétricos calculados sem recalcular grandezas.
/// </summary>
public sealed class SelectedElementDetailViewModel
{
    public string ElementId { get; init; } = string.Empty;
    public UnifilarElementKind ElementKind { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public IReadOnlyList<DetailCategoryGroup> Categories { get; init; } = new List<DetailCategoryGroup>();

    public static SelectedElementDetailViewModel FromNode(
        UnifilarNodeViewModel nodeVm,
        Node domainNode,
        NetworkCalculationReport report)
    {
        var generalItems = new List<DetailItem>
        {
            new("ID do Nó", nodeVm.NodeId),
            new("Chave Externa", nodeVm.ExternalKey),
            new("Circuito", nodeVm.CircuitId),
            new("Tipo Topológico", nodeVm.NodeType.ToString()),
            new("É Fonte?", domainNode.IsSource ? "Sim" : "Não")
        };

        if (domainNode.ParentNodeId != null)
        {
            generalItems.Add(new("Nó Pai", domainNode.ParentNodeId));
        }

        var electricalItems = new List<DetailItem>
        {
            new("Queda Acumulada (CA%)", $"{nodeVm.AccumulatedVoltageDropPercent:F3}", "%"),
            new("Tensão 127 V", $"{nodeVm.VoltageV127:F2}", "V"),
            new("Tensão 220 V", $"{nodeVm.VoltageV220:F2}", "V"),
            new("Carga Local", $"{nodeVm.LocalLoadKva:F2}", "kVA"),
            new("Consumidores Locais", $"{nodeVm.LocalConsumers}", "un")
        };

        var traceItems = new List<DetailItem>
        {
            new("RunId", report.RunId),
            new("Versão do Algoritmo", report.AlgorithmVersion),
            new("InputHash", report.InputHash),
            new("OutputHash", report.OutputHash)
        };

        return new SelectedElementDetailViewModel
        {
            ElementId = nodeVm.NodeId,
            ElementKind = UnifilarElementKind.Node,
            Title = $"Nó: {nodeVm.ExternalKey}",
            Subtitle = $"Tipo: {nodeVm.NodeType} · Queda: {nodeVm.AccumulatedVoltageDropPercent:F2}%",
            Categories = new[]
            {
                new DetailCategoryGroup("Identificação & Topologia", generalItems),
                new DetailCategoryGroup("Grandezas Elétricas Calculadas", electricalItems),
                new DetailCategoryGroup("Rastreabilidade & Auditoria", traceItems)
            }
        };
    }

    public static SelectedElementDetailViewModel FromEdge(
        UnifilarEdgeViewModel edgeVm,
        Edge domainEdge,
        NetworkCalculationReport report)
    {
        var generalItems = new List<DetailItem>
        {
            new("ID do Trecho", edgeVm.EdgeId),
            new("Origem → Destino", $"{edgeVm.FromNodeId} → {edgeVm.ToNodeId}"),
            new("Condutor", edgeVm.ConductorKey ?? "N/D"),
            new("Comprimento Físico", $"{edgeVm.PhysicalLengthMeters:F1}", "m"),
            new("Comprimento Equivalente", $"{edgeVm.EquivalentLengthMeters:F1}", "m"),
            new("Cabos em Paralelo (AP)", $"{edgeVm.ParallelCables:F0}", "un"),
            new("Número de Fases", $"{edgeVm.PhaseCount}", "fases")
        };

        var loadItems = new List<DetailItem>
        {
            new("Carga Acumulada a Jusante (E)", $"{edgeVm.DownstreamAccumulatedLoadKva:F2}", "kVA"),
            new("Consumidores a Jusante (D)", $"{edgeVm.DownstreamAccumulatedConsumers:F0}", "un"),
            new("Fator de Diversidade (G)", $"{edgeVm.DiversityFactor:F2}", "fator"),
            new("Carga Fim de Trecho (M)", $"{edgeVm.EndLoadSelectedKva:F2}", "kVA"),
            new("Corrente de Operação (Ib)", $"{edgeVm.OperatingCurrentAmperes:F1}", "A"),
            new("Capacidade do Cabo (Iz)", $"{edgeVm.RatedAmpacityAmperes:F0}", "A"),
            new("Sobrecarga?", edgeVm.OverloadStatus)
        };

        var thermalVoltageItems = new List<DetailItem>
        {
            new("Temperatura em Regime (T)", $"{edgeVm.OperatingTemperatureCelsius:F1}", "°C"),
            new("Resistência Térmica CA (Rca)", $"{edgeVm.ResistanceCaOhmPerKm:F4}", "Ω/km"),
            new("Reatância (X)", $"{edgeVm.ReactanceOhmPerKm:F4}", "Ω/km"),
            new("Impedância Linear (Z)", $"{edgeVm.ImpedanceOhmPerKm:F4}", "Ω/km"),
            new("Fator BJ", $"{edgeVm.FactorBj:G5}"),
            new("Queda de Tensão no Trecho (ΔV%)", $"{edgeVm.SegmentVoltageDropPercent:F3}", "%")
        };

        var shortCircuitItems = new List<DetailItem>
        {
            new("Icc Trifásico (3φ)", $"{edgeVm.ShortCircuit3PhaseAmperes:F1}", "A"),
            new("Icc Monofásico (1φ)", $"{edgeVm.ShortCircuit1PhaseAmperes:F1}", "A"),
            new("R a Montante (Zup R)", $"{edgeVm.UpstreamResistanceOhm:G5}", "Ω"),
            new("X a Montante (Zup X)", $"{edgeVm.UpstreamReactanceOhm:G5}", "Ω")
        };

        var traceItems = new List<DetailItem>
        {
            new("RunId", report.RunId),
            new("InputHash", report.InputHash),
            new("OutputHash", report.OutputHash)
        };

        return new SelectedElementDetailViewModel
        {
            ElementId = edgeVm.EdgeId,
            ElementKind = UnifilarElementKind.Edge,
            Title = $"Trecho: {edgeVm.FromNodeId} → {edgeVm.ToNodeId}",
            Subtitle = $"Ib: {edgeVm.OperatingCurrentAmperes:F1} A · ΔV: {edgeVm.SegmentVoltageDropPercent:F3}% · Icc3φ: {edgeVm.ShortCircuit3PhaseAmperes:F0} A",
            Categories = new[]
            {
                new DetailCategoryGroup("Identificação & Geometria", generalItems),
                new DetailCategoryGroup("Carga e Carregamento (Ib / Iz)", loadItems),
                new DetailCategoryGroup("Térmico & Queda de Tensão", thermalVoltageItems),
                new DetailCategoryGroup("Curto-Circuito na Ponta do Trecho", shortCircuitItems),
                new DetailCategoryGroup("Rastreabilidade", traceItems)
            }
        };
    }

    public static SelectedElementDetailViewModel FromTransformer(
        UnifilarTransformerViewModel trafoVm,
        Transformer domainTrafo,
        NetworkCalculationReport report)
    {
        var items = new List<DetailItem>
        {
            new("ID do Transformador", trafoVm.TransformerId),
            new("Chave Externa", trafoVm.ExternalKey),
            new("Potência Nominal", $"{trafoVm.NominalPowerKva:F1}", "kVA"),
            new("Carga de Operação", $"{trafoVm.OperatingLoadKva:F2}", "kVA"),
            new("Impedância Percentual", $"{trafoVm.ImpedancePercent:F2}", "%"),
            new("Queda Interna do Trafo", $"{trafoVm.TrafoVoltageDropPercent:F3}", "%"),
            new("Queda na Média Tensão", $"{trafoVm.MtVoltageDropPercent:F3}", "%"),
            new("Queda Total na Origem", $"{trafoVm.TotalOriginVoltageDropPercent:F3}", "%")
        };

        var traceItems = new List<DetailItem>
        {
            new("RunId", report.RunId),
            new("InputHash", report.InputHash),
            new("OutputHash", report.OutputHash)
        };

        return new SelectedElementDetailViewModel
        {
            ElementId = trafoVm.TransformerId,
            ElementKind = UnifilarElementKind.Transformer,
            Title = $"Trafo: {trafoVm.ExternalKey}",
            Subtitle = $"Potência: {trafoVm.NominalPowerKva:F0} kVA · Carga: {trafoVm.OperatingLoadKva:F1} kVA",
            Categories = new[]
            {
                new DetailCategoryGroup("Parâmetros do Transformador", items),
                new DetailCategoryGroup("Rastreabilidade", traceItems)
            }
        };
    }
}
