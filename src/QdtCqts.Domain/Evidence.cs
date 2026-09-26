namespace QdtCqts.Domain;

public sealed record SourceArtifact(string Id, string Path, string FileName, string FileType, string Sha256);
public sealed record SourceCell(string Id, string ArtifactId, string SheetName, string Address, string? Formula, string? CachedValue, string Classification);
public sealed record FormulaReference(string Id, string SourceCellId, string PrecedentRef, string Kind, string ResolutionStatus);
public sealed record GoldenCase(string Id, string ModelKind, string SourceArtifactId, string SourceHash, string InputSnapshotHash, string Status, string TolerancePolicy);
public sealed record GoldenObservation(string Id, string CaseId, string Path, string? SourceCellId, string? ExpectedText, double? ExpectedNumeric, UnitCode Unit, string ComparisonMode);
