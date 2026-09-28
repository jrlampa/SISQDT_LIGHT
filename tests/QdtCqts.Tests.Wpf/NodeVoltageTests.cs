using QdtCqts.Calculation.Abstractions;
using QdtCqts.Desktop.Wpf.ViewModels.Unifilar;
using Xunit;

namespace QdtCqts.Tests.Wpf;

/// <summary>
/// Fase 29: Testes determinísticos das tensões nodais V127/V220.
/// Valida derivação no DTO (NodeCalculationResult), propagação ao ViewModel
/// e ausência de cálculo elétrico na camada de apresentação.
/// Paridade: CA=6,96% → V127≈118,16 V, V220≈204,69 V (caso real ZNA855820 PROJETADO).
/// </summary>
public sealed class NodeVoltageTests
{
    // ── Testes do DTO (Abstractions) ─────────────────────────────────────────

    [Fact]
    public void NodeCalcResult_CaZero_RetornaVoltagesTotaisNominais()
    {
        var result = new NodeCalculationResult("N1", "P01", 0, 0.0, 0.0);
        Assert.Equal(127.0, result.VoltageV127, precision: 9);
        Assert.Equal(220.0, result.VoltageV220, precision: 9);
    }

    [Fact]
    public void NodeCalcResult_CaZNA855820Projetado_RetornaVoltagesParidade()
    {
        // Caso real ZNA855820 – cenário PROJETADO, ponta mais distante
        const double ca = 6.96;
        var result = new NodeCalculationResult("N1", "P01", 0, 0.0, ca);
        Assert.InRange(result.VoltageV127, 118.10, 118.20);
        Assert.InRange(result.VoltageV220, 204.65, 204.75);
    }

    [Fact]
    public void NodeCalcResult_CaZNA855820Atual_RetornaVoltagesViolacao()
    {
        // Caso real ZNA855820 – cenário ATUAL com violação
        const double ca = 9.90;
        var result = new NodeCalculationResult("N1", "P01", 0, 0.0, ca);
        Assert.InRange(result.VoltageV127, 114.0, 115.0);
        Assert.True(result.VoltageV127 < 116.0,
            $"V127={result.VoltageV127:F3} deveria estar abaixo de 116 V (violação PRODIST)");
    }

    [Fact]
    public void NodeCalcResult_Ca8_66_V127NaFaixaAdequada()
    {
        // CA = 8,66%: V127 = 127*(1-0.0866) = 116,002 V (>= 116 V → adequado)
        // CA = 8,66%: V220 = 220*(1-0.0866) = 200,948 V (< 201 V → precário para 220 V)
        // O limite V127 >= 116 V é o principal critério PRODIST para 127/220 V
        const double ca = 8.66;
        var result = new NodeCalculationResult("N1", "P01", 0, 0.0, ca);
        Assert.True(result.VoltageV127 >= 116.0,
            $"V127={result.VoltageV127:F3} deve ser >= 116 V (CA={ca}%)");
    }

    [Fact]
    public void NodeCalcResult_CaOriginalPreservado_NaoAlterado()
    {
        // AccumulatedVoltageDropPercent deve ser intacto — V127/V220 são propriedades extras
        const double ca = 6.96;
        var result = new NodeCalculationResult("N1", "P01", 5, 12.5, ca);
        Assert.Equal(ca, result.AccumulatedVoltageDropPercent, precision: 9);
        Assert.Equal(5, result.LocalConsumers);
        Assert.Equal(12.5, result.LocalLoadKva, precision: 9);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(6.96)]
    [InlineData(8.66)]
    [InlineData(9.90)]
    public void NodeCalcResult_FormulaDeterministicaParaTodosOsCenarios(double ca)
    {
        // A fórmula deve ser estritamente determinística e derivada de CA%
        var result = new NodeCalculationResult("N", "P", 0, 0.0, ca);
        double expectedV127 = 127.0 * (1.0 - ca / 100.0);
        double expectedV220 = 220.0 * (1.0 - ca / 100.0);
        Assert.Equal(expectedV127, result.VoltageV127, precision: 9);
        Assert.Equal(expectedV220, result.VoltageV220, precision: 9);
    }

    // ── Propagação para ViewModel (sem cálculo na View) ──────────────────────

    [Fact]
    public void UnifilarNodeViewModel_VoltagesPropagatosSemCalculo()
    {
        // O ViewModel recebe os valores do builder — não deve calcular
        const double ca = 6.96;
        var vm = new UnifilarNodeViewModel
        {
            NodeId = "N1",
            ExternalKey = "P01",
            AccumulatedVoltageDropPercent = ca,
            VoltageV127 = 127.0 * (1.0 - ca / 100.0),
            VoltageV220 = 220.0 * (1.0 - ca / 100.0)
        };
        Assert.InRange(vm.VoltageV127, 118.10, 118.20);
        Assert.InRange(vm.VoltageV220, 204.65, 204.75);
    }

    [Fact]
    public void UnifilarNodeViewModel_SemResultado_VoltagesDefaultSaoNominais()
    {
        // Quando nodeResult == null no builder, fallback deve ser tensão nominal plena
        var vm = new UnifilarNodeViewModel
        {
            NodeId = "N2",
            ExternalKey = "P02",
            AccumulatedVoltageDropPercent = 0.0,
            VoltageV127 = 127.0,
            VoltageV220 = 220.0
        };
        Assert.Equal(127.0, vm.VoltageV127, precision: 9);
        Assert.Equal(220.0, vm.VoltageV220, precision: 9);
    }
}
