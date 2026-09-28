using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using QdtCqts.Application;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Desktop.Wpf.ViewModels.Unifilar;
using QdtCqts.Domain;

namespace QdtCqts.Desktop.Wpf.ViewModels;

// ─────────────────────────────────────────────
// WS-E: Estados explícitos da UI (Fase 25)
// ─────────────────────────────────────────────
public enum CalculationUiState
{
    Idle,
    Calculating,
    Success,
    ValidationError,
    CalculationError,
    EvidenceBlocked
}

// ─────────────────────────────────────────────
// Modelo de apresentação de trecho (sem lógica elétrica)
// Fase 27B: consome IsOverloaded calculado diretamente pelo motor.
// ─────────────────────────────────────────────
public sealed class SegmentDisplayModel
{
    public string SegmentId { get; init; } = string.Empty;
    public string FromTo { get; init; } = string.Empty;
    public string Conductor { get; init; } = string.Empty;
    public double LengthMeters { get; init; }
    public double LoadKva { get; init; }
    public double OperatingCurrentAmperes { get; init; }
    public double RatedAmpacityAmperes { get; init; }
    public double TemperatureCelsius { get; init; }
    public double SegmentVoltageDropPercent { get; init; }
    public double ShortCircuit3PhaseAmperes { get; init; }
    public double ShortCircuit1PhaseAmperes { get; init; }
    public bool IsOverloaded { get; init; }
    public string OverloadStatus => IsOverloaded ? "SOBRECARGA" : "OK";
}

// ─────────────────────────────────────────────
// ViewModel principal — Fase 25 WS-D/E/F
// ─────────────────────────────────────────────
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly CalculationService _calculationService;

    // Estado
    private CalculationUiState _uiState = CalculationUiState.Idle;
    private string _calculationStatus = "Pronto";
    private string _statusColor = "#38BDF8";
    private bool _isCalculating;
    private string _statusMessage = "Selecione um projeto e clique em Executar Cálculo.";

    // Rastreabilidade (WS-F)
    private string _correlationId = "-";
    private string _calculationId = "-";
    private string _inputHash = "-";
    private string _outputHash = "-";
    private double _executionDurationMs;

    // Métricas elétricas
    private double _maxVoltageDropPercent;
    private double _maxShortCircuit3PhaseAmperes;
    private double _minShortCircuit1PhaseAmperes;

    // Proteção
    private string _assessedProtectionDevice = "-";
    private string _protectionStatus = "Aguardando cálculo";
    private bool? _isRatedCurrentAdequate;
    private bool? _isThermalWithstandAdequate;

    private string _selectedProject;

    public MainViewModel(CalculationService? calculationService = null)
    {
        _calculationService = calculationService ?? new CalculationService();
        AvailableProjects = new ObservableCollection<string>
        {
            "CQT PROJ 7 - AV PADRE DECAMINADA (Rev 2)",
            "CQT PROJ 4 - AV PADRE DECAMINADA (Rev 1)"
        };
        _selectedProject = AvailableProjects.First();
        Segments = new ObservableCollection<SegmentDisplayModel>();
        ExecuteCalculationCommand = new AsyncRelayCommand(ExecuteCalculationAsync, () => !IsCalculating);
    }

    public ObservableCollection<string> AvailableProjects { get; }
    public ObservableCollection<SegmentDisplayModel> Segments { get; }
    public ICommand ExecuteCalculationCommand { get; }

    // ── Seleção de projeto ──
    public string SelectedProject
    {
        get => _selectedProject;
        set { if (SetField(ref _selectedProject, value)) ResetResults(); }
    }

    // ── Estado UI ──
    public CalculationUiState UiState
    {
        get => _uiState;
        private set => SetField(ref _uiState, value);
    }

    public string CalculationStatus
    {
        get => _calculationStatus;
        private set => SetField(ref _calculationStatus, value);
    }

    public string StatusColor
    {
        get => _statusColor;
        private set => SetField(ref _statusColor, value);
    }

    public bool IsCalculating
    {
        get => _isCalculating;
        private set => SetField(ref _isCalculating, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    // ── Rastreabilidade (WS-F) ──
    public string CorrelationId
    {
        get => _correlationId;
        private set => SetField(ref _correlationId, value);
    }

    public string CalculationId
    {
        get => _calculationId;
        private set => SetField(ref _calculationId, value);
    }

    public string InputHash
    {
        get => _inputHash;
        private set => SetField(ref _inputHash, value);
    }

    public string OutputHash
    {
        get => _outputHash;
        private set => SetField(ref _outputHash, value);
    }

    public double ExecutionDurationMs
    {
        get => _executionDurationMs;
        private set => SetField(ref _executionDurationMs, value);
    }

    // ── Métricas elétricas ──
    public double MaxVoltageDropPercent
    {
        get => _maxVoltageDropPercent;
        private set => SetField(ref _maxVoltageDropPercent, value);
    }

    public double MaxShortCircuit3PhaseAmperes
    {
        get => _maxShortCircuit3PhaseAmperes;
        private set => SetField(ref _maxShortCircuit3PhaseAmperes, value);
    }

    public double MinShortCircuit1PhaseAmperes
    {
        get => _minShortCircuit1PhaseAmperes;
        private set => SetField(ref _minShortCircuit1PhaseAmperes, value);
    }

    // ── Proteção ──
    public string AssessedProtectionDevice
    {
        get => _assessedProtectionDevice;
        private set => SetField(ref _assessedProtectionDevice, value);
    }

    public string ProtectionStatus
    {
        get => _protectionStatus;
        private set => SetField(ref _protectionStatus, value);
    }

    /// <summary>Corrente de fusível adequada para a carga do circuito (Erro 06).</summary>
    public bool? IsRatedCurrentAdequate
    {
        get => _isRatedCurrentAdequate;
        private set => SetField(ref _isRatedCurrentAdequate, value);
    }

    /// <summary>Suportabilidade térmica do condutor no curto-circuito (Erro 07).</summary>
    public bool? IsThermalWithstandAdequate
    {
        get => _isThermalWithstandAdequate;
        private set => SetField(ref _isThermalWithstandAdequate, value);
    }

    // ── Unifilar (Fase 27B) ──
    private UnifilarDiagramViewModel? _unifilarDiagram;
    public UnifilarDiagramViewModel? UnifilarDiagram
    {
        get => _unifilarDiagram;
        private set => SetField(ref _unifilarDiagram, value);
    }

    // ── Execução ──
    public async Task ExecuteCalculationAsync()
    {
        IsCalculating = true;
        UiState = CalculationUiState.Calculating;
        CalculationStatus = "Calculando...";
        StatusColor = "#F59E0B";
        StatusMessage = "Executando pipeline de cálculo unificado...";

        try
        {
            var projectVersion = BuildSelectedProjectVersion();
            var runCorrelationId = Guid.NewGuid().ToString("N");

            var result = await Task.Run(() => _calculationService.ExecuteCalculation(
                projectVersion,
                CalculationMode.Cqts,
                "25.0.0",
                runCorrelationId));

            CorrelationId = runCorrelationId;

            if (result.Status == Domain.CalculationStatus.Pass && result.Report != null)
            {
                ApplySuccessState(result, runCorrelationId, projectVersion);
            }
            else
            {
                ApplyCalculationErrorState(result);
            }
        }
        catch (ArgumentException ex)
        {
            // Erro de validação de entrada — separado de erro de cálculo
            UiState = CalculationUiState.ValidationError;
            CalculationStatus = "Erro de Validação";
            StatusColor = "#F59E0B";
            StatusMessage = $"Dados de entrada inválidos: {ex.Message}";
        }
        catch (Exception ex)
        {
            UiState = CalculationUiState.CalculationError;
            CalculationStatus = "Erro";
            StatusColor = "#EF4444";
            StatusMessage = $"Erro inesperado: {ex.Message}";
        }
        finally
        {
            IsCalculating = false;
        }
    }

    private void ApplySuccessState(CalculationResult result, string runCorrelationId, ProjectVersion projectVersion)
    {
        var report = result.Report!;

        UiState = CalculationUiState.Success;
        CalculationStatus = "Concluído";
        StatusColor = "#10B981";
        CalculationId = report.RunId;
        InputHash = report.InputHash;
        OutputHash = report.OutputHash;
        ExecutionDurationMs = report.ExecutionDurationMs;

        Segments.Clear();
        foreach (var seg in report.Segments)
        {
            Segments.Add(new SegmentDisplayModel
            {
                SegmentId = seg.EdgeId,
                FromTo = $"{seg.FromNodeId} → {seg.ToNodeId}",
                Conductor = seg.ConductorKey ?? "N/D",
                LengthMeters = seg.PhysicalLengthMeters,
                LoadKva = seg.EndLoadSelectedKva,
                OperatingCurrentAmperes = seg.OperatingCurrentAmperes,
                RatedAmpacityAmperes = seg.RatedAmpacityAmperes,
                TemperatureCelsius = seg.OperatingTemperatureCelsius,
                SegmentVoltageDropPercent = seg.SegmentVoltageDropPercent,
                ShortCircuit3PhaseAmperes = seg.ShortCircuit3PhaseAmperes,
                ShortCircuit1PhaseAmperes = seg.ShortCircuit1PhaseAmperes,
                IsOverloaded = seg.IsOverloaded
            });
        }

        MaxVoltageDropPercent = report.Nodes.Count > 0
            ? report.Nodes.Max(n => n.AccumulatedVoltageDropPercent)
            : 0.0;

        MaxShortCircuit3PhaseAmperes = report.Segments.Count > 0
            ? report.Segments.Max(s => s.ShortCircuit3PhaseAmperes)
            : 0.0;

        MinShortCircuit1PhaseAmperes = report.Segments.Count > 0
            ? report.Segments.Min(s => s.ShortCircuit1PhaseAmperes)
            : 0.0;

        ApplyProtectionState(report.Protection);

        // Fase 27B: Projeção de apresentação do unifilar a partir dos resultados reais
        var topologyValidator = new TopologyValidator();
        var circuitId = projectVersion.NetworkModel.Circuits.FirstOrDefault()?.Id ?? "CIRC_1";
        var topologyResult = topologyValidator.Validate(projectVersion.NetworkModel, circuitId);
        var presentationBuilder = new UnifilarPresentationBuilder();
        UnifilarDiagram = presentationBuilder.BuildDiagram(
            projectVersion.NetworkModel,
            topologyResult,
            report,
            UiState);

        string calculationSummary = $"Cálculo elétrico executado em {ExecutionDurationMs:F1} ms · {Segments.Count} trechos · " +
                                    $"ΔV máx: {MaxVoltageDropPercent:F2}% · Icc1φ mín: {MinShortCircuit1PhaseAmperes:F0} A";
        StatusMessage = UiState == CalculationUiState.EvidenceBlocked
            ? $"{calculationSummary} · avaliação por curva bloqueada por falta de evidência."
            : calculationSummary;
    }

    private void ApplyProtectionState(ProtectionCalculationResult? protection)
    {
        if (protection is null)
        {
            AssessedProtectionDevice = "Não avaliado";
            ProtectionStatus = "EVIDENCE_BLOCKED: resultado de proteção ausente.";
            IsRatedCurrentAdequate = null;
            IsThermalWithstandAdequate = null;
            UiState = CalculationUiState.EvidenceBlocked;
            CalculationStatus = "Proteção bloqueada";
            StatusColor = "#F59E0B";
            return;
        }

        AssessedProtectionDevice = protection.DeviceEvidence is null
            ? "Não avaliado"
            : $"{protection.DeviceEvidence.Model} · {protection.DeviceEvidence.RatedCurrentAmperes:F0} A";
        ProtectionStatus = protection.StatusMessage;
        IsRatedCurrentAdequate = protection.IsRatedCurrentAdequate;
        IsThermalWithstandAdequate = protection.IsThermalWithstandAdequate;

        if (protection.AssessmentStatus == ProtectionAssessmentStatus.EvidenceBlocked)
        {
            UiState = CalculationUiState.EvidenceBlocked;
            CalculationStatus = "Proteção bloqueada";
            StatusColor = "#F59E0B";
        }
    }

    private void ApplyCalculationErrorState(CalculationResult result)
    {
        UiState = CalculationUiState.CalculationError;
        CalculationStatus = "Falha";
        StatusColor = "#EF4444";
        var firstDiag = result.Diagnostics.FirstOrDefault();
        StatusMessage = firstDiag != null
            ? $"Falha no cálculo: [{firstDiag.Code}] {firstDiag.Message}"
            : "Falha na execução do cálculo.";
    }

    private void ResetResults()
    {
        UiState = CalculationUiState.Idle;
        CalculationStatus = "Pronto";
        StatusColor = "#38BDF8";
        StatusMessage = "Projeto alterado. Clique em Executar Cálculo.";
        Segments.Clear();
        MaxVoltageDropPercent = 0.0;
        MaxShortCircuit3PhaseAmperes = 0.0;
        MinShortCircuit1PhaseAmperes = 0.0;
        AssessedProtectionDevice = "-";
        ProtectionStatus = "Aguardando cálculo";
        IsRatedCurrentAdequate = null;
        IsThermalWithstandAdequate = null;
        CorrelationId = "-";
        CalculationId = "-";
        InputHash = "-";
        OutputHash = "-";
        ExecutionDurationMs = 0.0;
        UnifilarDiagram = null;
    }

    private ProjectVersion BuildSelectedProjectVersion()
    {
        bool isProj4 = SelectedProject.Contains("PROJ 4", StringComparison.OrdinalIgnoreCase);

        var trafo = new Transformer("TR", "TR-112.5",
            new UnitValue(112.5, UnitCode.Kva),
            new UnitValue(3.5, UnitCode.Percent),
            new UnitValue(220, UnitCode.V),
            new UnitValue(0, UnitCode.Kva));

        var circuit = new Circuit("CIRC_1", "LADO 1", "TR", 1, CalculationMode.Cqts);

        var nodes = new List<Node>
        {
            new("TR",  "TR",  "CIRC_1", 0, 0, true),
            new("LID", "LID", "CIRC_1", 1, 0, false),
            new("P1",  "P1",  "CIRC_1", 2, 0, false),
            new("P2",  "P2",  "CIRC_1", 3, 0, false),
            new("P3",  "P3",  "CIRC_1", 4, 0, false),
            new("P4",  "P4",  "CIRC_1", 5, 0, false),
            new("P5",  "P5",  "CIRC_1", 6, 0, false),
            new("RL",  "RL",  "CIRC_1", 7, 0, false)
        };

        var edges = new List<Edge>
        {
            new("E1", "TR-LID",  "CIRC_1", "TR",  "LID", new UnitValue(4.0,  UnitCode.Meter), "240 Cu",        "3", "2"),
            new("E2", "LID-P1",  "CIRC_1", "LID", "P1",  new UnitValue(isProj4 ? 38.0 : 35.0, UnitCode.Meter), "185 Al - MX",                                       "3", "1"),
            new("E3", "P1-P2",   "CIRC_1", "P1",  "P2",  new UnitValue(isProj4 ? 38.0 : 32.0, UnitCode.Meter), isProj4 ? "70 Al - MX" : "185 Al - MX",              "3", "1"),
            new("E4", "P2-P3",   "CIRC_1", "P2",  "P3",  new UnitValue(isProj4 ? 31.0 : 32.0, UnitCode.Meter), isProj4 ? "70 Al - MX" : "185 Al - MX",              "3", "1"),
            new("E5", "P3-P4",   "CIRC_1", "P3",  "P4",  new UnitValue(isProj4 ? 28.0 : 35.0, UnitCode.Meter), isProj4 ? "70 Al - MX" : "185 Al - MX",              "3", "1"),
            new("E6", "P4-P5",   "CIRC_1", "P4",  "P5",  new UnitValue(isProj4 ? 27.0 : 35.0, UnitCode.Meter), isProj4 ? "70 Al - MX" : "185 Al - MX",              "3", "1"),
            new("E7", "P5-RL",   "CIRC_1", "P5",  "RL",  new UnitValue(30.0,  UnitCode.Meter), "16 Al_CONC_Tri", "3", "1")
        };

        var loads = new List<Load>
        {
            new("L_RL", "1", "RL", null, LoadKind.Client, 1,
                new UnitValue(isProj4 ? 75.686588 : 74.448, UnitCode.Kva), 1.0)
        };

        var conductors = new List<Conductor>
        {
            new("240_Cu",  "1", "240 Cu",        "240 Cu",        new UnitValue(430, UnitCode.Ampere), new UnitValue(0.0762, UnitCode.Ohm), new UnitValue(0.0897, UnitCode.Ohm)),
            new("185_Al",  "1", "185 Al - MX",   "185 Al - MX",   new UnitValue(335, UnitCode.Ampere), new UnitValue(0.164,  UnitCode.Ohm), new UnitValue(0.1178, UnitCode.Ohm)),
            new("70_Al",   "1", "70 Al - MX",    "70 Al - MX",    new UnitValue(195, UnitCode.Ampere), new UnitValue(0.472,  UnitCode.Ohm), new UnitValue(0.126,  UnitCode.Ohm)),
            new("16_Al",   "1", "16 Al_CONC_Tri","16 Al_CONC_Tri",new UnitValue(80,  UnitCode.Ampere), new UnitValue(2.06,   UnitCode.Ohm), new UnitValue(0.85,   UnitCode.Ohm))
        };

        var parameters = new List<ElectricalParameter>
        {
            new("V",                220.0,  "220",    UnitCode.V,              true),
            new("StationMva",        40.0,  "40",     UnitCode.Mva,            true),
            new("StationZPercent",   20.0,  "20",     UnitCode.Percent,        true),
            new("MtVoltageKv",       13.2,  "13.2",   UnitCode.Kv,             true),
            new("MtCableLengthKm",    2.0,  "2",      UnitCode.Meter,          true),
            new("MtResistancePerKm", 0.7171,"0.7171", UnitCode.OhmPerKilometer,true),
            new("MtReactancePerKm",  0.3512,"0.3512", UnitCode.OhmPerKilometer,true),
            new("CH5",              null,   "SIM",    UnitCode.Unknown,        true)
        };

        var model = new NetworkModel(
            "NET_1", "1",
            new[] { trafo }, new[] { circuit },
            nodes, edges,
            Array.Empty<Branch>(),
            loads, conductors, parameters);

        return new ProjectVersion(
            "V1",
            isProj4 ? "PROJ_4" : "PROJ_7",
            isProj4 ? "CQT PROJ 4" : "CQT PROJ 7",
            model, "HASH_1", true);
    }

    // ── INotifyPropertyChanged ──
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
