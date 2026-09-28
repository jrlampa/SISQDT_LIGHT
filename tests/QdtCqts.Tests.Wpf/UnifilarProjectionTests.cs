using System;
using System.Linq;
using System.Threading.Tasks;
using QdtCqts.Application;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Desktop.Wpf.ViewModels;
using QdtCqts.Desktop.Wpf.ViewModels.Unifilar;
using QdtCqts.Domain;
using Xunit;

namespace QdtCqts.Tests.Wpf;

/// <summary>
/// Fase 27B: Testes de unidade e integridade dos contratos de apresentação do unifilar.
/// Valida projeção de nós, trechos, transformadores, integridade topológica 1:1,
/// ausência de recálculo e consumo determinístico dos resultados do cálculo.
/// </summary>
public sealed class UnifilarProjectionTests
{
    [Fact]
    public void Layout_UsesLayoutCoordinatesAndNotPhysicalUtmPosition()
    {
        var spatialReference = new SpatialReference(SpatialSystemType.Utm, 23, null, null, null);
        var nodes = new[]
        {
            new Node("TR", "TR", "C1", null, null, true, PhysicalPosition: new PhysicalPosition(500000.0, 7400000.0, UnitCode.Meter, spatialReference, "DWG_JSON", "CAD-P-001", "POSTE PROJ")),
            new Node("P1", "P1", "C1", null, null, false, "TR", new PhysicalPosition(500050.0, 7400030.0, UnitCode.Meter, spatialReference, "DWG_JSON", "CAD-P-002", "POSTE PROJ"))
        };
        var edge = new Edge("E1", "TR-P1", "C1", "TR", "P1", new UnitValue(50, UnitCode.Meter), "240 Cu", "3", "1");
        var model = new NetworkModel("M1", "V1", Array.Empty<Transformer>(), new[] { new Circuit("C1", "C1", "TR", 1, CalculationMode.Cqts) },
            nodes, new[] { edge }, Array.Empty<Branch>(), Array.Empty<Load>(), Array.Empty<Conductor>(), Array.Empty<ElectricalParameter>());
        var topology = new TopologyValidator().Validate(model, "C1");

        var layout = new HierarchicalTreeLayoutEngine().ComputeLayout(model, topology);

        Assert.Equal((80.0, 80.0), layout["TR"]);
        Assert.Equal((240.0, 80.0), layout["P1"]);
        Assert.Equal(500000.0, model.Nodes[0].PhysicalPosition!.EastingX);
        Assert.Equal(7400000.0, model.Nodes[0].PhysicalPosition!.NorthingY);
    }

    private static ProjectVersion BuildTestProject(bool isProj4 = false)
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

        return new ProjectVersion("V1", isProj4 ? "PROJ_4" : "PROJ_7", isProj4 ? "CQT PROJ 4" : "CQT PROJ 7", model, "HASH_1", true);
    }

    [Fact]
    public void NodeProjection_PreservesIdentityAndCalculatedValues()
    {
        var project = BuildTestProject();
        var calcService = new CalculationService();
        var result = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");
        Assert.Equal(CalculationStatus.Pass, result.Status);

        var topologyValidator = new TopologyValidator();
        var topoResult = topologyValidator.Validate(project.NetworkModel, "CIRC_1");
        var builder = new UnifilarPresentationBuilder();
        var diagram = builder.BuildDiagram(project.NetworkModel, topoResult, result.Report!);

        Assert.Equal(project.NetworkModel.Nodes.Count, diagram.Nodes.Count);

        var rootNodeVm = diagram.Nodes.First(n => n.NodeId == "TR");
        Assert.Equal("TR", rootNodeVm.NodeId);
        Assert.Equal("TR", rootNodeVm.ExternalKey);
        Assert.Equal(UnifilarNodeType.Source, rootNodeVm.NodeType);

        var terminalNodeVm = diagram.Nodes.First(n => n.NodeId == "RL");
        Assert.Equal("RL", terminalNodeVm.NodeId);
        Assert.Equal(UnifilarNodeType.Terminal, terminalNodeVm.NodeType);
        Assert.True(terminalNodeVm.AccumulatedVoltageDropPercent > 0);
    }

    [Fact]
    public void EdgeProjection_PreservesIdentityAndConsumesIsOverloadedDirectly()
    {
        var project = BuildTestProject();
        var calcService = new CalculationService();
        var result = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");
        Assert.Equal(CalculationStatus.Pass, result.Status);

        var topoResult = new TopologyValidator().Validate(project.NetworkModel, "CIRC_1");
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(project.NetworkModel, topoResult, result.Report!);

        Assert.Equal(project.NetworkModel.Edges.Count, diagram.Edges.Count);

        foreach (var edgeVm in diagram.Edges)
        {
            var domainEdge = project.NetworkModel.Edges.First(e => e.Id == edgeVm.EdgeId);
            var reportSeg = result.Report!.Segments.First(s => s.EdgeId == edgeVm.EdgeId);

            Assert.Equal(domainEdge.FromNodeId, edgeVm.FromNodeId);
            Assert.Equal(domainEdge.ToNodeId, edgeVm.ToNodeId);
            Assert.Equal(reportSeg.OperatingCurrentAmperes, edgeVm.OperatingCurrentAmperes);
            Assert.Equal(reportSeg.RatedAmpacityAmperes, edgeVm.RatedAmpacityAmperes);
            Assert.Equal(reportSeg.IsOverloaded, edgeVm.IsOverloaded);
            Assert.Equal(reportSeg.IsOverloaded ? "SOBRECARGA" : "OK", edgeVm.OverloadStatus);
            Assert.Equal(reportSeg.SegmentVoltageDropPercent, edgeVm.SegmentVoltageDropPercent);
            Assert.Equal(reportSeg.ShortCircuit3PhaseAmperes, edgeVm.ShortCircuit3PhaseAmperes);
            Assert.Equal(reportSeg.ShortCircuit1PhaseAmperes, edgeVm.ShortCircuit1PhaseAmperes);
        }
    }

    [Fact]
    public void TransformerProjection_PreservesParametersAndReportResults()
    {
        var project = BuildTestProject();
        var calcService = new CalculationService();
        var result = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");

        var topoResult = new TopologyValidator().Validate(project.NetworkModel, "CIRC_1");
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(project.NetworkModel, topoResult, result.Report!);

        Assert.Single(diagram.Transformers);
        var trafoVm = diagram.Transformers.First();
        var reportTrafo = result.Report!.Transformers.First();

        Assert.Equal("TR", trafoVm.TransformerId);
        Assert.Equal(reportTrafo.NominalPowerKva, trafoVm.NominalPowerKva);
        Assert.Equal(reportTrafo.OperatingLoadKva, trafoVm.OperatingLoadKva);
        Assert.Equal(reportTrafo.TrafoVoltageDropPercent, trafoVm.TrafoVoltageDropPercent);
        Assert.Equal(reportTrafo.TotalOriginVoltageDropPercent, trafoVm.TotalOriginVoltageDropPercent);
    }

    [Fact]
    public void GraphIntegrity_TopologyValidatorMatchesUnifilarPresentationModel()
    {
        var project = BuildTestProject();
        var calcService = new CalculationService();
        var result = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");

        var validator = new TopologyValidator();
        var topoResult = validator.Validate(project.NetworkModel, "CIRC_1");
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(project.NetworkModel, topoResult, result.Report!);

        // 1. Mesma quantidade de nós e arestas
        Assert.Equal(project.NetworkModel.Nodes.Count, diagram.Nodes.Count);
        Assert.Equal(project.NetworkModel.Edges.Count, diagram.Edges.Count);

        // 2. Mesmos IDs de nós
        var modelNodeIds = project.NetworkModel.Nodes.Select(n => n.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray();
        var diagramNodeIds = diagram.Nodes.Select(n => n.NodeId).OrderBy(id => id, StringComparer.Ordinal).ToArray();
        Assert.Equal(modelNodeIds, diagramNodeIds);

        // 3. Mesmos IDs de arestas
        var modelEdgeIds = project.NetworkModel.Edges.Select(e => e.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray();
        var diagramEdgeIds = diagram.Edges.Select(e => e.EdgeId).OrderBy(id => id, StringComparer.Ordinal).ToArray();
        Assert.Equal(modelEdgeIds, diagramEdgeIds);

        // 4. Mesmas relações FromNode -> ToNode
        foreach (var edgeVm in diagram.Edges)
        {
            var modelEdge = project.NetworkModel.Edges.First(e => e.Id == edgeVm.EdgeId);
            Assert.Equal(modelEdge.FromNodeId, edgeVm.FromNodeId);
            Assert.Equal(modelEdge.ToNodeId, edgeVm.ToNodeId);
        }

        // 5. Ordem topológica preservada
        Assert.Equal(topoResult.OrderedNodeIds.Count, diagram.Nodes.Count);
        Assert.Equal(topoResult.OrderedEdgeIds.Count, diagram.Edges.Count);
    }

    [Fact]
    public void LayoutEngine_IsPurelyGeometricAndDeterministic()
    {
        var project = BuildTestProject();
        var topoResult = new TopologyValidator().Validate(project.NetworkModel, "CIRC_1");

        var engine = new HierarchicalTreeLayoutEngine();
        var layout1 = engine.ComputeLayout(project.NetworkModel, topoResult);
        var layout2 = engine.ComputeLayout(project.NetworkModel, topoResult);

        // Determinismo estrito
        Assert.Equal(layout1.Count, layout2.Count);
        foreach (var (nodeId, pos1) in layout1)
        {
            var pos2 = layout2[nodeId];
            Assert.Equal(pos1.X, pos2.X);
            Assert.Equal(pos1.Y, pos2.Y);
        }

        // Raiz (TR) fica mais à esquerda que a ponta (RL)
        var trPos = layout1["TR"];
        var rlPos = layout1["RL"];
        Assert.True(trPos.X < rlPos.X, "Raiz TR deve ter coordenada X menor que ponta RL");
    }

    [Fact]
    public void Selection_UpdatesSelectedElementAndBuildsStructuredDetails()
    {
        var project = BuildTestProject();
        var calcService = new CalculationService();
        var result = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");
        var topoResult = new TopologyValidator().Validate(project.NetworkModel, "CIRC_1");
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(project.NetworkModel, topoResult, result.Report!);

        Assert.Null(diagram.SelectedElement);
        Assert.Null(diagram.SelectedDetail);

        // Seleção de Trecho
        diagram.SelectEdge("E2");
        Assert.NotNull(diagram.SelectedElement);
        Assert.NotNull(diagram.SelectedDetail);
        Assert.Equal("E2", diagram.SelectedDetail!.ElementId);
        Assert.Equal(UnifilarElementKind.Edge, diagram.SelectedDetail.ElementKind);
        Assert.Contains(diagram.SelectedDetail.Categories, c => c.CategoryName.Contains("Curto-Circuito", StringComparison.OrdinalIgnoreCase));

        // Seleção de Nó
        diagram.SelectNode("P3");
        Assert.NotNull(diagram.SelectedElement);
        Assert.NotNull(diagram.SelectedDetail);
        Assert.Equal("P3", diagram.SelectedDetail!.ElementId);
        Assert.Equal(UnifilarElementKind.Node, diagram.SelectedDetail.ElementKind);

        // Limpeza de seleção
        diagram.ClearSelection();
        Assert.Null(diagram.SelectedElement);
        Assert.Null(diagram.SelectedDetail);
        Assert.All(diagram.Nodes, n => Assert.False(n.IsSelected));
        Assert.All(diagram.Edges, e => Assert.False(e.IsSelected));
    }

    [Fact]
    public async Task MainViewModel_IntegratesUnifilarDiagramOnExecution()
    {
        var vm = new MainViewModel();
        Assert.Null(vm.UnifilarDiagram);

        await vm.ExecuteCalculationAsync();

        Assert.NotNull(vm.UnifilarDiagram);
        Assert.Equal(7, vm.UnifilarDiagram!.Edges.Count);
        Assert.Equal(8, vm.UnifilarDiagram.Nodes.Count);
        Assert.Single(vm.UnifilarDiagram.Transformers);
        Assert.Equal(CalculationUiState.EvidenceBlocked, vm.UnifilarDiagram.UiState);

        // Troca de projeto reseta o unifilar
        vm.SelectedProject = "CQT PROJ 4 - AV PADRE DECAMINADA (Rev 1)";
        Assert.Null(vm.UnifilarDiagram);

        // Recálculo do PROJ 4
        await vm.ExecuteCalculationAsync();
        Assert.NotNull(vm.UnifilarDiagram);
        Assert.Equal(7, vm.UnifilarDiagram!.Edges.Count);
    }

    [Fact]
    public void ZoomAndPan_Operations_AreClampedAndFunctional()
    {
        var project = BuildTestProject();
        var calcService = new CalculationService();
        var result = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");
        var topoResult = new TopologyValidator().Validate(project.NetworkModel, "CIRC_1");
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(project.NetworkModel, topoResult, result.Report!);

        Assert.Equal(1.0, diagram.ZoomLevel);
        Assert.Equal(0.0, diagram.PanX);
        Assert.Equal(0.0, diagram.PanY);

        // Zoom In
        diagram.ZoomIn();
        Assert.True(diagram.ZoomLevel > 1.0);

        // Zoom Out
        diagram.ZoomOut();
        diagram.ZoomOut();
        Assert.True(diagram.ZoomLevel < 1.0);

        // Pan
        diagram.PanX = 120.5;
        diagram.PanY = -45.0;
        Assert.Equal(120.5, diagram.PanX);
        Assert.Equal(-45.0, diagram.PanY);

        // Reset
        diagram.ResetZoom();
        Assert.Equal(1.0, diagram.ZoomLevel);
        Assert.Equal(0.0, diagram.PanX);
        Assert.Equal(0.0, diagram.PanY);

        // Clamp verifications
        diagram.ZoomLevel = 10.0;
        Assert.Equal(4.0, diagram.ZoomLevel);

        diagram.ZoomLevel = 0.05;
        Assert.Equal(0.25, diagram.ZoomLevel);
    }

    [Fact]
    public void FitToView_ComputesValidScaleAndCentering()
    {
        var project = BuildTestProject();
        var calcService = new CalculationService();
        var result = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");
        var topoResult = new TopologyValidator().Validate(project.NetworkModel, "CIRC_1");
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(project.NetworkModel, topoResult, result.Report!);

        diagram.FitToView(1000, 600);

        Assert.True(diagram.ZoomLevel >= 0.25 && diagram.ZoomLevel <= 2.5);
        // Pan deve ser calculado sem NaN ou Infinity
        Assert.False(double.IsNaN(diagram.PanX));
        Assert.False(double.IsNaN(diagram.PanY));
        Assert.False(double.IsInfinity(diagram.PanX));
        Assert.False(double.IsInfinity(diagram.PanY));
    }

    [Fact]
    public async Task Selection_SynchronizesBetweenSegmentAndUnifilarDiagram()
    {
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();

        Assert.NotNull(vm.UnifilarDiagram);
        Assert.Null(vm.SelectedSegment);
        Assert.Null(vm.UnifilarDiagram!.SelectedElement);

        // Seleciona um segmento na VM
        var targetSegment = vm.Segments.First(s => s.SegmentId == "E2");
        vm.SelectedSegment = targetSegment;

        // O diagrama deve ter E2 selecionado e detalhe montado
        Assert.NotNull(vm.UnifilarDiagram.SelectedElement);
        var edgeVm = vm.UnifilarDiagram.SelectedElement as UnifilarEdgeViewModel;
        Assert.NotNull(edgeVm);
        Assert.Equal("E2", edgeVm!.EdgeId);
        Assert.True(edgeVm.IsSelected);
        Assert.Equal("E2", vm.UnifilarDiagram.SelectedDetail?.ElementId);

        // Limpeza de seleção
        vm.UnifilarDiagram.ClearSelection();
        Assert.Null(vm.UnifilarDiagram.SelectedElement);
        Assert.Null(vm.UnifilarDiagram.SelectedDetail);
    }

    [Fact]
    public void BranchingTopology_Bifurcation_ProducesCorrectNodesAndTreeLayout()
    {
        // Teste de rede com bifurcação 1 -> N:
        // Fonte (TR) -> Trecho (E1) -> Bifurcação (LID)
        //                                ├-> Trecho (E2) -> Terminal A (P1)
        //                                └-> Trecho (E3) -> Terminal B (P2)
        var trafo = new Transformer("TR", "TR-112.5", new UnitValue(112.5, UnitCode.Kva), new UnitValue(3.5, UnitCode.Percent), new UnitValue(220, UnitCode.V), new UnitValue(0, UnitCode.Kva));
        var circuit = new Circuit("CIRC_1", "LADO 1", "TR", 1, CalculationMode.Cqts);

        var nodes = new[]
        {
            new Node("TR",  "TR",  "CIRC_1", 0, 0, true),
            new Node("LID", "LID", "CIRC_1", 1, 0, false),
            new Node("P1",  "P1",  "CIRC_1", 2, 0, false),
            new Node("P2",  "P2",  "CIRC_1", 2, 1, false)
        };

        var edges = new[]
        {
            new Edge("E1", "TR-LID", "CIRC_1", "TR", "LID", new UnitValue(10.0, UnitCode.Meter), "240 Cu", "3", "1"),
            new Edge("E2", "LID-P1", "CIRC_1", "LID", "P1", new UnitValue(20.0, UnitCode.Meter), "185 Al - MX", "3", "1"),
            new Edge("E3", "LID-P2", "CIRC_1", "LID", "P2", new UnitValue(25.0, UnitCode.Meter), "185 Al - MX", "3", "1")
        };

        var loads = new[]
        {
            new Load("L1", "1", "P1", null, LoadKind.Client, 1, new UnitValue(20.0, UnitCode.Kva), 1.0),
            new Load("L2", "1", "P2", null, LoadKind.Client, 1, new UnitValue(30.0, UnitCode.Kva), 1.0)
        };

        var conductors = new[]
        {
            new Conductor("240_Cu", "1", "240 Cu", "240 Cu", new UnitValue(430, UnitCode.Ampere), new UnitValue(0.0762, UnitCode.Ohm), new UnitValue(0.0897, UnitCode.Ohm)),
            new Conductor("185_Al", "1", "185 Al - MX", "185 Al - MX", new UnitValue(335, UnitCode.Ampere), new UnitValue(0.164, UnitCode.Ohm), new UnitValue(0.1178, UnitCode.Ohm))
        };

        var parameters = new[]
        {
            new ElectricalParameter("V", 220.0, "220", UnitCode.V, true),
            new ElectricalParameter("StationMva", 40.0, "40", UnitCode.Mva, true),
            new ElectricalParameter("StationZPercent", 20.0, "20", UnitCode.Percent, true),
            new ElectricalParameter("MtVoltageKv", 13.2, "13.2", UnitCode.Kv, true),
            new ElectricalParameter("MtCableLengthKm", 2.0, "2", UnitCode.Meter, true),
            new ElectricalParameter("MtResistancePerKm", 0.7171, "0.7171", UnitCode.OhmPerKilometer, true),
            new ElectricalParameter("MtReactancePerKm", 0.3512, "0.3512", UnitCode.OhmPerKilometer, true),
            new ElectricalParameter("CH5", null, "SIM", UnitCode.Unknown, true)
        };

        var model = new NetworkModel("NET_BRANCH", "1", new[] { trafo }, new[] { circuit }, nodes, edges, Array.Empty<Branch>(), loads, conductors, parameters);
        var project = new ProjectVersion("V1", "PROJ_BRANCH", "Projeto Bifurcado", model, "HASH_BRANCH", true);

        var calcService = new CalculationService();
        var calcResult = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");
        Assert.Equal(CalculationStatus.Pass, calcResult.Status);

        var topoResult = new TopologyValidator().Validate(model, "CIRC_1");
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(model, topoResult, calcResult.Report!);

        Assert.Equal(4, diagram.Nodes.Count);
        Assert.Equal(3, diagram.Edges.Count);

        var lidNode = diagram.Nodes.First(n => n.NodeId == "LID");
        Assert.Equal(UnifilarNodeType.Branch, lidNode.NodeType);

        var p1Node = diagram.Nodes.First(n => n.NodeId == "P1");
        var p2Node = diagram.Nodes.First(n => n.NodeId == "P2");
        Assert.Equal(UnifilarNodeType.Terminal, p1Node.NodeType);
        Assert.Equal(UnifilarNodeType.Terminal, p2Node.NodeType);

        // Ambas as pontas devem estar mais à direita que o nó de bifurcação
        Assert.True(p1Node.X > lidNode.X);
        Assert.True(p2Node.X > lidNode.X);

        // Pontas devem estar distribuídas verticalmente (posições Y distintas)
        Assert.NotEqual(p1Node.Y, p2Node.Y);
    }

    [Fact]
    public void VisualStates_RepresentNormalOverloadAndEvidenceBlockedFaithfully()
    {
        var project = BuildTestProject();
        var calcService = new CalculationService();
        var result = calcService.ExecuteCalculation(project, CalculationMode.Cqts, "25.0.0");
        var topoResult = new TopologyValidator().Validate(project.NetworkModel, "CIRC_1");

        // Cria apresentação passando estado de EvidenceBlocked
        var diagram = new UnifilarPresentationBuilder().BuildDiagram(
            project.NetworkModel,
            topoResult,
            result.Report!,
            CalculationUiState.EvidenceBlocked);

        Assert.Equal(CalculationUiState.EvidenceBlocked, diagram.UiState);

        // Arestas consom IsOverloaded estritamente do cálculo do backend
        foreach (var edgeVm in diagram.Edges)
        {
            var segReport = result.Report!.Segments.First(s => s.EdgeId == edgeVm.EdgeId);
            Assert.Equal(segReport.IsOverloaded, edgeVm.IsOverloaded);
            Assert.Equal(segReport.IsOverloaded ? "SOBRECARGA" : "OK", edgeVm.OverloadStatus);
        }
    }
}
