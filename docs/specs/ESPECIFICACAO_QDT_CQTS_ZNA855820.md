# Especificacao consolidada QDT + CQTS

**Projeto:** ZNA855820  
**Fase:** 2 - consolidacao da engenharia reversa  
**Data:** 2026-09-26  
**Persistencia oficial:** SQLite3  
**Referencia de calculo:** QDT Excel  
**Status:** especificacao executavel para fundacao, banco, dominio e validacao; motor eletrico ainda bloqueado por lacunas de paridade listadas neste documento. A matriz de bloqueios e a especificacao UX da Fase 3 estao em [FASE3_BLOQUEIOS_UX_QDT_CQTS_ZNA855820.md](FASE3_BLOQUEIOS_UX_QDT_CQTS_ZNA855820.md), e o readiness report da Fase 4 em [FASE4_CALCULATION_ENGINE_READINESS_QDT_CQTS_ZNA855820.md](FASE4_CALCULATION_ENGINE_READINESS_QDT_CQTS_ZNA855820.md).

## 1. Decisao executiva

A aplicacao deve ser um desktop Windows offline com motor nativo, SQLite como persistencia e Excel apenas como oraculo/importador controlado. A recomendacao atual e **.NET 8 + WPF + MVVM + Microsoft.Data.Sqlite**, com o dominio e os motores em assemblies independentes da UI.

A decisao nao autoriza ainda a implementacao das formulas eletricas finais. O que esta autorizado e construir a fundacao de dados, evidencia, rastreabilidade e harness de paridade. A implementacao do `QdtCalculationModel` so pode iniciar depois da extracao completa dos precedentes, constantes, unidades e diferencas VBA relevantes.

## 2. Validacao e correcoes da auditoria anterior

A auditoria inicial em [AUDITORIA_QDT_CQTS_ZNA855820.md](AUDITORIA_QDT_CQTS_ZNA855820.md) permanece como registro de evidencias, mas esta especificacao prevalece quando detalha correcoes verificadas diretamente:

| Tema | Conclusao consolidada |
|---|---|
| QDT nomes definidos | Os 7 nomes expostos pelo Excel sao majoritariamente `Print_Area`; nao devem ser tratados como parametros eletricos. |
| QDT link externo | O Excel expôs o link `DecInv` para um arquivo legado em `J:\Celeridade 2016_3_SET\...xlsm`; o pacote possui duas partes XML de link. O conteudo externo nao foi fornecido. |
| CQTS nomes | Ha nomes globais e nomes com escopo de aba, inclusive parametros `solver_*`, aliases de tabela e nomes quebrados `RAMA_PROJ1`, `RAMA_PROJ2`, `RAMAIS_PROJ1` com `#REF!`. |
| CQTS campos | Foram confirmados `PONTO`, `PONTO MONTANTE`, `TRECHO`, `COMPRIMENTO`, coordenadas, `CARGA PONTO`, `ACUMULADA`, `CONDUTOR`, `FASE`, `Ib`, `In`, `Iz`, `PROTECAO` e `CQT`. |
| VBA | `olevba` extraiu 15 componentes no QDT. O mesmo conjunto nominal aparece no PROJ, mas os binarios SHA-256 sao diferentes; deve haver comparacao textual por modulo antes de concluir igualdade. |
| Macro eletrica | Nao foi encontrada, no material extraido, uma rotina que calcule diretamente a rede. O VBA observado atua em selecao, layout, protecao, filtros, formulario e inicializacao; isso nao elimina dependencias nao descobertas. |
| Solver | Os nomes `solver_*` no CQTS indicam configuracao de Solver/otimizacao em `RAMAL` e `PROJ1`; seu uso efetivo e resultado precisam ser testados antes de reproduzir. |

## 3. Escopo, linguagem e limites

### 3.1 Linguagem ubíqua

- **Projeto:** conjunto persistente de versões de uma instalação/rede.
- **Versão:** snapshot nomeado, como `ATUAL`, `PROJ` ou uma versão futura.
- **Modelo:** `QDT` ou `CQTS`; não são sinônimos.
- **Ponto/Nó:** entidade topológica identificada por `PONTO`.
- **Ponto montante:** origem elétrica imediata do trecho.
- **Trecho/Edge:** ligação dirigida entre ponto montante e ponto destino.
- **Lado/Circuito:** agrupamento elétrico calculado a partir do transformador.
- **Ramal:** agrupamento de carga/condutor de baixa tensão observado em `Ramais`/`RAMAL`.
- **Carga no ponto:** demanda atribuída diretamente ao ponto.
- **Acumulada:** carga propagada que alimenta o ponto/trecho.
- **Condutor:** item de catálogo com ampacidade e parâmetros elétricos.
- **Run:** execução reproduzível do motor sobre um snapshot de entradas.
- **Golden dataset:** entradas e saídas capturadas do Excel, com origem celular.
- **Paridade:** igualdade textual para status e igualdade numérica com tolerância justificada para valores que não possam ser bit-a-bit.

### 3.2 Bounded contexts

```text
Reference Evidence
  Arquivos, abas, células, fórmulas, nomes, tabelas, VBA, links e caches.

Project Domain
  Projeto, versões, transformador, circuitos, nós, trechos, cargas e condutores.

Calculation
  QDT engine e CQTS engine; sem dependência de UI ou Excel.

Parity and QA
  Golden cases, runs, comparação, rastreabilidade e regressão.
```

O sistema não precisa de event sourcing. Versionamento por snapshot e `calculation_runs` imutáveis fornecem auditabilidade suficiente para esta fase.

## 4. Entidades de domínio

| Entidade | Finalidade | Campos mínimos confirmados/propostos | Unidade | Origem |
|---|---|---|---|---|
| Project | Identidade persistente da rede | `id`, `code`, `name`, `created_at` | - | nome/projeto Excel |
| ProjectVersion | Snapshot ATUAL/PROJ/futuro | `id`, `project_id`, `label`, `model_kind`, `source_hash`, `status` | - | arquivos distintos |
| Transformer | Fonte e parâmetros do circuito | `id`, `version_id`, `name`, `power`, `impedance`, `line_voltage`, `demand` | MVA, %, kV, kVA | QDT `LADO`, CQTS `DB` |
| Circuit | Lado/circuito calculável | `id`, `version_id`, `transformer_id`, `code`, `side_index`, `model_kind` | - | QDT LADO 1/2; CQTS LADO 1..4 |
| Node | Ponto da rede | `id`, `circuit_id`, `external_key`, `parent_node_id`, `x`, `y`, `load_kva` | coordenada, kVA | CQTS `PONTO`, `COORDENADAS` |
| Edge | Trecho dirigido | `id`, `circuit_id`, `from_node_id`, `to_node_id`, `length`, `conductor_id`, `phase`, `installation` | m | CQTS `TRECHO`, QDT linhas |
| Branch | Agrupamento de ramificação | `id`, `circuit_id`, `root_node_id`, `label` | - | conceito CQTS; confirmar origem celular |
| Load | Carga direta ou ramal | `id`, `node_id`, `branch_id`, `kind`, `clients`, `kva`, `factor` | clientes, kVA | `CARGA PONTO`, `RAMAL`, `PROJ1` |
| Conductor | Catálogo elétrico | `id`, `catalog_key`, `name`, `ampacity`, `r_ohm`, `x_ohm`, `metadata` | A, ohm/km ou unidade a confirmar | CQTS `CABOS`, QDT `Ramais` |
| ElectricalParameter | Parâmetro nomeado | `id`, `version_id`, `key`, `value_numeric`, `value_text`, `unit`, `source` | variável | nomes/DB/abas auxiliares |
| CalculationInput | Snapshot normalizado de entrada | `run_id`, `entity_type`, `entity_id`, `key`, `value_*`, `source_ref` | conforme chave | domínio + evidência |
| CalculationRun | Execução imutável | `id`, `version_id`, `engine`, `input_hash`, `status`, `started_at`, `completed_at` | - | aplicação |
| EdgeResult | Resultado por trecho | corrente, tensão, queda, acumulada, `ib`, `in`, `iz`, status | variáveis elétricas | QDT/CQTS |
| NodeResult | Resultado por ponto | carga, acumulada, tensão, corrente, status | variáveis elétricas | CQTS |
| CircuitResult | Consolidação por lado | demanda, corrente máxima, tensão mínima, status | variáveis elétricas | relatórios |
| Validation | Regra e resultado | `code`, `scope`, `severity`, `status`, `message`, `source_ref` | - | `OK`, `VERIFICAR`, `Erro 02`, sobrecarga |
| SourceArtifact | Arquivo/aba/célula de referência | hash, workbook, sheet, address, formula, cached_value | - | Excel |
| TraceabilityRule | Regra rastreável | origem, regra, implementação futura, teste | - | auditoria |
| GoldenCase | Caso congelado | entradas, esperados, tolerâncias, origem | - | Excel/oráculo |

Campos não confirmados, como `Branch` persistente ou unidade exata de `R`, `X` e `T`, devem permanecer marcados como `pending_verification` no catálogo de metadados, não escondidos como decisão definitiva.

## 5. Topologia

### 5.1 Modelo

O modelo comum deve ser um grafo dirigido, com validação de radialidade por circuito. A configuração observada no CQTS é compatível com uma árvore: um ponto tem no máximo um montante e pode ter vários descendentes. A orientação elétrica é `from_node_id -> to_node_id`.

```mermaid
erDiagram
    PROJECT ||--o{ PROJECT_VERSION : contains
    PROJECT_VERSION ||--o{ CIRCUIT : has
    CIRCUIT ||--|| TRANSFORMER : uses
    CIRCUIT ||--o{ NODE : contains
    CIRCUIT ||--o{ EDGE : contains
    NODE ||--o{ NODE : parent_of
    NODE ||--o{ LOAD : receives
    EDGE }o--|| NODE : from
    EDGE }o--|| NODE : to
    EDGE }o--o| CONDUCTOR : uses
```

### 5.2 Invariantes

- `from_node_id` e `to_node_id` pertencem ao mesmo circuito.
- Um circuito possui uma fonte identificável no transformador.
- Um nó não fonte possui no máximo um pai para o modelo radial.
- Não são aceitos ciclos no `QdtEngine` nem no `CqtsEngine` enquanto a planilha não provar suporte.
- Um trecho não pode ter origem e destino iguais.
- Comprimento não pode ser negativo; zero só será aceito se houver evidência do caso Excel.
- Um nó terminal é um nó sem filhos; um nó intermediário possui um ou mais filhos.
- A carga direta fica no nó/ramal; a carga acumulada é resultado, nunca entrada duplicada.

### 5.3 Separação topologia/cálculo

`TopologyValidator` constrói ordem topológica, detecta ciclo, desconexão, múltiplos pais e pontos faltantes. Ele não calcula corrente ou queda. Os motores recebem a topologia validada e produzem resultados sem conhecer tabelas, planilhas ou controles de interface.

## 6. Modelo QDT

### 6.1 Boundary

O QDT deve ser implementado como engine própria para os dois lados, com entradas normalizadas de transformador, circuito, trechos, ramais, condutores e parâmetros. Não se deve projetar um parser genérico de Excel como dependência do cálculo.

### 6.2 Regras confirmadas

- `Ramais` possui tabela de ampacidade/carga por condutor.
- `LADO 1` e `LADO 2` usam colunas como trecho, número de consumidores, kVA por consumidor, fator de divisão, fases, carga escolhida, temperatura, corrente e validações.
- Há seleção entre duas cargas calculadas por `=IF($CH$5="SIM",K13,L13)`.
- Há validação lógica com mensagens `OK !`, `Erro 02` e `Erro 05 !`.
- A composição observada em `Ramais` é `=entrada_1*0.85+entrada_2*0.5268`.
- O bloco `AM1/AM2` depende de `B255="SIM"` e pode impedir a operação quando o estado de macro não estiver pronto.

### 6.3 Fórmulas ainda não congeladas

As amostras não são suficientes para declarar um `QdtEngine` completo. Ainda faltam, por regra e coluna:

- significado e unidade de cada coluna de `LADO 1/2`;
- fórmula completa de queda de tensão e curto-circuito;
- precedentes das células com fórmulas compartilhadas;
- origem dos coeficientes `0.85`, `0.5268`, `4`, `8`, `6`, `30`, `60`;
- comportamento de `CH5`, `B255`, `AR`, `AM` e demais flags;
- impacto das rotinas que alteram layout/proteção e dos links para `DecInv`;
- arredondamento e formatos que possam afetar o valor usado por comparações.

## 7. Modelo CQTS

### 7.1 Estrutura confirmada

As tabelas `LADO 1` a `LADO 4` e variantes de projeto possuem os campos reais:

```text
ID LIGHT, PONTO, PONTO MONTANTE, TRECHO (m), CLT ACUM,
COORDENADAS UTM, CARGA PONTO (kVA), ACUMULADO (kVA), CONDUTOR,
K(A), T, R CORR, QT-PONTO, QT ACUMULADA, QT %, TENSÃO PONTO,
TEMP2, FASE, ETA, METODO_INST, TEMP_AMB, Ib, In, Iz, PROTECAO.
```

`A8="TR, LID"`, `A9="LID, P2"` e `A10="P2, P3"` demonstram a representação de trecho/origem-destino no arquivo atual. `C9=1` e `C10=2` demonstram a referência ao trecho/ponto montante em uma cadeia linear; os múltiplos lados demonstram as ramificações do projeto completo.

### 7.2 Regras confirmadas

- `ACUMULADA` é propagada por consultas e somas condicionais.
- `SUMPRODUCT(I8:I23,F8:F23)/SUM(I8:I23)` calcula média ponderada de centro de carga.
- `Ib`, `In`, `Iz` e `PROTECAO` são comparados para produzir `OK`/`VERIFICAR`.
- A corrente usa caminho monofásico ou trifásico conforme `FASE`, com `SQRT(3)` no caminho não monofásico.
- Condutor, tensão, método de instalação e temperatura influenciam os resultados.
- `PROJ1`, `GERAL PROJ` e tabelas de lado projetado consolidam condutores, metragem, demanda e saturação.

### 7.3 Solver e nomes quebrados

Os nomes `solver_*` devem ser importados como evidência/configuração, mas não executados ou traduzidos para um otimizador sem um caso demonstrando a função objetivo, variáveis, restrições e saída. Os nomes `RAMA_PROJ1`, `RAMA_PROJ2` e `RAMAIS_PROJ1` com `#REF!` são inconsistências de origem e devem gerar `Validation` de severidade `high`; não devem ser corrigidos silenciosamente.

## 8. VBA consolidado

### 8.1 Inventário

| Componente | Tipo | Rotinas | Impacto |
|---|---|---|---|
| `Módulo1` | BAS | `Sequencial_DI`, `Orgao_DI`, `Assina_*`, `Retorna`, `Dropdown21_Alteração`, `Filtrar`, `Mostrar` | selecao, ocultacao de linhas, controles, layout, protecao e filtro |
| `EstaPasta_de_trabalho` | classe | `Workbook_Open` | seleciona `Menu` ao abrir |
| `UserForm1` | formulário | `DesprotegeVBA`, `CopyFile`, `CommandButton1_Click` | copia arquivo, abre arquivo binário e tenta desproteger VBA; risco alto, não é lógica elétrica |
| `Módulo3` | BAS | `Macro2`, `Macro3` | seleção e desproteção da planilha |
| `Módulo2` | BAS | vazio | sem efeito observado |
| `Plan21`, `Plan26`, `Plan31`, `Plan25`, `Plan27`, `Plan19`, `Plan22`, `Plan18`, `Plan32`, `Plan28` | classes de planilha | vazias | sem evento observado na extração |

### 8.2 Consequências

- O VBA extraído não prova uma rotina de cálculo elétrico, mas prova automação de abertura, proteção, filtro, visibilidade e alteração de fórmulas/layout.
- `Workbook_Open` deve ser representado no importador como metadado de comportamento, não executado.
- O formulário de desproteção deve ser isolado e classificado como código legado inseguro; não deve ser portado para o aplicativo.
- O binário ATUAL e PROJ tem hashes diferentes. Deve-se comparar os módulos descompilados e registrar a diferença textual; não deduzir que a diferença é apenas assinatura/metadado.

### 8.3 Regra de portabilidade

Portar somente efeitos funcionais verificáveis e seguros: estado de visibilidade, filtro e proteção podem virar operações explícitas da UI. Não portar `Open`, `Put`, `Binary`, `CopyFile` ou rotina de quebra de senha.

## 9. Links, nomes e precedentes

### 9.1 Link externo

O link Excel exposto é `DecInv`, apontando para arquivo legado em unidade `J:`. Como o arquivo não foi fornecido, qualquer célula que dependa dele deve ser marcada como `external_dependency_missing`. O importador deve conservar o caminho, workbook, célula e fórmula, mas não substituir o valor por zero.

### 9.2 Estratégia de precedentes

O importador de evidência deve produzir `source_formula_edges` com:

- workbook, sheet, cell;
- formula original e fórmula normalizada;
- precedentes internos explícitos;
- nome definido referenciado;
- tabela/coluna estruturada;
- referência externa;
- tipo `direct`, `named_range`, `structured_reference`, `dynamic`, `vba`, `external`;
- estado `resolved`, `missing`, `not_supported`.

Pesquisar e registrar especificamente `INDIRECT`, `OFFSET`, `INDEX`, `MATCH`, `XLOOKUP/PROCX`, referências 3D, fórmulas matriciais e `#REF!`. Nenhuma dessas ocorrências deve ser reinterpretada como fórmula elétrica sem evidência.

## 10. Schema SQLite lógico

### 10.1 Regras de banco

- SQLite 3 com `PRAGMA foreign_keys = ON`.
- WAL para execução desktop com leitura e escrita curtas; checkpoint no fechamento/backup.
- Todas as alterações de snapshot em transação.
- `project_versions` é imutável após congelamento; edições criam nova versão.
- `calculation_runs` e resultados são imutáveis; uma nova entrada gera novo `input_hash`.
- O banco pode armazenar JSON auxiliar em `metadata_json`, mas JSON não é a fonte do domínio.
- Migrações numeradas, transacionais e registradas em `schema_migrations`.

### 10.2 DDL inicial executável

```sql
PRAGMA foreign_keys = ON;

CREATE TABLE schema_migrations (
    version INTEGER PRIMARY KEY,
    applied_at TEXT NOT NULL
);

CREATE TABLE projects (
    id TEXT PRIMARY KEY,
    code TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE TABLE project_versions (
    id TEXT PRIMARY KEY,
    project_id TEXT NOT NULL REFERENCES projects(id),
    label TEXT NOT NULL,
    model_kind TEXT NOT NULL CHECK (model_kind IN ('QDT', 'CQTS')),
    source_workbook TEXT,
    source_hash TEXT,
    state TEXT NOT NULL CHECK (state IN ('draft', 'frozen', 'superseded')),
    parent_version_id TEXT REFERENCES project_versions(id),
    created_at TEXT NOT NULL,
    UNIQUE(project_id, label)
);

CREATE TABLE transformers (
    id TEXT PRIMARY KEY,
    version_id TEXT NOT NULL REFERENCES project_versions(id),
    external_key TEXT,
    name TEXT,
    power_mva REAL,
    impedance_percent REAL,
    line_voltage_kv REAL,
    demand_kva REAL,
    metadata_json TEXT
);

CREATE TABLE circuits (
    id TEXT PRIMARY KEY,
    version_id TEXT NOT NULL REFERENCES project_versions(id),
    transformer_id TEXT NOT NULL REFERENCES transformers(id),
    external_key TEXT NOT NULL,
    side_index INTEGER,
    name TEXT,
    model_kind TEXT NOT NULL CHECK (model_kind IN ('QDT', 'CQTS')),
    UNIQUE(version_id, external_key)
);

CREATE TABLE nodes (
    id TEXT PRIMARY KEY,
    circuit_id TEXT NOT NULL REFERENCES circuits(id),
    external_key TEXT NOT NULL,
    parent_node_id TEXT REFERENCES nodes(id),
    x REAL,
    y REAL,
    coordinate_system TEXT,
    is_source INTEGER NOT NULL DEFAULT 0 CHECK (is_source IN (0,1)),
    metadata_json TEXT,
    UNIQUE(circuit_id, external_key)
);

CREATE TABLE conductors (
    id TEXT PRIMARY KEY,
    version_id TEXT NOT NULL REFERENCES project_versions(id),
    catalog_key TEXT NOT NULL,
    name TEXT NOT NULL,
    ampacity_a REAL,
    resistance_ohm REAL,
    reactance_ohm REAL,
    metadata_json TEXT,
    UNIQUE(version_id, catalog_key)
);

CREATE TABLE edges (
    id TEXT PRIMARY KEY,
    circuit_id TEXT NOT NULL REFERENCES circuits(id),
    from_node_id TEXT NOT NULL REFERENCES nodes(id),
    to_node_id TEXT NOT NULL REFERENCES nodes(id),
    external_key TEXT,
    length_m REAL NOT NULL CHECK (length_m >= 0),
    conductor_id TEXT REFERENCES conductors(id),
    phase TEXT,
    installation_method TEXT,
    ambient_temperature_c REAL,
    metadata_json TEXT,
    CHECK (from_node_id <> to_node_id),
    UNIQUE(circuit_id, external_key)
);

CREATE TABLE branches (
    id TEXT PRIMARY KEY,
    circuit_id TEXT NOT NULL REFERENCES circuits(id),
    root_node_id TEXT NOT NULL REFERENCES nodes(id),
    external_key TEXT,
    label TEXT,
    UNIQUE(circuit_id, external_key)
);

CREATE TABLE loads (
    id TEXT PRIMARY KEY,
    version_id TEXT NOT NULL REFERENCES project_versions(id),
    node_id TEXT REFERENCES nodes(id),
    branch_id TEXT REFERENCES branches(id),
    kind TEXT NOT NULL CHECK (kind IN ('point', 'ramal', 'client', 'distributed', 'unknown')),
    external_key TEXT,
    clients REAL,
    kva REAL,
    factor REAL,
    source_ref TEXT,
    metadata_json TEXT
);

CREATE TABLE electrical_parameters (
    id TEXT PRIMARY KEY,
    version_id TEXT NOT NULL REFERENCES project_versions(id),
    key TEXT NOT NULL,
    value_numeric REAL,
    value_text TEXT,
    unit TEXT,
    source_ref TEXT,
    certainty TEXT NOT NULL CHECK (certainty IN ('confirmed', 'inferred', 'unknown')),
    UNIQUE(version_id, key)
);

CREATE TABLE calculation_runs (
    id TEXT PRIMARY KEY,
    version_id TEXT NOT NULL REFERENCES project_versions(id),
    engine TEXT NOT NULL CHECK (engine IN ('QDT', 'CQTS')),
    input_hash TEXT NOT NULL,
    algorithm_version TEXT NOT NULL,
    status TEXT NOT NULL CHECK (status IN ('running', 'succeeded', 'failed', 'invalidated')),
    started_at TEXT NOT NULL,
    completed_at TEXT,
    error_text TEXT,
    UNIQUE(version_id, engine, input_hash, algorithm_version)
);

CREATE TABLE node_results (
    id TEXT PRIMARY KEY,
    run_id TEXT NOT NULL REFERENCES calculation_runs(id),
    node_id TEXT NOT NULL REFERENCES nodes(id),
    load_kva REAL,
    accumulated_kva REAL,
    voltage_v REAL,
    current_a REAL,
    voltage_drop REAL,
    status TEXT,
    values_json TEXT,
    UNIQUE(run_id, node_id)
);

CREATE TABLE edge_results (
    id TEXT PRIMARY KEY,
    run_id TEXT NOT NULL REFERENCES calculation_runs(id),
    edge_id TEXT NOT NULL REFERENCES edges(id),
    accumulated_kva REAL,
    current_a REAL,
    ib_a REAL,
    in_a REAL,
    iz_a REAL,
    voltage_v REAL,
    voltage_drop REAL,
    protection_status TEXT,
    status TEXT,
    values_json TEXT,
    UNIQUE(run_id, edge_id)
);

CREATE TABLE circuit_results (
    id TEXT PRIMARY KEY,
    run_id TEXT NOT NULL REFERENCES calculation_runs(id),
    circuit_id TEXT NOT NULL REFERENCES circuits(id),
    demand_kva REAL,
    max_current_a REAL,
    min_voltage_v REAL,
    status TEXT,
    values_json TEXT,
    UNIQUE(run_id, circuit_id)
);

CREATE TABLE validations (
    id TEXT PRIMARY KEY,
    run_id TEXT REFERENCES calculation_runs(id),
    version_id TEXT NOT NULL REFERENCES project_versions(id),
    code TEXT NOT NULL,
    scope_type TEXT,
    scope_id TEXT,
    severity TEXT NOT NULL CHECK (severity IN ('info', 'low', 'medium', 'high', 'blocking')),
    status TEXT NOT NULL CHECK (status IN ('pass', 'fail', 'unknown', 'not_applicable')),
    message TEXT NOT NULL,
    source_ref TEXT
);
```

### 10.3 Tabelas de evidência e paridade

```sql
CREATE TABLE source_artifacts (
    id TEXT PRIMARY KEY,
    path TEXT NOT NULL,
    file_name TEXT NOT NULL,
    file_type TEXT NOT NULL,
    sha256 TEXT NOT NULL,
    captured_at TEXT NOT NULL
);

CREATE TABLE source_cells (
    id TEXT PRIMARY KEY,
    artifact_id TEXT NOT NULL REFERENCES source_artifacts(id),
    sheet_name TEXT NOT NULL,
    address TEXT NOT NULL,
    formula TEXT,
    cached_value_text TEXT,
    cached_value_numeric REAL,
    classification TEXT CHECK (classification IN ('input', 'intermediate', 'result', 'unknown')),
    UNIQUE(artifact_id, sheet_name, address)
);

CREATE TABLE source_formula_edges (
    id TEXT PRIMARY KEY,
    source_cell_id TEXT NOT NULL REFERENCES source_cells(id),
    precedent_ref TEXT NOT NULL,
    ref_kind TEXT NOT NULL CHECK (ref_kind IN ('direct', 'named_range', 'structured', 'dynamic', 'vba', 'external')),
    resolution_status TEXT NOT NULL CHECK (resolution_status IN ('resolved', 'missing', 'not_supported'))
);

CREATE TABLE traceability_rules (
    id TEXT PRIMARY KEY,
    rule_code TEXT NOT NULL UNIQUE,
    description TEXT NOT NULL,
    source_ref TEXT NOT NULL,
    future_component TEXT,
    verification_status TEXT NOT NULL CHECK (verification_status IN ('confirmed', 'inferred', 'unknown'))
);

CREATE TABLE golden_cases (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    artifact_id TEXT REFERENCES source_artifacts(id),
    version_id TEXT REFERENCES project_versions(id),
    engine TEXT NOT NULL CHECK (engine IN ('QDT', 'CQTS')),
    status TEXT NOT NULL CHECK (status IN ('draft', 'frozen', 'blocked')),
    tolerance_abs REAL,
    tolerance_rel REAL,
    notes TEXT
);

CREATE TABLE golden_observations (
    id TEXT PRIMARY KEY,
    case_id TEXT NOT NULL REFERENCES golden_cases(id),
    source_cell_id TEXT REFERENCES source_cells(id),
    path TEXT NOT NULL,
    expected_text TEXT,
    expected_numeric REAL,
    unit TEXT,
    comparison TEXT NOT NULL CHECK (comparison IN ('exact_text', 'exact_numeric', 'tolerance'))
);

CREATE INDEX ix_nodes_circuit ON nodes(circuit_id);
CREATE INDEX ix_edges_circuit ON edges(circuit_id);
CREATE INDEX ix_edges_from ON edges(from_node_id);
CREATE INDEX ix_edges_to ON edges(to_node_id);
CREATE INDEX ix_loads_node ON loads(node_id);
CREATE INDEX ix_results_run ON edge_results(run_id);
CREATE INDEX ix_validations_run ON validations(run_id);
CREATE INDEX ix_source_cells_sheet ON source_cells(artifact_id, sheet_name);
```

A constraint adicional necessária no código transacional é verificar que os nós referenciados por uma edge pertencem ao mesmo circuito. SQLite não expressa essa regra apenas com as FKs atuais sem chaves compostas; a primeira migração deve adicionar FKs compostas ou um `TopologyValidator` obrigatório antes do commit.

## 11. Entrada e saída do motor

```text
CalculationRequest
  versionId
  engineKind: QDT | CQTS
  transformer
  circuits[]
  topology
  loads[]
  conductors[]
  parameters
  calculationOptions

CalculationResult
  runMetadata
  transformerResults[]
  circuitResults[]
  branchResults[]
  nodeResults[]
  edgeResults[]
  terminalResults[]
  validations[]
  trace[]
```

O request deve ser um objeto de domínio validado, nunca uma coleção de células. O result deve manter resultados intermediários mesmo quando a UI exibe apenas o resumo.

## 12. Resultados, hash e invalidação

- Calcular sob demanda e persistir o `CalculationRun` bem-sucedido.
- O hash deve cobrir versão congelada, entradas normalizadas, parâmetros, topologia, engine e `algorithm_version`.
- Alterar carga, condutor, comprimento, transformador, topologia ou parâmetro cria nova versão ou marca runs anteriores como `invalidated`.
- Nunca sobrescrever resultados de um run anterior.
- A UI deve exibir `stale` quando o hash atual não coincide com o último resultado.
- Falha de cálculo não deve apagar o último resultado; deve criar run `failed` com erro e manter o anterior explicitamente marcado como antigo.

## 13. Paridade e golden dataset

### 13.1 Pipeline

```text
Excel somente leitura
  -> extractor de células/formulas/valores/VBA
  -> GoldenCase congelado no SQLite
  -> CalculationRequest
  -> engine nativa
  -> CalculationResult
  -> comparação por caminho/origem
```

### 13.2 Casos

| ID | Fonte | Estado | Observações |
|---|---|---|---|
| TEST-QDT-001 | QDT ATUAL | bloqueado | capturar todos os resultados de `LADO 1/2` |
| TEST-QDT-002 | QDT PROJ | bloqueado | comparar somente depois de confirmar entradas alteradas |
| TEST-CQTS-001 | CQTS ATUAL | parcial | `LADO 1` linhas 8-10 já fornecem valores cacheados |
| TEST-CQTS-002 | CQTS PROJ | bloqueado | capturar `PROJ1`, `GERAL PROJ` e lados projetados |
| TEST-TOPO-001 | CQTS ATUAL | bloqueado | cadeia `TR,LID -> LID,P2 -> P2,P3` |
| TEST-TOPO-002 | CQTS PROJ | bloqueado | ramificações e múltiplos lados |
| TEST-INVALID-001 | CQTS | bloqueado | nomes `#REF!`, ponto faltante e ciclo artificial |
| TEST-VBA-001 | QDT | bloqueado | verificar efeito de `B255` sem executar macro perigosa |

### 13.3 Comparação

- status/texto: igualdade exata;
- identificadores e contagens: igualdade exata;
- valores numéricos: primeiro tentar igualdade após preservar a mesma ordem de operações; usar tolerância apenas quando a divergência for de representação;
- cada divergência deve apontar para `source_cell`, fórmula, operandos, unidade e run.

A tolerância inicial de diagnóstico do relatório anterior (`1e-12 * max(1, abs(excel))`) não é ainda uma decisão final. Deve ser calibrada por família de variável após o golden dataset real existir.

## 14. Arquitetura do aplicativo

```text
WPF / MVVM UI
    |
Application Layer
    |-- ProjectApplicationService
    |-- CalculationApplicationService
    |-- ImportEvidenceService
    |-- ParityComparisonService
    |
Domain Layer
    |-- Project / Version / Topology aggregates
    |-- Transformer / Circuit / Node / Edge / Load / Conductor
    |-- TopologyValidator
    |
Calculation Layer
    |-- QdtCalculationModel
    |-- CqtsCalculationModel
    |-- CurrentCalculator
    |-- VoltageDropCalculator
    |-- LoadingValidator
    |
Infrastructure
    |-- SQLite repositories
    |-- Excel OOXML reader
    |-- Excel COM oracle adapter (test/import only)
    |-- VBA/evidence extractor
```

O cálculo não referencia WPF, Excel Interop, controles ou SQLite. Repositórios e adaptadores vivem fora do domínio. A UI não acessa SQLite diretamente.

## 15. Escolha tecnológica

### Recomendação: .NET 8 + WPF

- Windows e offline são o contexto primário.
- SQLite e transações têm suporte maduro.
- WPF suporta interface densa de planilha, árvore, tabelas e inspeção de resultados.
- `System.Double` corresponde ao uso normal de dupla precisão do Excel; a ordem das operações continua responsabilidade do engine.
- Integração futura com AutoCAD e Excel é mais direta no ecossistema Windows/.NET.
- O cálculo pode ser testado sem iniciar a UI.

### Alternativas rejeitadas nesta fase

- **Electron:** viável, mas acrescenta runtime, IPC, empacotamento e superfície de segurança sem vantagem clara para um desktop Windows offline técnico.
- **Python/Qt:** boa velocidade de prototipação, porém maior esforço de distribuição e integração corporativa Windows para o produto final.
- **WinUI:** não oferece vantagem suficiente sobre WPF para a primeira versão; WPF tem menor risco de compatibilidade em ambientes existentes.

## 16. Segurança e integridade

- Nunca executar macros importadas.
- Nunca portar a rotina `DesprotegeVBA`.
- Importar arquivos em modo somente leitura e calcular hashes.
- Validar tamanho, extensão e caminho antes de ler.
- SQLite com `foreign_keys`, transações e backup consistente.
- Sanitizar qualquer exportação de fórmula/texto para evitar injeção de fórmula em arquivos futuros.
- Registrar arquivos externos ausentes; não buscar automaticamente em unidades de rede sem ação explícita.
- Separar dados importados de dados aprovados pelo usuário.

## 17. Backlog técnico

### Fase 1 - Fundação

- [ ] criar solução .NET e projetos Domain/Application/Calculation/Infrastructure/UI/Tests;
- [ ] definir IDs, unidades, enums e erros de domínio;
- [ ] adicionar política de precisão e comparação;
- [ ] implementar hash determinístico do snapshot.

### Fase 2 - Banco

- [ ] aplicar migração SQLite inicial;
- [ ] ligar FKs e transações;
- [ ] adicionar FKs compostas ou validação transacional de circuito/nó/edge;
- [ ] implementar backup e restauração;
- [ ] implementar `stale`/invalidação.

### Fase 3 - Domínio

- [ ] implementar agregados `ProjectVersion` e `Circuit`;
- [ ] implementar `Node`, `Edge`, `Load`, `Conductor`, `Transformer`;
- [ ] validar árvore radial;
- [ ] congelar glossário e catálogo de unidades.

### Fase 4 - Motor QDT

- [ ] extrair fórmulas críticas completas;
- [ ] mapear cada coluna QDT;
- [ ] implementar uma regra por vez com teste de origem;
- [ ] validar intermediários de ATUAL e PROJ;
- [ ] bloquear release enquanto VBA/link/precedente crítico estiver pendente.

### Fase 5 - Motor CQTS

- [ ] implementar acumulação por ponto/trecho;
- [ ] implementar corrente, `Ib/In/Iz`, tensão e queda;
- [ ] tratar lados projetados sem misturar QDT;
- [ ] investigar Solver antes de qualquer otimização.

### Fase 6 - Topologia

- [ ] importar cadeia de pontos;
- [ ] suportar ramificações;
- [ ] detectar ciclos, desconexões e múltiplos pais;
- [ ] gerar resultados por ponta e ramo.

### Fase 7 - UI

- [ ] editor de projeto/versão;
- [ ] tabela de circuitos e trechos;
- [ ] árvore topológica;
- [ ] painel de resultados e validações;
- [ ] visualização de rastreabilidade.

### Fase 8 - Importação

- [ ] importar OOXML sem executar VBA;
- [ ] importar tabelas e nomes com escopo;
- [ ] registrar fórmulas, caches e precedentes;
- [ ] resolver ou marcar links externos;
- [ ] criar snapshot no SQLite.

### Fase 9 - Validação

- [ ] congelar golden datasets;
- [ ] comparar por célula/caminho;
- [ ] executar regressão em cada build;
- [ ] medir tolerância por variável;
- [ ] produzir relatório de divergências.

### Fase 10 - Release

- [ ] instalador Windows;
- [ ] migração/backup;
- [ ] logs e suporte;
- [ ] documentação de limitações;
- [ ] aprovação formal de paridade QDT.

## 18. Matriz de rastreabilidade consolidada

| Regra | Origem | Domínio | Componente | Teste | Estado |
|---|---|---|---|---|---|
| CALC-001 composição de carga | `QDT!Ramais`, `CQTS!RAMAL` | `Load` | `LoadAggregation` | TEST-QDT-001 | confirmado, falta unidade |
| CALC-002 kVA por consumidor | `QDT!LADO 1!K13` | `Load` | `QdtCalculationModel` | TEST-QDT-001 | fórmula amostrada |
| CALC-003 seleção K/L | `QDT!LADO 1!M13` | `ElectricalParameter` | `QdtCalculationModel` | TEST-QDT-001 | confirmado |
| CALC-004 validação lógica | `QDT!LADO 1!I13` | `Validation` | `LoadingValidator` | TEST-QDT-001 | confirmado |
| CALC-005 média centro de carga | `CQTS!LADO 1!AF4` | `NodeResult` | `CqtsCalculationModel` | TEST-CQTS-001 | confirmado |
| CALC-006 corrente por fase | `CQTS!LADO 4!X8` | `EdgeResult` | `CurrentCalculator` | TEST-CQTS-001 | unidade a confirmar |
| CALC-007 proteção | `CQTS!LADO 1!AA8` | `Validation` | `LoadingValidator` | TEST-CQTS-001 | confirmado |
| CALC-008 status projeto | `CQTS!PROJ1!Q2` | `CircuitResult` | `ProjectValidation` | TEST-CQTS-002 | confirmado |
| CALC-009 macro de abertura | `ThisWorkbook!Workbook_Open` | `SourceArtifact` | não portar; registrar | TEST-VBA-001 | comportamento de UI |
| CALC-010 link DecInv | QDT external link / VBA | `SourceArtifact` | `ExternalDependencyValidator` | TEST-QDT-001 | bloqueante até arquivo |
| CALC-011 Solver | nomes `solver_*` em `RAMAL/PROJ1` | `CalculationRun` | não implementar ainda | TEST-INVALID-001 | desconhecido |

## 19. Gate para iniciar o motor de produção

### Bloqueantes

- [ ] arquivo externo `DecInv` obtido ou todas as células dependentes isoladas;
- [ ] comparação normalizada VBA ATUAL×PROJ concluída;
- [ ] precedentes de fórmulas críticas resolvidos;
- [ ] unidades de entradas e resultados documentadas;
- [ ] casos QDT ATUAL e PROJ com entradas e saídas congeladas;
- [ ] impacto do Solver determinado;
- [ ] tolerância final definida por variável.

### Alto

- [ ] nomes `#REF!` classificados e reproduzidos como erro ou substituídos apenas com decisão formal;
- [ ] arredondamentos e formatos que participam de lógica identificados;
- [ ] ciclos e múltiplos caminhos testados como inválidos ou suportados;
- [ ] todas as colunas de QDT classificadas.

### Médio

- [ ] exportação de volta para Excel;
- [ ] compatibilidade visual de relatórios;
- [ ] otimização incremental.

### Baixo

- [ ] editor gráfico avançado;
- [ ] múltiplos formatos de relatório.

## 20. Critério de sucesso

A Fase 2 é considerada documentada quando o banco, o domínio, a topologia, a arquitetura e o processo de paridade estão definidos sem transformar células em modelo de negócio. A implementação do motor só será considerada correta quando os mesmos snapshots de entrada produzirem resultados QDT equivalentes por transformador, circuito, trecho, nó, ramo, ponta, intermediário, status e resultado final, com cada diferença explicada por origem rastreável.

**Decisão final desta fase:** implementar somente fundação SQLite, modelo de evidência, importador somente leitura e harness de paridade. Manter o motor elétrico de produção bloqueado até os gates acima serem satisfeitos.
