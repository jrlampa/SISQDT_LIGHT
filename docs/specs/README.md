# Especificações Técnicas e de Arquitetura (QDT + CQTS)

Este diretório concentra todas as especificações consolidadas, modelos de domínio, topologia, contratos de cálculo, esquemas de dados e catálogos do motor SISQDT_LIGHT.

---

## Índice de Documentos Técnicos

| Documento | Descrição |
|---|---|
| [APPLICATION_ARCHITECTURE.md](APPLICATION_ARCHITECTURE.md) | Arquitetura da solução desktop unificada em .NET 8 / WPF / MVVM / SQLite. |
| [ESPECIFICACAO_QDT_CQTS_ZNA855820.md](ESPECIFICACAO_QDT_CQTS_ZNA855820.md) | Especificação consolidada da engenharia reversa QDT + CQTS do projeto ZNA855820. |
| [AUDITORIA_QDT_CQTS_ZNA855820.md](AUDITORIA_QDT_CQTS_ZNA855820.md) | Auditoria e levantamento inicial dos workbooks e macros legadas. |
| [DOMAIN_MODEL_QDT_CQTS.md](DOMAIN_MODEL_QDT_CQTS.md) | Modelo de domínio unificado (Aggregates, Entities, Value Objects e Invariantes). |
| [TOPOLOGY_MODEL_QDT_CQTS.md](TOPOLOGY_MODEL_QDT_CQTS.md) | Modelo topológico comum (árvores dirigidas radiais, circuitos Lado 1/2, validação de ciclos). |
| [CALCULATION_CONTRACTS_QDT_CQTS.md](CALCULATION_CONTRACTS_QDT_CQTS.md) | Interfaces e contratos de cálculo (`ICalculationEngine`, `CalculationRequest`, `CalculationResult`). |
| [CALCULATION_PARITY_HARNESS_SPEC.md](CALCULATION_PARITY_HARNESS_SPEC.md) | Especificação do harness de paridade e níveis de teste (P0 a P4). |
| [PRECEDENCE_GRAPH_QDT_ZNA855820.md](PRECEDENCE_GRAPH_QDT_ZNA855820.md) | Grafo de precedência e ordem de dependência das células e fórmulas das planilhas. |
| [UNITS_PRECISION_CATALOG_QDT.md](UNITS_PRECISION_CATALOG_QDT.md) | Catálogo oficial de grandezas, unidades de engenharia e regras de arredondamento. |
| [SQLITE_UNIFIED_SCHEMA.md](SQLITE_UNIFIED_SCHEMA.md) | Esquema relacional DDL unificado do banco de dados SQLite local. |
| [QDT_CQTS_CAPABILITY_MATRIX.md](QDT_CQTS_CAPABILITY_MATRIX.md) | Matriz de capacidades e recursos comparados entre os modos QDT e CQTS. |
| [QDT_CQTS_RULE_SHARING_MATRIX.md](QDT_CQTS_RULE_SHARING_MATRIX.md) | Matriz de compartilhamento e isolamento de regras matemáticas. |
| [GOLDEN_DATASET_QDT_CQTS_ZNA855820.md](GOLDEN_DATASET_QDT_CQTS_ZNA855820.md) | Conjunto de dados dourados (Golden Dataset) para testes de regressão e paridade. |
