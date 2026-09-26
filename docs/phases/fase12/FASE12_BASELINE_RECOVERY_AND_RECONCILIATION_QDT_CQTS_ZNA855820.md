# Fase 12 — Recuperação e reconciliação do baseline

## Decisão factual

Os quatro workbooks oficiais não foram recuperados no ambiente atual. Os caminhos históricos fornecidos foram verificados e estão ausentes. A busca recursiva no OneDrive também não encontrou os nomes oficiais.

`HISTORICAL_BASELINE = NOT FOUND`  
`APPLICATION_DB = MISSING`  
`BASELINE = UNRECONCILED`

## Evidência documental recuperada

`AUDITORIA_QDT_CQTS_ZNA855820.md` registra quatro arquivos auditados, tamanhos, data, abas, tabelas, VBA e links. Essa evidência confirma que a auditoria existiu, mas não fornece SHA-256 verificável dos arquivos presentes hoje.

## Candidatos atuais

`base_jon_rev101.xlsm`, `CQT - ZERADO.xlsm`, `CQT - Light (Robusto).xlsm` e `CQTS_NOVA_REV0.xlsx` possuem hashes atuais registrados na Fase 11. Eles não têm origem suficiente para serem promovidos a oficial ATUAL/PROJ.

## Bancos

A busca encontrou bancos de outros produtos e auxiliares. Nenhum é `application.db` QDT/CQTS. Não foi criado banco substituto.

## Impacto

Sem os quatro bytes oficiais não é possível reconciliar hash histórico, defined names, fórmulas, macros, links externos, Evidence Store ou golden cases. A fase termina em `NO-GO`, sem nova matemática.
