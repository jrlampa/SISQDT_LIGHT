# Fase 11 — Reconciliação do baseline

## Resultado

`HISTORICAL_BASELINE = NOT FOUND`  
`APPLICATION_DB = MISSING`  
`IDENTITY = UNRECONCILED`

Os quatro nomes oficiais não existem na árvore pesquisada:

- `QDT_ZNA855820_ATUAL.xlsm`
- `QDT_ZNA855820_PROJ.xlsm`
- `CQTS__ZNA855820_ATUAL.xlsx`
- `CQTS__ZNA855820_PROJ.xlsx`

## Artefatos Fase 8

Os seis relatórios Fase 8 estão presentes na raiz de `SISQDT_LIGHT`. Os hashes históricos não foram encontrados em arquivos do workspace, bancos, logs ou artefatos pesquisados. Os valores registrados no contexto anterior permanecem referências históricas não verificáveis, não um novo baseline.

## Arquivos atuais encontrados

| Arquivo | SHA-256 atual | Classificação |
|---|---|---|
| `models/base_jon_rev101.xlsm` | `FF27E2BDD6482F9978E7D0A0A5BE6B273AD19246B90CD3E3E7B34CD9FD831622` | CURRENT_ARTIFACT_HASH; identidade não reconciliada |
| `models/CQT - ZERADO.xlsm` | `8267B0C09FE4D947A39BF76F426F8FB75016648B49BC3EA61158448ADCF31EAC` | CURRENT_ARTIFACT_HASH; identidade não reconciliada |
| `models/models_light/CQT - Light (Robusto).xlsm` | `F7976F9A6160EAD7925D2CDDEB246B60EFD85E22501CAA7F53E30668248A98B6` | CURRENT_ARTIFACT_HASH; identidade não reconciliada |
| `models/CQTS_NOVA_REV0.xlsx` | `5824A52029C6ABB5976261086935A877E3F7281502C9483FD733D5B5B7618C22` | CURRENT_ARTIFACT_HASH; identidade não reconciliada |

Esses quatro arquivos não foram declarados `NEW VERIFIED BASELINE`: não há evidência suficiente de que sejam exatamente os quatro arquivos oficiais ATUAL/PROJ.

## Bancos

`application.db` não foi encontrado. Foram encontrados bancos de PROJ/sisRUA/orçamento e bibliotecas auxiliares, todos classificados como **UNRELATED_DATABASE**. Nenhum foi aberto ou tratado como Evidence Store QDT/CQTS.

## Impacto

Sem identidade reconciliada não é possível validar hashes, snapshots, Evidence Store, golden cases ou parity histórica. O gate permanece `NO-GO` para nova matemática.
