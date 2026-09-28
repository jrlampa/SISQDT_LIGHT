using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Modelo de apresentação para um nó no diagrama unifilar (Fase 27B).
/// Projeta estritamente Node e NodeCalculationResult sem recalcular grandezas elétricas.
/// </summary>
public sealed class UnifilarNodeViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private double _x;
    private double _y;

    public string NodeId { get; init; } = string.Empty;
    public string ExternalKey { get; init; } = string.Empty;
    public string CircuitId { get; init; } = string.Empty;
    public UnifilarNodeType NodeType { get; init; } = UnifilarNodeType.PassThrough;

    public double X
    {
        get => _x;
        set => SetField(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => SetField(ref _y, value);
    }

    public int LocalConsumers { get; init; }
    public double LocalLoadKva { get; init; }
    public double AccumulatedVoltageDropPercent { get; init; }
    /// <summary>Tensão fase-neutro no nó (V) — projetada do DTO, não calculada aqui.</summary>
    public double VoltageV127 { get; init; }
    /// <summary>Tensão fase-fase no nó (V) — projetada do DTO, não calculada aqui.</summary>
    public double VoltageV220 { get; init; }
    public string RunId { get; init; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    private bool _isInTrace;
    public bool IsInTrace
    {
        get => _isInTrace;
        set => SetField(ref _isInTrace, value);
    }

    private bool _isDimmed;
    public bool IsDimmed
    {
        get => _isDimmed;
        set => SetField(ref _isDimmed, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
