# Arquitetura do aplicativo desktop único

## Estrutura da solução futura

O workspace atual contém documentação e nenhum `.sln`/`.csproj`. A solução futura deve ser criada somente na fase autorizada de implementação, com esta separação:

```text
src/
  QdtCqts.Desktop.Wpf
  QdtCqts.Application
  QdtCqts.Domain
  QdtCqts.Calculation.Abstractions
  QdtCqts.Calculation.Qdt
  QdtCqts.Calculation.Cqts
  QdtCqts.Infrastructure.Sqlite
  QdtCqts.Infrastructure.ExcelEvidence
  QdtCqts.Infrastructure.Parity
  QdtCqts.Tests.Domain
  QdtCqts.Tests.Topology
  QdtCqts.Tests.Import
  QdtCqts.Tests.Parity
```

## Dependências permitidas

```text
WPF -> Application -> Domain
WPF -> Application -> Infrastructure interfaces
Application -> Calculation.Abstractions
Calculation.Qdt/Cqts -> Domain + Calculation.Abstractions
Infrastructure -> Domain + Application interfaces
Tests -> todos os contratos necessários
```

Domain e engines não referenciam WPF, Excel Interop, COM, SQLite ou filesystem direto.

## Application services

- `ProjectService`: criar/duplicar/congelar versão.
- `ImportService`: importar QDT XLSM/CQTS XLSX em modo somente leitura.
- `EvidenceService`: armazenar fórmulas, células, links, nomes e erros.
- `TopologyService`: validar/traversar rede.
- `CalculationService`: selecionar `QDT` ou `CQTS`, criar run e persistir resultados.
- `ParityService`: carregar golden, comparar e gerar divergências.

## UI arquitetural

Uma janela WPF com modos/painéis:

```text
Project/Version
  -> LADO / RAMAL / DIST. DE CARGA
  -> Rede/Árvore
  -> Resultados
  -> Evidência/Parity
```

Grid e TreeView são projeções do mesmo domínio. A UI definitiva continua posterior ao motor e aos primeiros testes de paridade.

## Segurança

- `AppContext`/IPC não se aplica; WPF usa services injetados.
- Excel não é aberto pelo runtime de cálculo.
- Importação valida caminho, extensão, hash e não executa macros.
- Fórmulas e strings importadas não são executadas como código.
- Banco usa transações, foreign keys, WAL e backup consistente.

## Determinismo

`CalculationRun` registra `project_version_id`, `calculation_mode`, `algorithm_version`, `input_hash`, timestamp, status e evidência. Para inputs iguais, o resultado deve ser determinístico; timestamp não entra no hash.