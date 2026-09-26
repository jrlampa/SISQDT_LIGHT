using System.IO;
using Xunit;

namespace QdtCqts.Tests.Domain;

/// <summary>
/// Testes automatizados para validação de integridade física dos assets de identidade visual do sisQDT_LIGHT.
/// </summary>
public sealed class BrandAssetTests
{
    private static readonly string SolutionRoot = FindSolutionRoot();
    private static readonly string BrandDir = Path.Combine(SolutionRoot, "assets", "brand");
    private static readonly string WpfAssetsDir = Path.Combine(SolutionRoot, "src", "QdtCqts.Desktop.Wpf", "Assets");

    [Theory]
    [InlineData("sisQDT_LIGHT.svg")]
    [InlineData("sisQDT_LIGHT.png")]
    [InlineData("sisQDT_LIGHT.ico")]
    [InlineData("sisQDT_LIGHT_dark.png")]
    [InlineData("sisQDT_LIGHT_dark.svg")]
    [InlineData("sisQDT_LIGHT_light.png")]
    [InlineData("sisQDT_LIGHT_light.svg")]
    [InlineData("sisQDT_LIGHT_mono.png")]
    [InlineData("sisQDT_LIGHT_mono.svg")]
    [InlineData("sisQDT_LIGHT_icon.png")]
    [InlineData("sisQDT_LIGHT_icon.svg")]
    public void OfficialBrandFilesExistAndAreNonEmpty(string fileName)
    {
        var filePath = Path.Combine(BrandDir, fileName);
        Assert.True(File.Exists(filePath), $"Brand asset file missing: {fileName}");

        var fileInfo = new FileInfo(filePath);
        Assert.True(fileInfo.Length > 0, $"Brand asset file is empty: {fileName}");
    }

    [Theory]
    [InlineData("sisQDT_LIGHT.png")]
    [InlineData("sisQDT_LIGHT_dark.png")]
    [InlineData("sisQDT_LIGHT_light.png")]
    [InlineData("sisQDT_LIGHT_mono.png")]
    [InlineData("sisQDT_LIGHT_icon.png")]
    public void PngFilesHaveValidPngHeader(string fileName)
    {
        var filePath = Path.Combine(BrandDir, fileName);
        using var stream = File.OpenRead(filePath);
        var header = new byte[8];
        var read = stream.Read(header, 0, 8);
        Assert.Equal(8, read);

        // Assinatura PNG: 89 50 4E 47 0D 0A 1A 0A
        var expectedPngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        Assert.Equal(expectedPngHeader, header);
    }

    [Theory]
    [InlineData("sisQDT_LIGHT.svg")]
    [InlineData("sisQDT_LIGHT_dark.svg")]
    [InlineData("sisQDT_LIGHT_light.svg")]
    [InlineData("sisQDT_LIGHT_mono.svg")]
    [InlineData("sisQDT_LIGHT_icon.svg")]
    public void SvgFilesAreValidXmlSvg(string fileName)
    {
        var filePath = Path.Combine(BrandDir, fileName);
        var content = File.ReadAllText(filePath);
        Assert.Contains("<svg", content);
        Assert.Contains("</svg>", content);
        Assert.Contains("xmlns=\"http://www.w3.org/2000/svg\"", content);
    }

    [Fact]
    public void WindowsIcoFileHasValidMultiResolutionStructure()
    {
        var icoPath = Path.Combine(BrandDir, "sisQDT_LIGHT.ico");
        Assert.True(File.Exists(icoPath));

        using var stream = File.OpenRead(icoPath);
        using var reader = new BinaryReader(stream);

        // Header ICO: 2 bytes reservados (0), 2 bytes tipo (1 = icon), 2 bytes contagem de imagens
        var reserved = reader.ReadUInt16();
        var type = reader.ReadUInt16();
        var count = reader.ReadUInt16();

        Assert.Equal(0, reserved);
        Assert.Equal(1, type);
        Assert.True(count >= 7, $"Expected at least 7 resolutions in multi-res ICO, but found {count}");

        // Coleta dimensões presentes nas entradas de diretório
        var dimensions = new List<int>();
        for (int i = 0; i < count; i++)
        {
            byte width = reader.ReadByte();
            byte height = reader.ReadByte();
            reader.ReadByte(); // colorCount
            reader.ReadByte(); // reserved
            reader.ReadUInt16(); // planes
            reader.ReadUInt16(); // bitCount
            reader.ReadUInt32(); // bytesInRes
            reader.ReadUInt32(); // imageOffset

            // No formato ICO, largura 0 representa 256 px
            int actualWidth = width == 0 ? 256 : width;
            dimensions.Add(actualWidth);
        }

        // Verifica a presença de todas as resoluções Windows requeridas
        var requiredSizes = new[] { 16, 24, 32, 48, 64, 128, 256 };
        foreach (var size in requiredSizes)
        {
            Assert.Contains(size, dimensions);
        }
    }

    [Fact]
    public void WpfAssetsFolderIsSynchronized()
    {
        Assert.True(File.Exists(Path.Combine(WpfAssetsDir, "sisQDT_LIGHT.ico")));
        Assert.True(File.Exists(Path.Combine(WpfAssetsDir, "sisQDT_LIGHT_icon.png")));
        Assert.True(File.Exists(Path.Combine(WpfAssetsDir, "sisQDT_LIGHT.png")));
    }

    private static string FindSolutionRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "VERSION_MANIFEST.json")))
            {
                return current;
            }
            var parent = Directory.GetParent(current);
            if (parent is null) break;
            current = parent.FullName;
        }
        return Directory.GetCurrentDirectory();
    }
}
