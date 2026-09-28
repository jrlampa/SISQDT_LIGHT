using System;
using System.Collections.Generic;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Calculation.Cqts;
using QdtCqts.Domain;
using Xunit;

namespace QdtCqts.Tests.Domain;

/// <summary>
/// Testes de paridade matemática estrita e Golden Tests para a Fase 24 (Curto-Circuito + Proteção).
/// Confronto numérico direto contra as células comprovadas de CQT PROJ 7 REV2 e CQT PROJ 4 REV1.
/// </summary>
public sealed class Fase24ShortCircuitAndProtectionParityTests
{
    private static readonly RuleExecutionContext Context = new("EXEC-F24-SC-PARITY", "GC-F24");

    // Parâmetros Upstream reais do Corpus Homologado (C6=40MVA, D6=20%, E6=13.2kV, G4=53SC, G6=2km, AS6=112.5kVA, BW6=3.5%, BX6=220V)
    private const double Vnom = 220.0d;
    private const double ZUpstreamReal = 0.000398388888888889d;
    private const double ZUpstreamImag = 0.015494888888888889d;

    [Fact]
    public void UpstreamImpedance_ComponentsMatchExcelFormulasExactly()
    {
        // 1. Estação AT/MT (CA8)
        double ratioSq = Math.Pow(220.0 / (13.2 * 1000.0), 2.0); // 1 / 3600
        double zEstX = (20.0 / 100.0 * Math.Pow(13.2, 2.0) / 40.0) * ratioSq;
        Assert.Equal(0.000242d, zEstX, 12);

        // 2. Cabo MT (CF8)
        double zMtR = 0.7171d * ratioSq * 2.0d;
        double zMtX = 0.3512d * ratioSq * 2.0d;
        Assert.Equal(0.000398388888888889d, zMtR, 12);
        Assert.Equal(0.000195111111111111d, zMtX, 12);

        // 3. Trafo de Distribuição (AS8)
        double zTrafoX = (3.5 / 100.0 * Math.Pow(220.0, 2.0)) / (112.5 * 1000.0);
        Assert.Equal(0.015057777777777778d, zTrafoX, 12);

        // Soma total a montante
        double totalX = zEstX + zMtX + zTrafoX;
        Assert.Equal(ZUpstreamImag, totalX, 12);
    }

    [Fact]
    public void ImpedanceDecomposition_FullDecompositionMatchesExcel()
    {
        // Parâmetros de entrada PROJ 7 LADO 1 Linha 13:
        // Fonte AT/MT: C6=40 MVA, D6=20%, E6=13.2 kV -> R_fonte=0, X_fonte=0.000242 Ohm
        // Linha MT: 53 SC, G6=2 km -> R_mt=0.000398388888888889 Ohm, X_mt=0.000195111111111111 Ohm
        // Trafo: AS6=112.5 kVA, BW6=3.5%, BX6=220 V -> R_trafo=0, X_trafo=0.015057777777777778 Ohm
        // Trecho BT (TR->LID): L=2m, BI13=0.08870830611364977, BB13=0.0897 -> R_bt=0.0001774166122273 Ohm, X_bt=0.0001794 Ohm

        double rFonte = 0.0;
        double xFonte = 0.000242d;
        double rMt = 0.000398388888888889d;
        double xMt = 0.000195111111111111d;
        double rTrafo = 0.0;
        double xTrafo = 0.015057777777777778d;
        double rBt = 0.08870830611364977d * 2.0d / 1000.0d;
        double xBt = 0.0897d * 2.0d / 1000.0d;

        double rTotal = rFonte + rMt + rTrafo + rBt;
        double xTotal = xFonte + xMt + xTrafo + xBt;
        double zMag = Math.Sqrt(rTotal * rTotal + xTotal * xTotal);
        double icc3ph = (220.0d / Math.Sqrt(3.0)) / zMag;

        Assert.Equal(0.000575805501116189d, rTotal, 12);
        Assert.Equal(0.015674288888888889d, xTotal, 12);
        Assert.Equal(0.015684861623471893d, zMag, 10);
        Assert.Equal(8098.066930449716d, icc3ph, 4);
    }

    [Fact]
    public void Proj7_Row13_TrToLid_ShortCircuitParity()
    {
        // PROJ 7 LADO 1 Linha 13:
        // BI13 = 0.08870830611364977, BB13 = 0.0897, BM13 = 0.1792792538254864, Lequiv = 2 m
        // Excel CB13 (Icc 3ph) = 8098.066930449716 A
        // Excel CC13 (Icc 1ph) = 8182.488711140876 A
        var rule = new CandidateShortCircuitRule();
        var result = rule.Execute(new[]
        {
            new RuleInput("Vnom", Vnom, UnitCode.V),
            new RuleInput("Z_Upstream_Real", ZUpstreamReal, UnitCode.Ohm),
            new RuleInput("Z_Upstream_Imag", ZUpstreamImag, UnitCode.Ohm),
            new RuleInput("Z_BtPrior_Real", 0.0d, UnitCode.Ohm),
            new RuleInput("Z_BtPrior_Imag", 0.0d, UnitCode.Ohm),
            new RuleInput("Rca_Phase", 0.08870830611364977d, UnitCode.Ohm),
            new RuleInput("Rca_Neutral", 0.1792792538254864d, UnitCode.Ohm),
            new RuleInput("X_Phase", 0.0897d, UnitCode.Ohm),
            new RuleInput("Length_Equiv_Meters", 2.0d, UnitCode.Meter),
            new RuleInput("Rfn_Prior", 0.0d, UnitCode.Ohm)
        }, Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        var tuple = (ValueTuple<double, double, double, double, double, double, double>)result.OutputValue!;

        Assert.Equal(8098.066930449716d, tuple.Item1, 4); // Icc 3ph
        Assert.Equal(8182.488711140876d, tuple.Item2, 4); // Icc 1ph
    }

    [Fact]
    public void Proj7_Row14_LidToP1_ShortCircuitParity()
    {
        // PROJ 7 LADO 1 Linha 14:
        // Prior: BK13 = 0.0001774166122273 + j0.0001794, BN13 = 0.0005359751198782723
        // Trecho: BI14 = 0.19033362623960937, BB14 = 0.1178, BM14 = 0.2936244356013486, Lequiv = 35 m
        // Excel CB14 = 6025.833850528486 A, CC14 = 5369.69463883112 A
        var rule = new CandidateShortCircuitRule();
        var result = rule.Execute(new[]
        {
            new RuleInput("Vnom", Vnom, UnitCode.V),
            new RuleInput("Z_Upstream_Real", ZUpstreamReal, UnitCode.Ohm),
            new RuleInput("Z_Upstream_Imag", ZUpstreamImag, UnitCode.Ohm),
            new RuleInput("Z_BtPrior_Real", 0.0001774166122273d, UnitCode.Ohm),
            new RuleInput("Z_BtPrior_Imag", 0.0001794d, UnitCode.Ohm),
            new RuleInput("Rca_Phase", 0.19033362623960937d, UnitCode.Ohm),
            new RuleInput("Rca_Neutral", 0.2936244356013486d, UnitCode.Ohm),
            new RuleInput("X_Phase", 0.1178d, UnitCode.Ohm),
            new RuleInput("Length_Equiv_Meters", 35.0d, UnitCode.Meter),
            new RuleInput("Rfn_Prior", 0.0005359751198782723d, UnitCode.Ohm)
        }, Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        var tuple = (ValueTuple<double, double, double, double, double, double, double>)result.OutputValue!;

        Assert.Equal(6025.833850528486d, tuple.Item1, 4); // Icc 3ph
        Assert.Equal(5369.69463883112d, tuple.Item2, 4);  // Icc 1ph
    }

    [Fact]
    public void Proj7_Row21_EndPoint_ShortCircuitParity()
    {
        // PROJ 7 LADO 1 Linha 21 (Ponta do circuito RL-P8):
        // Prior: BK20 = 0.0543296550522093 + j0.0286206, BN20 = 0.1292544975977728
        // Trecho: BI21 = 2.0636037487625933, BB21 = 0.85, BM21 = 2.0636037487625933, Lequiv = 30 m
        // Excel CB21 = 935.1046750325303 A, CC21 = 500.1808236827155 A
        var rule = new CandidateShortCircuitRule();
        var result = rule.Execute(new[]
        {
            new RuleInput("Vnom", Vnom, UnitCode.V),
            new RuleInput("Z_Upstream_Real", ZUpstreamReal, UnitCode.Ohm),
            new RuleInput("Z_Upstream_Imag", ZUpstreamImag, UnitCode.Ohm),
            new RuleInput("Z_BtPrior_Real", 0.0543296550522093d, UnitCode.Ohm),
            new RuleInput("Z_BtPrior_Imag", 0.0286206d, UnitCode.Ohm),
            new RuleInput("Rca_Phase", 2.0636037487625933d, UnitCode.Ohm),
            new RuleInput("Rca_Neutral", 2.0636037487625933d, UnitCode.Ohm),
            new RuleInput("X_Phase", 0.85d, UnitCode.Ohm),
            new RuleInput("Length_Equiv_Meters", 30.0d, UnitCode.Meter),
            new RuleInput("Rfn_Prior", 0.1292544975977728d, UnitCode.Ohm)
        }, Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        var tuple = (ValueTuple<double, double, double, double, double, double, double>)result.OutputValue!;

        Assert.Equal(935.1046750325303d, tuple.Item1, 4); // Icc 3ph
        Assert.Equal(500.1808236827155d, tuple.Item2, 4); // Icc 1ph
    }

    [Fact]
    public void Proj4_Row13_TrToLid_ShortCircuitParity()
    {
        // PROJ 4 LADO 1 Linha 13:
        // BI13 = 0.08878064721159933, BB13 = 0.0897, BM13 = 0.1794254549945708, Lequiv = 2 m
        // Excel CB13 = 8098.06418783237 A, CC13 = 8182.474839937445 A
        var rule = new CandidateShortCircuitRule();
        var result = rule.Execute(new[]
        {
            new RuleInput("Vnom", Vnom, UnitCode.V),
            new RuleInput("Z_Upstream_Real", ZUpstreamReal, UnitCode.Ohm),
            new RuleInput("Z_Upstream_Imag", ZUpstreamImag, UnitCode.Ohm),
            new RuleInput("Z_BtPrior_Real", 0.0d, UnitCode.Ohm),
            new RuleInput("Z_BtPrior_Imag", 0.0d, UnitCode.Ohm),
            new RuleInput("Rca_Phase", 0.08878064721159933d, UnitCode.Ohm),
            new RuleInput("Rca_Neutral", 0.1794254549945708d, UnitCode.Ohm),
            new RuleInput("X_Phase", 0.0897d, UnitCode.Ohm),
            new RuleInput("Length_Equiv_Meters", 2.0d, UnitCode.Meter),
            new RuleInput("Rfn_Prior", 0.0d, UnitCode.Ohm)
        }, Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        var tuple = (ValueTuple<double, double, double, double, double, double, double>)result.OutputValue!;

        Assert.Equal(8098.06418783237d, tuple.Item1, 4); // Icc 3ph
        Assert.Equal(8182.474839937445d, tuple.Item2, 4); // Icc 1ph
    }

    [Fact]
    public void Proj7_ProtectionRule_ProjectCurrentMatchesExcelCH40()
    {
        // PROJ 7 Linha 40: BW13 = 74.448 kVA, BX6 = 220 V
        // I_projeto = 74.448 / (sqrt(3) * 220 / 1000) = 195.37533109376938 A
        var rule = new CandidateProtectionRule();
        var result = rule.Execute(new[]
        {
            new RuleInput("RootLoadKva", 74.448d, UnitCode.Kva),
            new RuleInput("Vnom", 220.0d, UnitCode.V),
            new RuleInput("MinIcc1Phase", 500.1808236827155d, UnitCode.Ampere),
            new RuleInput("CriticalTemperature", 34.698781411586566d, UnitCode.Celsius),
            new RuleInput("CriticalConductorKey", "16 Al_CONC_Tri", UnitCode.ConductorKey)
        }, Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        var ass = Assert.IsType<ProtectionAssessment>(result.OutputValue);
        Assert.Equal(195.37533109376938d, ass.ProjectCurrentAmperes, 8);
        Assert.True(ass.IsRatedCurrentAdequate);
    }

    [Fact]
    public void Proj4_ProtectionRule_ProjectCurrentMatchesExcelCH40()
    {
        // PROJ 4 Linha 40: BW13 = 75.68658823529411 kVA, BX6 = 220 V
        // I_projeto = 75.68658823529411 / (sqrt(3) * 220 / 1000) = 198.625782234961 A
        var rule = new CandidateProtectionRule();
        var result = rule.Execute(new[]
        {
            new RuleInput("RootLoadKva", 75.68658823529411d, UnitCode.Kva),
            new RuleInput("Vnom", 220.0d, UnitCode.V),
            new RuleInput("MinIcc1Phase", 486.4088034361912d, UnitCode.Ampere),
            new RuleInput("CriticalTemperature", 34.698781411586566d, UnitCode.Celsius),
            new RuleInput("CriticalConductorKey", "16 Al_CONC_Tri", UnitCode.ConductorKey)
        }, Context);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        var ass = Assert.IsType<ProtectionAssessment>(result.OutputValue);
        Assert.Equal(198.625782234961d, ass.ProjectCurrentAmperes, 8);
        Assert.True(ass.IsRatedCurrentAdequate);
    }

    [Fact]
    public void Onderdonk_ThermalWithstand_CopperAndAluminumEquations()
    {
        // Teste direto das fórmulas de suportabilidade térmica comprovadas em CR37 e CR38
        // Alumínio: t = 48686 * ln((250+228)/(T+228)) * (S / Icc)^2
        // Para S=70 mm2, T=40 C, Icc=1800 A:
        double s = 63.8014d;
        double tOp = 40.0d;
        double icc = 1800.0d;
        double expectedTAl = 48686.0d * Math.Log((250.0 + 228.0) / (tOp + 228.0)) * Math.Pow(s / icc, 2.0);

        var rule = new CandidateProtectionRule();
        var result = rule.Execute(new[]
        {
            new RuleInput("RootLoadKva", 50.0d, UnitCode.Kva),
            new RuleInput("Vnom", 220.0d, UnitCode.V),
            new RuleInput("MinIcc1Phase", icc, UnitCode.Ampere),
            new RuleInput("CriticalTemperature", tOp, UnitCode.Celsius),
            new RuleInput("CriticalConductorKey", "70 Al - MX", UnitCode.ConductorKey)
        }, Context);

        var ass = Assert.IsType<ProtectionAssessment>(result.OutputValue);
        Assert.Equal(expectedTAl, ass.MaxAdmissibleTimeSeconds, 6);
    }
}
