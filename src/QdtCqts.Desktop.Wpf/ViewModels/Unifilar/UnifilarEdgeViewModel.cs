using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Modelo de apresentação para um trecho (aresta) no diagrama unifilar (Fase 27B).
/// Projeta Edge e SegmentCalculationResult sem duplicar cálculo elétrico.
/// </summary>
public sealed class UnifilarEdgeViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private double _x1;
    private double _y1;
    private double _x2;
    private double _y2;

    public string EdgeId { get; init; } = string.Empty;
    public string CircuitId { get; init; } = string.Empty;
    public string FromNodeId { get; init; } = string.Empty;
    public string ToNodeId { get; init; } = string.Empty;
    public string? ConductorKey { get; init; }
    public int PhaseCount { get; init; }
    public double ParallelCables { get; init; }
    public double PhysicalLengthMeters { get; init; }
    public double EquivalentLengthMeters { get; init; }

    public double DownstreamAccumulatedConsumers { get; init; }
    public double DownstreamAccumulatedLoadKva { get; init; }
    public double DiversityFactor { get; init; }
    public double EndLoadSelectedKva { get; init; }

    public double OperatingCurrentAmperes { get; init; }
    public double RatedAmpacityAmperes { get; init; }

    /// <summary>Condição de sobrecarga vinda diretamente do cálculo do backend.</summary>
    public bool IsOverloaded { get; init; }

    /// <summary>Status de apresentação textual baseado estritamente no IsOverloaded do backend.</summary>
    public string OverloadStatus => IsOverloaded ? "SOBRECARGA" : "OK";

    public double OperatingTemperatureCelsius { get; init; }
    public double ResistanceCaOhmPerKm { get; init; }
    public double ReactanceOhmPerKm { get; init; }
    public double ImpedanceOhmPerKm { get; init; }
    public double FactorBj { get; init; }
    public double PhaseFactor { get; init; }
    public double SegmentVoltageDropPercent { get; init; }

    public double ShortCircuit3PhaseAmperes { get; init; }
    public double ShortCircuit1PhaseAmperes { get; init; }
    public double UpstreamResistanceOhm { get; init; }
    public double UpstreamReactanceOhm { get; init; }
    public string RunId { get; init; } = string.Empty;

    public double X1
    {
        get => _x1;
        set => SetField(ref _x1, value);
    }

    public double Y1
    {
        get => _y1;
        set => SetField(ref _y1, value);
    }

    public double X2
    {
        get => _x2;
        set => SetField(ref _x2, value);
    }

    public double Y2
    {
        get => _y2;
        set => SetField(ref _y2, value);
    }

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
