# Fase 12 — Evidência matemática válida

## Válido

- As três regras da Fase 9 permanecem isoladas, testadas e sem dependência demonstrada de `Analise Ponto a Ponto`.
- A política de exclusão do protótipo está implementada no precedence graph.
- Fórmulas, caches e unidades descritos na auditoria histórica são evidência documental/observacional, não autorização de cálculo.

## Excluído ou bloqueado

- Toda fórmula, valor, nome, tabela ou cadeia cuja origem exclusiva seja `Analise Ponto a Ponto`: `EXCLUDED_PROTOTYPE`.
- Carga QDT: `PARTIAL/BLOCKED`.
- Corrente, tensão, queda, fluxo, carregamento: `BLOCKED`.
- Solver: `REQUIRES LIMITED VERIFICATION`/não implementado.
- DecInv: `EXTERNAL_DEPENDENCY / BLOCKED` quando a cadeia depender do arquivo externo não recuperado.

## Ramais!C13

A fórmula histórica observada `C13=C11*0.85+C12*0.5268` não foi recalculada no baseline oficial. Ela não prova composição de carga, e `B9/B10` continuam contextuais, não globais. Sem o arquivo oficial, a classificação permanece `OBSERVED_ONLY/PARTIAL`.

`TOLERANCE = UNDEFINED`.
