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
/// Orquestrador de apresentação do diagrama unifilar, painel de detalhes, trace elétrico
/// e filtros visuais (Fases 27B, 27C e 27D).
/// Mantém as coleções visuais sincronizadas e gerencia a navegação e seleção de elementos.
/// </summary>
public sealed class UnifilarDiagramViewModel : INotifyPropertyChanged
{
    private readonly NetworkModel _model;
    private readonly TopologyValidationResult _topology;
    private readonly NetworkCalculationReport _report;

    // Estruturas indexadas para busca O(1) e navegação topológica sem alocações repetidas (27D-AUDIT-03)
    private readonly Dictionary<string, UnifilarNodeViewModel> _nodeVmById;
    private readonly Dictionary<string, UnifilarEdgeViewModel> _edgeVmById;
    private readonly Dictionary<string, UnifilarTransformerViewModel> _trafoVmById;
    private readonly Dictionary<string, UnifilarEdgeViewModel> _incomingEdgeByToNodeId;
    private readonly Dictionary<string, Node> _domainNodesById;
    private readonly Dictionary<string, Edge> _domainEdgesById;
    private readonly Dictionary<string, Transformer> _domainTrafosById;

    private object? _selectedElement;
    private SelectedElementDetailViewModel? _selectedDetail;
    private UnifilarVisualFilter _currentFilter = UnifilarVisualFilter.Todos;
    private double _voltageDropThresholdPercent = 5.0;

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

        // Monta índices O(1)
        _nodeVmById = Nodes.ToDictionary(n => n.NodeId, StringComparer.Ordinal);
        _edgeVmById = Edges.ToDictionary(e => e.EdgeId, StringComparer.Ordinal);
        _trafoVmById = Transformers.ToDictionary(t => t.TransformerId, StringComparer.Ordinal);
        _incomingEdgeByToNodeId = Edges.ToDictionary(e => e.ToNodeId, StringComparer.Ordinal);

        _domainNodesById = _model.Nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);
        _domainEdgesById = _model.Edges.ToDictionary(e => e.Id, StringComparer.Ordinal);
        _domainTrafosById = _model.Transformers.ToDictionary(t => t.Id, StringComparer.Ordinal);
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

    public UnifilarVisualFilter CurrentFilter
    {
        get => _currentFilter;
        set
        {
            if (SetField(ref _currentFilter, value))
            {
                ApplyVisualFilter();
            }
        }
    }

    /// <summary>
    /// Limite regulamentar / operacional configurável de queda de tensão em % (default 5.0%).
    /// Consumido pelo filtro de apresentação visual sem recalcular a física elétrica.
    /// </summary>
    public double VoltageDropThresholdPercent
    {
        get => _voltageDropThresholdPercent;
        set
        {
            if (SetField(ref _voltageDropThresholdPercent, value))
            {
                if (_currentFilter == UnifilarVisualFilter.QuedaTensaoLimite)
                {
                    ApplyVisualFilter();
                }
            }
        }
    }

    // ── Zoom & Pan (UX de Apresentação) ──
    private double _zoomLevel = 1.0;
    private double _panX;
    private double _panY;

    public double ZoomLevel
    {
        get => _zoomLevel;
        set => SetField(ref _zoomLevel, Math.Clamp(value, UnifilarVisualMetrics.MinZoom, UnifilarVisualMetrics.MaxZoom));
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
        if (Nodes.Count == 0 ||
            double.IsNaN(viewportWidth) || double.IsInfinity(viewportWidth) || viewportWidth <= 50 ||
            double.IsNaN(viewportHeight) || double.IsInfinity(viewportHeight) || viewportHeight <= 50)
        {
            ResetZoom();
            return;
        }

        // Bounding box rigoroso de todos os nós e transformadores com métricas centralizadas
        double minX = Nodes.Min(n => n.X);
        double maxX = Nodes.Max(n => n.X + UnifilarVisualMetrics.NodeWidth);
        double minY = Nodes.Min(n => n.Y);
        double maxY = Nodes.Max(n => n.Y + UnifilarVisualMetrics.NodeHeight);

        foreach (var t in Transformers)
        {
            minX = Math.Min(minX, t.X);
            maxX = Math.Max(maxX, t.X + UnifilarVisualMetrics.TransformerWidth);
            minY = Math.Min(minY, t.Y);
            maxY = Math.Max(maxY, t.Y + UnifilarVisualMetrics.TransformerHeight);
        }

        // Margem de respiro periférica
        minX -= UnifilarVisualMetrics.DefaultMargin;
        maxX += UnifilarVisualMetrics.DefaultMargin;
        minY -= UnifilarVisualMetrics.DefaultMargin;
        maxY += UnifilarVisualMetrics.DefaultMargin;

        double contentWidth = Math.Max(100.0, maxX - minX);
        double contentHeight = Math.Max(100.0, maxY - minY);

        double scaleX = viewportWidth / contentWidth;
        double scaleY = viewportHeight / contentHeight;
        double targetZoom = Math.Min(scaleX, scaleY) * UnifilarVisualMetrics.ViewportPaddingRatio;

        if (double.IsNaN(targetZoom) || double.IsInfinity(targetZoom))
        {
            ResetZoom();
            return;
        }

        ZoomLevel = Math.Clamp(targetZoom, UnifilarVisualMetrics.MinZoom, UnifilarVisualMetrics.FitToViewMaxZoom);

        // Centraliza a rede perfeitamente no viewport
        double scaledWidth = contentWidth * ZoomLevel;
        double scaledHeight = contentHeight * ZoomLevel;
        PanX = ((viewportWidth - scaledWidth) / 2.0) - (minX * ZoomLevel);
        PanY = ((viewportHeight - scaledHeight) / 2.0) - (minY * ZoomLevel);
    }

    public void SelectNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId) || !_nodeVmById.TryGetValue(nodeId, out var nodeVm))
        {
            ClearSelection();
            return;
        }

        // ZERO FALLBACKS SINTÉTICOS (27D-AUDIT-01): Se o ID não existe no NetworkModel,
        // não inventar entidade de domínio nem mascarar a inconsistência.
        if (!_domainNodesById.TryGetValue(nodeId, out var domainNode))
        {
            ClearSelection();
            return;
        }

        ClearSelectionFlags();
        nodeVm.IsSelected = true;
        SelectedElement = nodeVm;
        SelectedDetail = SelectedElementDetailViewModel.FromNode(nodeVm, domainNode, _report);

        ComputeAndApplyTrace(nodeId, UnifilarElementKind.Node);
        ApplyVisualFilter();
    }

    public void SelectEdge(string edgeId)
    {
        if (string.IsNullOrWhiteSpace(edgeId) || !_edgeVmById.TryGetValue(edgeId, out var edgeVm))
        {
            ClearSelection();
            return;
        }

        // ZERO FALLBACKS SINTÉTICOS (27D-AUDIT-01)
        if (!_domainEdgesById.TryGetValue(edgeId, out var domainEdge))
        {
            ClearSelection();
            return;
        }

        ClearSelectionFlags();
        edgeVm.IsSelected = true;
        SelectedElement = edgeVm;
        SelectedDetail = SelectedElementDetailViewModel.FromEdge(edgeVm, domainEdge, _report);

        ComputeAndApplyTrace(edgeId, UnifilarElementKind.Edge);
        ApplyVisualFilter();
    }

    public void SelectTransformer(string trafoId)
    {
        if (string.IsNullOrWhiteSpace(trafoId) || !_trafoVmById.TryGetValue(trafoId, out var trafoVm))
        {
            ClearSelection();
            return;
        }

        // ZERO FALLBACKS SINTÉTICOS (27D-AUDIT-01)
        if (!_domainTrafosById.TryGetValue(trafoId, out var domainTrafo))
        {
            ClearSelection();
            return;
        }

        ClearSelectionFlags();
        trafoVm.IsSelected = true;
        SelectedElement = trafoVm;
        SelectedDetail = SelectedElementDetailViewModel.FromTransformer(trafoVm, domainTrafo, _report);

        ComputeAndApplyTrace(trafoId, UnifilarElementKind.Transformer);
        ApplyVisualFilter();
    }

    public void ClearSelection()
    {
        ClearSelectionFlags();
        ClearTraceFlags();
        SelectedElement = null;
        SelectedDetail = null;
        ApplyVisualFilter();
    }

    private void ClearSelectionFlags()
    {
        foreach (var n in Nodes) n.IsSelected = false;
        foreach (var e in Edges) e.IsSelected = false;
        foreach (var t in Transformers) t.IsSelected = false;
    }

    private void ClearTraceFlags()
    {
        foreach (var n in Nodes) n.IsInTrace = false;
        foreach (var e in Edges) e.IsInTrace = false;
        foreach (var t in Transformers) t.IsInTrace = false;
    }

    /// <summary>
    /// Calcula e aplica o trace elétrico topológico determinístico da raiz até o elemento selecionado (Fase 27D-A).
    /// Complexidade O(profundidade da árvore). Respeita isolamento de bifurcações e não recalcula grandezas elétricas.
    /// </summary>
    private void ComputeAndApplyTrace(string elementId, UnifilarElementKind kind)
    {
        ClearTraceFlags();

        var traceNodeIds = new HashSet<string>(StringComparer.Ordinal);
        var traceEdgeIds = new HashSet<string>(StringComparer.Ordinal);
        var traceTrafoIds = new HashSet<string>(StringComparer.Ordinal);

        string? currentWalkNodeId = null;

        switch (kind)
        {
            case UnifilarElementKind.Node:
                traceNodeIds.Add(elementId);
                currentWalkNodeId = elementId;
                break;

            case UnifilarElementKind.Edge:
                if (_edgeVmById.TryGetValue(elementId, out var edgeVm))
                {
                    traceEdgeIds.Add(edgeVm.EdgeId);
                    traceNodeIds.Add(edgeVm.FromNodeId);
                    currentWalkNodeId = edgeVm.FromNodeId;
                }
                break;

            case UnifilarElementKind.Transformer:
                traceTrafoIds.Add(elementId);
                foreach (var rootId in _topology.RootNodeIds)
                {
                    traceNodeIds.Add(rootId);
                }
                break;
        }

        // Caminha a montante estritamente na topologia radial (da ponta para a raiz)
        while (!string.IsNullOrEmpty(currentWalkNodeId))
        {
            if (_topology.RootNodeIds.Contains(currentWalkNodeId) ||
                (_domainNodesById.TryGetValue(currentWalkNodeId, out var dn) && dn.IsSource))
            {
                // Atingiu a fonte: inclui os transformadores associados à alimentação
                foreach (var trafo in Transformers)
                {
                    traceTrafoIds.Add(trafo.TransformerId);
                }
                break;
            }

            if (_incomingEdgeByToNodeId.TryGetValue(currentWalkNodeId, out var incomingEdge))
            {
                traceEdgeIds.Add(incomingEdge.EdgeId);
                traceNodeIds.Add(incomingEdge.FromNodeId);
                currentWalkNodeId = incomingEdge.FromNodeId;
            }
            else
            {
                foreach (var trafo in Transformers)
                {
                    traceTrafoIds.Add(trafo.TransformerId);
                }
                break;
            }
        }

        foreach (var n in Nodes) n.IsInTrace = traceNodeIds.Contains(n.NodeId);
        foreach (var e in Edges) e.IsInTrace = traceEdgeIds.Contains(e.EdgeId);
        foreach (var t in Transformers) t.IsInTrace = traceTrafoIds.Contains(t.TransformerId);
    }

    /// <summary>
    /// Aplica o filtro visual ativo atenuando (IsDimmed = true) elementos não conformes (Fase 27D-B).
    /// Não reconstrói coleções nem altera dados calculados.
    /// </summary>
    public void ApplyVisualFilter()
    {
        switch (CurrentFilter)
        {
            case UnifilarVisualFilter.Todos:
                foreach (var n in Nodes) n.IsDimmed = false;
                foreach (var e in Edges) e.IsDimmed = false;
                foreach (var t in Transformers) t.IsDimmed = false;
                break;

            case UnifilarVisualFilter.Sobrecarga:
                foreach (var e in Edges) e.IsDimmed = !e.IsOverloaded;
                var overloadedNodes = new HashSet<string>(
                    Edges.Where(e => e.IsOverloaded).SelectMany(e => new[] { e.FromNodeId, e.ToNodeId }),
                    StringComparer.Ordinal);
                foreach (var n in Nodes) n.IsDimmed = !overloadedNodes.Contains(n.NodeId);
                foreach (var t in Transformers) t.IsDimmed = true;
                break;

            case UnifilarVisualFilter.QuedaTensaoLimite:
                foreach (var n in Nodes)
                    n.IsDimmed = n.AccumulatedVoltageDropPercent <= VoltageDropThresholdPercent;
                foreach (var e in Edges)
                    e.IsDimmed = e.SegmentVoltageDropPercent <= VoltageDropThresholdPercent;
                foreach (var t in Transformers)
                    t.IsDimmed = t.TotalOriginVoltageDropPercent <= VoltageDropThresholdPercent;
                break;

            case UnifilarVisualFilter.EvidenceBlocked:
                bool isBlocked = _report.Protection?.AssessmentStatus == ProtectionAssessmentStatus.EvidenceBlocked;
                foreach (var n in Nodes) n.IsDimmed = !isBlocked;
                foreach (var e in Edges) e.IsDimmed = !isBlocked;
                foreach (var t in Transformers) t.IsDimmed = !isBlocked;
                break;

            case UnifilarVisualFilter.Selecionados:
                foreach (var n in Nodes) n.IsDimmed = !n.IsSelected;
                foreach (var e in Edges) e.IsDimmed = !e.IsSelected;
                foreach (var t in Transformers) t.IsDimmed = !t.IsSelected;
                break;

            case UnifilarVisualFilter.Trace:
                foreach (var n in Nodes) n.IsDimmed = !n.IsInTrace;
                foreach (var e in Edges) e.IsDimmed = !e.IsInTrace;
                foreach (var t in Transformers) t.IsDimmed = !t.IsInTrace;
                break;
        }
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
