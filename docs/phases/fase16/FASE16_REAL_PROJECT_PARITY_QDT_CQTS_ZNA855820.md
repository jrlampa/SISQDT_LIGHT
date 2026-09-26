# Fase 16 — Parity de projeto real

| Projeto | Cadeia | Observado | Nativo | Resultado |
|---|---|---:|---:|---|
| CQT PROJ 7 | `BX13 <- P13 <- Q13...` | 43.630837053053675 | não calculado com precedência completa | `BLOCKED` |
| CQT PROJ 4 | mesma família estrutural | não usado para confirmar ramo | não calculado | `BLOCKED` |

## Motivo do bloqueio

A fórmula P13 possui condições por condutor/fase e referencia células seguintes. O trecho observado tem `AM13=277`, enquanto os ramos explícitos incluem textos como `240 Al - Arm`; selecionar um ramo por engenharia seria inferência. `AN13` e a cadeia Q13 em diante precisam ser resolvidos.

Não foi usada tolerância. Não há `EXACT`, `NUMERICALLY_REPRODUCED` ou `DIVERGENT` para o resultado final porque a reprodução foi interrompida corretamente em `BLOCKED`.
