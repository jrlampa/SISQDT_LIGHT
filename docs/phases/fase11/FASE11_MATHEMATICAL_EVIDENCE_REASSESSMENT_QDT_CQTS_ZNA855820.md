# Fase 11 — Reavaliação da evidência matemática

## Conclusões anteriores

| Conclusão | Usou aba protótipo? | Impacto | Ação |
|---|---|---|---|
| `QDT.LADO1.SELECT_KL_TO_M13` | não demonstrado | nenhuma evidência direta de dependência | manter, regressão aprovada |
| `QDT.LADO1.VALIDATION_I13` | não demonstrado | nenhuma evidência direta de dependência | manter, regressão aprovada |
| `CQTS.PROTECTION.IB_IN_IZ` | não demonstrado | nenhuma evidência direta de dependência | manter, regressão aprovada |
| `QDT.RAMAL.LOAD_COMPOSITION` | cadeia não fechada; protótipo não pode completar precedentes | permanece bloqueada | não implementar |
| primeiro golden encadeado | não criado | nenhum impacto | continuar sem golden |
| fórmulas históricas da aba | sim, por definição | inválidas para matemática produtiva | marcar `EXCLUDED_PROTOTYPE` |

## Ramais/C13

A observação anterior `Ramais!C13 = C11*0.85+C12*0.5268` só pode permanecer como evidência após confirmação no baseline reconciliado. Ela não foi usada para promover carga e não resolve `B9/B10`. A conclusão operacional permanece `PARTIAL/BLOCKED`.

## Fase 9

As três regras continuam independentes da aba protótipo nos testes e contratos atuais. O novo filtro não altera seus RuleIds.
