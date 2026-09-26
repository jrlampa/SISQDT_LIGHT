# Matriz de capacidades QDT x CQTS

| Capacidade | QDT | CQTS | Modelo comum | Regra específica |
|---|---|---|---|---|
| Transformador | parâmetros em `LADO`/base | `DB`/`PROJ1` | `Transformer` | fórmula/seleção por engine |
| LADO | LADO 1 e LADO 2 | LADO 1..4 e PROJ | `Circuit` | organização e resultados |
| Ramal | `Ramais` | `RAMAL` | `Load`/`Branch` | composição de carga própria |
| Trecho | sequência em colunas | `PONTO MONTANTE` + trecho | `Edge` | ordem e cálculo próprios |
| Ponto | implícito/linhas | explícito `PONTO` | `Node` | mapeamento QDT a provar |
| Carga | consumidor/ramal/kVA | carga ponto/acumulada | `Load` | regras distintas |
| Condutor | tabela QDT | `CABOS` e catálogos CQTS | `Conductor` | catálogo e unidades mapeados |
| Topologia | linear por lado | árvore radial | `TopologyModel` | traversal comum; cálculo separado |
| Acumulação | QDT específica | montante/filhos | interface de traversal | fórmula separada |
| Corrente | QDT bloqueada | CQTS parcial | resultado `Current` | engines separados |
| Queda | QDT bloqueada | CQTS parcial | `VoltageDrop` result type | formulas separadas |
| Proteção | QDT específica | `Ib <= In <= Iz` | `Validation` | regra CQTS isolada |
| Resultados | por lado/trecho/validação | por ponto/edge/branch/lado | `CalculationResult` | campos opcionais por mode |
| Evidência | XLSM/VBA/links | XLSX/tabelas | `EvidenceStore` | origem preservada |
| Paridade | golden QDT obrigatório | testes CQTS estruturais/matemáticos | `ParityHarness` | QDT é prioridade |

QDT pode ser representado como caso linear do modelo topológico, mas não como versão matemática reduzida do CQTS.