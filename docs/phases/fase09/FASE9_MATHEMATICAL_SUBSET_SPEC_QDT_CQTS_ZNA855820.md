# Fase 9 — Matriz de regras matemáticas candidatas

**Baseline:** build OK; 30 testes OK; Excel preservado; hashes registrados no relatório de parity.  
**Regra:** somente `READY` pode entrar no subconjunto executável.

| RuleId | Workbook/sheet/cell | Fórmula/regra | Precedentes/inputs | Unidade | Golden | Status | Implementar |
|---|---|---|---|---|---|---|---|
| `QDT.LADO1.SELECT_KL_TO_M13` | QDT ATUAL/PROJ `LADO 1!M13` | `IF(CH5="SIM",K13,L13)` | `CH5`, `K13`, `L13` | CH5 texto; K/L kVA | observado `M13=186.08282352941154`; fórmula direta | READY isolado | SIM |
| `QDT.LADO1.VALIDATION_I13` | QDT `LADO 1!I13` | `IF(OR(D13="",H13=""),"",IF(OR(D13=0,H13=0),"Erro 02",IF(AND(H13>=H9,D14<=D13),"OK !","Erro 02")))` | D13,H13,D14,H9 | números/resultado texto | `I13=OK !`; input snapshot completo ainda deve ser capturado | READY isolado por fórmula; parity do caso real RESTRICTED | SIM |
| `CQTS.PROTECTION.IB_IN_IZ` | CQTS ATUAL `LADO 1!AA8` e tabela `Tabela7` | `Ib <= In <= Iz` | Ib/In/Iz | A | linha 8: 162.780254933745, 160, 430, status `VERIFICAR` | READY isolado | SIM |
| `QDT.RAMAL.LOAD_COMPOSITION` | QDT `Ramais!C13` | `B9*0.85+B10*0.5268` | significado físico/units de B9/B10 não fechados | unknown | não reproduzível isoladamente | PARTIAL | NÃO |
| QDT corrente/tensão/queda | QDT `LADO 1/2` | fórmulas críticas incompletas | precedentes/unidades/#REF!/DecInv | pendente | full cases bloqueados | BLOCKED | NÃO |
| CQTS corrente/queda/fluxo | CQTS `LADO` | regras parcialmente fechadas | ETA/R CORR/unidades/topologia completa | parcial | full cases bloqueados | BLOCKED | NÃO |
| Solver/DecInv | fontes externas/configuração | sem regra comprovada | fonte/semântica ausentes | pendente | não reproduzível | UNKNOWN/BLOCKED | NÃO |

## Contratos de regra

Cada regra implementada deve registrar:

```text
RuleId
FormulaId
EvidenceId
GoldenCaseId
InputDefinitions
OutputDefinition
UnitDefinition
CalculationFunction
TraceMetadata
```

## Decisão de escopo

Implementar apenas as três regras isoladas acima. A composição de carga permanece fora do código porque seus operandos/unidades não foram comprovados. Corrente, tensão, queda, carregamento, fluxo, Solver, DecInv e engines completos permanecem bloqueados.