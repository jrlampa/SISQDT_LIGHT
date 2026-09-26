using QdtCqts.Domain;

namespace QdtCqts.Calculation.Abstractions;

/// <summary>
/// Registro estruturado e auditável de rastreabilidade de cálculo (Calculation Trace).
/// Registra a cadeia exata de execução matemática, insumos, operações e resultados.
/// </summary>
public sealed record CalculationTraceDetail(
    string CalculationId,
    string CorrelationId,
    string RuleId,
    string RuleVersion,
    RuleStatus RuleStatus,
    TraceMode Mode,
    string InputHash,
    string OutputHash,
    IReadOnlyList<RuleInput> Inputs,
    IReadOnlyList<RuleTrace> Steps,
    object? OutputValue,
    UnitCode OutputUnit,
    CalculationStatus Status,
    string? FormulaString,
    string? EvidenceSource,
    double DurationMs,
    DateTimeOffset Timestamp);

/// <summary>
/// Registro estruturado de confronto numérico entre Excel e Native Engine (Parity Trail).
/// </summary>
public sealed record ParityTraceRecord(
    string CalculationId,
    string CorrelationId,
    string RuleId,
    string CaseId,
    string Source,
    string Target,
    double ExpectedValue,
    double ActualValue,
    double Delta,
    double Tolerance,
    string Status, // MATCH, MISMATCH
    DateTimeOffset Timestamp);

/// <summary>
/// Registro estruturado de execução e validação de Caso de Ouro (Golden Case Trail).
/// </summary>
public sealed record GoldenTraceRecord(
    string GoldenCaseId,
    string GoldenVersion,
    string CalculationId,
    string CorrelationId,
    string SourceHash,
    double ExpectedValue,
    double ActualValue,
    double Delta,
    double Tolerance,
    string Status, // PASS, FAIL
    DateTimeOffset Timestamp);

/// <summary>
/// Registro de proveniência de fatos e dados extraídos de workbooks reais (Audit / Evidence Trail).
/// </summary>
public sealed record EvidenceTrailRecord(
    string EvidenceId,
    string SourceFile,
    string SourceHash,
    string Sheet,
    string Cell,
    string Formula,
    object? CachedValue,
    string ExtractionVersion,
    DateTimeOffset Timestamp);
