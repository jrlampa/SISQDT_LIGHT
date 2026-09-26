using QdtCqts.Calculation.Abstractions;
using QdtCqts.Calculation.Cqts;
using QdtCqts.Domain;
using Xunit;

namespace QdtCqts.Tests.Domain;

/// <summary>
/// Testes de auditoria e paridade matemática estrita para a Fase 22.1.
/// Validação direta contra células e fórmulas do CQT PROJ 7 REV2 e CQT PROJ 4 REV1.
/// </summary>
public sealed class Fase22_1ParityTests
{
    private static readonly RuleExecutionContext Context = new("EXEC-AUDIT-F22.1", "GC-F22.1");

    [Fact]
    public void Caso01_TrToLid_Proj7_FullParity()
    {
        // CQT PROJ 7 REV2 Linha 13: TR7 -> LID
        // M13 = 74.448 kVA, AP13 = 2, AQ13 = 4 m (Lequiv = 2 m), H = 3, V = 220 V
        // BI13 (Rca) = 0.088708306113649771 Ohm/km, BB13 (X) = 0.0897 Ohm/km
        const double m = 74.448d;
        const double r = 0.088708306113649771d;
        const double x = 0.0897d;
        const double l = 4.0d;
        const double ap = 2.0d;
        const double v = 220.0d;
        const double h = 3.0d;

        var segmentRule = new CandidateSegmentVoltageDropRule();
        var bzResult = segmentRule.Execute(Inputs(
            ("M", m, UnitCode.Kva),
            ("R", r, UnitCode.Ohm),
            ("X", x, UnitCode.Ohm),
            ("L", l, UnitCode.Meter),
            ("AP", ap, UnitCode.Unknown),
            ("V", v, UnitCode.V),
            ("H", h, UnitCode.ConductorKey)), Context);

        Assert.Equal(CalculationStatus.Pass, bzResult.Status);
        Assert.Equal(0.038810072181038206d, (double)bzResult.OutputValue!, 12);

        // Queda do Transformador (BV4 = (74.448 / 112.5) * 3.5%)
        var trafoRule = new CandidateTransformerVoltageDropRule();
        var trafoResult = trafoRule.Execute(Inputs(
            ("TrafoLoadKva", 74.448d, UnitCode.Kva),
            ("TrafoNominalKva", 112.5d, UnitCode.Kva),
            ("TrafoImpedancePercent", 3.5d, UnitCode.Percent)), Context);

        Assert.Equal(CalculationStatus.Pass, trafoResult.Status);
        Assert.Equal(2.3161599999999996d, (double)trafoResult.OutputValue!, 12);

        // Queda MT (CV105 = 1.8330629395275368%)
        const double mtDrop = 1.8330629395275368d;

        // Queda Acumulada em LID (CA13 = BZ13 + BV4 + CV105)
        var caRule = new CandidateAccumulatedVoltageDropRule();
        var caResult = caRule.Execute(Inputs(
            ("BZ", (double)bzResult.OutputValue!, UnitCode.Percent),
            ("BV4", (double)trafoResult.OutputValue!, UnitCode.Percent),
            ("CV105", mtDrop, UnitCode.Percent)), Context);

        Assert.Equal(CalculationStatus.Pass, caResult.Status);
        Assert.Equal(4.1880330117085744d, (double)caResult.OutputValue!, 12);
    }

    [Fact]
    public void Caso02_P1ToP2_Proj7_Parity()
    {
        // CQT PROJ 7 Linha 15: P1 -> P2
        // M15 = 51.0232 kVA, AP15 = 1, AQ15 = 32 m, H = 3, V = 220 V
        // BI15 = 0.1896577016501888 Ohm/km, BB15 = 0.1178 Ohm/km
        const double m = 51.023199999999996d;
        const double r = 0.1896577016501888d;
        const double x = 0.1178d;
        const double l = 32.0d;
        const double ap = 1.0d;
        const double v = 220.0d;
        const double h = 3.0d;

        var bzResult = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", m, UnitCode.Kva),
            ("R", r, UnitCode.Ohm),
            ("X", x, UnitCode.Ohm),
            ("L", l, UnitCode.Meter),
            ("AP", ap, UnitCode.Unknown),
            ("V", v, UnitCode.V),
            ("H", h, UnitCode.ConductorKey)), Context);

        Assert.Equal(CalculationStatus.Pass, bzResult.Status);
        Assert.Equal(0.7531670568457773d, (double)bzResult.OutputValue!, 12);

        // CA14 = 5.050446229135348d
        const double ca14 = 5.050446229135348d;
        var caResult = new CandidateAccumulatedVoltageDropRule().Execute(Inputs(
            ("BZ", (double)bzResult.OutputValue!, UnitCode.Percent),
            ("CA_Parent", ca14, UnitCode.Percent)), Context);

        Assert.Equal(CalculationStatus.Pass, caResult.Status);
        Assert.Equal(5.803613285981125d, (double)caResult.OutputValue!, 12);
    }

    [Fact]
    public void Caso03_Proj4_TrToLid_Parity()
    {
        // CQT PROJ 4 REV1 Linha 13: TR4 -> LID
        // M13 = 75.68658823529411 kVA, AP = 2, AQ = 4 m (Lequiv = 2 m), H = 3, V = 220 V
        // BI13 = 0.08878064721159933 Ohm/km, BB13 = 0.0897 Ohm/km
        const double m = 75.68658823529411d;
        const double r = 0.08878064721159933d;
        const double x = 0.0897d;
        const double l = 4.0d;
        const double ap = 2.0d;
        const double v = 220.0d;
        const double h = 3.0d;

        var bzResult = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", m, UnitCode.Kva),
            ("R", r, UnitCode.Ohm),
            ("X", x, UnitCode.Ohm),
            ("L", l, UnitCode.Meter),
            ("AP", ap, UnitCode.Unknown),
            ("V", v, UnitCode.V),
            ("H", h, UnitCode.ConductorKey)), Context);

        Assert.Equal(CalculationStatus.Pass, bzResult.Status);
        Assert.Equal(0.039471666113062395d, (double)bzResult.OutputValue!, 12);
    }

    [Fact]
    public void Caso04_Proj4_LidToP1_Parity()
    {
        // CQT PROJ 4 REV1 Linha 14: LID -> P1
        // M14 = 26.921599999999998 kVA, AP = 1, AQ = 38 m, H = 3, V = 220 V
        // BI14 = 0.1824365739532124 Ohm/km, BB14 = 0.1178 Ohm/km
        const double m = 26.921599999999998d;
        const double r = 0.1824365739532124d;
        const double x = 0.1178d;
        const double l = 38.0d;
        const double ap = 1.0d;
        const double v = 220.0d;
        const double h = 3.0d;

        var bzResult = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", m, UnitCode.Kva),
            ("R", r, UnitCode.Ohm),
            ("X", x, UnitCode.Ohm),
            ("L", l, UnitCode.Meter),
            ("AP", ap, UnitCode.Unknown),
            ("V", v, UnitCode.V),
            ("H", h, UnitCode.ConductorKey)), Context);

        Assert.Equal(CalculationStatus.Pass, bzResult.Status);
        Assert.Equal(0.45901379765173433d, (double)bzResult.OutputValue!, 12);
    }

    [Fact]
    public void Caso05_TwoPhaseSegment_UsesFactorTwo()
    {
        // Trecho bifásico (H = 2): multiplicador de fase = 2
        var drop3Ph = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 20.0d, UnitCode.Kva),
            ("R", 0.2d, UnitCode.Ohm),
            ("X", 0.1d, UnitCode.Ohm),
            ("L", 50.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        var drop2Ph = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 20.0d, UnitCode.Kva),
            ("R", 0.2d, UnitCode.Ohm),
            ("X", 0.1d, UnitCode.Ohm),
            ("L", 50.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 2.0d, UnitCode.ConductorKey)), Context);

        Assert.Equal((double)drop3Ph.OutputValue! * 2.0, (double)drop2Ph.OutputValue!, 10);
    }

    [Fact]
    public void Caso06_SinglePhaseSegment_UsesFactorSix()
    {
        // Trecho monofásico (H = 1): multiplicador de fase = 6
        var drop3Ph = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 10.0d, UnitCode.Kva),
            ("R", 0.2d, UnitCode.Ohm),
            ("X", 0.1d, UnitCode.Ohm),
            ("L", 50.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        var drop1Ph = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 10.0d, UnitCode.Kva),
            ("R", 0.2d, UnitCode.Ohm),
            ("X", 0.1d, UnitCode.Ohm),
            ("L", 50.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 1.0d, UnitCode.ConductorKey)), Context);

        Assert.Equal((double)drop3Ph.OutputValue! * 6.0, (double)drop1Ph.OutputValue!, 10);
    }

    [Fact]
    public void Caso07_ParallelCables_HalvesEquivalentLength()
    {
        // AP = 2 vs AP = 1
        var dropSingle = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 50.0d, UnitCode.Kva),
            ("R", 0.1d, UnitCode.Ohm),
            ("X", 0.08d, UnitCode.Ohm),
            ("L", 100.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        var dropDouble = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 50.0d, UnitCode.Kva),
            ("R", 0.1d, UnitCode.Ohm),
            ("X", 0.08d, UnitCode.Ohm),
            ("L", 100.0d, UnitCode.Meter),
            ("AP", 2.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        Assert.Equal((double)dropSingle.OutputValue! / 2.0, (double)dropDouble.OutputValue!, 10);
    }

    [Fact]
    public void Caso08_DifferentConductors_ReproducedFromExcel()
    {
        // 1. Condutor 70 Al - MX (Linha 20 de CQT PROJ 7)
        // M20 = 7.2944 kVA, AP = 1, AQ = 35 m, BI20 = 0.47243844080184794, BB20 = 0.126
        var drop70Al = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 7.2943999999999996d, UnitCode.Kva),
            ("R", 0.47243844080184794d, UnitCode.Ohm),
            ("X", 0.126d, UnitCode.Ohm),
            ("L", 35.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        Assert.Equal(0.25791613050185525d, (double)drop70Al.OutputValue!, 12);

        // 2. Condutor 16 Al_CONC_Tri (Linha 21 de CQT PROJ 7 - Ramal)
        // M21 = 1.88 kVA, AP = 1, AQ = 30 m, BI21 = 2.0636037487625933, BB21 = 0.85
        var drop16Al = new CandidateSegmentVoltageDropRule().Execute(Inputs(
            ("M", 1.88d, UnitCode.Kva),
            ("R", 2.0636037487625933d, UnitCode.Ohm),
            ("X", 0.85d, UnitCode.Ohm),
            ("L", 30.0d, UnitCode.Meter),
            ("AP", 1.0d, UnitCode.Unknown),
            ("V", 220.0d, UnitCode.V),
            ("H", 3.0d, UnitCode.ConductorKey)), Context);

        Assert.Equal(0.26007001829543885d, (double)drop16Al.OutputValue!, 12);
    }

    [Fact]
    public void Caso09_ThermalResistanceSensitivity_MatchesExcel()
    {
        // CQT PROJ 7 Linha 13: 240 Cu
        // AT13 = 0.0762 Ohm/km, AW13 = 0.00393, BE13 = 1.0652244659520294, BX13 = 43.630837053053675 °C
        var rRule = new CandidateThermalResistanceRule();
        var rResult = rRule.Execute(Inputs(
            ("Rcc20", 0.0762d, UnitCode.Ohm),
            ("Alpha20", 0.00393d, UnitCode.Unknown),
            ("Temperature", 43.630837053053675d, UnitCode.Celsius),
            ("KStar", 1.0652244659520294d, UnitCode.Unknown)), Context);

        Assert.Equal(CalculationStatus.Pass, rResult.Status);
        Assert.Equal(0.08870830611364977d, (double)rResult.OutputValue!, 12);

        // Se temperatura subir para 75 °C
        var rHotResult = rRule.Execute(Inputs(
            ("Rcc20", 0.0762d, UnitCode.Ohm),
            ("Alpha20", 0.00393d, UnitCode.Unknown),
            ("Temperature", 75.0d, UnitCode.Celsius),
            ("KStar", 1.0652244659520294d, UnitCode.Unknown)), Context);

        Assert.True((double)rHotResult.OutputValue! > (double)rResult.OutputValue!);
    }

    [Fact]
    public void Caso10_BranchDerivation_PreservesIndependentPaths()
    {
        // CQT PROJ 7: Nó de bifurcação P2 (CA15 = 5.803613285981125%)
        // Ramo Lado 1 (P2 -> P3): BZ16 = 0.3865379348646184% -> CA16 = 6.190151220845744%
        // Ramo Lado 3 (P2 -> P8): BZ16 = 0.21742758836134785% -> CA16 = 6.021040874342473%
        const double caP2 = 5.803613285981125d;
        const double bzP2P3 = 0.3865379348646184d;
        const double bzP2P8 = 0.21742758836134785d;

        var caRule = new CandidateAccumulatedVoltageDropRule();
        var caP3 = caRule.Execute(Inputs(
            ("BZ", bzP2P3, UnitCode.Percent),
            ("CA_Parent", caP2, UnitCode.Percent)), Context);

        var caP8 = caRule.Execute(Inputs(
            ("BZ", bzP2P8, UnitCode.Percent),
            ("CA_Parent", caP2, UnitCode.Percent)), Context);

        Assert.Equal(6.190151220845744d, (double)caP3.OutputValue!, 12);
        Assert.Equal(6.021040874342473d, (double)caP8.OutputValue!, 12);

        // Isolamento de caminhos: quedas em P3 e P8 não se somam
        Assert.NotEqual((double)caP3.OutputValue!, (double)caP8.OutputValue!);
    }

    private static IReadOnlyList<RuleInput> Inputs(params (string Name, object? Value, UnitCode Unit)[] values) =>
        values.Select(value => new RuleInput(value.Name, value.Value, value.Unit)).ToArray();
}
