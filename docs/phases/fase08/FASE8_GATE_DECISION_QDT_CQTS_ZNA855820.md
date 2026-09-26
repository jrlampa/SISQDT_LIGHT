# Fase 8 — Gate Decision

**Decisão:** `GO RESTRICTED — QDT ENGINE PREPARATION` para infraestrutura e regras isoladas; `NO-GO` para QDT/CQTS engines de produção.  
**Data:** 2026-09-26.  
**Excel original preservado:** Sim.  
**Baseline:** build/test inicial da Fase 7 passou com 23 testes; Fase 8 passa com 30 testes totais, incluindo 14 testes de import/SQLite.

## Matriz final

| Área | Fase 7 | Fase 8 | Estado |
|---|---|---|---|
| SQLite | GO RESTRICTED | GO RESTRICTED | hardening validado; migrations futuras pendentes |
| Evidence Store | GO RESTRICTED | GO RESTRICTED | P1 real contra XLSM/XLSX |
| Excel Import | GO RESTRICTED | GO RESTRICTED | sem execução Excel/VBA |
| Precedence Graph | P0 RESTRICTED | P1 RESTRICTED | ciclos de fórmula separados |
| Golden Dataset | BLOCKED | BLOCKED/PARTIAL | isolated cases preparados |
| Parity P0/P1 | GO | GO | UNKNOWN/BLOCKED preservados |
| QDT Engine | NO-GO | MUST REMAIN NO-GO | dependências críticas abertas |
| CQTS Engine | NO-GO | MUST REMAIN NO-GO | cálculo não implementado |
| UI | DESIGN ONLY | DESIGN ONLY | nenhum workflow produtivo |

## Saída autorizada

A próxima fase pode iniciar a preparação matemática de um subconjunto isolado que tenha fórmula, precedentes, inputs, unidade, ordem de operações e golden reproduzível. Não pode iniciar o engine QDT completo.

## Bloqueios restantes

1. DecInv e prova de isolamento elétrico.
2. Solver: `NO_ACTIVE_SOLVER_EVIDENCE` ainda não formalizado como decisão final.
3. Grafo crítico QDT completo.
4. R/X/R CORR/ETA/queda/CRS e arredondamentos.
5. Full golden cases QDT ATUAL/PROJ e variações.
6. Persistência P1 de names/tables/structured refs no schema.

## Próxima Fase 9 autorizada

Implementar somente a especificação matemática executável de regras isoladas `READY` e seus testes, ou continuar hardening P1 de persistência. Não implementar corrente/queda/carregamento QDT completos até novo gate. Não iniciar automaticamente.