using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using QdtCqts.Domain;

namespace QdtCqts.Infrastructure.ExcelEvidence;

public sealed class PrecedenceGraphExtractor
{
    public const string ExcludedPrototypeKind = "EXCLUDED_PROTOTYPE";
    public const string ExcludedPrototypeDependencyStatus = "EXCLUDED_PROTOTYPE_DEPENDENCY";

    private static readonly Regex CellReference = new(@"(?:(?:'(?<sheet>[^']+)'|(?<bareSheet>[A-Za-z0-9 _.-]+))!)?\$?(?<column>[A-Z]{1,3})\$?(?<row>\d+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<FormulaReference> Extract(IEnumerable<SourceCell> cells)
    {
        var references = new List<FormulaReference>();
        foreach (var cell in cells.Where(item => !string.IsNullOrWhiteSpace(item.Formula)))
        {
            var formula = cell.Formula!;
            if (formula.Contains("#REF!", StringComparison.OrdinalIgnoreCase))
            {
                references.Add(new FormulaReference(Guid.NewGuid().ToString("N"), cell.Id, "#REF!", "BROKEN_REFERENCE", "missing"));
            }

            if (formula.Contains("[", StringComparison.Ordinal) && formula.Contains("]", StringComparison.Ordinal))
            {
                references.Add(new FormulaReference(Guid.NewGuid().ToString("N"), cell.Id, "EXTERNAL_REFERENCE", "EXTERNAL_REFERENCE", "missing"));
            }

            foreach (Match match in CellReference.Matches(formula))
            {
                var sheet = match.Groups["sheet"].Success ? match.Groups["sheet"].Value : match.Groups["bareSheet"].Value;
                var address = match.Groups["column"].Value + match.Groups["row"].Value;
                var target = string.IsNullOrWhiteSpace(sheet) ? address : $"{sheet}!{address}";
                var sourceIsPrototype = IsPrototypeSheet(cell.SheetName);
                var targetIsPrototype = IsPrototypeSheet(sheet);
                references.Add(new FormulaReference(
                    Guid.NewGuid().ToString("N"),
                    cell.Id,
                    target,
                    sourceIsPrototype || targetIsPrototype ? ExcludedPrototypeKind : "CELL",
                    sourceIsPrototype
                        ? ExcludedPrototypeKind
                        : targetIsPrototype ? ExcludedPrototypeDependencyStatus : "unknown"));
            }
        }

        return references;
    }

    public bool HasFormulaCycle(IEnumerable<SourceCell> cells)
    {
        var formulaCells = cells
            .Where(cell => !string.IsNullOrWhiteSpace(cell.Formula) && !IsPrototypeSheet(cell.SheetName))
            .ToDictionary(cell => $"{cell.SheetName}!{cell.Address}", StringComparer.OrdinalIgnoreCase);
        var graph = formulaCells.Keys.ToDictionary(key => key, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var cell in formulaCells.Values)
        {
            foreach (Match match in CellReference.Matches(cell.Formula!))
            {
                var sheet = match.Groups["sheet"].Success ? match.Groups["sheet"].Value : match.Groups["bareSheet"].Success ? match.Groups["bareSheet"].Value : cell.SheetName;
                if (IsPrototypeSheet(sheet)) continue;
                var target = $"{sheet}!{match.Groups["column"].Value}{match.Groups["row"].Value}";
                if (formulaCells.ContainsKey(target)) graph[$"{cell.SheetName}!{cell.Address}"].Add(target);
            }
        }

        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return graph.Keys.Any(node => Visit(node, graph, visiting, visited));
    }

    private static bool Visit(string node, IReadOnlyDictionary<string, List<string>> graph, ISet<string> visiting, ISet<string> visited)
    {
        if (visiting.Contains(node)) return true;
        if (!visited.Add(node)) return false;
        visiting.Add(node);
        foreach (var child in graph[node])
        {
            if (Visit(child, graph, visiting, visited)) return true;
        }
        visiting.Remove(node);
        return false;
    }

    public static bool IsPrototypeSheet(string? sheetName)
    {
        if (string.IsNullOrWhiteSpace(sheetName)) return false;
        var normalized = sheetName.Normalize(NormalizationForm.FormD);
        var withoutDiacritics = new string(normalized.Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark).ToArray());
        return string.Equals(withoutDiacritics.Replace(" ", string.Empty), "ANALISEPONTOAPONTO", StringComparison.OrdinalIgnoreCase);
    }
}
