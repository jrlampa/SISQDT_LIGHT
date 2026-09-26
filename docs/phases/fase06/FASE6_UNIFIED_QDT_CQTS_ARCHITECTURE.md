# Fase 6 — Arquitetura unificada QDT + CQTS

**Projeto:** ZNA855820  
**Produto:** um único aplicativo desktop Windows offline  
**Stack-base:** .NET 8 + WPF + SQLite3  
**Estado matemático:** QDT/CQTS engines ainda NO-GO; contratos e infraestrutura autorizados.

## Decisão arquitetural central

```text
Um Desktop App
  -> QDT Mode
  -> CQTS Mode
  -> mesmo Project/Version/NetworkModel
  -> mesma Topology Layer
  -> mesma SQLite application.db
  -> engines separados
```

Não haverá `qdt.db`, `cqts.db`, aplicativo QDT separado ou aplicativo CQTS separado.

## Invariantes arquiteturais

1. QDT é golden reference matemática; CQTS nunca preenche lacunas QDT.
2. Fórmula só pode ser compartilhada quando classificada `COMMON` ou `EQUIVALENT-BY-EVIDENCE`.
3. `UNKNOWN`, `BLOCKED`, `#REF!`, unidade desconhecida e dependência externa ausente nunca viram zero/default silencioso.
4. Excel, COM, VBA, Solver e links externos ficam fora do runtime final.
5. Topologia não contém fórmulas elétricas.
6. Todo resultado crítico possui `RuleId`, `FormulaId` quando aplicável, `EvidenceId`, `CalculationRunId` e `GoldenCaseId` quando houver.
7. O estado do gate é separado por subsistema; arquitetura pronta não significa motor pronto.

## Camadas

```text
Presentation
  WPF/MVVM: LADO, RAMAL, DIST. DE CARGA, árvore, resultados, evidência

Application
  ProjectService, VersionService, ImportService, EvidenceService,
  CalculationService, ParityService, ReportService

Domain
  Project, ProjectVersion, NetworkModel, Transformer, Circuit,
  Node, Edge, Branch, Load, Conductor, ElectricalParameter,
  TopologyValidator, Calculation contracts, status/invariants

Calculation
  Abstractions, QDTCalculationEngine, CQTSCalculationEngine,
  rule registry, blocked/unknown policy

Infrastructure
  SQLite repositories, migrations, WAL/backup, OOXML reader,
  evidence store, golden store, hash service

Testing
  domain, topology, import, parity P0/P1, QDT, CQTS, integration
```

## Modos

`CalculationMode.QDT` e `CalculationMode.CQTS` são obrigatórios. Um modo `UNIFIED` não será criado nesta fase: unificar o fluxo de chamada não significa unificar fórmulas.

## Evolução permitida

A primeira implementação pode entregar o shell do aplicativo único, SQLite, domínio, topologia e evidência. O cálculo QDT só entra após o gate Fase 5; CQTS entra depois de topologia e regras CQTS comprovadas.