# Fase 18 — Reconstrução da corrente

## AN13

Nos CQT PROJ 7 e PROJ 4, `AN13` possui valor numérico `430` e não possui fórmula OOXML. O cabeçalho indica `Corrente do cabo para a condição`. `AO13=AN13*AP13` e `AP13=2`.

Conclusão: `AN13` é uma entrada manual/externa ao cálculo da temperatura neste snapshot, coerente com o contexto fornecido pelo usuário. Não há fórmula interna que derive `430 A` de `M13=74.448` ou `75.68658823529411`.

`CQTS.REAL_PROJECT.CURRENT = EXTERNAL/MANUAL INPUT, NOT RECONSTRUCTED`.

Não foi inventada fórmula elétrica nem substituído o valor manual.
