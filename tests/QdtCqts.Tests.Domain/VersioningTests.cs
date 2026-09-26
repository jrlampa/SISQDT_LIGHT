using System.Text.RegularExpressions;
using QdtCqts.Domain;

namespace QdtCqts.Tests.Domain;

public sealed class VersioningTests
{
    [Fact]
    public void CodeBaselineHasIndependentVersionDimensions()
    {
        Assert.Matches(new Regex(@"^\d+\.\d+\.\d+$"), VersioningMetadata.CodeVersion);
        Assert.Equal("BASELINE-CODE-F12", VersioningMetadata.CodeBaseline);
        Assert.Equal("UNRECONCILED", VersioningMetadata.ElectricalBaselineVersion);
        Assert.Equal("1", VersioningMetadata.SchemaVersion);
    }

    [Fact]
    public void UnestablishedArtifactsAreNotPromoted()
    {
        Assert.Equal("NOT_ESTABLISHED", VersioningMetadata.GoldenDatasetVersion);
        Assert.Equal("NOT_ESTABLISHED", VersioningMetadata.ModelVersion);
        Assert.Equal("NOT_ESTABLISHED", VersioningMetadata.CalculationRunVersion);
    }

    [Fact]
    public void PrototypeExclusionHasAnExplicitVersion()
    {
        Assert.Equal("PROTOTYPE-EXCLUSION-1", VersioningMetadata.PrototypeExclusionPolicyVersion);
        Assert.Equal("PRECEDENCE-1-PROTOTYPE-EXCLUSION", VersioningMetadata.PrecedenceGraphVersion);
    }

    [Fact]
    public void ReconstructionVersionDoesNotChangeExecutableRuleset()
    {
        Assert.Equal("F14-RECON-1", VersioningMetadata.ReconstructionVersion);
        Assert.Equal("F9-RULESET-1", VersioningMetadata.RuleSetVersion);
    }
}
