# Fase 14 — Matriz de reconstrução matemática

`RECONSTRUCTION_VERSION = F14-RECON-1`  
Nesta fase não se usa `READY` como promoção executável.

| RuleId | Sistema/aba/célula | Fórmula/regra | Entradas/saída | Unidade | Precedência | Evidência | Classificação | Status |
|---|---|---|---|---|---|---|---|---|
| `QDT.LADO1.SELECT_KL_TO_M13` | QDT `LADO 1!M13` | `IF(CH5="SIM",K13,L13)` | CH5, K13, L13 -> M13 | texto + numérico compatível; kVA documental não oficial | fechada para a seleção | EVID-F9-M13-FORMULA | DIRECT_FORMULA | RECONSTRUCTED |
| `QDT.LADO1.VALIDATION_I13` | QDT `LADO 1!I13` | IF/OR/AND textual | D13,H13,D14,H9 -> I13 | saída textual; unidades numéricas não fechadas | precedentes nominados | EVID-F9-I13-FORMULA | DIRECT_FORMULA | RECONSTRUCTED |
| `CQTS.PROTECTION.IB_IN_IZ` | CQTS `AA8`/Tabela7 | `Ib <= In <= Iz` | Ib, In, Iz -> status | A documental para comparação | fechada somente para comparação | EVID-F9-PROTECTION-RANGE | DIRECT_FORMULA | RECONSTRUCTED |
| `QDT.RAMAL.LOAD_COMPOSITION` | QDT `Ramais!C13` | `B9*0.85+B10*0.5268` catalogada; cópia mostra `C11*0.85+C12*0.5268` | B9/B10 ou C11/C12 -> C13 | UNKNOWN/PARTIAL | não reconciliada | EVID-F10-RAMAIS-C13 | DOCUMENTED_FORMULA | PARTIAL/BLOCKED |
| `CQTS.ACCUMULATED_LOAD` | CQTS tabela LADO | SUMIF estruturado | PONTO,TRECHO,ACUMULADA,TOTAL -> acumulada | kVA documental; ordem incompleta | parcial | EVID-CQTS-ACCUMULATED-SUMIF | DOCUMENTED_FORMULA | PARTIAL |
| `QDT.CURRENT` | QDT LADO | não fechada | carga/tensão/impedância | A não suficiente para regra | #REF/external/unknown | documentos F4/F5 | UNKNOWN | BLOCKED |
| `CQTS.CURRENT` | CQTS LADO | padrão por fase documentado | acumulada/ETA/fase | A | parcial | auditoria/precedence | INFERRED | BLOCKED |
| `QDT.VOLTAGE_DROP` | QDT LADO | não fechada | R/X/comprimento/tensão/FP | V/% incompleta | parcial/broken | F4/F5/F10 | UNKNOWN | BLOCKED |
| `CQTS.VOLTAGE_DROP` | CQTS LADO | parcial | R CORR/ETA/linha | V/% incompleta | parcial | F4/F5 | INFERRED | BLOCKED |
| `QDT/CQTS.SOLVER` | CQTS RAMAL/PROJ1 | sem modelo ativo comprovado | objective/variables/constraints ausentes | N/A | residual/unknown | EVID-SOLVER-NAMES | UNKNOWN | BLOCKED |
| `QDT.DECINV` | QDT PF | external link | workbook externo ausente | N/A | externa | EVID-DECINV-LINK | EXTERNAL_REFERENCE | BLOCKED |

`RECONSTRUCTED` significa reconstrução documental da regra, não parity oficial nem autorização de implementação.
