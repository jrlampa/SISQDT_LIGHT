using Microsoft.Extensions.Logging;

namespace QdtCqts.Domain;

/// <summary>
/// Taxonomia formal de eventos estruturados do sistema SISQDT_LIGHT.
/// Convenção:
///   APP-0xx    : Ciclo de vida da aplicação e infraestrutura global
///   DB-0xx     : Banco de dados SQLite, migrações e persistência
///   IMPORT-0xx : Ingestão e parsing de evidências celulares (Excel)
///   CALC-0xx   : Ciclo de vida de execução de cálculo (request/result)
///   RULE-0xx   : Execução granular de regras matemáticas
///   TOPO-0xx   : Grafo topológico, nós, arestas e invariantes de rede
///   PARITY-0xx : Harness de paridade numérica e confronto com baseline
///   EVID-0xx   : Proveniência e rastreabilidade de evidência celular
///   GOLDEN-0xx : Validação de casos de ouro (Golden Cases)
/// </summary>
public static class CalculationEventIds
{
    // Application Lifecycle
    public static readonly EventId AppStartup = new(1001, "APP-001");
    public static readonly EventId AppShutdown = new(1002, "APP-002");
    public static readonly EventId AppUnhandledException = new(1003, "APP-003");

    // Database & Persistence
    public static readonly EventId DbOpened = new(2001, "DB-001");
    public static readonly EventId DbMigrationStarted = new(2002, "DB-002");
    public static readonly EventId DbMigrationCompleted = new(2003, "DB-003");
    public static readonly EventId DbMigrationFailed = new(2004, "DB-004");

    // Evidence Import
    public static readonly EventId ImportStarted = new(3001, "IMPORT-001");
    public static readonly EventId ImportCompleted = new(3002, "IMPORT-002");
    public static readonly EventId ImportWarning = new(3003, "IMPORT-003");
    public static readonly EventId ImportFailed = new(3004, "IMPORT-004");

    // Calculation Run Lifecycle
    public static readonly EventId CalcStarted = new(4001, "CALC-001");
    public static readonly EventId CalcRuleExecuted = new(4002, "CALC-002");
    public static readonly EventId CalcCompleted = new(4003, "CALC-003");
    public static readonly EventId CalcBlocked = new(4004, "CALC-004");

    // Rule Execution
    public static readonly EventId RuleExecuted = new(5001, "RULE-001");
    public static readonly EventId RuleFailed = new(5002, "RULE-002");
    public static readonly EventId RuleCandidateWarning = new(5003, "RULE-003");

    // Topology & Invariants
    public static readonly EventId TopoNodeCreated = new(6001, "TOPO-001");
    public static readonly EventId TopoEdgeCreated = new(6002, "TOPO-002");
    public static readonly EventId TopoParentAssigned = new(6003, "TOPO-003");
    public static readonly EventId TopoValidated = new(6004, "TOPO-004");
    public static readonly EventId TopoInvariantViolation = new(6005, "TOPO-005");

    // Parity Verification
    public static readonly EventId ParityMatch = new(7001, "PARITY-001");
    public static readonly EventId ParityMismatch = new(7002, "PARITY-002");

    // Evidence Provenance
    public static readonly EventId EvidCaptured = new(8001, "EVID-001");
    public static readonly EventId EvidMissingOrIncomplete = new(8002, "EVID-002");

    // Golden Case Execution
    public static readonly EventId GoldenPass = new(9001, "GOLDEN-001");
    public static readonly EventId GoldenMismatch = new(9002, "GOLDEN-002");
}
