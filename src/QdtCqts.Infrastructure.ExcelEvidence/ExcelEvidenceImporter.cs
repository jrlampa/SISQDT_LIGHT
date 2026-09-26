using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using QdtCqts.Domain;

namespace QdtCqts.Infrastructure.ExcelEvidence;

public sealed record ImportedWorkbook(
    SourceArtifact Artifact,
    IReadOnlyList<string> Sheets,
    IReadOnlyList<SourceCell> Cells,
    IReadOnlyList<string> ExternalReferences,
    IReadOnlyList<DefinedNameEvidence> DefinedNames,
    IReadOnlyList<TableEvidence> Tables,
    IReadOnlyList<StructuredReferenceEvidence> StructuredReferences,
    IReadOnlyList<PrintMetadataEvidence> PrintMetadata,
    IReadOnlyList<FormulaReference> FormulaReferences);

public sealed class ExcelEvidenceImporter
{
    public ImportedWorkbook Import(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Workbook was not found.", path);
        }

        var bytes = File.ReadAllBytes(path);
        var artifact = new SourceArtifact(Guid.NewGuid().ToString("N"), path, Path.GetFileName(path), Path.GetExtension(path), SnapshotHash.Sha256(bytes));
        using var archive = new MemoryStream(bytes, writable: false);
        using var zip = new ZipArchive(archive, ZipArchiveMode.Read, leaveOpen: false);
        var workbookEntry = zip.GetEntry("xl/workbook.xml") ?? throw new InvalidDataException("Workbook XML is missing.");
        var relationships = ReadRelationships(zip);
        var workbook = LoadXml(workbookEntry);
        XNamespace spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace documentRelationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        var sheets = new List<string>();
        var cells = new List<SourceCell>();
        var sheetPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sheet in workbook.Descendants(spreadsheet + "sheet"))
        {
            var name = (string?)sheet.Attribute("name") ?? string.Empty;
            sheets.Add(name);
            var relationshipId = (string?)sheet.Attribute(documentRelationships + "id");
            if (relationshipId is null || !relationships.TryGetValue(relationshipId, out var target)) continue;
            var sheetPath = NormalizePackagePath(target, "xl/workbook.xml");
            sheetPaths[sheetPath] = name;
            var entry = zip.GetEntry(sheetPath);
            if (entry is null) continue;
            var sheetXml = LoadXml(entry);
            foreach (var cell in sheetXml.Descendants(spreadsheet + "c"))
            {
                var address = (string?)cell.Attribute("r") ?? string.Empty;
                var formula = cell.Element(spreadsheet + "f")?.Value;
                var cachedValue = cell.Element(spreadsheet + "v")?.Value;
                cells.Add(new SourceCell(Guid.NewGuid().ToString("N"), artifact.Id, name, address, formula, cachedValue, formula is null ? "input" : "unknown"));
            }
        }

        var definedNames = workbook.Descendants(spreadsheet + "definedName")
            .Select(item => new DefinedNameEvidence(
                (string?)item.Attribute("name") ?? string.Empty,
                ResolveScope((string?)item.Attribute("localSheetId"), sheets),
                item.Value,
                string.Equals((string?)item.Attribute("hidden"), "1", StringComparison.Ordinal)))
            .ToArray();
        var tables = ExtractTables(zip, sheetPaths);
        var structuredReferences = cells.Where(cell => cell.Formula?.Contains('[', StringComparison.Ordinal) == true)
            .Select(cell => new StructuredReferenceEvidence(cell.Id, cell.Formula!, ExtractStructuredTokens(cell.Formula!)))
            .ToArray();
        var printMetadata = definedNames.Where(item => item.Name.Contains("Print_Area", StringComparison.OrdinalIgnoreCase))
            .Select(item => new PrintMetadataEvidence(item.Scope ?? string.Empty, item.Reference, null, null, null, null))
            .ToArray();
        var externalReferences = zip.Entries.Where(entry => entry.FullName.StartsWith("xl/externalLinks/", StringComparison.OrdinalIgnoreCase)).Select(entry => entry.FullName).ToArray();
        var formulaReferences = new PrecedenceGraphExtractor().Extract(cells);
        return new ImportedWorkbook(artifact, sheets, cells, externalReferences, definedNames, tables, structuredReferences, printMetadata, formulaReferences);
    }

    private static XDocument LoadXml(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
    }

    private static Dictionary<string, string> ReadRelationships(ZipArchive zip)
    {
        var entry = zip.GetEntry("xl/_rels/workbook.xml.rels");
        if (entry is null) return new(StringComparer.Ordinal);
        var xml = LoadXml(entry);
        XNamespace relationships = "http://schemas.openxmlformats.org/package/2006/relationships";
        return xml.Descendants(relationships + "Relationship").ToDictionary(item => (string)item.Attribute("Id")!, item => (string)item.Attribute("Target")!, StringComparer.Ordinal);
    }

    private static string? ResolveScope(string? localSheetId, IReadOnlyList<string> sheets)
    {
        return int.TryParse(localSheetId, out var index) && index >= 0 && index < sheets.Count ? sheets[index] : null;
    }

    private static string NormalizePackagePath(string target, string source)
    {
        var basePath = source[..source.LastIndexOf('/')];
        var combined = target.StartsWith("/", StringComparison.Ordinal) ? target.TrimStart('/') : $"{basePath}/{target}";
        var parts = new List<string>();
        foreach (var part in combined.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == ".") continue;
            if (part == "..") { if (parts.Count > 0) parts.RemoveAt(parts.Count - 1); continue; }
            parts.Add(part);
        }
        return string.Join('/', parts);
    }

    private static IReadOnlyList<TableEvidence> ExtractTables(ZipArchive zip, IReadOnlyDictionary<string, string> sheetPaths)
    {
        var tableToSheet = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in sheetPaths)
        {
            var relationshipPath = $"{pair.Key[..pair.Key.LastIndexOf('/')]}_rels/{Path.GetFileName(pair.Key)}.rels";
            var entry = zip.GetEntry(relationshipPath);
            if (entry is null) continue;
            var relationships = LoadXml(entry);
            XNamespace packageRelationships = "http://schemas.openxmlformats.org/package/2006/relationships";
            foreach (var relation in relationships.Descendants(packageRelationships + "Relationship").Where(item => ((string?)item.Attribute("Type"))?.Contains("table", StringComparison.OrdinalIgnoreCase) == true))
            {
                var target = (string?)relation.Attribute("Target");
                if (target is null) continue;
                tableToSheet[NormalizePackagePath(target, pair.Key)] = pair.Value;
            }
        }

        XNamespace spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        return zip.Entries.Where(entry => entry.FullName.StartsWith("xl/tables/", StringComparison.OrdinalIgnoreCase) && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .Select(entry =>
            {
                var xml = LoadXml(entry);
                var table = xml.Root;
                var columns = table?.Element(spreadsheet + "tableColumns")?.Elements(spreadsheet + "tableColumn").Select(column => (string?)column.Attribute("name") ?? string.Empty).ToArray() ?? Array.Empty<string>();
                return new TableEvidence((string?)table?.Attribute("displayName") ?? (string?)table?.Attribute("name") ?? entry.Name, tableToSheet.TryGetValue(entry.FullName, out var sheet) ? sheet : string.Empty, (string?)table?.Attribute("ref") ?? string.Empty, columns);
            }).ToArray();
    }

    private static IReadOnlyList<string> ExtractStructuredTokens(string formula) =>
        Regex.Matches(formula, @"\[[^\]]+\]").Select(match => match.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
}
