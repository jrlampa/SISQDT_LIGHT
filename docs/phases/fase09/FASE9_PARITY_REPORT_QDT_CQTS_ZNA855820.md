# Fase 9 — Relatório de parity

## Critério

Somente comparação exata; nenhuma tolerância foi introduzida (`TOLERANCE = UNDEFINED`). Casos sem snapshot de precedentes completo permanecem `RESTRICTED`, não `PASS` de produção.

| GoldenCaseId | RuleId | Evidência | Resultado | Parity |
|---|---|---|---|---|
| `GOLDEN-ISOLATED-001` | `QDT.LADO1.SELECT_KL_TO_M13` | fórmula `M13=IF(CH5="SIM",K13,L13)`; valor observado `186.08282352941154` | seleção reproduzida exatamente em teste | PASS ISOLATED |
| `GOLDEN-ISOLATED-002` | `QDT.LADO1.VALIDATION_I13` | fórmula de `I13`; saída observada `OK !` | saídas textuais reproduzidas; snapshot real completo ainda pendente | RESTRICTED |
| `GOLDEN-ISOLATED-003` | `CQTS.PROTECTION.IB_IN_IZ` | linha observada `Ib=162.780254933745`, `In=160`, `Iz=430`, status `VERIFICAR` | relação falsa reproduzida exatamente | PASS ISOLATED |

A parity acima valida somente as regras isoladas. Não autoriza cálculo completo nem altera o status `BLOCKED` dos engines de rede.
