# Fase 14 — Catálogo de evidência histórica

`RECONSTRUCTION_VERSION = F14-RECON-1`  
`OFFICIAL_BASELINE_EVIDENCE = NONE ESTABLISHED`

| EvidenceId | Phase/document | Source | Cell/range | Fórmula/observação | Classificação | Confiança |
|---|---|---|---|---|---|---|
| `EVID-F9-M13-FORMULA` | F9 subset/parity | QDT documental | `LADO 1!M13` | `IF(CH5="SIM",K13,L13)` | DIRECT_FORMULA / HISTORICAL_DOCUMENTARY_EVIDENCE | DIRECT |
| `EVID-F9-I13-FORMULA` | F9 subset | QDT documental | `LADO 1!I13` | fórmula IF/OR/AND textual | DIRECT_FORMULA / HISTORICAL_DOCUMENTARY_EVIDENCE | DIRECT |
| `EVID-F9-PROTECTION-RANGE` | F9 subset/parity | CQTS documental | `LADO 1!AA8`/Tabela7 | `Ib <= In <= Iz` | DIRECT_FORMULA / HISTORICAL_DOCUMENTARY_EVIDENCE | DIRECT |
| `EVID-F10-RAMAIS-C13` | F10 reports | candidato/cópia documentada | `Ramais!C13` | `C11*0.85+C12*0.5268` | DOCUMENTED_FORMULA / CURRENT_CANDIDATE_EVIDENCE | PARTIAL |
| `EVID-F10-LOAD-CATALOG` | F9/F10 reports | histórico documental | `Ramais!C13` | `B9*0.85+B10*0.5268` catalogada | DOCUMENTED_FORMULA | PARTIAL |
| `EVID-CQTS-ACCUMULATED-SUMIF` | precedence/auditoria | CQTS documental | tabela LADO | `SUMIF([TRECHO],[@PONTO],[ACUMULADA])+[@[TOTAL DO TRECHO]]` | DOCUMENTED_FORMULA | PARTIAL |
| `EVID-CQTS-PROTECTION-OBS` | golden dataset | CQTS observação | linha 8 | `Ib=162.780...`, `In=160`, `Iz=430`, `VERIFICAR` | DIRECT_OBSERVATION/CACHED_VALUE | OBSERVED_ONLY |
| `EVID-QDT-M13-OBS` | golden dataset | QDT observação | `LADO 1` | `M13=186.08282352941154` | DIRECT_OBSERVATION/CACHED_VALUE | OBSERVED_ONLY |
| `EVID-DECINV-LINK` | F3/F5 | workbook documental | PF cells | external link `J:` | EXTERNAL_REFERENCE | BLOCKED |
| `EVID-SOLVER-NAMES` | F4/F5/F9 | CQTS documental | RAMAL/PROJ1 | nomes `solver_*`, sem SolverOk/Add/Solve | UNKNOWN | REQUIRES LIMITED VERIFICATION |
| `EVID-PROTOTYPE-TAB` | F11 | histórico documental | Analise Ponto a Ponto | aba protótipo | EXCLUDED_PROTOTYPE | EXCLUDED |

Caches não são tratados como regra. Candidatos atuais não são tratados como baseline oficial. Qualquer evidência exclusiva da aba protótipo é excluída.
