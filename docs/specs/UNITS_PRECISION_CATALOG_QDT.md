# Catálogo de unidades, precisão e arredondamento — QDT

**Status:** PARTIAL / BLOCKED para o motor completo. Unidades confirmadas por cabeçalhos são utilizáveis no armazenamento; unidades de impedância corrigida, ETA e queda ainda exigem validação por fórmula e golden case.

## Catálogo

| Grandeza/campo | Unidade Excel | Unidade interna proposta | Conversão | Produtor/consumidor | Precisão/arredondamento | Status |
|---|---|---|---|---|---|---|
| potência do transformador | MVA | MVA | 1 | QDT `LADO` | valor interno Excel | READY |
| impedância do transformador | % | percentual Excel | 1 | QDT `LADO` | fórmula consumidora não fechada | PARTIAL |
| tensão de linha | kV | kV | 1 | `T_LINHA_*`, `LADO` | valor interno | READY |
| tensão de ponto | kV/V conforme fórmula | manter unidade explícita por campo | ainda não fixada | QDT/CQTS | não arredondar | BLOCKED |
| carga/demanda | kVA | kVA | 1 | `CARGA`, `ACUMULADA`, `DEMANDA` | valor interno | READY |
| corrente | A | A | 1 | `Ib`, `In`, `Iz`, carregamento | valor interno | READY |
| comprimento/trecho | m | m | 1 | `TRECHO`, `COMPRIMENTO` | valor interno | READY |
| temperatura | °C | °C | 1 | `TEMP`, `TEMP_AMB` | valor interno | READY |
| resistência R | Ohms no QDT | decimal com unidade de escopo anexada | não presumir `/km` | `Ramais`, fórmulas de queda | pendente | PARTIAL |
| reatância X | Ohms no QDT | decimal com unidade de escopo anexada | não presumir `/km` | `Ramais`, fórmulas de queda | pendente | PARTIAL |
| R corrigida / `R CORR` | campo resistivo | decimal com unidade documentada | fórmula necessária | CQTS | pendente | BLOCKED |
| ETA | fator | adimensional | 1 | CQTS corrente | valor interno | PARTIAL |
| fator de potência | fator | adimensional | 1 | `fator_potencia`/DB | valor interno | READY como armazenamento |
| percentual de queda/quantidade | `%` | proporção ou percentual conforme fórmula | não converter antes de provar | `QT %`, queda | pendente | PARTIAL |
| seção/condutor | texto de catálogo | chave de catálogo | nenhuma | `CONDUTOR`, `CABOS` | identificação exata | READY |
| coordenada UTM | X/Y em m | m + CRS/zona | 1 | `COORDENADAS` | sem arredondar | PARTIAL |

## Regras de precisão

1. Armazenar valores numéricos internos como dupla precisão equivalente ao Excel.
2. Nunca alimentar fórmula com o texto formatado na tela.
3. Preservar a ordem de operações da fórmula original.
4. Não aplicar `ROUND`, truncamento ou casas de apresentação sem evidência de que a planilha usa isso na cadeia.
5. Status, identificadores e textos são comparados por igualdade exata.
6. Valores numéricos permanecem com `TOLERANCE = UNDEFINED` até existir golden dataset completo e teste de sensibilidade.

## Busca de arredondamento

A inspeção das fórmulas amostradas não encontrou `ROUND`, `ROUNDUP`, `ROUNDDOWN`, `INT`, `TRUNC` ou `MROUND`. Isso não fecha todas as abas/fórmulas compartilhadas. O extrator deve pesquisar também equivalentes localizados (`ARRED`, `ARREDONDAR.PARA.CIMA`, `ARREDONDAR.PARA.BAIXO`) e VBA, embora o VBA tenha sido classificado como não cálculo.

## Itens que impedem READY completo

- fórmula QDT completa de queda de tensão;
- escopo físico de R/X e R CORR;
- interpretação de ETA;
- transformação kV/V em cada ramo;
- CRS/zona UTM;
- arredondamentos fora das amostras;
- tolerâncias por variável.

**Conclusão:** o catálogo é suficiente para definir tipos e armazenamento, não para liberar o cálculo elétrico completo.