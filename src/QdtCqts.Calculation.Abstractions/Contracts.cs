using QdtCqts.Domain;

namespace QdtCqts.Calculation.Abstractions;

public sealed record CalculationRequest(
    ProjectVersion ProjectVersion,
    CalculationMode Mode,
    string AlgorithmVersion,
    string InputHash);

public sealed record CalculationDiagnostic(string Code, string Message, IReadOnlyList<SourceRef> Sources);

public sealed record CalculationResult(
    string RunId,
    CalculationMode Mode,
    string AlgorithmVersion,
    string InputHash,
    CalculationStatus Status,
    IReadOnlyList<CalculationDiagnostic> Diagnostics,
    NetworkCalculationReport? Report = null)
{
    public static CalculationResult Blocked(CalculationRequest request, string code, string message) =>
        new(Guid.NewGuid().ToString("N"), request.Mode, request.AlgorithmVersion, request.InputHash,
            CalculationStatus.Blocked, new[] { new CalculationDiagnostic(code, message, Array.Empty<SourceRef>()) });

    public static CalculationResult Succeeded(
        CalculationRequest request,
        NetworkCalculationReport report,
        IReadOnlyList<CalculationDiagnostic>? diagnostics = null) =>
        new(report.RunId, request.Mode, request.AlgorithmVersion, request.InputHash,
            CalculationStatus.Pass, diagnostics ?? Array.Empty<CalculationDiagnostic>(), report);
}

public interface ICalculationEngine
{
    CalculationMode Mode { get; }
    CalculationResult Calculate(CalculationRequest request);
}
