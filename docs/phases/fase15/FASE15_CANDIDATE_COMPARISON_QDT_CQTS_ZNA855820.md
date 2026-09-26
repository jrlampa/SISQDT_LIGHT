# Fase 15 — Comparação objetiva de candidatos

| Candidato | Hash | Fórmulas | #REF! | SUMIF | Termos elétricos | Leitura |
|---|---|---:|---:|---:|---:|---|
| `base_jon_rev101.xlsm` | `FF27E2BDD6482F9978E7D0A0A5BE6B273AD19246B90CD3E3E7B34CD9FD831622` | 13024 | 6 | 901 | 1909 | melhor cobertura QDT estrutural |
| `CQT - ZERADO.xlsm` | `8267B0C09FE4D947A39BF76F426F8FB75016648B49BC3EA61158448ADCF31EAC` | 5316 | 196 | 0 | 1406 | possui C13 reproduzível; muitos bloqueios |
| `CQT - Light (Robusto).xlsm` | `F7976F9A6160EAD7925D2CDDEB246B60EFD85E22501CAA7F53E30668248A98B6` | 5311 | 196 | 0 | 1399 | confirma fórmula C13 do candidato anterior |
| `CQTS_NOVA_REV0.xlsx` | `5824A52029C6ABB5976261086935A877E3F7281502C9483FD733D5B5B7618C22` | 2104 | 2 | 101 | 991 | fórmulas CQTS, mas cadeia funcional não fechada |

## Seleção

Para a primeira ação funcional, foram selecionados `CQT - ZERADO.xlsm` como candidato primário e `CQT - Light (Robusto).xlsm` como corroborador independente, pois ambos convergem em `Ramais!C13`. Isso não altera suas classificações `CURRENT_CANDIDATE / UNKNOWN_ORIGIN`.

`Analise Ponto a Ponto` não foi usada.
