using System.Collections.Concurrent;
using QdtCqts.Calculation.Abstractions;

namespace QdtCqts.Application;

public interface ICalculationTraceRecorder
{
    void RecordTrace(CalculationTraceDetail trace);
    void RecordParity(ParityTraceRecord parity);
    void RecordGolden(GoldenTraceRecord golden);
    void RecordEvidence(EvidenceTrailRecord evidence);

    IReadOnlyList<CalculationTraceDetail> GetTraces(string? calculationId = null);
    IReadOnlyList<ParityTraceRecord> GetParityRecords(string? calculationId = null);
    IReadOnlyList<GoldenTraceRecord> GetGoldenRecords(string? goldenCaseId = null);
    IReadOnlyList<EvidenceTrailRecord> GetEvidenceRecords(string? evidenceId = null);
    void Clear();
}

public sealed class CalculationTraceRecorder : ICalculationTraceRecorder
{
    private readonly ConcurrentQueue<CalculationTraceDetail> _traces = new();
    private readonly ConcurrentQueue<ParityTraceRecord> _parities = new();
    private readonly ConcurrentQueue<GoldenTraceRecord> _goldens = new();
    private readonly ConcurrentQueue<EvidenceTrailRecord> _evidences = new();

    public void RecordTrace(CalculationTraceDetail trace) => _traces.Enqueue(trace);

    public void RecordParity(ParityTraceRecord parity) => _parities.Enqueue(parity);

    public void RecordGolden(GoldenTraceRecord golden) => _goldens.Enqueue(golden);

    public void RecordEvidence(EvidenceTrailRecord evidence) => _evidences.Enqueue(evidence);

    public IReadOnlyList<CalculationTraceDetail> GetTraces(string? calculationId = null) =>
        string.IsNullOrWhiteSpace(calculationId)
            ? _traces.ToArray()
            : _traces.Where(t => t.CalculationId == calculationId).ToArray();

    public IReadOnlyList<ParityTraceRecord> GetParityRecords(string? calculationId = null) =>
        string.IsNullOrWhiteSpace(calculationId)
            ? _parities.ToArray()
            : _parities.Where(p => p.CalculationId == calculationId).ToArray();

    public IReadOnlyList<GoldenTraceRecord> GetGoldenRecords(string? goldenCaseId = null) =>
        string.IsNullOrWhiteSpace(goldenCaseId)
            ? _goldens.ToArray()
            : _goldens.Where(g => g.GoldenCaseId == goldenCaseId).ToArray();

    public IReadOnlyList<EvidenceTrailRecord> GetEvidenceRecords(string? evidenceId = null) =>
        string.IsNullOrWhiteSpace(evidenceId)
            ? _evidences.ToArray()
            : _evidences.Where(e => e.EvidenceId == evidenceId).ToArray();

    public void Clear()
    {
        _traces.Clear();
        _parities.Clear();
        _goldens.Clear();
        _evidences.Clear();
    }
}
