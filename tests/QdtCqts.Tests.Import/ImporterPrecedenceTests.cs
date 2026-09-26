using QdtCqts.Infrastructure.ExcelEvidence;
using QdtCqts.Domain;

namespace QdtCqts.Tests.Import;

public sealed class ImporterPrecedenceTests
{
    [Fact]
    public void ImportedFormulaCanProduceConservativeReferences()
    {
        const string path = @"C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\REDE INVERTIDA\REDE_INVERTIDA_ZNA855820\1.QDT\QDT_ZNA855820_ATUAL.xlsm";
        Assert.True(File.Exists(path));
        var workbook = new ExcelEvidenceImporter().Import(path);
        var references = new PrecedenceGraphExtractor().Extract(workbook.Cells);
        Assert.NotEmpty(references);
        Assert.Contains(references, item => item.Kind == "EXTERNAL_REFERENCE");
    }

    [Fact]
    public void PrototypeSheetIsExcludedFromMathematicalReferences()
    {
        var artifact = new SourceArtifact("artifact", "prototype.xlsx", "prototype.xlsx", ".xlsx", "hash");
        var cells = new[]
        {
            new SourceCell("valid", artifact.Id, "LADO 1", "A1", "'Analise Ponto a Ponto'!B2", null, "unknown"),
            new SourceCell("prototype", artifact.Id, "ANÁLISE PONTO A PONTO", "B2", "A1", null, "unknown")
        };

        var references = new PrecedenceGraphExtractor().Extract(cells);

        Assert.Contains(references, item => item.Kind == PrecedenceGraphExtractor.ExcludedPrototypeKind && item.ResolutionStatus == PrecedenceGraphExtractor.ExcludedPrototypeDependencyStatus);
        Assert.Contains(references, item => item.SourceCellId == "prototype" && item.Kind == PrecedenceGraphExtractor.ExcludedPrototypeKind && item.ResolutionStatus == PrecedenceGraphExtractor.ExcludedPrototypeKind);
        Assert.False(new PrecedenceGraphExtractor().HasFormulaCycle(cells));
    }

    [Theory]
    [InlineData("Analise Ponto a Ponto")]
    [InlineData("ANÁLISE PONTO A PONTO")]
    public void PrototypeSheetNamesAreRecognized(string sheetName)
    {
        Assert.True(PrecedenceGraphExtractor.IsPrototypeSheet(sheetName));
    }
}
