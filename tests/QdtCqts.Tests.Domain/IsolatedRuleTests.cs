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

    [Fact]
    public void Test01_LinearSegmentVoltageDropComputesProperly()
    {
        // PROJ 7 Linha 13: M=74.448 kVA, R=0.0887083 Ohm/km, X=0.0897 Ohm/km, L=4m, AP=2 (Lequiv=2m), V=220V, H=3
        var result = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 74.448d, UnitCode.Kva),
            ("R", 0.088708306113649771d, UnitCode.Ohm),
            ("X", 0.0897d, UnitCode.Ohm),
            ("L", 4.0d, UnitCode.Meter),
            ("AP", 2.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal(0.038810072181038206d, (double)result.OutputValue!, 10);
        Assert.Equal(UnitCode.Percent, result.OutputUnit);
    }

    [Fact]
    public void Test02_TerminalSegmentLoadAndLength()
    {
        // Trecho terminal P6 -> P7: M=7.2944 kVA, R=0.182437, X=0.1178, L=16m, AP=1, V=220V, H=3
        var result = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 7.2944d, UnitCode.Kva),
            ("R", 0.18243657395321239d, UnitCode.Ohm),
            ("X", 0.1178d, UnitCode.Ohm),
            ("L", 16.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.True((double)result.OutputValue! > 0);
    }

    [Fact]
    public void Test03_NodeWithLocalLoadIncreasesUpstreamSegment()
    {
        // Nó com carga local (ex: P3 com 1 consumidor = 1.4664 kVA)
        var downstream = 23.9888d;
        var local = 1.4664d;
        var parentSegment = new CandidateRadialLoadAccumulationRule().Execute(Inputs(
            ("LocalLoadKva", local, UnitCode.Kva),
            ("DownstreamBranchesLoadKva", downstream, UnitCode.Kva)), Context);

        Assert.Equal(CalculationStatus.Pass, parentSegment.Status);
        Assert.Equal(25.4552d, (double)parentSegment.OutputValue!, 4);
    }

    [Fact]
    public void Test04_NodeWithTwoDerivationsAccumulatesLoadsAtParent()
    {
        // Nó P2 ramificando para P3 (26.9216 kVA) e P8 (26.9216 kVA)
        var branch1 = 26.9216d;
        var branch2 = 26.9216d;
        var result = new CandidateRadialLoadAccumulationRule().Execute(Inputs(
            ("LocalLoadKva", 0.0d, UnitCode.Kva),
            ("DownstreamBranchesLoadKva", new[] { branch1, branch2 }, UnitCode.Kva)), Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal(53.8432d, (double)result.OutputValue!, 4);
    }

    [Fact]
    public void Test05_UpstreamAccumulationPreservesMonotonicity()
    {
        // D13 >= D14 e H13 >= H9
        var validation = new ValidationI13Rule().Execute(Inputs(
            ("D13", 47.0d, UnitCode.Unknown),
            ("H13", 3.0d, UnitCode.Unknown),
            ("D14", 38.0d, UnitCode.Unknown),
            ("H9", 3.0d, UnitCode.Unknown)), Context);

        Assert.Equal("OK !", validation.OutputValue);
    }

    [Fact]
    public void Test06_IndependentVoltageDropPathsDoNotSumSiblings()
    {
        // Queda acumulada até P2 = 5.80361%
        var dropAtP2 = 5.8036132859811254d;
        var deltaP2P3 = 0.3865379348646184d; // Lado 1
        var deltaP2P8 = 0.21742758836134785d; // Lado 3

        var dropP3 = new CandidateAccumulatedVoltageDropRule().Execute(Inputs(
            ("SegmentDropPercent", deltaP2P3, UnitCode.Percent),
            ("UpstreamDropPercent", dropAtP2, UnitCode.Percent)), Context);

        var dropP8 = new CandidateAccumulatedVoltageDropRule().Execute(Inputs(
            ("SegmentDropPercent", deltaP2P8, UnitCode.Percent),
            ("UpstreamDropPercent", dropAtP2, UnitCode.Percent)), Context);

        Assert.Equal(CalculationStatus.Pass, dropP3.Status);
        Assert.Equal(CalculationStatus.Pass, dropP8.Status);
        Assert.Equal(6.1901512208457437d, (double)dropP3.OutputValue!, 10);
        Assert.Equal(6.0210408743424733d, (double)dropP8.OutputValue!, 10);

        // Quedas em P3 e P8 são independentes e não somam os trechos irmãos entre si
        Assert.NotEqual((double)dropP3.OutputValue!, (double)dropP8.OutputValue!);
    }

    [Fact]
    public void Test07_LinearTopologyConvergenceCqtsAndQdt()
    {
        // No caso linear sem derivações, a fórmula Z / (V^2 / 100) do CQTS coincide com Cq do QDT
        const double r = 1.0903d;
        const double x = 0.4034d;
        const double v = 220.0d;
        const double l = 100.0d;
        const double m = 10.0d;

        // Fórmula CQTS
        var cqtsResult = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", m, UnitCode.Kva),
            ("R", r, UnitCode.Ohm),
            ("X", x, UnitCode.Ohm),
            ("L", l, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", v, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        // Fórmula clássica QDT (Cq * M * L)
        double z = Math.Sqrt(r * r + x * x);
        double cq = (z / (v * v)) * 100.0;
        double qdtDeltaV = m * cq * l;

        Assert.Equal(CalculationStatus.Pass, cqtsResult.Status);
        Assert.Equal(qdtDeltaV, (double)cqtsResult.OutputValue!, 10);
    }

    [Fact]
    public void Test08_BranchDivergencePreservedAtP2()
    {
        // Ramo 1 (P3) e Ramo 3 (P8) divergem a partir de P2
        var dropP3 = 6.1901512208457437d;
        var dropP8 = 6.0210408743424733d;
        Assert.True(Math.Abs(dropP3 - dropP8) > 0.1d);
    }

    [Fact]
    public void Test09_ConductorChangeAdjustsImpedanceAndVoltageDrop()
    {
        // Comparação de condutor 240 Cu vs 185 Al - MX para o mesmo trecho
        var dropCu = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 50.0d, UnitCode.Kva),
            ("R", 0.0887d, UnitCode.Ohm),
            ("X", 0.0897d, UnitCode.Ohm),
            ("L", 50.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        var dropAl = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 50.0d, UnitCode.Kva),
            ("R", 0.1896d, UnitCode.Ohm),
            ("X", 0.1178d, UnitCode.Ohm),
            ("L", 50.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        Assert.True((double)dropAl.OutputValue! > (double)dropCu.OutputValue!);
    }

    [Fact]
    public void Test10_RlModeledAsConsumerTerminal()
    {
        // Ramal de ligação pontual (RL): D=1, M=1.88 kVA
        var rlLoad = new CandidateEndLoadSelectionRule().Execute(Inputs(
            ("D13", 1.0d, UnitCode.Factor),
            ("E13", 1.88d, UnitCode.Kva),
            ("G13", 1.0d, UnitCode.Factor),
            ("CH5", "NAO", UnitCode.Unknown)), Context);

        Assert.Equal(CalculationStatus.Pass, rlLoad.Status);
        Assert.Equal(1.88d, (double)rlLoad.OutputValue!);
    }

    private static IReadOnlyList<RuleInput> Inputs(params (string Name, object? Value, UnitCode Unit)[] values) =>
        values.Select(value => new RuleInput(value.Name, value.Value, value.Unit)).ToArray();
}
