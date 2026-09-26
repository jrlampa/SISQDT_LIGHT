# Fase 17 — Decisão de gate

## Gate

**A — GO RESTRICTED — FIRST REAL PROJECT RULE IMPLEMENTED**

## Resposta final

1. CQTS analisado: CQT PROJ 7 REV2.
2. Cadeia: `M13 + BX6 + H13 + AN13 + AP13 -> AO13 -> temperatura P13/BX13`.
3. Identificado: AM=condutor, AN=ampacidade/corrente de condição, AP=comprimento, AO=AN*AP, BX6=tensão secundária.
4. Ambiguidade resolvida pelo usuário: P13/BX13 são temperatura do condutor, não tensão; AM13 é condutor, AN13 é amperagem.
5. Perguntas feitas: significado de AM13, AN13 e P13/BX13.
6. Respostas recebidas: contexto operacional fornecido pelo usuário e confirmado em cabeçalhos/valores.
7. Fórmula fechada: `M13/(BX6*SQRT(3)/1000)/((AN13*AP13)/(90-30))+30`.
8. Inputs: 74.448 kVA; 220 V; 3 fases; 430 A; 2 m.
9. Output: 43.6308370530537 °C.
10. Unidades: kVA, V, A, m e °C no contexto do projeto real.
11. Reprodução: `NUMERICALLY_REPRODUCED`; Excel 43.630837053053675 vs nativo 43.6308370530537.
12. Nova regra: `CQTS.REAL_PROJECT.CABLE_TEMPERATURE`, candidata, não oficial.
13. Testes: 26 focados; suíte completa final: **53 aprovados, 0 falhas, 0 diagnósticos**.
14. Divergências: generalização para outros condutores/fases e validação no segundo CQTS ainda pendentes.
15. Próximo elo: validar a mesma regra no CQT PROJ 4 e depois localizar a fórmula de corrente do trecho.
16. Gate: A.

Baseline elétrico permanece `UNRECONCILED`, `F9-RULESET-1` permanece preservado, engines completos continuam `ENGINE_NOT_READY`, protótipo permanece excluído. Fase 18 não iniciada.
