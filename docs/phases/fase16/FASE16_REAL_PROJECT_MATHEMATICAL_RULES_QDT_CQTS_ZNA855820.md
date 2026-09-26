# Fase 16 — Regras matemáticas em projeto real

## Melhor evidência

No CQT PROJ 7 real, fora da aba protótipo:

- `LADO 1!P13` contém fórmula condicional de queda/tensão baseada em `M13`, `$BX$6`, `SQRT(3)`, `AO13`, fase `H13`, condutor `AM13` e ramos sucessivos `Q13...`.
- `LADO 1!BX13` referencia `P13` quando `AM2` confirma o modo de cálculo.
- Observações: `BX6=220`, `M13=74.448`, `H13=3`, `AM13=277`, `AO13=860`, `AP13=2`, `AQ13=4`, `AR13=2`, `P13/BX13=43.630837053053675`, `I13="OK !"`.

## Cadeia encontrada

```text
M13 / AA13 (intermediário de carga)
  -> P13/Q13... (fórmula condicional de queda/tensão)
  -> BX13 (resultado exposto)
  -> I13 (validação textual independente)
```

## Estado

A cadeia é **REAL_PROJECT_EVIDENCE**, mas não `CANDIDATE_REPRODUCED`: `AM13=277` não coincide diretamente com os ramos textuais da fórmula P13, e a escolha recai em fórmulas seguintes (`Q13...`) cujos precedentes não foram fechados neste ciclo. Há ainda `#REF!` e links externos no workbook.

Não foi implementada corrente ou queda. A regra F15 R/X não foi presumida como dependência desta cadeia.
