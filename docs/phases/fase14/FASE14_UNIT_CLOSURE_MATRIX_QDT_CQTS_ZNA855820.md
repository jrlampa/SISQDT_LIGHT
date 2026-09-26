# Fase 14 — Matriz de fechamento de unidades

| Grandeza | Unidade armazenada/exibida | Unidade de cálculo | Origem | Status |
|---|---|---|---|---|
| CH5 | texto | texto | fórmula M13 | CONFIRMED |
| K13/L13/M13 | valor numérico; kVA documentado | não confirmado no baseline | F9/docs | PARTIAL |
| I13 | texto literal | não aplicável | fórmula I13 | CONFIRMED para saída |
| D13/H13/D14/H9 | numérico | dimensão não fechada | fórmula I13 | UNKNOWN |
| Ib/In/Iz | A documental | A para comparação | CQTS proteção | CONFIRMED para regra isolada |
| B9/B10 | contexto variável | UNKNOWN | C13 catalogada | UNKNOWN |
| C11/C12 | R/X; Ohms no cabeçalho de cópia | não formalizado no baseline | F10 candidate evidence | PARTIAL |
| C13 | combinação observada | Ohm apenas inferido na cópia | F10 | UNKNOWN/PARTIAL |
| ACUMULADA | kVA documental | kVA para cadeia completa não validado | CQTS docs | PARTIAL |
| corrente | A como cabeçalho | fórmula QDT unknown; CQTS parcial | F4/F5 | BLOCKED |
| tensão linha | kV | kV | catálogo histórico | DOCUMENTARY_ONLY |
| tensão ponto | kV/V | conversão não fechada | catálogo | BLOCKED |
| comprimento | m | m | documentação | DOCUMENTARY_ONLY |
| R/X | Ohm | Ohm ou Ohm/km não fechado | F10/catálogo | PARTIAL |
| queda | V/% | base não fechada | F4/F5 | BLOCKED |

`INFERRED` não é `CONFIRMED`. `TOLERANCE = UNDEFINED`; não foi introduzida tolerância.
