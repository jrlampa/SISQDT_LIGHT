# Fase 17 — Reconstrução assistida pelo usuário

## Perguntas e respostas

- `AM13=277`: o usuário esclareceu que a coluna AM recebe o condutor; no arquivo atual o valor cacheado 277 não representa o texto semântico do condutor porque a célula pode depender de seleção/lookup. O condutor operacional informado para o trecho é `185 Al - MX`.
- `AN13`: o usuário esclareceu que é a amperagem do condutor, normalmente preenchida manualmente ou por VLOOKUP. No trecho real observado: `AN13=430 A`.
- `P13/BX13`: o usuário esclareceu que são dados de **temperatura do condutor**; BX13 valida/exibe o resultado no fluxo subterrâneo. Não são tensão.

## Evidência confirmada no arquivo

- `P10`: `Temp. do cabo [°C]`.
- `AN11`: corrente do cabo para a condição.
- `AP11`: comprimento `[m]`.
- `BX5`: tensão `[V]`; `BX6=220` é a tensão de referência do secundário.
- `AO13=AN13*AP13=860`.

O conhecimento operacional foi usado para direcionar a leitura e confirmado pelos cabeçalhos/valores OOXML. O candidato continua não sendo baseline oficial.
