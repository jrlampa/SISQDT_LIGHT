# Fase 15 — Achados matemáticos dos candidatos

## Cadeia fechada no candidato

```text
Ramais!C11 (R, Ohms)
        + 0.85
Ramais!C12 (X, Ohms)
        + 0.5268
Ramais!C13
```

Fórmula observada nos dois candidatos:

`C13 = C11*0.85 + C12*0.5268`

Snapshot comum:

- C11 = `1.0903`
- C12 = `0.4034`
- C13 observado = `1.13926612`
- unidade de C11/C12: Ohms pelos cabeçalhos dos candidatos
- unidade de C13: Ohm no contexto da combinação R/X do candidato

Reprodução externa: `1.0903*0.85 + 0.4034*0.5268 = 1.13926612`, sem tolerância.

## Limites

A fórmula não foi chamada de composição de carga. Não há prova de que C13 alimente o cálculo elétrico oficial ou que os candidatos sejam o baseline ZNA855820. A regra é `CANDIDATE_REPRODUCED`, não `OFFICIAL_BASELINE_EVIDENCE`.

Nenhuma cadeia nova de corrente, queda, fluxo ou acumulada atingiu inputs, unidades, precedência e output reproduzível simultaneamente.
