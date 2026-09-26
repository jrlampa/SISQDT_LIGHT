# Fase 19 — Decisão de Gate

## 1. Parecer do Gate

**GO RESTRICTED — AN13 ORIGIN ESTABLISHED AS CONDUCTOR AMPACITY / INTERMEDIATE THERMAL CHAIN CLOSED**

## 2. Resultados Consolidados

- A variável `AN13 = 430 A` foi formalmente identificada e confirmada como a **ampacidade/capacidade de condução admissível ($I_z$)** do cabo sob as condições do projeto (e não uma corrente de carga $I_b$ calculada a partir de $M13$).
- O teste de sanidade teórico confirmou que a corrente de carga nominal seria de $195.38\text{ A}$, atestando que $430\text{ A}$ não provém de divisão da potência por tensão.
- A semântica foi corroborada pelos rótulos das células OOXML (*"Corrente do cabo para a condição"*) e pela aba `Tabela` que referencia as normas de ampacidade subterrânea da Light.
- Não foi implementada regra arbitrária de corrente de carga, preservando o princípio de zero adivinhação.
- A regra de temperatura de cabo (`CQTS.REAL_PROJECT.CABLE_TEMPERATURE`) permanece plenamente estável e testada.
- Todos os 53 testes existentes continuam aprovados com 0 falhas.

## 3. Próximo Elo

Investigar a composição da potência acumulada de carga ($M13$) e sua propagação ao longo da árvore de nós e trechos do CQTS. Motores em produção permanecem restritos (`ENGINE_NOT_READY`) até que todas as ramificações e proteções estejam comprovadas. Fase 20 não iniciada.
