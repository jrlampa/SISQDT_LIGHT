namespace QdtCqts.Domain;

public sealed record DefinedNameEvidence(string Name, string? Scope, string Reference, bool Hidden);
public sealed record TableEvidence(string Name, string SheetName, string Range, IReadOnlyList<string> Columns);
public sealed record StructuredReferenceEvidence(string SourceCellId, string Formula, IReadOnlyList<string> Tokens);
public sealed record PrintMetadataEvidence(string SheetName, string? PrintArea, string? Orientation, string? PaperSize, string? PrintTitleRows, string? PrintTitleColumns);
