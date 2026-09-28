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
