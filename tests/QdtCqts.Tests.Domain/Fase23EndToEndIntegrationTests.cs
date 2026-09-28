using System;
using System.Collections.Generic;
using System.Linq;
using QdtCqts.Application;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Calculation.Cqts;
using QdtCqts.Domain;
using Xunit;

namespace QdtCqts.Tests.Domain;

/// <summary>
/// Testes de integrao ponta a ponta e paridade com o orculo Excel para a Fase 23.
/// Valida a cadeia completa: Cargas Individuais -> Acumulao Radial -> Demanda M ->
/// Capacidade Ib/Iz -> Temperatura -> Impedncia -> Queda no Trecho -> Trafo/MT -> Queda Acumulada.
/// </summary>
public sealed class Fase23EndToEndIntegrationTests
{
    [Fact]
    public void EndToEnd_Proj7_FullChain_MatchesExcelOracle()
    {
        // 1. Constri o modelo de rede do CQT PROJ 7 REV2 (Alimentador Lado 1 com ramificaes)
        var networkModel = CreateProj7NetworkModel();
        var projectVersion = new ProjectVersion("V_PROJ7", "PROJ7", "REV2", networkModel, "SHA256_PROJ7", true);

        var traceRecorder = new CalculationTraceRecorder();
        var auditService = new CalculationAuditService(Microsoft.Extensions.Logging.Abstractions.NullLogger<CalculationAuditService>.Instance, traceRecorder);
        var service = new CalculationService(traceRecorder: traceRecorder, auditService: auditService);

        // 2. Executa a cadeia unificada de clculo
        var result = service.ExecuteCalculation(projectVersion, CalculationMode.Cqts, "23.0.0");

        // 3. Validao de status e integridade do relatrio
        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.NotNull(result.Report);
        var report = result.Report!;

        // 4. Validao do Transformador e Origem de Tenso
        Assert.Single(report.Transformers);
        var trafoResult = report.Transformers[0];
        Assert.Equal(112.5, trafoResult.NominalPowerKva, 2);
        Assert.Equal(3.5, trafoResult.ImpedancePercent, 2);
        // Queda do trafo BV4 = (74.448 / 112.5) * 3.5 = 2.31616%
        Assert.Equal(2.3161599999999996d, trafoResult.TrafoVoltageDropPercent, 10);
        // Queda MT CV105 = 1.8330629395275368%
        Assert.Equal(1.8330629395275368d, trafoResult.MtVoltageDropPercent, 10);
        // Origem total em TR = 2.31616% + 1.83306% = 4.149222939527536%
        Assert.Equal(4.149222939527536d, trafoResult.TotalOriginVoltageDropPercent, 10);

        // 5. Validao do Trecho Inicial TR -> LID (Linha 13 do Excel)
        var segTrLid = report.Segments.Single(s => s.FromNodeId == "N_TR" && s.ToNodeId == "N_LID");
        // M13 = 74.448 kVA
        Assert.Equal(74.448d, segTrLid.EndLoadSelectedKva, 3);
        // Cabos paralelos AP = 2, Comprimento fsico L = 4 m, Lequiv = 2 m
        Assert.Equal(2.0d, segTrLid.ParallelCables);
        Assert.Equal(4.0d, segTrLid.PhysicalLengthMeters);
        Assert.Equal(2.0d, segTrLid.EquivalentLengthMeters);
        // Ampacidade nominal = 430 A, total = 860 A, sem sobrecarga
        Assert.Equal(430.0d, segTrLid.RatedAmpacityAmperes / 2.0);
        Assert.False(segTrLid.IsOverloaded);
        // Temperatura do cabo BX13 = 43.630837053053675 C
        Assert.Equal(43.630837053053675d, segTrLid.OperatingTemperatureCelsius, 10);
        // Resistncia CA corrigida BI13 = 0.088708306113649771 Ohm/km
        Assert.Equal(0.088708306113649771d, segTrLid.ResistanceCaOhmPerKm, 10);
        // Queda no trecho BZ13 = 0.038810072181038206%
        Assert.Equal(0.038810072181038206d, segTrLid.SegmentVoltageDropPercent, 12);

        // 6. Validao da Queda Acumulada em LID (CA13 = BZ13 + BV4 + CV105)
        var nodeLid = report.Nodes.Single(n => n.NodeId == "N_LID");
        Assert.Equal(4.1880330117085744d, nodeLid.AccumulatedVoltageDropPercent, 12);

        // 7. Validao de Bifurcao e Isolamento de Caminhos Radiais em P2
        var segP2P3 = report.Segments.Single(s => s.FromNodeId == "N_P2" && s.ToNodeId == "N_P3");
        var segP2P8 = report.Segments.Single(s => s.FromNodeId == "N_P2" && s.ToNodeId == "N_P8");
        var nodeP3 = report.Nodes.Single(n => n.NodeId == "N_P3");
        var nodeP8 = report.Nodes.Single(n => n.NodeId == "N_P8");

        // Os ns P3 e P8 possuem caminhos independentes
        Assert.NotEqual(nodeP3.AccumulatedVoltageDropPercent, nodeP8.AccumulatedVoltageDropPercent);
        Assert.True(nodeP3.AccumulatedVoltageDropPercent > nodeLid.AccumulatedVoltageDropPercent);
        Assert.True(nodeP8.AccumulatedVoltageDropPercent > nodeLid.AccumulatedVoltageDropPercent);

        // 8. Determinismo estrito de hashes
        Assert.False(string.IsNullOrWhiteSpace(report.InputHash));
        Assert.False(string.IsNullOrWhiteSpace(report.OutputHash));

        // Segunda execuo idntica produz exatamente os mesmos hashes
        var result2 = service.ExecuteCalculation(projectVersion, CalculationMode.Cqts, "23.0.0");
        Assert.Equal(result.Report!.InputHash, result2.Report!.InputHash);
        Assert.Equal(result.Report!.OutputHash, result2.Report!.OutputHash);

        // 9. Observabilidade e traces registrados
        Assert.NotEmpty(traceRecorder.GetTraces());
        var firstTrace = traceRecorder.GetTraces().First();
        Assert.Equal("CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP", firstTrace.RuleId);
        Assert.Equal(RuleStatus.RealProjectConfirmed, firstTrace.RuleStatus);
    }

    [Fact]
    public void CycleTopology_IsRejected_WithDiagnostic()
    {
        var nodes = new[]
        {
            new Node("N1", "TR", "C1", 0, 0, true),
            new Node("N2", "P1", "C1", 0, 0, false)
        };
        var edges = new[]
        {
            new Edge("E1", "TR-P1", "C1", "N1", "N2", new UnitValue(10, UnitCode.Meter), "240_Cu", "3", "1"),
            new Edge("E2", "P1-TR", "C1", "N2", "N1", new UnitValue(10, UnitCode.Meter), "240_Cu", "3", "1")
        };
        var model = new NetworkModel("M_CYCLE", "1", Array.Empty<Transformer>(), new[] { new Circuit("C1", "CIRC", "T1", 1, CalculationMode.Cqts) }, nodes, edges, Array.Empty<Branch>(), Array.Empty<Load>(), Array.Empty<Conductor>(), Array.Empty<ElectricalParameter>());
        var version = new ProjectVersion("V_CYCLE", "P_CYCLE", "1", model, "HASH", true);

        var service = new CalculationService();
        var result = service.ExecuteCalculation(version, CalculationMode.Cqts);

        Assert.Equal(CalculationStatus.Blocked, result.Status);
        Assert.Contains(result.Diagnostics, d => d.Code.Contains("Cycle", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OverloadDetection_FlagsExcessiveCurrent()
    {
        var trafo = new Transformer("TR_OV", "TR", new UnitValue(300, UnitCode.Kva), new UnitValue(3.5, UnitCode.Percent), new UnitValue(220, UnitCode.V), new UnitValue(250, UnitCode.Kva));
        var circuit = new Circuit("C_OV", "C", trafo.Id, 1, CalculationMode.Cqts);
        var nodes = new[]
        {
            new Node("N_TR", "TR", circuit.Id, 0, 0, true),
            new Node("N_LID", "LID", circuit.Id, 0, 10, false, "N_TR")
        };
        // Cabo fino 16 Al (ampacidade 80 A), mas com carga de 100 kVA (corrente Ib = 100 / (220 * sqrt(3) / 1000) = 262.4 A > 80 A!)
        var edges = new[]
        {
            new Edge("E_OV", "TR-LID", circuit.Id, "N_TR", "N_LID", new UnitValue(10, UnitCode.Meter), "16_Al", "3", "1")
        };
        var loads = new[]
        {
            // 50 consumidores x 2.0 kVA = 100 kVA (D = 50 > 2, sem piso de 4/8 kVA)
            new Load("L_OV", "1", "N_LID", null, LoadKind.Client, 50, new UnitValue(2.0, UnitCode.Kva), 1.0)
        };
        var conductors = new[]
        {
            new Conductor("16_Al", "1", "16 Al_CONC_Tri", "16 Al_CONC_Tri", new UnitValue(80.0, UnitCode.Ampere), new UnitValue(2.06, UnitCode.Ohm), new UnitValue(0.85, UnitCode.Ohm))
        };
        var model = new NetworkModel("M_OV", "1", new[] { trafo }, new[] { circuit }, nodes, edges, Array.Empty<Branch>(), loads, conductors,
            new[] { new ElectricalParameter("V", 220.0, "220", UnitCode.V, true) });
        var version = new ProjectVersion("V_OV", "P_OV", "1", model, "HASH", true);

        var service = new CalculationService();
        var result = service.ExecuteCalculation(version, CalculationMode.Cqts);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.NotNull(result.Report);
        var seg = result.Report!.Segments.Single();
        Assert.True(seg.IsOverloaded);
        Assert.True(seg.OperatingCurrentAmperes > seg.RatedAmpacityAmperes);

        var modelWithoutConductor = new NetworkModel(model.Id, model.VersionId, model.Transformers, model.Circuits, model.Nodes, model.Edges,
            model.Branches, model.Loads, Array.Empty<Conductor>(), model.Parameters);
        var missingConductorResult = service.ExecuteCalculation(
            new ProjectVersion("V_NO_CONDUCTOR", "P_OV", "1", modelWithoutConductor, "HASH", true), CalculationMode.Cqts);
        Assert.Contains(missingConductorResult.Diagnostics, diagnostic => diagnostic.Code == "MISSING_CONDUCTOR_CATALOG");

        var modelWithoutTransformer = new NetworkModel(model.Id, model.VersionId, Array.Empty<Transformer>(), model.Circuits, model.Nodes, model.Edges,
            model.Branches, model.Loads, model.Conductors, model.Parameters);
        var missingTransformerResult = service.ExecuteCalculation(
            new ProjectVersion("V_NO_TRANSFORMER", "P_OV", "1", modelWithoutTransformer, "HASH", true), CalculationMode.Cqts);
        Assert.Contains(missingTransformerResult.Diagnostics, diagnostic => diagnostic.Code == "MISSING_TRANSFORMER_CATALOG");
    }

    [Fact]
    public void EndToEnd_Proj4_MatchesExcelOracle()
    {
        // CQT PROJ 4 REV1:
        // Linha 13: M13 = 75.68658823529411 kVA, L = 4 m, AP = 2 cabos, condutor 240 Cu
        // Linha 14: M14 = 26.9216 kVA, L = 38 m, AP = 1 cabo, condutor 70 Al
        var trafo = new Transformer("TR4", "TR4", new UnitValue(112.5, UnitCode.Kva), new UnitValue(3.5, UnitCode.Percent), new UnitValue(220, UnitCode.V), new UnitValue(75.68658823529411, UnitCode.Kva));
        var circuit = new Circuit("C_P4", "LADO 1", trafo.Id, 1, CalculationMode.Cqts);

        var nodes = new[]
        {
            new Node("N_TR", "TR", circuit.Id, 0, 0, true),
            new Node("N_LID", "LID", circuit.Id, 0, 4, false, "N_TR"),
            new Node("N_P1", "P1", circuit.Id, 0, 42, false, "N_LID")
        };

        var edges = new[]
        {
            new Edge("E_13", "TR->LID", circuit.Id, "N_TR", "N_LID", new UnitValue(4.0, UnitCode.Meter), "240_Cu", "3", "2"),
            new Edge("E_14", "LID->P1", circuit.Id, "N_LID", "N_P1", new UnitValue(38.0, UnitCode.Meter), "70_Al", "3", "1")
        };

        var loads = new[]
        {
            // Cargas com D > 2 para refletir o projeto real
            // P1: 10 consumidores x 2.69216 kVA = 26.9216 kVA
            new Load("L_P1", "1", "N_P1", null, LoadKind.Client, 10, new UnitValue(2.6921599999999998, UnitCode.Kva), 1.0),
            // LID: 15 consumidores x 3.251 kVA tal que total seja 75.686588 kVA
            new Load("L_LID", "1", "N_LID", null, LoadKind.Point, 15, new UnitValue((75.68658823529411 - 26.921599999999998) / 15.0, UnitCode.Kva), 1.0)
        };

        var conductors = new[]
        {
            new Conductor("240_Cu", "1", "240 Cu", "240 Cu", new UnitValue(430.0, UnitCode.Ampere), new UnitValue(0.0762, UnitCode.Ohm), new UnitValue(0.0897, UnitCode.Ohm)),
            new Conductor("70_Al", "1", "70 Al - MX", "70 Al - MX", new UnitValue(195.0, UnitCode.Ampere), new UnitValue(0.472, UnitCode.Ohm), new UnitValue(0.126, UnitCode.Ohm))
        };

        var model = new NetworkModel("M_P4", "1", new[] { trafo }, new[] { circuit }, nodes, edges, Array.Empty<Branch>(), loads, conductors,
            new[] { new ElectricalParameter("V", 220.0, "220", UnitCode.V, true) });
        var version = new ProjectVersion("V_P4", "P4", "REV1", model, "HASH_P4", true);

        var service = new CalculationService();
        var result = service.ExecuteCalculation(version, CalculationMode.Cqts);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.NotNull(result.Report);

        var seg13 = result.Report!.Segments.Single(s => s.EdgeId == "E_13");
        var seg14 = result.Report!.Segments.Single(s => s.EdgeId == "E_14");

        // Delta V% Linha 13: 0.039471666113062395%
        Assert.Equal(0.039471666113062395d, seg13.SegmentVoltageDropPercent, 10);
        // Queda do trecho 14 (LID -> P1) é calculada com condutor 70 Al
        Assert.True(seg14.SegmentVoltageDropPercent > 0.4);
    }

    private static NetworkModel CreateProj7NetworkModel()
    {
        var trafo = new Transformer("TR7", "TR7", new UnitValue(112.5, UnitCode.Kva), new UnitValue(3.5, UnitCode.Percent), new UnitValue(220.0, UnitCode.V), new UnitValue(74.448, UnitCode.Kva));
        var circuit = new Circuit("CIRCUIT_LADO1", "LADO 1", trafo.Id, 1, CalculationMode.Cqts);

        var nodes = new[]
        {
            new Node("N_TR", "TR", circuit.Id, 0, 0, true),
            new Node("N_LID", "LID", circuit.Id, 0, 4, false, "N_TR"),
            new Node("N_P1", "P1", circuit.Id, 0, 36, false, "N_LID"),
            new Node("N_P2", "P2", circuit.Id, 0, 68, false, "N_P1"),
            new Node("N_P3", "P3", circuit.Id, -20, 90, false, "N_P2"),
            new Node("N_P8", "P8", circuit.Id, 20, 90, false, "N_P2")
        };

        var edges = new[]
        {
            // Trecho 1: TR -> LID (240 Cu, 2 cabos paralelos, L = 4 m, 3 fases)
            new Edge("E_TR_LID", "TR->LID", circuit.Id, "N_TR", "N_LID", new UnitValue(4.0, UnitCode.Meter), "240_Cu", "3", "2"),
            // Trecho 2: LID -> P1 (240 Cu, 1 cabo, L = 32 m, 3 fases)
            new Edge("E_LID_P1", "LID->P1", circuit.Id, "N_LID", "N_P1", new UnitValue(32.0, UnitCode.Meter), "240_Cu", "3", "1"),
            // Trecho 3: P1 -> P2 (240 Cu, 1 cabo, L = 32 m, 3 fases)
            new Edge("E_P1_P2", "P1->P2", circuit.Id, "N_P1", "N_P2", new UnitValue(32.0, UnitCode.Meter), "240_Cu", "3", "1"),
            // Trecho 4: P2 -> P3 (70 Al, 1 cabo, L = 30 m, 3 fases)
            new Edge("E_P2_P3", "P2->P3", circuit.Id, "N_P2", "N_P3", new UnitValue(30.0, UnitCode.Meter), "70_Al", "3", "1"),
            // Trecho 5: P2 -> P8 (70 Al, 1 cabo, L = 25 m, 3 fases)
            new Edge("E_P2_P8", "P2->P8", circuit.Id, "N_P2", "N_P8", new UnitValue(25.0, UnitCode.Meter), "70_Al", "3", "1")
        };

        var loads = new[]
        {
            // Cargas terminais: (Count x UnitLoadKva)
            // P3: 10 consumidores x 2.5 kVA = 25.0 kVA
            new Load("L_P3", "1", "N_P3", null, LoadKind.Client, 10, new UnitValue(2.5, UnitCode.Kva), 1.0),
            // P8: 8 consumidores x 3.2529 kVA = 26.0232 kVA
            new Load("L_P8", "1", "N_P8", null, LoadKind.Client, 8, new UnitValue(3.2529, UnitCode.Kva), 1.0),
            // P1: 5 consumidores x 2.4 kVA = 12.0 kVA
            new Load("L_P1", "1", "N_P1", null, LoadKind.Client, 5, new UnitValue(2.4, UnitCode.Kva), 1.0),
            // LID: 4 consumidores x 2.8562 kVA = 11.4248 kVA
            new Load("L_LID", "1", "N_LID", null, LoadKind.Point, 4, new UnitValue(2.8562, UnitCode.Kva), 1.0)
            // Soma acumulada no TR = 25.0 + 26.0232 + 12.0 + 11.4248 = 74.448 kVA!
        };

        var conductors = new[]
        {
            new Conductor("240_Cu", "1", "240 Cu", "240 Cu", new UnitValue(430.0, UnitCode.Ampere), new UnitValue(0.0762, UnitCode.Ohm), new UnitValue(0.0897, UnitCode.Ohm)),
            new Conductor("70_Al", "1", "70 Al - MX", "70 Al - MX", new UnitValue(195.0, UnitCode.Ampere), new UnitValue(0.472, UnitCode.Ohm), new UnitValue(0.126, UnitCode.Ohm)),
            new Conductor("16_Al", "1", "16 Al_CONC_Tri", "16 Al_CONC_Tri", new UnitValue(80.0, UnitCode.Ampere), new UnitValue(2.06, UnitCode.Ohm), new UnitValue(0.85, UnitCode.Ohm))
        };

        var parameters = new[]
        {
            new ElectricalParameter("V", 220.0, "220", UnitCode.V, true),
            new ElectricalParameter("CH5", null, "SIM", UnitCode.Unknown, true),
            new ElectricalParameter("MtVoltageDropPercent", 1.8330629395275368d, "1.833", UnitCode.Percent, true)
        };

        return new NetworkModel(
            "MODEL_PROJ7",
            "V1",
            new[] { trafo },
            new[] { circuit },
            nodes,
            edges,
            Array.Empty<Branch>(),
            loads,
            conductors,
            parameters);
    }
}
