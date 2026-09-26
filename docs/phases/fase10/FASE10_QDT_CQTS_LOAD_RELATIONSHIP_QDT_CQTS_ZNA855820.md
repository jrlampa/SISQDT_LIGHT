# Fase 10 — Relação de carga QDT/CQTS

| Relação | Evidência | Classificação |
|---|---|---|
| QDT `Ramais!C13` ↔ CQTS campos de carga | nomenclatura e fórmulas não reconciliadas no mesmo baseline | UNKNOWN |
| QDT R/X ↔ CQTS proteção `Ib/In/Iz` | dimensões e função diferentes | RELATED_BUT_DIFFERENT ou UNKNOWN |
| QDT `M13` ↔ CQTS `TOTAL/ACUMULADA` | referência direta não encontrada | UNKNOWN |
| CQTS `SUMIF` de tabelas de materiais ↔ carga elétrica | contexto de orçamento/material, não prova de carga | UNKNOWN |

## Acumulada CQTS

Foram observadas fórmulas `SUMIF` em vários sheets e tabelas, mas não foi fechado um caso com definições simultâneas de `TRECHO`, `PONTO`, `ACUMULADA` e `TOTAL DO TRECHO`, unidade e ramificação. Portanto não há regra de acumulação pronta para implementação.

## Topologia

A correspondência entre `PONTO/TRECHO/ACUMULADA` e `Node/Edge/Branch` continua `UNKNOWN`. A infraestrutura topológica existente não foi alterada.

Conclusão: QDT e CQTS permanecem em regras separadas; nenhuma implementação compartilhada é autorizada.
