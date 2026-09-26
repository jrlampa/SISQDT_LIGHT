# Golden Dataset QDT + CQTS — ZNA855820

**Status:** BLOCKED para paridade completa. Este arquivo congela somente observações já extraídas; campos ausentes ficam explicitamente `BLOCKED`, nunca inventados.

## Estrutura obrigatória

Cada caso deve conter `case_id`, workbook, aba, célula de origem, input, unidade, intermediário, esperado, fórmula, tolerância e observações. A tolerância permanece `UNDEFINED` até ser derivada.

## Casos observados

| Case ID | Fonte | Entradas conhecidas | Intermediários/saídas conhecidas | Estado |
|---|---|---|---|---|
| QDT-001 | QDT ATUAL `LADO 1` | `C6=40`, `D6=20`, `E6=13.2`, `G6=2`, `H6=2` | `I13=OK !`; `K13=186.08282352941154`; `L13=186.08282352941154`; `M13=186.08282352941154` | BLOCKED: cadeia completa ausente |
| QDT-002 | QDT ATUAL `LADO 2` | cabeçalhos/estrutura equivalentes | valores cacheados presentes | BLOCKED: snapshot completo ausente |
| QDT-003 | QDT PROJ | alterações de cache em `LADO 1/2` | diferenças ATUAL/PROJ observadas | BLOCKED: entradas completas ausentes |
| QDT-004 | QDT | alteração isolada de carga | não congelada | BLOCKED |
| QDT-005 | QDT | alteração isolada de condutor | não congelada | BLOCKED |
| QDT-006 | QDT | alteração isolada de comprimento | não congelada | BLOCKED |
| QDT-007 | QDT | múltiplos trechos/ATUAL×PROJ | estrutura conhecida, valores incompletos | BLOCKED |
| CQTS-001 | CQTS ATUAL `LADO 1` linha 8 | `PONTO=1`, `TRECHO=0`, `D=4`, `H=22.5224`, `J=240 Cu`, `FASE=TRI`, `U=3`, `W=20` | `I=186.082823529412`; `K=162.780254933745`; `X=162.780254933745`; `Y=160`; `Z=430`; `AA=VERIFICAR` | PARTIAL |
| CQTS-002 | CQTS ATUAL `LADO 1` linha 9 | `PONTO=2`, montante 1, `D=40`, `H=91.5896`, `J=185 Al - MX` | `I=97.4552`; `X=85.2511908392861`; `Y=80`; `Z=355`; `AA=VERIFICAR` | PARTIAL |
| CQTS-003 | CQTS ATUAL `LADO 1` linha 10 | `PONTO=3`, montante 2, `D=28`, `H=7.2944`, `J=70 Al - MX` | `I=7.2944`; `X=6.38094515693455`; `Y=6`; `Z=202`; `AA=VERIFICAR` | PARTIAL |
| CQTS-004 | CQTS PROJ `PROJ1`/`GERAL PROJ` | `J2=SIM`, `M=50`, condutores projetados | `Q2=SOBRECARGA`; `T4=28`; `T5=80` | PARTIAL |
| CQTS-005 | CQTS PROJ | múltiplos lados/ramos inferidos | resultados projetados incompletos | BLOCKED |

## Casos exigidos ainda não congelados

- QDT ATUAL completo: transformador, LADO 1, LADO 2, ramal e distribuição de cargas.
- QDT PROJ completo e diferença de cada entrada.
- alteração isolada de carga, condutor e comprimento em cópias de trabalho.
- sequência de três ou mais trechos com todos os acumulados.
- CQTS cadeia linear completa e múltiplos ramos com inputs e resultados.

## Registro de tolerância

```text
TOLERANCE = UNDEFINED
```

Não usar os valores cacheados como substitutos de inputs ausentes. Cada caso só pode mudar para `FROZEN` depois de possuir hash do snapshot e todas as células dependentes críticas.