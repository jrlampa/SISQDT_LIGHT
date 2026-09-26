# Fase 14 — Reconstrução de carga e impedância

## Ramais!C13

Há duas descrições incompatíveis nos artefatos: `B9*0.85+B10*0.5268` catalogada e `C11*0.85+C12*0.5268` observada em cópia. A segunda aparece ao lado de `R (Ohms)` e `X (Ohms)`, mas a cópia não é baseline oficial. Portanto não é legítimo escolher entre carga, impedância ou parâmetro comercial.

`QDT.RAMAL.LOAD_COMPOSITION = PARTIAL/BLOCKED`.

## Coeficientes

`0.85` ocorre em contextos distintos, inclusive divisão e combinação; `0.5268` aparece na combinação observada. Não há evidência suficiente de constante global, fator de demanda, utilização, simultaneidade ou fator de potência.

## Acumulada CQTS

O padrão `SUMIF` sobre `TRECHO/PONTO/ACUMULADA/TOTAL DO TRECHO` é uma fórmula documentada, mas a ordem, ramificações, terminais e múltiplos filhos não estão fechados. `CQTS.ACCUMULATED_LOAD = PARTIAL`; não se afirma equivalência com QDT.

## Corrente/queda

A corrente QDT não possui fórmula fechada. A fórmula CQTS por fase é apenas documental/inferida e depende de acumulada, ETA, tensão e fase. Queda de tensão permanece bloqueada por precedentes, unidades e dependências incompletos.

Nenhum cálculo foi implementado.
