using QdtCqts.Infrastructure.ExcelEvidence;

namespace QdtCqts.Tests.Import;

public sealed class ImportTests
{
    private const string QdtPath = @"C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\REDE INVERTIDA\REDE_INVERTIDA_ZNA855820\1.QDT\QDT_ZNA855820_ATUAL.xlsm";
    private const string CqtsPath = @"C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\REDE INVERTIDA\REDE_INVERTIDA_ZNA855820\CQTS__ZNA855820_ATUAL.xlsx";

    [Fact]
    public void ImportsXlsmWithoutExecutingMacros()
    {
        Assert.True(File.Exists(QdtPath));
        var result = new ExcelEvidenceImporter().Import(QdtPath);
        Assert.Equal(".xlsm", result.Artifact.FileType);
        Assert.Contains("LADO 1", result.Sheets);
        Assert.NotEmpty(result.Cells);
        Assert.NotEmpty(result.ExternalReferences);
    }

    [Fact]
    public void ImportsXlsxAndPreservesCells()
    {
        Assert.True(File.Exists(CqtsPath));
        var result = new ExcelEvidenceImporter().Import(CqtsPath);
        Assert.Equal(".xlsx", result.Artifact.FileType);
        Assert.Contains("RAMAL", result.Sheets);
        Assert.Contains(result.Cells, cell => cell.SheetName == "LADO 1" && cell.Address == "A8");
    }

    [Fact]
    public void ImportsP1NamesTablesStructuredReferencesAndPrintAreas()
    {
        Assert.True(File.Exists(CqtsPath));
        var result = new ExcelEvidenceImporter().Import(CqtsPath);
        Assert.NotEmpty(result.DefinedNames);
        Assert.Contains(result.DefinedNames, item => item.Name == "CABOS" || item.Name == "CONDUTORES");
        Assert.NotEmpty(result.Tables);
        Assert.Contains(result.Tables, item => item.Name == "CABOS");
        Assert.NotEmpty(result.StructuredReferences);
        Assert.NotEmpty(result.FormulaReferences);

        Assert.True(File.Exists(QdtPath));
        var qdt = new ExcelEvidenceImporter().Import(QdtPath);
        Assert.NotEmpty(qdt.PrintMetadata);
    }
}
