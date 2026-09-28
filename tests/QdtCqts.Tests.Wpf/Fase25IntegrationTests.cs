using System;
using System.Linq;
using System.Threading.Tasks;
using QdtCqts.Application;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Calculation.Cqts;
using QdtCqts.Desktop.Wpf.ViewModels;
using QdtCqts.Domain;
using Xunit;

namespace QdtCqts.Tests.Wpf;

/// <summary>
/// Fase 25 — WS-I/J: Testes de integração Application → Pipeline → Output → ViewModel.
/// Cobre os 9 cenários do WS-I e o teste E2E do WS-J.
/// Todos os valores elétricos são oráculos reais das Fases 22–24.
/// </summary>
public sealed class Fase25IntegrationTests
{
    // ─────────────────────────────────────────────────────────────────
    // Helpers — constroem ProjectVersion idêntica à usada no ViewModel
    // ─────────────────────────────────────────────────────────────────
    private static ProjectVersion BuildProj7() => BuildProjectVersion(isProj4: false);
    private static ProjectVersion BuildProj4() => BuildProjectVersion(isProj4: true);

    private static ProjectVersion BuildProjectVersion(bool isProj4)
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

        return new ProjectVersion("V1",
            isProj4?"PROJ_4":"PROJ_7",
            isProj4?"CQT PROJ 4":"CQT PROJ 7",
            model,"HASH_1",true);
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I.1 — Cálculo válido produz ViewModel de sucesso
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsI1_ValidCalculation_ProducesSuccessState()
    {
        var vm = new MainViewModel();
        vm.SelectedProject = "CQT PROJ 7 - AV PADRE DECAMINADA (Rev 2)";

        await vm.ExecuteCalculationAsync();

        Assert.Equal(CalculationUiState.EvidenceBlocked, vm.UiState);
        Assert.Equal("Proteção bloqueada", vm.CalculationStatus);
        Assert.False(vm.IsCalculating);
        Assert.NotEmpty(vm.Segments);
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I.2 — Estado inicial é Idle (não ValidationError)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void WsI2_InitialState_IsIdle()
    {
        var vm = new MainViewModel();
        Assert.Equal(CalculationUiState.Idle, vm.UiState);
        Assert.Equal("Pronto", vm.CalculationStatus);
        Assert.Equal("-", vm.CorrelationId);
        Assert.Equal("-", vm.InputHash);
        Assert.Equal("-", vm.OutputHash);
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I.3 — Cálculo produz Icc3φ dentro do intervalo esperado
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsI3_Calculation_ProducesIcc3PhaseAbove5000A()
    {
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();

        // PROJ 7: CB13 = 8098 A (nó TR→LID é o máximo)
        Assert.True(vm.MaxShortCircuit3PhaseAmperes > 5000,
            $"Esperado Icc3φ > 5000 A, obtido: {vm.MaxShortCircuit3PhaseAmperes:F2} A");
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I.4 — Cálculo produz Icc1φ na ponta do circuito
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsI4_Calculation_ProducesIcc1PhaseAtEndPoint()
    {
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();

        // PROJ 7: CC21 (ponta) ≈ 500.18 A
        Assert.True(vm.MinShortCircuit1PhaseAmperes > 100 && vm.MinShortCircuit1PhaseAmperes < 1000,
            $"Esperado Icc1φ entre 100–1000 A na ponta, obtido: {vm.MinShortCircuit1PhaseAmperes:F2} A");
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I.5 — Proteção térmica disponível no resultado
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsI5_Calculation_KeepsThermalLimitAndBlocksCurveAssessment()
    {
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();

        Assert.Equal(CalculationUiState.EvidenceBlocked, vm.UiState);
        Assert.Equal("Não avaliado", vm.AssessedProtectionDevice);
        Assert.Contains("EVIDENCE_BLOCKED", vm.ProtectionStatus, StringComparison.Ordinal);
        Assert.Contains("suportabilidade térmica", vm.ProtectionStatus, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task P07ToP09_WpfBlocksOnlyProtectionAndKeepsShortCircuitResults()
    {
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();

        Assert.Equal(CalculationUiState.EvidenceBlocked, vm.UiState);
        Assert.NotEqual(CalculationUiState.CalculationError, vm.UiState);
        Assert.Equal(7, vm.Segments.Count);
        Assert.True(vm.Segments[0].ShortCircuit3PhaseAmperes > 5000.0);
        Assert.Contains(vm.Segments, segment => segment.ShortCircuit1PhaseAmperes > 0.0);
        Assert.True(vm.MaxShortCircuit3PhaseAmperes > 8000.0);
        Assert.True(vm.MinShortCircuit1PhaseAmperes > 0.0);
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I.6 — Contract mismatch não existe: campos booleanos são canônicos
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsI6_ContractNormalized_BooleanFieldsUseCanonicalNames()
    {
        // Verifica via reflexão que ProtectionCalculationResult NÃO contém
        // os nomes antigos "IsCurrentAdequate" e "IsThermalAdequate"
        var type = typeof(ProtectionCalculationResult);
        var props = type.GetProperties();
        var propNames = props.Select(p => p.Name).ToArray();

        Assert.DoesNotContain("IsCurrentAdequate", propNames);
        Assert.DoesNotContain("IsThermalAdequate", propNames);
        Assert.Contains("IsRatedCurrentAdequate", propNames);
        Assert.Contains("IsThermalWithstandAdequate", propNames);

        // Executa o cálculo e verifica que a ViewModel projeta os campos corretamente
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();

        // Expectativa histórica substituída: sem curva, a avaliação agora é EvidenceBlocked.
        Assert.Null(vm.IsRatedCurrentAdequate);
        Assert.Null(vm.IsThermalWithstandAdequate);
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I.7 — Hashes disponíveis após cálculo
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsI7_Calculation_HashesArePopulated()
    {
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();

        Assert.NotEqual("-", vm.InputHash);
        Assert.NotEqual("-", vm.OutputHash);
        Assert.False(string.IsNullOrWhiteSpace(vm.InputHash));
        Assert.False(string.IsNullOrWhiteSpace(vm.OutputHash));
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I.8 — CorrelationId disponível após cálculo
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsI8_Calculation_CorrelationIdIsPopulated()
    {
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();

        Assert.NotEqual("-", vm.CorrelationId);
        Assert.NotEqual("-", vm.CalculationId);
        Assert.False(string.IsNullOrWhiteSpace(vm.CorrelationId));
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I.9 — Múltiplos segmentos chegam corretamente à ViewModel
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsI9_MultipleSegments_AreCorrectlyProjectedToViewModel()
    {
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();

        // PROJ 7 tem 7 trechos (E1…E7)
        Assert.Equal(7, vm.Segments.Count);

        // Todos os segmentos devem ter FromTo preenchido
        Assert.All(vm.Segments, s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.FromTo));
            Assert.False(string.IsNullOrWhiteSpace(s.Conductor));
            Assert.True(s.LengthMeters > 0);
        });

        // O primeiro segmento (TR→LID) deve ter o maior Icc3φ
        var first = vm.Segments.First();
        Assert.True(first.ShortCircuit3PhaseAmperes > 5000,
            $"Icc3φ do TR→LID deve ser > 5000 A, obtido: {first.ShortCircuit3PhaseAmperes:F2} A");
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-J — End-to-End: Input → Application → Pipeline → Output → ViewModel
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsJ_EndToEnd_FullFlowFromInputToViewModelProj7()
    {
        // 1. Construção do modelo (sem WPF real)
        var projectVersion = BuildProj7();
        var svc = new CalculationService();

        // 2. Execução via Application Service
        var result = svc.ExecuteCalculation(projectVersion, CalculationMode.Cqts, "25.0.0");

        // 3. Resultado não nulo e bem-sucedido
        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.NotNull(result.Report);

        var report = result.Report!;

        // 4. Icc3φ disponível e correto (oráculo: PROJ 7 CB13 = 8098.066... A)
        var firstSeg = report.Segments.First();
        Assert.True(firstSeg.ShortCircuit3PhaseAmperes > 8090 && firstSeg.ShortCircuit3PhaseAmperes < 8110,
            $"Icc3φ PROJ 7 TR→LID esperado ~8098 A, obtido: {firstSeg.ShortCircuit3PhaseAmperes:F6} A");

        // 5. Icc1φ disponível e correto (oráculo: PROJ 7 CC13 = 8182.488... A)
        Assert.True(firstSeg.ShortCircuit1PhaseAmperes > 8170 && firstSeg.ShortCircuit1PhaseAmperes < 8195,
            $"Icc1φ PROJ 7 TR→LID esperado ~8182 A, obtido: {firstSeg.ShortCircuit1PhaseAmperes:F6} A");

        // 6. Queda de tensão disponível
        Assert.True(report.Nodes.Any(n => n.AccumulatedVoltageDropPercent > 0),
            "Nenhum nó com queda de tensão acumulada > 0%.");

        // 7. Avaliação térmica disponível
        Assert.NotNull(report.Protection);
        Assert.True(report.Protection!.MaxAdmissibleTimeSeconds > 0);
        Assert.Equal(ProtectionAssessmentStatus.EvidenceBlocked, report.Protection.AssessmentStatus);

        // 8. Status coerente
        Assert.Equal(CalculationStatus.Pass, result.Status);

        // 9. Trace disponível via InputHash/OutputHash
        Assert.False(string.IsNullOrWhiteSpace(report.InputHash));
        Assert.False(string.IsNullOrWhiteSpace(report.OutputHash));

        // 10. ViewModel projeta corretamente o resultado
        var vm = new MainViewModel();
        vm.SelectedProject = "CQT PROJ 7 - AV PADRE DECAMINADA (Rev 2)";
        await vm.ExecuteCalculationAsync();

        Assert.Equal(CalculationUiState.EvidenceBlocked, vm.UiState);
        Assert.Equal(7, vm.Segments.Count);
        Assert.True(vm.MaxShortCircuit3PhaseAmperes > 8090);
        Assert.True(vm.MinShortCircuit1PhaseAmperes > 400);
        Assert.True(vm.MaxVoltageDropPercent > 0);
        Assert.NotEqual("-", vm.InputHash);
        Assert.NotEqual("-", vm.OutputHash);
        Assert.NotEqual("-", vm.CorrelationId);
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-J — End-to-End PROJ 4 (cross-check de paridade)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task WsJ_EndToEnd_FullFlowFromInputToViewModelProj4()
    {
        var vm = new MainViewModel();
        vm.SelectedProject = "CQT PROJ 4 - AV PADRE DECAMINADA (Rev 1)";
        await vm.ExecuteCalculationAsync();

        Assert.Equal(CalculationUiState.EvidenceBlocked, vm.UiState);
        Assert.Equal(7, vm.Segments.Count);

        // PROJ 4: Icc3φ CB13 = 8098.064... A
        var firstSeg = vm.Segments.First();
        Assert.True(firstSeg.ShortCircuit3PhaseAmperes > 8090 && firstSeg.ShortCircuit3PhaseAmperes < 8110,
            $"Icc3φ PROJ 4 TR→LID esperado ~8098 A, obtido: {firstSeg.ShortCircuit3PhaseAmperes:F6} A");

        // PROJ 4: Icc1φ CC13 = 8182.474... A
        Assert.True(firstSeg.ShortCircuit1PhaseAmperes > 8170 && firstSeg.ShortCircuit1PhaseAmperes < 8195,
            $"Icc1φ PROJ 4 TR→LID esperado ~8182 A, obtido: {firstSeg.ShortCircuit1PhaseAmperes:F6} A");
    }

    // ═══════════════════════════════════════════════════════════════
    // WS-I — Regressão: troca de projeto reseta o estado corretamente
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task ProjectSwitch_ResetsAllFieldsAndRecalculatesCorrectly()
    {
        var vm = new MainViewModel();
        await vm.ExecuteCalculationAsync();
        Assert.Equal(CalculationUiState.EvidenceBlocked, vm.UiState);

        vm.SelectedProject = "CQT PROJ 4 - AV PADRE DECAMINADA (Rev 1)";

        // Após troca, deve voltar a Idle e limpar
        Assert.Equal(CalculationUiState.Idle, vm.UiState);
        Assert.Equal("Pronto", vm.CalculationStatus);
        Assert.Empty(vm.Segments);
        Assert.Equal("-", vm.InputHash);
        Assert.Equal("-", vm.OutputHash);

        await vm.ExecuteCalculationAsync();
        Assert.Equal(CalculationUiState.EvidenceBlocked, vm.UiState);
        Assert.Equal(7, vm.Segments.Count);
    }
}
