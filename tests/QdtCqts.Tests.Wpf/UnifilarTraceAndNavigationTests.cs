using System;
using System.Linq;
using QdtCqts.Application;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Desktop.Wpf.ViewModels.Unifilar;
using QdtCqts.Domain;
using Xunit;

namespace QdtCqts.Tests.Wpf;

/// <summary>
/// Fase 27D: Testes de unidade e integridade para Trace Elétrico Topológico,
/// Filtros Visuais de Condição Elétrica, Robustez do FitToView e Zero Fallbacks Sintéticos.
/// </summary>
public sealed class UnifilarTraceAndNavigationTests
{
    private static (ProjectVersion Project, NetworkCalculationReport Report, TopologyValidationResult Topology, UnifilarDiagramViewModel Diagram) CreateStandardDiagram(bool isProj4 = false)
    {
        var trafo = new Transformer("TR", "TR-112.5",
            new UnitValue(112.5, UnitCode.Kva), new UnitValue(3.5, UnitCode.Percent),
            new UnitValue(220, UnitCode.V), new UnitValue(0, UnitCode.Kva));

        var circuit = new Circuit("CIRC_1", "LADO 1", "TR", 1, CalculationMode.Cqts);

        var nodes = new[]
        {
            new Node("TR",  "TR",  "CIRC_1", 0, 0, true),
            new Node("LID", "LID", "CIRC_1", 1, 0, false),
            new Node("P1",  "P1",  "CIRC_1", 2, 0, false),
            new Node("P2",  "P2",  "CIRC_1", 3, 0, false),
            new Node("P3",  "P3",  "CIRC_1", 4, 0, false),
            new Node("P4",  "P4",  "CIRC_1", 5, 0, false),
            new Node("P5",  "P5",  "CIRC_1", 6, 0, false),
            new Node("RL",  "RL",  "CIRC_1", 7, 0, false)
        };

        var edges = new[]
        {
            new Edge("E1","TR-LID", "CIRC_1","TR", "LID",new UnitValue(4.0,              UnitCode.Meter),"240 Cu",        "3","2"),
            new Edge("E2","LID-P1", "CIRC_1","LID","P1", new UnitValue(isProj4?38.0:35.0,UnitCode.Meter),"185 Al - MX",   "3","1"),
            new Edge("E3","P1-P2",  "CIRC_1","P1", "P2", new UnitValue(isProj4?38.0:32.0,UnitCode.Meter),isProj4?"70 Al - MX":"185 Al - MX","3","1"),
            new Edge("E4","P2-P3",  "CIRC_1","P2", "P3", new UnitValue(isProj4?31.0:32.0,UnitCode.Meter),isProj4?"70 Al - MX":"185 Al - MX","3","1"),
            new Edge("E5","P3-P4",  "CIRC_1","P3", "P4", new UnitValue(isProj4?28.0:35.0,UnitCode.Meter),isProj4?"70 Al - MX":"185 Al - MX","3","1"),
            new Edge("E6","P4-P5",  "CIRC_1","P4", "P5", new UnitValue(isProj4?27.0:35.0,UnitCode.Meter),isProj4?"70 Al - MX":"185 Al - MX","3","1"),
            new Edge("E7","P5-RL",  "CIRC_1","P5", "RL", new UnitValue(30.0,             UnitCode.Meter),"16 Al_CONC_Tri","3","1")
        };

        var loads = new[]
        {
            new Load("L_RL","1","RL",null,LoadKind.Client,1,
                new UnitValue(isProj4?75.686588:74.448,UnitCode.Kva),1.0)
        };

        var conductors = new[]
        {
            new Conductor("240_Cu","1","240 Cu",        "240 Cu",        new UnitValue(430,UnitCode.Ampere),new UnitValue(0.0762,UnitCode.Ohm),new UnitValue(0.0897,UnitCode.Ohm)),
            new Conductor("185_Al","1","185 Al - MX",   "185 Al - MX",   new UnitValue(335,UnitCode.Ampere),new UnitValue(0.164, UnitCode.Ohm),new UnitValue(0.1178,UnitCode.Ohm)),
            new Conductor("70_Al", "1","70 Al - MX",    "70 Al - MX",    new UnitValue(195,UnitCode.Ampere),new UnitValue(0.472, UnitCode.Ohm),new UnitValue(0.126, UnitCode.Ohm)),
            new Conductor("16_Al", "1","16 Al_CONC_Tri","16 Al_CONC_Tri",new UnitValue(80, UnitCode.Ampere),new UnitValue(2.06,  UnitCode.Ohm),new UnitValue(0.85,  UnitCode.Ohm))
        };

        var parameters = new[]
        {
            new ElectricalParameter("V",              220.0, "220",    UnitCode.V,              true),
            new ElectricalParameter("StationMva",      40.0, "40",     UnitCode.Mva,            true),
            new ElectricalParameter("StationZPercent", 20.0, "20",     UnitCode.Percent,        true),
            new ElectricalParameter("MtVoltageKv",     13.2, "13.2",   UnitCode.Kv,             true),
            new ElectricalParameter("MtCableLengthKm",  2.0, "2",      UnitCode.Meter,          true),
            new ElectricalParameter("MtResistancePerKm",0.7171,"0.7171",UnitCode.OhmPerKilometer,true),
            new ElectricalParameter("MtReactancePerKm", 0.3512,"0.3512",UnitCode.OhmPerKilometer,true),
            new ElectricalParameter("CH5",            null,  "SIM",    UnitCode.Unknown,        true)
        };

        var model = new NetworkModel("NET_1","1",
            new[]{trafo}, new[]{circuit}, nodes, edges,
            Array.Empty<Branch>(), loads, conductors, parameters);

        var project = new ProjectVersion("V1", isProj4 ? "PROJ_4" : "PROJ_7", isProj4 ? "CQT PROJ 4" : "CQT PROJ 7", model, "HASH_1", true);

        var calcService = new CalculationService();
        var result = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");
        Assert.Equal(CalculationStatus.Pass, result.Status);

        var topology = new TopologyValidator().Validate(model, "CIRC_1");
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(model, topology, result.Report!);

        return (project, result.Report!, topology, diagram);
    }

    private static (ProjectVersion Project, TopologyValidationResult Topology, UnifilarDiagramViewModel Diagram) CreateForkedDiagram()
    {
        var trafo = new Transformer("TR", "TR-112.5",
            new UnitValue(112.5, UnitCode.Kva), new UnitValue(3.5, UnitCode.Percent),
            new UnitValue(220, UnitCode.V), new UnitValue(0, UnitCode.Kva));

        var circuit = new Circuit("CIRC_FORK", "BIFURCADO", "TR", 1, CalculationMode.Cqts);

        // Topologia: TR -> LID com bifurcação em LID:
        // LID -> P1 (ramo A)
        // LID -> P2 (ramo B)
        var nodes = new[]
        {
            new Node("TR",  "TR",  "CIRC_FORK", 0, 0, true),
            new Node("LID", "LID", "CIRC_FORK", 1, 0, false),
            new Node("P1",  "P1",  "CIRC_FORK", 2, -1, false),
            new Node("P2",  "P2",  "CIRC_FORK", 2, 1, false)
        };

        var edges = new[]
        {
            new Edge("E_TR_LID", "TR-LID", "CIRC_FORK", "TR",  "LID", new UnitValue(4.0, UnitCode.Meter), "240 Cu", "3", "2"),
            new Edge("E_LID_P1", "LID-P1", "CIRC_FORK", "LID", "P1",  new UnitValue(35.0, UnitCode.Meter), "185 Al - MX", "3", "1"),
            new Edge("E_LID_P2", "LID-P2", "CIRC_FORK", "LID", "P2",  new UnitValue(35.0, UnitCode.Meter), "185 Al - MX", "3", "1")
        };

        var loads = new[]
        {
            new Load("L_P1", "1", "P1", null, LoadKind.Client, 1, new UnitValue(30.0, UnitCode.Kva), 1.0),
            new Load("L_P2", "1", "P2", null, LoadKind.Client, 1, new UnitValue(30.0, UnitCode.Kva), 1.0)
        };

        var conductors = new[]
        {
            new Conductor("240_Cu", "1", "240 Cu",      "240 Cu",      new UnitValue(430, UnitCode.Ampere), new UnitValue(0.0762, UnitCode.Ohm), new UnitValue(0.0897, UnitCode.Ohm)),
            new Conductor("185_Al", "1", "185 Al - MX", "185 Al - MX", new UnitValue(335, UnitCode.Ampere), new UnitValue(0.164,  UnitCode.Ohm), new UnitValue(0.1178, UnitCode.Ohm))
        };

        var parameters = new[]
        {
            new ElectricalParameter("V",              220.0, "220",    UnitCode.V,              true),
            new ElectricalParameter("StationMva",      40.0, "40",     UnitCode.Mva,            true),
            new ElectricalParameter("StationZPercent", 20.0, "20",     UnitCode.Percent,        true),
            new ElectricalParameter("MtVoltageKv",     13.2, "13.2",   UnitCode.Kv,             true),
            new ElectricalParameter("MtCableLengthKm",  2.0, "2",      UnitCode.Meter,          true),
            new ElectricalParameter("MtResistancePerKm",0.7171,"0.7171",UnitCode.OhmPerKilometer,true),
            new ElectricalParameter("MtReactancePerKm", 0.3512,"0.3512",UnitCode.OhmPerKilometer,true),
            new ElectricalParameter("CH5",            null,  "SIM",    UnitCode.Unknown,        true)
        };

        var model = new NetworkModel("NET_FORK", "1",
            new[] { trafo }, new[] { circuit }, nodes, edges,
            Array.Empty<Branch>(), loads, conductors, parameters);

        var project = new ProjectVersion("V1", "PROJ_FORK", "FORK", model, "HASH_F", true);
        var calcResult = new CalculationService().ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");
        var topology = new TopologyValidator().Validate(model, "CIRC_FORK");
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(model, topology, calcResult.Report!);

        return (project, topology, diagram);
    }

    // ─────────────────────────────────────────────────────────────
    // 1. AUDITORIA 27D-AUDIT-01: ZERO FALLBACKS SINTÉTICOS DE DOMÍNIO
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void SelectNode_WithNonExistentId_DoesNotCreateSyntheticDomainEntity()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.SelectNode("ID_INEXISTENTE_999");

        Assert.Null(diagram.SelectedElement);
        Assert.Null(diagram.SelectedDetail);
        Assert.All(diagram.Nodes, n => Assert.False(n.IsSelected));
        Assert.All(diagram.Nodes, n => Assert.False(n.IsInTrace));
    }

    [Fact]
    public void SelectEdge_WithNonExistentId_DoesNotCreateSyntheticDomainEntity()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.SelectEdge("EDGE_INEXISTENTE_999");

        Assert.Null(diagram.SelectedElement);
        Assert.Null(diagram.SelectedDetail);
        Assert.All(diagram.Edges, e => Assert.False(e.IsSelected));
        Assert.All(diagram.Edges, e => Assert.False(e.IsInTrace));
    }

    [Fact]
    public void SelectTransformer_WithNonExistentId_DoesNotCreateSyntheticDomainEntity()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.SelectTransformer("TRAFO_INEXISTENTE_999");

        Assert.Null(diagram.SelectedElement);
        Assert.Null(diagram.SelectedDetail);
        Assert.All(diagram.Transformers, t => Assert.False(t.IsSelected));
        Assert.All(diagram.Transformers, t => Assert.False(t.IsInTrace));
    }

    // ─────────────────────────────────────────────────────────────
    // 2. AUDITORIA 27D-AUDIT-02: ROBUSTEZ DO FIT TO VIEW
    // ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1200, 800)]
    [InlineData(3840, 2160)] // Viewport grande
    [InlineData(200, 150)]   // Viewport pequeno
    public void FitToView_ProducesFiniteZoomAndCoordinates_WithoutNaNOrInfinity(double vw, double vh)
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.FitToView(vw, vh);

        Assert.False(double.IsNaN(diagram.ZoomLevel));
        Assert.False(double.IsInfinity(diagram.ZoomLevel));
        Assert.InRange(diagram.ZoomLevel, UnifilarVisualMetrics.MinZoom, UnifilarVisualMetrics.FitToViewMaxZoom);

        Assert.False(double.IsNaN(diagram.PanX));
        Assert.False(double.IsInfinity(diagram.PanX));
        Assert.False(double.IsNaN(diagram.PanY));
        Assert.False(double.IsInfinity(diagram.PanY));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(40, 40)]
    [InlineData(double.NaN, 500)]
    [InlineData(800, double.PositiveInfinity)]
    public void FitToView_WithDegenerateViewport_ResetsToDefaultsSafely(double vw, double vh)
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.FitToView(vw, vh);

        Assert.Equal(1.0, diagram.ZoomLevel);
        Assert.Equal(0.0, diagram.PanX);
        Assert.Equal(0.0, diagram.PanY);
    }

    [Fact]
    public void FitToView_WithForkedNetwork_ComputesAccurateBounds()
    {
        var (_, _, diagram) = CreateForkedDiagram();

        diagram.FitToView(1000, 600);

        Assert.InRange(diagram.ZoomLevel, UnifilarVisualMetrics.MinZoom, UnifilarVisualMetrics.FitToViewMaxZoom);
        Assert.False(double.IsNaN(diagram.PanX));
        Assert.False(double.IsNaN(diagram.PanY));
    }

    // ─────────────────────────────────────────────────────────────
    // 3. FASE 27D-A: TRACE ELÉTRICO TOPOLÓGICO DA RAIZ À PONTA
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void Trace_TerminalNodeSelected_HighlightsFullPathToRootAndTransformer()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.SelectNode("RL");

        var rl = diagram.Nodes.First(n => n.NodeId == "RL");
        Assert.True(rl.IsSelected);
        Assert.True(rl.IsInTrace);

        // Todos os nós a montante (P5, P4, P3, P2, P1, LID, TR) devem estar no trace
        var upstreamNodeIds = new[] { "P5", "P4", "P3", "P2", "P1", "LID", "TR" };
        foreach (var id in upstreamNodeIds)
        {
            var node = diagram.Nodes.First(n => n.NodeId == id);
            Assert.True(node.IsInTrace, $"Nó montante {id} deveria estar em trace");
        }

        // Todas as arestas da cadeia devem estar no trace
        Assert.All(diagram.Edges, e => Assert.True(e.IsInTrace));

        // Transformador de alimentação deve estar no trace
        Assert.All(diagram.Transformers, t => Assert.True(t.IsInTrace));
    }

    [Fact]
    public void Trace_IntermediateNodeSelected_DoesNotHighlightDownstreamElements()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        // Seleciona P3 (nó intermediário)
        diagram.SelectNode("P3");

        var p3 = diagram.Nodes.First(n => n.NodeId == "P3");
        Assert.True(p3.IsInTrace);

        // Nós a montante pertencem ao caminho
        var upstream = new[] { "P2", "P1", "LID", "TR" };
        foreach (var id in upstream)
        {
            Assert.True(diagram.Nodes.First(n => n.NodeId == id).IsInTrace);
        }

        // Elementos a jusante NÃO pertencem ao caminho selecionado
        var downstream = new[] { "P4", "P5", "RL" };
        foreach (var id in downstream)
        {
            Assert.False(diagram.Nodes.First(n => n.NodeId == id).IsInTrace, $"Nó a jusante {id} não deveria estar no trace");
        }

        // Arestas a jusante não devem estar no trace
        Assert.False(diagram.Edges.First(e => e.EdgeId == "E5").IsInTrace); // P3-P4
        Assert.False(diagram.Edges.First(e => e.EdgeId == "E6").IsInTrace); // P4-P5
        Assert.False(diagram.Edges.First(e => e.EdgeId == "E7").IsInTrace); // P5-RL
    }

    [Fact]
    public void Trace_IntermediateEdgeSelected_HighlightsUpstreamPathAndEdgeItself()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        // Seleciona trecho E4 (P2-P3)
        diagram.SelectEdge("E4");

        var e4 = diagram.Edges.First(e => e.EdgeId == "E4");
        Assert.True(e4.IsSelected);
        Assert.True(e4.IsInTrace);

        // Nós a montante (P2, P1, LID, TR) devem estar no trace
        Assert.True(diagram.Nodes.First(n => n.NodeId == "P2").IsInTrace);
        Assert.True(diagram.Nodes.First(n => n.NodeId == "P1").IsInTrace);
        Assert.True(diagram.Nodes.First(n => n.NodeId == "LID").IsInTrace);
        Assert.True(diagram.Nodes.First(n => n.NodeId == "TR").IsInTrace);

        // Arestas anteriores (E3, E2, E1) devem estar no trace
        Assert.True(diagram.Edges.First(e => e.EdgeId == "E3").IsInTrace);
        Assert.True(diagram.Edges.First(e => e.EdgeId == "E2").IsInTrace);
        Assert.True(diagram.Edges.First(e => e.EdgeId == "E1").IsInTrace);

        // Aresta seguinte (E5) não deve estar no trace
        Assert.False(diagram.Edges.First(e => e.EdgeId == "E5").IsInTrace);
    }

    [Fact]
    public void Trace_ForkIsolation_SelectingBranchA_DoesNotHighlightBranchB()
    {
        var (_, _, diagram) = CreateForkedDiagram();

        // Na rede bifurcada: TR -> LID -> P1 e LID -> P2
        // Seleciona ramo A (P1)
        diagram.SelectNode("P1");

        Assert.True(diagram.Nodes.First(n => n.NodeId == "P1").IsInTrace);
        Assert.True(diagram.Edges.First(e => e.EdgeId == "E_LID_P1").IsInTrace);
        Assert.True(diagram.Nodes.First(n => n.NodeId == "LID").IsInTrace);
        Assert.True(diagram.Edges.First(e => e.EdgeId == "E_TR_LID").IsInTrace);
        Assert.True(diagram.Nodes.First(n => n.NodeId == "TR").IsInTrace);
        Assert.True(diagram.Transformers.First(t => t.TransformerId == "TR").IsInTrace);

        // RAMO B DEVE ESTAR ESTRITAMENTE ISOLADO
        Assert.False(diagram.Nodes.First(n => n.NodeId == "P2").IsInTrace, "P2 não pode ser destacado quando P1 é selecionado!");
        Assert.False(diagram.Edges.First(e => e.EdgeId == "E_LID_P2").IsInTrace, "E_LID_P2 não pode ser destacado quando P1 é selecionado!");

        // Agora seleciona ramo B (P2)
        diagram.SelectNode("P2");

        Assert.True(diagram.Nodes.First(n => n.NodeId == "P2").IsInTrace);
        Assert.True(diagram.Edges.First(e => e.EdgeId == "E_LID_P2").IsInTrace);

        // RAMO A DEVE ESTAR ESTRITAMENTE DESMARCADO
        Assert.False(diagram.Nodes.First(n => n.NodeId == "P1").IsInTrace, "P1 deve desmarcar quando P2 é selecionado!");
        Assert.False(diagram.Edges.First(e => e.EdgeId == "E_LID_P1").IsInTrace, "E_LID_P1 deve desmarcar quando P2 é selecionado!");
    }

    [Fact]
    public void Trace_ClearSelection_ClearsAllTraceFlagsDeterministically()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.SelectNode("RL");
        Assert.Contains(diagram.Nodes, n => n.IsInTrace);

        diagram.ClearSelection();

        Assert.Null(diagram.SelectedElement);
        Assert.Null(diagram.SelectedDetail);
        Assert.All(diagram.Nodes, n => Assert.False(n.IsInTrace));
        Assert.All(diagram.Edges, e => Assert.False(e.IsInTrace));
        Assert.All(diagram.Transformers, t => Assert.False(t.IsInTrace));
    }

    [Fact]
    public void Trace_RootNodeSelected_OnlyHighlightsRootAndTransformer()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.SelectNode("TR");

        var tr = diagram.Nodes.First(n => n.NodeId == "TR");
        Assert.True(tr.IsSelected);
        Assert.True(tr.IsInTrace);
        Assert.All(diagram.Transformers, t => Assert.True(t.IsInTrace));

        // Nenhum elemento a jusante deve estar no trace
        Assert.All(diagram.Nodes.Where(n => n.NodeId != "TR"), n => Assert.False(n.IsInTrace));
        Assert.All(diagram.Edges, e => Assert.False(e.IsInTrace));
    }

    [Fact]
    public void Trace_Determinism_ProducesIdenticalTraceAcrossRepeatedInvocations()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        for (int i = 0; i < 5; i++)
        {
            diagram.SelectNode("P3");

            var inTraceNodeIds = diagram.Nodes.Where(n => n.IsInTrace).Select(n => n.NodeId).OrderBy(x => x).ToList();
            var inTraceEdgeIds = diagram.Edges.Where(e => e.IsInTrace).Select(e => e.EdgeId).OrderBy(x => x).ToList();

            Assert.Equal(new[] { "LID", "P1", "P2", "P3", "TR" }, inTraceNodeIds);
            Assert.Equal(new[] { "E1", "E2", "E3", "E4" }, inTraceEdgeIds);

            diagram.ClearSelection();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 4. FASE 27D-B: FILTROS VISUAIS DE CONDIÇÃO ELÉTRICA
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void VisualFilter_Todos_KeepsAllElementsNonDimmed()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.CurrentFilter = UnifilarVisualFilter.Todos;

        Assert.All(diagram.Nodes, n => Assert.False(n.IsDimmed));
        Assert.All(diagram.Edges, e => Assert.False(e.IsDimmed));
        Assert.All(diagram.Transformers, t => Assert.False(t.IsDimmed));
    }

    [Fact]
    public void VisualFilter_Sobrecarga_DimsOnlyNonOverloadedElements()
    {
        var (_, _, _, diagram) = CreateStandardDiagram(isProj4: true); // PROJ 4 possui trechos com e sem sobrecarga

        diagram.CurrentFilter = UnifilarVisualFilter.Sobrecarga;

        foreach (var edge in diagram.Edges)
        {
            if (edge.IsOverloaded)
            {
                Assert.False(edge.IsDimmed, $"Aresta sobrecarregada {edge.EdgeId} não deve ser atenuada");
            }
            else
            {
                Assert.True(edge.IsDimmed, $"Aresta não-sobrecarregada {edge.EdgeId} deve ser atenuada");
            }
        }
    }

    [Fact]
    public void VisualFilter_QuedaTensaoLimite_FiltersAgainstConfigurableThreshold()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        // Define threshold para 4.0%
        diagram.VoltageDropThresholdPercent = 4.0;
        diagram.CurrentFilter = UnifilarVisualFilter.QuedaTensaoLimite;

        foreach (var node in diagram.Nodes)
        {
            if (node.AccumulatedVoltageDropPercent > 4.0)
            {
                Assert.False(node.IsDimmed, $"Nó {node.NodeId} com CA={node.AccumulatedVoltageDropPercent}% deve estar em destaque");
            }
            else
            {
                Assert.True(node.IsDimmed, $"Nó {node.NodeId} com CA={node.AccumulatedVoltageDropPercent}% deve estar atenuado");
            }
        }
    }

    [Fact]
    public void VisualFilter_Selecionados_DimsAllNonSelectedElements()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.SelectNode("P3");
        diagram.CurrentFilter = UnifilarVisualFilter.Selecionados;

        var p3 = diagram.Nodes.First(n => n.NodeId == "P3");
        Assert.False(p3.IsDimmed);

        var others = diagram.Nodes.Where(n => n.NodeId != "P3");
        Assert.All(others, n => Assert.True(n.IsDimmed));
        Assert.All(diagram.Edges, e => Assert.True(e.IsDimmed));
        Assert.All(diagram.Transformers, t => Assert.True(t.IsDimmed));
    }

    [Fact]
    public void VisualFilter_Trace_DimsAllElementsNotInTrace()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.SelectNode("P2");
        diagram.CurrentFilter = UnifilarVisualFilter.Trace;

        // Elementos no caminho de P2 à raiz não devem ser atenuados
        var inTraceNodes = diagram.Nodes.Where(n => n.IsInTrace);
        Assert.All(inTraceNodes, n => Assert.False(n.IsDimmed));

        // Elementos fora do caminho devem ser atenuados
        var outTraceNodes = diagram.Nodes.Where(n => !n.IsInTrace);
        Assert.All(outTraceNodes, n => Assert.True(n.IsDimmed));
    }

    [Fact]
    public void VisualFilter_EvidenceBlocked_PreservesEvidenceSemanticsDeterministically()
    {
        var (_, report, topology, diagram) = CreateStandardDiagram();

        // No StandardDiagram (CQT PROJ 7 real), Protection status é EvidenceBlocked.
        // Portanto, ao ativar o filtro EvidenceBlocked, os elementos do circuito ficam em evidência (IsDimmed == false)
        diagram.CurrentFilter = UnifilarVisualFilter.EvidenceBlocked;

        Assert.All(diagram.Nodes, n => Assert.False(n.IsDimmed));
        Assert.All(diagram.Edges, e => Assert.False(e.IsDimmed));
        Assert.All(diagram.Transformers, t => Assert.False(t.IsDimmed));

        // Cria um relatório com ProtectionAssessmentStatus.Pass (sem bloqueio de evidência)
        var passProtection = new ProtectionCalculationResult(
            ProjectCurrentAmperes: 100,
            MinSinglePhaseShortCircuitAmperes: 500,
            MaxThreePhaseShortCircuitAmperes: 1200,
            CriticalConductorKey: "185 Al - MX",
            CriticalConductorSectionMm2: 185,
            ConductorSectionEvidenceId: "EV_1",
            CriticalOperatingTemperatureCelsius: 70,
            ConductorTemperatureEvidenceId: "EV_2",
            MaxAdmissibleTimeSeconds: 5.0,
            EvidenceStatus: ProtectionEvidenceStatus.Available,
            AssessmentStatus: ProtectionAssessmentStatus.Pass,
            DeviceEvidence: null,
            IsRatedCurrentAdequate: true,
            IsThermalWithstandAdequate: true,
            IsInterruptingCapacityAdequate: true,
            StatusMessage: "Adequado");

        var passReport = report with { Protection = passProtection };
        var passDiagram = new UnifilarPresentationBuilder().BuildDiagram(CreateStandardDiagram().Project.NetworkModel, topology, passReport);

        passDiagram.CurrentFilter = UnifilarVisualFilter.EvidenceBlocked;

        // Quando o circuito NÃO tem bloqueio de evidência, os elementos devem ser atenuados sob esse filtro
        Assert.All(passDiagram.Nodes, n => Assert.True(n.IsDimmed));
        Assert.All(passDiagram.Edges, e => Assert.True(e.IsDimmed));
        Assert.All(passDiagram.Transformers, t => Assert.True(t.IsDimmed));
    }

    [Fact]
    public void VisualFilter_SwitchBackToTodos_RestoresFullVisibilityForEveryElement()
    {
        var (_, _, _, diagram) = CreateStandardDiagram();

        diagram.SelectNode("P3");
        diagram.CurrentFilter = UnifilarVisualFilter.Selecionados;
        Assert.Contains(diagram.Nodes, n => n.IsDimmed);

        // Retorna para Todos
        diagram.CurrentFilter = UnifilarVisualFilter.Todos;

        Assert.All(diagram.Nodes, n => Assert.False(n.IsDimmed));
        Assert.All(diagram.Edges, e => Assert.False(e.IsDimmed));
        Assert.All(diagram.Transformers, t => Assert.False(t.IsDimmed));
    }
}
