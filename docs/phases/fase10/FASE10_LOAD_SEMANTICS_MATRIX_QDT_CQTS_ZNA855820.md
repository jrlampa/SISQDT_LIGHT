# Fase 10 — Matriz de semântica de carga QDT/CQTS

## Baseline e evidência

Build e suíte baseline passaram: **42 testes, 0 falhas**. Porém, nenhum arquivo disponível em `models` possui os quatro hashes registrados na Fase 8. A inspeção abaixo usa cópias disponíveis apenas como evidência exploratória; não constitui golden oficial.

## QDT/CQTS — campos investigados

| Campo | Contexto observado | Fórmula/valor | Significado físico comprovado | Unidade comprovada | Consumidores | Status |
|---|---|---|---|---|---|---|
| `B9` | `Ramais` | texto de condutor/item em cópia; em outros sheets é código/item | não uniforme entre sheets | UNKNOWN | fórmulas locais dependentes do sheet | UNKNOWN |
| `B10` | `Ramais` | família/seção textual do condutor em cópia | não uniforme entre sheets | UNKNOWN | lookup/seleção local | UNKNOWN |
| `C11` | `Ramais` | `1.0903` | resistência `R` | Ohm, pelo cabeçalho | `C13` | OBSERVED, não baseline |
| `C12` | `Ramais` | `0.4034` | reatância `X` | Ohm, pelo cabeçalho | `C13` | OBSERVED, não baseline |
| `C13` | `Ramais` | `C11*0.85+C12*0.5268` | combinação de R/X; não é carga comprovada | Ohm inferido dimensionalmente, sem contrato formal | tabelas/consultas posteriores | PARTIAL |
| `D` | `Ramais` e outros sheets | coluna variável por tabela | UNKNOWN | UNKNOWN | contexto local | UNKNOWN |
| `H` | Lado/ramais | coluna variável | UNKNOWN | UNKNOWN | contexto local | UNKNOWN |
| `I` | QDT Lado | validações/seleções locais | UNKNOWN fora de `I13` | UNKNOWN | regras de validação | PARTIAL |
| `K` | QDT Lado | valor candidato em `K13` | valor de tabela/ramal; unidade não comprovada aqui | UNKNOWN | `M13` | PARTIAL |
| `L` | QDT Lado | valor candidato em `L13` | valor de tabela/ramal; unidade não comprovada aqui | UNKNOWN | `M13` | PARTIAL |
| `M` | QDT Lado | `M13=IF(CH5="SIM",K13,L13)` | saída selecionada de K/L; semântica elétrica ainda restrita | UNKNOWN | consumidores downstream | READY somente isolado |
| `P/Q` | QDT/CQTS | ocorrência dependente do sheet | UNKNOWN | UNKNOWN | não fechado | UNKNOWN |
| `X/Y/Z/AA` | CQTS Lado/ proteção | campos de proteção e derivados | `AA` participa de observação de proteção; mapeamento completo não fechado | Ampere somente quando cabeçalho/evidência o confirma | proteção | PARTIAL |

## Semântica física

- `Ramais!B2` na cópia observada diz **Ampacidade dos Condutores de Ramais de BT (A)**.
- `Ramais!B6` diz **Carregamento** e contém valores de referência de condutor.
- `Ramais!B11` diz `R (Ohms)` e `B12` diz `X (Ohms)`.
- `Ramais!B13` diz `Comercial`; a fórmula de `C13` combina resistência e reatância com coeficientes ainda sem origem comprovada.
- Logo, a expressão conhecida não deve ser chamada de composição de carga sem provar que `C11/C12` representam carga em outro contexto. A cópia observada aponta para impedância.

## Coeficientes

`0.85` aparece em divisões e em combinações de R/X; `0.5268` aparece junto de `X` na fórmula de `Ramais!C13` em várias cópias. A multiplicidade de contextos impede atribuir globalmente demanda, utilização, simultaneidade ou fator de potência. Classificação: **UNKNOWN/CONTEXT_DEPENDENT**.
