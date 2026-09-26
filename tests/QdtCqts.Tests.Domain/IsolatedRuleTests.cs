using QdtCqts.Calculation.Cqts;
using QdtCqts.Calculation.Qdt;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Tests.Domain;

public sealed class IsolatedRuleTests
{
    private static readonly RuleExecutionContext Context = new("run-1", "GOLDEN-ISOLATED-001");

    [Fact]
    public void SelectKlUsesKWhenFlagIsSim()
    {
        var result = new SelectKlToM13Rule().Execute(Inputs(("CH5", "SIM", UnitCode.ConductorKey), ("K13", 10d, UnitCode.Kva), ("L13", 20d, UnitCode.Kva)), Context);
        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal(10d, result.OutputValue);
        Assert.Equal("QDT.LADO1.SELECT_KL_TO_M13", result.RuleId);
        Assert.NotEmpty(result.Trace);
    }

    [Fact]
    public void SelectKlUsesLForAnyNonSimValue()
    {
        var result = new SelectKlToM13Rule().Execute(Inputs(("CH5", "NAO", UnitCode.ConductorKey), ("K13", 10d, UnitCode.Kva), ("L13", 20d, UnitCode.Kva)), Context);
        Assert.Equal(20d, result.OutputValue);
    }

    [Fact]
    public void SelectKlPreservesObservedGoldenValue()
    {
        const double observed = 186.08282352941154;
        var result = new SelectKlToM13Rule().Execute(Inputs(("CH5", "SIM", UnitCode.ConductorKey), ("K13", observed, UnitCode.Kva), ("L13", observed, UnitCode.Kva)), Context);
        Assert.Equal(observed, result.OutputValue);
        Assert.Equal(UnitCode.Kva, result.OutputUnit);
    }

    [Fact]
    public void SelectKlBlocksMissingBranchInput()
    {
        var result = new SelectKlToM13Rule().Execute(Inputs(("CH5", "SIM", UnitCode.ConductorKey), ("K13", null, UnitCode.Kva), ("L13", 20d, UnitCode.Kva)), Context);
        Assert.Equal(CalculationStatus.Blocked, result.Status);
    }

    [Fact]
    public void ValidationReturnsExactOkText()
    {
        var result = new ValidationI13Rule().Execute(Inputs(("D13", 3d, UnitCode.Unknown), ("H13", 10d, UnitCode.Unknown), ("D14", 2d, UnitCode.Unknown), ("H9", 10d, UnitCode.Unknown)), Context);
        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal("OK !", result.OutputValue);
        Assert.Equal("QDT.LADO1.VALIDATION_I13", result.RuleId);
    }

    [Fact]
    public void ValidationReturnsExactErrorText()
    {
        var result = new ValidationI13Rule().Execute(Inputs(("D13", 1d, UnitCode.Unknown), ("H13", 10d, UnitCode.Unknown), ("D14", 2d, UnitCode.Unknown), ("H9", 10d, UnitCode.Unknown)), Context);
        Assert.Equal("Erro 02", result.OutputValue);
    }

    [Fact]
    public void ValidationReturnsBlankForBlankD13OrH13()
    {
        var result = new ValidationI13Rule().Execute(Inputs(("D13", null, UnitCode.Unknown), ("H13", 10d, UnitCode.Unknown)), Context);
        Assert.Equal(string.Empty, result.OutputValue);
    }

    [Fact]
    public void ProtectionAcceptsStrictlyValidRange()
    {
        var result = new IbInIzProtectionRule().Execute(Inputs(("Ib", 100d, UnitCode.Ampere), ("In", 160d, UnitCode.Ampere), ("Iz", 430d, UnitCode.Ampere)), Context);
        Assert.Equal("OK", result.OutputValue);
        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal(3, result.Trace.Count);
    }

    [Theory]
    [InlineData(160d, 160d, 160d, "OK")]
    [InlineData(200d, 160d, 430d, "VERIFICAR")]
    [InlineData(100d, 500d, 430d, "VERIFICAR")]
    public void ProtectionClassifiesRange(double ib, double current, double iz, string expected)
    {
        var result = new IbInIzProtectionRule().Execute(Inputs(("Ib", ib, UnitCode.Ampere), ("In", current, UnitCode.Ampere), ("Iz", iz, UnitCode.Ampere)), Context);
        Assert.Equal(expected, result.OutputValue);
    }

    [Fact]
    public void ProtectionBlocksMissingInputAndUnknownUnit()
    {
        var missing = new IbInIzProtectionRule().Execute(Inputs(("Ib", null, UnitCode.Ampere), ("In", 80d, UnitCode.Ampere), ("Iz", 355d, UnitCode.Ampere)), Context);
        var wrongUnit = new IbInIzProtectionRule().Execute(Inputs(("Ib", 10d, UnitCode.Kva), ("In", 80d, UnitCode.Ampere), ("Iz", 355d, UnitCode.Ampere)), Context);
        Assert.Equal(CalculationStatus.Blocked, missing.Status);
        Assert.Equal(CalculationStatus.Unknown, wrongUnit.Status);
    }

    [Fact]
    public void CandidateRamalImpedanceReproducesTwoCandidateWorkbooks()
    {
        var result = new CandidateRamalImpedanceRule().Execute(Inputs(("C11", 1.0903d, UnitCode.Ohm), ("C12", 0.4034d, UnitCode.Ohm)), Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal(1.13926612d, result.OutputValue);
        Assert.Equal(UnitCode.Ohm, result.OutputUnit);
        Assert.Equal("QDT.RAMAL.CANDIDATE_RX_COMBINATION", result.RuleId);
        Assert.Equal(2, result.Trace.Count);
    }

    [Fact]
    public void CandidateRamalImpedanceDoesNotAcceptUnknownUnits()
    {
        var result = new CandidateRamalImpedanceRule().Execute(Inputs(("C11", 1.0903d, UnitCode.Unknown), ("C12", 0.4034d, UnitCode.Ohm)), Context);

        Assert.Equal(CalculationStatus.Unknown, result.Status);
    }

    [Fact]
    public void CandidateRealProjectCableTemperatureReproducesBx13()
    {
        var result = new CandidateCableTemperatureRule().Execute(Inputs(
            ("M13", 74.448d, UnitCode.Kva),
            ("BX6", 220d, UnitCode.V),
            ("H13", 3d, UnitCode.ConductorKey),
            ("AN13", 430d, UnitCode.Ampere),
            ("AP13", 2d, UnitCode.Meter)), Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal(43.630837053053675d, (double)result.OutputValue!, 12);
        Assert.Equal(UnitCode.Celsius, result.OutputUnit);
        Assert.Equal("CQTS.REAL_PROJECT.CABLE_TEMPERATURE", result.RuleId);
    }

    [Fact]
    public void CandidateRealProjectCableTemperatureBlocksNonThreePhaseBranch()
    {
        var result = new CandidateCableTemperatureRule().Execute(Inputs(
            ("M13", 74.448d, UnitCode.Kva),
            ("BX6", 220d, UnitCode.V),
            ("H13", 1d, UnitCode.ConductorKey),
            ("AN13", 430d, UnitCode.Ampere),
            ("AP13", 2d, UnitCode.Meter)), Context);

        Assert.Equal(CalculationStatus.Blocked, result.Status);
    }

    [Fact]
    public void CandidateEndLoadSelectionReproducesProj7AndProj4()
    {
        var proj7 = new CandidateEndLoadSelectionRule().Execute(Inputs(
            ("D13", 47d, UnitCode.Factor),
            ("E13", 74.448d, UnitCode.Kva),
            ("G13", 1d, UnitCode.Factor),
            ("CH5", "SIM", UnitCode.Unknown)), Context);

        Assert.Equal(CalculationStatus.Pass, proj7.Status);
        Assert.Equal(74.448d, (double)proj7.OutputValue!);
        Assert.Equal(UnitCode.Kva, proj7.OutputUnit);
        Assert.Equal("CQTS.REAL_PROJECT.END_LOAD_SELECTION", proj7.RuleId);

        var proj4 = new CandidateEndLoadSelectionRule().Execute(Inputs(
            ("D13", 48d, UnitCode.Factor),
            ("E13", 75.68658823529411d, UnitCode.Kva),
            ("G13", 1d, UnitCode.Factor),
            ("CH5", "SIM", UnitCode.Unknown)), Context);

        Assert.Equal(CalculationStatus.Pass, proj4.Status);
        Assert.Equal(75.68658823529411d, (double)proj4.OutputValue!);
    }

    [Fact]
    public void CandidateEndLoadSelectionAppliesConsumerFloors()
    {
        var singleConsumer = new CandidateEndLoadSelectionRule().Execute(Inputs(
            ("D13", 1d, UnitCode.Factor),
            ("E13", 1.88d, UnitCode.Kva),
            ("G13", 1d, UnitCode.Factor),
            ("CH5", "SIM", UnitCode.Unknown)), Context);

        Assert.Equal(CalculationStatus.Pass, singleConsumer.Status);
        Assert.Equal(4.0d, (double)singleConsumer.OutputValue!);

        var twoConsumers = new CandidateEndLoadSelectionRule().Execute(Inputs(
            ("D13", 2d, UnitCode.Factor),
            ("E13", 3.0d, UnitCode.Kva),
            ("G13", 1d, UnitCode.Factor),
            ("CH5", "SIM", UnitCode.Unknown)), Context);

        Assert.Equal(CalculationStatus.Pass, twoConsumers.Status);
        Assert.Equal(8.0d, (double)twoConsumers.OutputValue!);
    }

    [Fact]
    public void CandidateEndLoadSelectionBlocksMissingOrInvalidInputs()
    {
        var missing = new CandidateEndLoadSelectionRule().Execute(Inputs(
            ("D13", null, UnitCode.Factor),
            ("E13", 74.448d, UnitCode.Kva),
            ("G13", 1d, UnitCode.Factor)), Context);

        Assert.Equal(CalculationStatus.Blocked, missing.Status);

        var wrongUnit = new CandidateEndLoadSelectionRule().Execute(Inputs(
            ("D13", 47d, UnitCode.Factor),
            ("E13", 74.448d, UnitCode.V),
            ("G13", 1d, UnitCode.Factor)), Context);

        Assert.Equal(CalculationStatus.Unknown, wrongUnit.Status);
    }

    [Fact]
    public void CandidateConsumerLoadAggregationComputesLocalNodeLoad()
    {
        var terminalGroups = new (int, double)[] { (1, 1.88), (3, 1.8048) };
        var terminalResult = new CandidateConsumerLoadAggregationRule().Execute(
            Inputs(("ConsumerGroups", terminalGroups, UnitCode.Kva)), Context);

        Assert.Equal(CalculationStatus.Pass, terminalResult.Status);
        Assert.Equal(7.2944d, (double)terminalResult.OutputValue!, 4);
        Assert.Equal(UnitCode.Kva, terminalResult.OutputUnit);
        Assert.Equal("CQTS.REAL_PROJECT.CONSUMER_LOAD_AGGREGATION", terminalResult.RuleId);

        var mixedGroups = new (int, double)[] { (6, 1.8048), (2, 1.4664) };
        var mixedResult = new CandidateConsumerLoadAggregationRule().Execute(
            Inputs(("ConsumerGroups", mixedGroups, UnitCode.Kva)), Context);

        Assert.Equal(CalculationStatus.Pass, mixedResult.Status);
        Assert.Equal(13.7616d, (double)mixedResult.OutputValue!, 4);
    }

    [Fact]
    public void CandidateRadialLoadAccumulationComputesLinearSegment()
    {
        var result = new CandidateRadialLoadAccumulationRule().Execute(Inputs(
            ("LocalLoadKva", 1.4664d, UnitCode.Kva),
            ("DownstreamBranchesLoadKva", 23.9888d, UnitCode.Kva),
            ("LocalConsumers", 1d, UnitCode.Unknown),
            ("DownstreamConsumers", 14d, UnitCode.Unknown)), Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal(25.4552d, (double)result.OutputValue!, 4);
        Assert.Equal(UnitCode.Kva, result.OutputUnit);
        Assert.Equal("CQTS.REAL_PROJECT.RADIAL_LOAD_ACCUMULATION", result.RuleId);
    }

    [Fact]
    public void CandidateRadialLoadAccumulationReproducesProj7BifurcationAtLid()
    {
        var branchLado1 = 53.2792d;
        var branchLado2 = 14.5136d;
        var localLidLoad = 6.6552d;

        var result = new CandidateRadialLoadAccumulationRule().Execute(Inputs(
            ("LocalLoadKva", localLidLoad, UnitCode.Kva),
            ("DownstreamBranchesLoadKva", new[] { branchLado1, branchLado2 }, UnitCode.Kva),
            ("LocalConsumers", 1d, UnitCode.Unknown),
            ("DownstreamConsumers", 46d, UnitCode.Unknown)), Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal(74.4480d, (double)result.OutputValue!, 4);
        Assert.Equal(UnitCode.Kva, result.OutputUnit);

        // Cadeia completa Fase 21: E13 -> M13 -> Cable Temperature
        var m13Result = new CandidateEndLoadSelectionRule().Execute(Inputs(
            ("D13", 47d, UnitCode.Factor),
            ("E13", (double)result.OutputValue!, UnitCode.Kva),
            ("G13", 1d, UnitCode.Factor),
            ("CH5", "SIM", UnitCode.Unknown)), Context);

        Assert.Equal(74.4480d, (double)m13Result.OutputValue!, 4);

        var tempResult = new CandidateCableTemperatureRule().Execute(Inputs(
            ("M13", (double)m13Result.OutputValue!, UnitCode.Kva),
            ("BX6", 220d, UnitCode.V),
            ("H13", 3d, UnitCode.ConductorKey),
            ("AN13", 430d, UnitCode.Ampere),
            ("AP13", 2d, UnitCode.Meter)), Context);

        Assert.Equal(43.630837053053675d, (double)tempResult.OutputValue!, 12);
    }

    [Fact]
    public void CandidateRadialLoadAccumulationReproducesProj4BifurcationAtLid()
    {
        var branchLado1 = 26.9216d;
        var branchLado2 = 41.9616d;
        var localLidLoad = 6.80338823529411d;

        var result = new CandidateRadialLoadAccumulationRule().Execute(Inputs(
            ("LocalLoadKva", localLidLoad, UnitCode.Kva),
            ("DownstreamBranchesLoadKva", new[] { branchLado1, branchLado2 }, UnitCode.Kva),
            ("LocalConsumers", 4d, UnitCode.Unknown),
            ("DownstreamConsumers", 44d, UnitCode.Unknown)), Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal(75.68658823529411d, (double)result.OutputValue!, 10);

        var m13Result = new CandidateEndLoadSelectionRule().Execute(Inputs(
            ("D13", 48d, UnitCode.Factor),
            ("E13", (double)result.OutputValue!, UnitCode.Kva),
            ("G13", 1d, UnitCode.Factor),
            ("CH5", "SIM", UnitCode.Unknown)), Context);

        Assert.Equal(75.68658823529411d, (double)m13Result.OutputValue!, 10);
    }

    private static IReadOnlyList<RuleInput> Inputs(params (string Name, object? Value, UnitCode Unit)[] values) =>
        values.Select(value => new RuleInput(value.Name, value.Value, value.Unit)).ToArray();
}
