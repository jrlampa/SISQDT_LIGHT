using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Modelo de apresentação para um transformador no diagrama unifilar (Fase 27B).
/// Projeta Transformer e TransformerCalculationResult.
/// </summary>
public sealed class UnifilarTransformerViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private double _x;
    private double _y;

    public string TransformerId { get; init; } = string.Empty;
    public string ExternalKey { get; init; } = string.Empty;
    public double NominalPowerKva { get; init; }
    public double OperatingLoadKva { get; init; }
    public double ImpedancePercent { get; init; }
    public double TrafoVoltageDropPercent { get; init; }
    public double MtVoltageDropPercent { get; init; }
    public double TotalOriginVoltageDropPercent { get; init; }
    public string RunId { get; init; } = string.Empty;

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

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
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
