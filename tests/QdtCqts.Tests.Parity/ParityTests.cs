using QdtCqts.Domain;
using QdtCqts.Infrastructure.Parity;

namespace QdtCqts.Tests.Parity;

public sealed class ParityTests
{
    [Fact]
    public void IncompleteEvidenceIsBlocked()
    {
        var expected = new GoldenObservationValue("LADO1!K13", "186.08282352941154", null, UnitCode.Kva, ComparisonMode.ExactText, null, null, false);
        var result = new ParityComparator().Compare(expected, "186.08282352941154", null, UnitCode.Kva);
        Assert.Equal(ComparisonState.Blocked, result.State);
    }

    [Fact]
    public void UndefinedToleranceIsUnknown()
    {
        var expected = new GoldenObservationValue("LADO1!K13", null, 186.0, UnitCode.Kva, ComparisonMode.Tolerance, null, null, true);
        var result = new ParityComparator().Compare(expected, null, 186.0, UnitCode.Kva);
        Assert.Equal(ComparisonState.Unknown, result.State);
    }

    [Fact]
    public void UnitsMustMatchExactly()
    {
        var expected = new GoldenObservationValue("LADO1!K13", null, 186.0, UnitCode.Kva, ComparisonMode.ExactNumeric, null, null, true);
        var result = new ParityComparator().Compare(expected, null, 186.0, UnitCode.Ampere);
        Assert.Equal(ComparisonState.Fail, result.State);
    }
}
