namespace QdtCqts.Domain;

/// <summary>
/// Contexto de correlação assíncrono ambiente para rastreamento de operações e cálculos.
/// Permite correlacionar o fluxo operacional (CorrelationId) com a execução matemática pontual (CalculationId).
/// </summary>
public sealed class CorrelationContext
{
    private static readonly AsyncLocal<CorrelationContext?> CurrentContext = new();

    public string CorrelationId { get; }
    public string? CalculationId { get; }
    public DateTimeOffset Timestamp { get; }

    public CorrelationContext(string correlationId, string? calculationId = null)
    {
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? GenerateCorrelationId() : correlationId;
        CalculationId = calculationId;
        Timestamp = DateTimeOffset.UtcNow;
    }

    public static CorrelationContext Current => CurrentContext.Value ??= new CorrelationContext(GenerateCorrelationId());

    public static IDisposable BeginScope(string correlationId, string? calculationId = null)
    {
        var prior = CurrentContext.Value;
        CurrentContext.Value = new CorrelationContext(correlationId, calculationId);
        return new ContextScope(prior);
    }

    public static IDisposable SetCalculationId(string calculationId)
    {
        var prior = CurrentContext.Value;
        var correlationId = prior?.CorrelationId ?? GenerateCorrelationId();
        CurrentContext.Value = new CorrelationContext(correlationId, calculationId);
        return new ContextScope(prior);
    }

    public static string GenerateCorrelationId() => $"CORR-{Guid.NewGuid():N}";

    public static string GenerateCalculationId() => $"CALC-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";

    private sealed class ContextScope : IDisposable
    {
        private readonly CorrelationContext? _prior;
        private bool _disposed;

        public ContextScope(CorrelationContext? prior)
        {
            _prior = prior;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                CurrentContext.Value = _prior;
                _disposed = true;
            }
        }
    }
}
