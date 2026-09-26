# Matriz de compartilhamento de regras QDT x CQTS

| Regra/capacidade | QDT | CQTS | Equivalência comprovada | Compartilhar fórmula? | Evidência |
|---|---|---|---|---|---|
| carga direta | específica por colunas/ramais | carga por ponto/ramal | não | não; compartilhar apenas `Load` | Fase 4/5 |
| carga acumulada | cadeia linear/colunas QDT | `SUMIF` por ponto/trecho | parcial | não | grafo parcial |
| corrente | fórmula QDT não fechada | fórmula por fase com `SQRT(3)` | não provada | não | Fase 5 |
| tensão nominal | existe em ambos | existe em ambos | parcial | apenas value object, não cálculo | Fase 4/5 |
| queda de tensão | fórmula QDT bloqueada | fórmula CQTS própria/fallback | não | não | readiness matrix |
| condutor | catálogos e parâmetros próprios | `CABOS`/tabelas próprias | parcial | compartilhar entidade catálogo somente quando mapeada | cabecalhos/tabelas |
| ampacidade/proteção | regras QDT incompletas | `Ib <= In <= Iz` isolada | parcial | compartilhar validador apenas para inputs compatíveis | CQTS `AA` |
| carregamento | QDT específico | CQTS específico | não provada | não | Fases 4/5 |
| centro de carga | não comprovado equivalente | `SUMPRODUCT` | diferente | não | CQTS LADO |
| status | mensagens QDT (`OK !`, `Erro`) | `OK`, `VERIFICAR`, `SOBRECARGA` | semântica diferente | compartilhar enum base, não mensagens/regra | planilhas |
| topologia linear/radial | lado linear | árvore/ramificações | estrutural | compartilhar modelo topológico | auditoria |

## Classificações permitidas

- `COMMON`: só para conceito/contrato comprovado.
- `EQUIVALENT-BY-EVIDENCE`: exige teste e fórmula rastreada.
- `QDT-SPECIFIC` / `CQTS-SPECIFIC`: engine separado.
- `SIMILAR-BUT-NOT-PROVEN-EQUIVALENT`: não compartilhar fórmula.
- `UNKNOWN`: bloquear execução.

**Regra:** sem evidência de equivalência, não compartilhar cálculo.