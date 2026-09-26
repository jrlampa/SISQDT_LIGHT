using QdtCqts.Domain;

namespace QdtCqts.Infrastructure.Parity;

public enum ComparisonMode { ExactText, ExactStatus, ExactStructure, ExactNumeric, Tolerance, ExactUnit }
public enum ComparisonState { Pass, Fail, Unknown, Blocked }

public sealed record GoldenObservationValue(string Path, string? ExpectedText, double? ExpectedNumeric, UnitCode Unit, ComparisonMode Mode, double? AbsoluteTolerance, double? RelativeTolerance, bool EvidenceComplete);
public sealed record ComparisonResult(string Path, ComparisonState State, string? ExcelValue, string? AppValue, string Reason);

public sealed class ParityComparator
{
    public ComparisonResult Compare(GoldenObservationValue expected, string? actualText, double? actualNumeric, UnitCode actualUnit)
    {
        if (!expected.EvidenceComplete) return new(expected.Path, ComparisonState.Blocked, expected.ExpectedText, actualText, "Evidence chain is incomplete.");
        if (expected.Unit == UnitCode.Unknown || actualUnit == UnitCode.Unknown) return new(expected.Path, ComparisonState.Unknown, expected.ExpectedText, actualText, "Unit is unknown.");
        if (expected.Unit != actualUnit) return new(expected.Path, ComparisonState.Fail, expected.ExpectedText, actualText, "Units differ.");
        return expected.Mode switch
        {
            ComparisonMode.ExactText or ComparisonMode.ExactStatus or ComparisonMode.ExactStructure =>
                string.Equals(expected.ExpectedText, actualText, StringComparison.Ordinal) ? new(expected.Path, ComparisonState.Pass, expected.ExpectedText, actualText, "Exact match.") : new(expected.Path, ComparisonState.Fail, expected.ExpectedText, actualText, "Text differs."),
            ComparisonMode.ExactNumeric => expected.ExpectedNumeric == actualNumeric ? new(expected.Path, ComparisonState.Pass, expected.ExpectedNumeric?.ToString("R"), actualNumeric?.ToString("R"), "Exact numeric match.") : new(expected.Path, ComparisonState.Fail, expected.ExpectedNumeric?.ToString("R"), actualNumeric?.ToString("R"), "Numeric value differs."),
            ComparisonMode.Tolerance => expected.AbsoluteTolerance is null && expected.RelativeTolerance is null
                ? new(expected.Path, ComparisonState.Unknown, expected.ExpectedNumeric?.ToString("R"), actualNumeric?.ToString("R"), "Tolerance is undefined.")
                : CompareTolerance(expected, actualNumeric),
            _ => new(expected.Path, ComparisonState.Unknown, expected.ExpectedText, actualText, "Comparison mode is unsupported.")
        };
    }

    private static ComparisonResult CompareTolerance(GoldenObservationValue expected, double? actualNumeric)
    {
        if (expected.ExpectedNumeric is null || actualNumeric is null) return new(expected.Path, ComparisonState.Unknown, expected.ExpectedNumeric?.ToString("R"), actualNumeric?.ToString("R"), "Numeric value is missing.");
        var absoluteDelta = Math.Abs(expected.ExpectedNumeric.Value - actualNumeric.Value);
        var relativeLimit = expected.RelativeTolerance is null ? double.PositiveInfinity : expected.RelativeTolerance.Value * Math.Max(1, Math.Abs(expected.ExpectedNumeric.Value));
        var limit = Math.Min(expected.AbsoluteTolerance ?? double.PositiveInfinity, relativeLimit);
        return absoluteDelta <= limit
            ? new(expected.Path, ComparisonState.Pass, expected.ExpectedNumeric.Value.ToString("R"), actualNumeric.Value.ToString("R"), "Within tolerance.")
            : new(expected.Path, ComparisonState.Fail, expected.ExpectedNumeric.Value.ToString("R"), actualNumeric.Value.ToString("R"), $"Delta {absoluteDelta:R} exceeds {limit:R}.");
    }
}
