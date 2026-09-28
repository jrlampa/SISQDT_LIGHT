using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Orquestrador de apresentação do diagrama unifilar e painel de detalhes (Fase 27B).
/// Mantém as coleções visuais sincronizadas e gerencia a seleção de elementos.
/// </summary>
public sealed class UnifilarDiagramViewModel : INotifyPropertyChanged
{
    private readonly NetworkModel _model;
    private readonly TopologyValidationResult _topology;
    private readonly NetworkCalculationReport _report;

    private object? _selectedElement;
    private SelectedElementDetailViewModel? _selectedDetail;

    public UnifilarDiagramViewModel(
        NetworkModel model,
        TopologyValidationResult topology,
        NetworkCalculationReport report,
        IEnumerable<UnifilarNodeViewModel> nodes,
        IEnumerable<UnifilarEdgeViewModel> edges,
        IEnumerable<UnifilarTransformerViewModel> transformers,
        CalculationUiState uiState = CalculationUiState.Success)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _topology = topology ?? throw new ArgumentNullException(nameof(topology));
        _report = report ?? throw new ArgumentNullException(nameof(report));

        Nodes = new ObservableCollection<UnifilarNodeViewModel>(nodes);
        Edges = new ObservableCollection<UnifilarEdgeViewModel>(edges);
        Transformers = new ObservableCollection<UnifilarTransformerViewModel>(transformers);
        UiState = uiState;
    }

    public ObservableCollection<UnifilarNodeViewModel> Nodes { get; }
    public ObservableCollection<UnifilarEdgeViewModel> Edges { get; }
    public ObservableCollection<UnifilarTransformerViewModel> Transformers { get; }

    public CalculationUiState UiState { get; }
    public string RunId => _report.RunId;
    public string AlgorithmVersion => _report.AlgorithmVersion;
    public string InputHash => _report.InputHash;
    public string OutputHash => _report.OutputHash;

    public object? SelectedElement
    {
        get => _selectedElement;
        private set => SetField(ref _selectedElement, value);
    }

    public SelectedElementDetailViewModel? SelectedDetail
    {
        get => _selectedDetail;
        private set => SetField(ref _selectedDetail, value);
    }

    // ── Zoom & Pan (UX de Apresentação) ──
    private double _zoomLevel = 1.0;
    private double _panX;
    private double _panY;

    public double ZoomLevel
    {
        get => _zoomLevel;
        set => SetField(ref _zoomLevel, Math.Clamp(value, 0.25, 4.0));
    }

    public double PanX
    {
        get => _panX;
        set => SetField(ref _panX, value);
    }

    public double PanY
    {
        get => _panY;
        set => SetField(ref _panY, value);
    }

    public void ZoomIn() => ZoomLevel = Math.Round(ZoomLevel + 0.15, 2);
    public void ZoomOut() => ZoomLevel = Math.Round(ZoomLevel - 0.15, 2);
    public void ResetZoom()
    {
        ZoomLevel = 1.0;
        PanX = 0;
        PanY = 0;
    }

    public void FitToView(double viewportWidth, double viewportHeight)
    {
        if (Nodes.Count == 0 || viewportWidth <= 50 || viewportHeight <= 50)
        {
            ResetZoom();
            return;
        }

        // Bounding box de todos os elementos renderizados (Nodes, Transformers)
        double minX = Nodes.Min(n => n.X);
        double maxX = Nodes.Max(n => n.X);
        double minY = Nodes.Min(n => n.Y);
        double maxY = Nodes.Max(n => n.Y);

        foreach (var t in Transformers)
        {
            minX = Math.Min(minX, t.X);
            maxX = Math.Max(maxX, t.X);
            minY = Math.Min(minY, t.Y);
            maxY = Math.Max(maxY, t.Y);
        }

        // Considera tamanho aproximado dos blocos (largura ~140, altura ~80)
        minX -= 40;
        maxX += 150;
        minY -= 40;
        maxY += 90;

        double contentWidth = Math.Max(100, maxX - minX);
        double contentHeight = Math.Max(100, maxY - minY);

        double scaleX = viewportWidth / contentWidth;
        double scaleY = viewportHeight / contentHeight;
        double targetZoom = Math.Min(scaleX, scaleY) * 0.92; // 8% padding

        ZoomLevel = Math.Clamp(targetZoom, 0.25, 2.5);

        // Centraliza
        double scaledWidth = contentWidth * ZoomLevel;
        double scaledHeight = contentHeight * ZoomLevel;
        PanX = ((viewportWidth - scaledWidth) / 2.0) - (minX * ZoomLevel);
        PanY = ((viewportHeight - scaledHeight) / 2.0) - (minY * ZoomLevel);
    }

    public void SelectNode(string nodeId)
    {
        var nodeVm = Nodes.FirstOrDefault(n => n.NodeId == nodeId);
        if (nodeVm == null) return;

        ClearSelectionFlags();
        nodeVm.IsSelected = true;
        SelectedElement = nodeVm;

        var domainNode = _model.Nodes.FirstOrDefault(n => n.Id == nodeId)
                         ?? new Node(nodeId, nodeVm.ExternalKey, nodeVm.CircuitId, nodeVm.X, nodeVm.Y, nodeVm.NodeType == UnifilarNodeType.Source);

        SelectedDetail = SelectedElementDetailViewModel.FromNode(nodeVm, domainNode, _report);
    }

    public void SelectEdge(string edgeId)
    {
        var edgeVm = Edges.FirstOrDefault(e => e.EdgeId == edgeId);
        if (edgeVm == null) return;

        ClearSelectionFlags();
        edgeVm.IsSelected = true;
        SelectedElement = edgeVm;

        var domainEdge = _model.Edges.FirstOrDefault(e => e.Id == edgeId)
                         ?? new Edge(edgeId, edgeId, edgeVm.CircuitId, edgeVm.FromNodeId, edgeVm.ToNodeId, new UnitValue(edgeVm.PhysicalLengthMeters, UnitCode.Meter), edgeVm.ConductorKey, edgeVm.PhaseCount.ToString(), edgeVm.ParallelCables.ToString());

        SelectedDetail = SelectedElementDetailViewModel.FromEdge(edgeVm, domainEdge, _report);
    }

    public void SelectTransformer(string trafoId)
    {
        var trafoVm = Transformers.FirstOrDefault(t => t.TransformerId == trafoId);
        if (trafoVm == null) return;

        ClearSelectionFlags();
        trafoVm.IsSelected = true;
        SelectedElement = trafoVm;

        var domainTrafo = _model.Transformers.FirstOrDefault(t => t.Id == trafoId)
                          ?? new Transformer(trafoId, trafoVm.ExternalKey, new UnitValue(trafoVm.NominalPowerKva, UnitCode.Kva), new UnitValue(trafoVm.ImpedancePercent, UnitCode.Percent), new UnitValue(220, UnitCode.V), new UnitValue(trafoVm.OperatingLoadKva, UnitCode.Kva));

        SelectedDetail = SelectedElementDetailViewModel.FromTransformer(trafoVm, domainTrafo, _report);
    }

    public void ClearSelection()
    {
        ClearSelectionFlags();
        SelectedElement = null;
        SelectedDetail = null;
    }

    private void ClearSelectionFlags()
    {
        foreach (var n in Nodes) n.IsSelected = false;
        foreach (var e in Edges) e.IsSelected = false;
        foreach (var t in Transformers) t.IsSelected = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
