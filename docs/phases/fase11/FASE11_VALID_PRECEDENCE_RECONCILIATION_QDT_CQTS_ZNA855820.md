# Fase 11 — Reconciliação do precedence graph válido

## Política

Sem os quatro workbooks oficiais, a contagem completa de nós/arestas do baseline não pode ser afirmada. O filtro, entretanto, está definido e testado:

| Métrica | Resultado |
|---|---|
| Nós válidos do baseline | NOT COMPUTED; baseline ausente |
| Nós `EXCLUDED_PROTOTYPE` | todas as células de `Analise Ponto a Ponto` quando importadas |
| Arestas válidas do baseline | NOT COMPUTED; baseline ausente |
| Arestas excluídas | referências originadas ou destinadas à aba protótipo |
| Dependências bloqueadas pela aba | marcadas `EXCLUDED_PROTOTYPE_DEPENDENCY` |
| Ciclos exclusivos da aba | ignorados pelo detector válido |

## Consequência

Uma fórmula produtiva que dependa da aba protótipo não pode ser `READY`; seu resultado deve ser `BLOCKED / EXCLUDED_PROTOTYPE_DEPENDENCY`. Nenhum cache, zero ou valor de outra revisão é usado para preencher a lacuna.

O grafo agora diferencia a exclusão deliberada (`EXCLUDED_PROTOTYPE`) de incerteza semântica (`UNKNOWN`).
