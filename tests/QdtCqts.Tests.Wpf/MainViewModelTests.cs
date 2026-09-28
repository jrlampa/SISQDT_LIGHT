using System.Threading.Tasks;
using QdtCqts.Desktop.Wpf.ViewModels;
using Xunit;

namespace QdtCqts.Tests.Wpf;

public sealed class MainViewModelTests
{
    [Fact]
    public void InitialState_IsCorrectlyConfigured()
    {
        var vm = new MainViewModel();

        Assert.Equal("Pronto", vm.CalculationStatus);
        Assert.False(vm.IsCalculating);
        Assert.Contains("PROJ 7", vm.SelectedProject);
        Assert.Empty(vm.Segments);
        Assert.True(vm.ExecuteCalculationCommand.CanExecute(null));
        Assert.Equal("-", vm.CorrelationId);
        Assert.Equal("-", vm.CalculationId);
    }

    [Fact]
    public async Task ExecuteCalculation_Proj7_PopulatesResultsAndTraceability()
    {
        var vm = new MainViewModel();
        vm.SelectedProject = "CQT PROJ 7 - AV PADRE DECAMINADA (Rev 2)";

        await vm.ExecuteCalculationAsync();

        Assert.Equal("Concluído", vm.CalculationStatus);
        Assert.False(vm.IsCalculating);
        Assert.NotEqual("-", vm.CorrelationId);
        Assert.NotEqual("-", vm.CalculationId);
        Assert.True(vm.ExecutionDurationMs >= 0);
        Assert.NotEmpty(vm.Segments);
        Assert.Equal(7, vm.Segments.Count);

        // Métricas elétricas principais
        Assert.True(vm.MaxVoltageDropPercent > 0);
        Assert.True(vm.MaxShortCircuit3PhaseAmperes > 5000); // 8098 A no TR
        Assert.True(vm.MinShortCircuit1PhaseAmperes > 0 && vm.MinShortCircuit1PhaseAmperes < 1000); // 500 A na ponta
        Assert.StartsWith("NH-", vm.RecommendedFuse);
        Assert.Contains("conforme", vm.ProtectionStatus, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SwitchProject_ToProj4_ResetsAndCalculatesSuccessfully()
    {
        var vm = new MainViewModel();

        // Executa primeiro PROJ 7
        await vm.ExecuteCalculationAsync();
        Assert.NotEmpty(vm.Segments);

        // Troca para PROJ 4 -> deve resetar
        vm.SelectedProject = "CQT PROJ 4 - AV PADRE DECAMINADA (Rev 1)";
        Assert.Equal("Pronto", vm.CalculationStatus);
        Assert.Empty(vm.Segments);
        Assert.Equal(0, vm.MaxVoltageDropPercent);

        // Executa PROJ 4
        await vm.ExecuteCalculationAsync();
        Assert.Equal("Concluído", vm.CalculationStatus);
        Assert.Equal(7, vm.Segments.Count);
        Assert.True(vm.MaxShortCircuit3PhaseAmperes > 5000);
    }
}
