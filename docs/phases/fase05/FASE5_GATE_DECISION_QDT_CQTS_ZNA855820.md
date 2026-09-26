# FASE 5 — Gate Decision QDT + CQTS

## FASE 5 — RESULTADO

A arquitetura unificada posterior está documentada em [FASE6_ARCHITECTURE_GATE.md](FASE6_ARCHITECTURE_GATE.md).

**Decisão:** `NO-GO`  
**Data:** 2026-09-26  
**Commit:** não aplicável; nenhum commit criado.  
**Arquivos originais preservados:** Sim.

## Matriz final de gate

| Critério | Status | Evidência | Impacto | Ação |
|---|---|---|---|---|
| VBA | RESOLVED — NON-CALCULATION | rotinas extraídas são operacionais; nenhuma rotina elétrica identificada | baixo no motor | não reabrir salvo evidência nova |
| DecInv | BLOCKED | fonte `J:` ausente; fórmulas externas em `PF` | crítico/unknown | obter fonte ou provar isolamento |
| Solver | REQUIRES LIMITED VERIFICATION | nomes `solver_*`; nenhuma chamada encontrada | crítico até fechar | inspeção controlada/residualidade |
| Precedentes críticos | OPEN | grafo parcial; fórmulas `#REF!`, externos e dinâmicas | crítico | completar grafo até saídas |
| `#REF!` | UNKNOWN/CRITICAL POTENTIAL | `Base de Dados`, `FML`, nomes CQTS quebrados | pode alterar parâmetros | classificar cada ocorrência |
| Unidades | PARTIAL | kVA/MVA/kV/A/m/°C confirmados; R/X/ETA/queda pendentes | crítico | fechar catálogo |
| Precisão | PARTIAL/BLOCKED | dupla precisão e sem arredondamento amostrado; busca/tolerância incompletas | crítico para paridade | executar sensibilidade |
| Arredondamento | UNKNOWN | nenhum round nas amostras, cobertura total ausente | importante | varrer fórmulas completas |
| Golden Dataset | BLOCKED | casos reais parciais, sem snapshots completos | crítico | congelar casos QDT/CQTS |
| Parity Harness | P0/P1 READY; P3/P4 BLOCKED | especificação produzida | importante/crítico | implementar leitura/comparação |
| Topologia | READY para infraestrutura | modelo de árvore e invariantes definidos | não bloqueia núcleo isolado | implementar/validar independentemente |
| SQLite | READY para infraestrutura | schema e DDL validados anteriormente | não bloqueia núcleo isolado | implementar migrations/repositories |

Regra vigente: `TOLERANCE = UNDEFINED` até que a precisão do Excel, a ordem das operações, os arredondamentos reais e os golden cases permitam derivar tolerâncias por variável.

## Decisão operacional

```text
NO-GO — não iniciar QDT Calculation Engine de produção.
GO restrito — iniciar apenas infraestrutura, topologia, evidence store e parity harness P0/P1.
```

## Condições para `GO RESTRITO — QDT ENGINE PROTOTYPE`

1. DecInv comprovadamente isolado do núcleo elétrico ou fonte disponível.
2. Solver reclassificado como residual com evidência ou formalmente especificado.
3. Grafo sem dependências UNKNOWN até o primeiro escopo QDT prototipado.
4. Unidades críticas do escopo prototipado fechadas.
5. Golden case reproduzível para cada regra prototipada.
6. Harness executando comparação de intermediários e status.

## Condições para `GO — QDT CALCULATION ENGINE`

Além das anteriores: LADO 1, TRAFO, LADO 2, RAMAL, múltiplos trechos, ATUAL×PROJ, alterações de carga/condutor/comprimento, tolerâncias justificadas e parity suite automatizada.

## Próximo backlog

1. Fechar/isolar `DecInv`.
2. Resolver Solver.
3. Implementar grafo de precedentes no SQLite.
4. Fechar unidades, precisão e arredondamento.
5. Capturar snapshots golden completos.
6. Implementar P0/P1 do harness.
7. Validar topologia e SQLite.
8. Reexecutar o gate.
