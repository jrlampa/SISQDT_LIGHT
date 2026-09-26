# Grafo de precedentes QDT — ZNA855820

**Status:** PARTIAL / BLOCKED para saídas elétricas críticas. Este artefato registra somente cadeias comprovadas ou explicitamente desconhecidas; não corrige `#REF!`.

## Legenda

- `CELL`: referência de célula.
- `DEFINED_NAME`: nome definido.
- `STRUCTURED_REFERENCE`: tabela/coluna estruturada.
- `EXTERNAL_REFERENCE`: workbook externo.
- `FORMULA`: transformação local.
- `CONSTANT`: literal hardcoded.
- `BROKEN`: referência quebrada.
- `UNKNOWN`: cadeia não resolvida.

## Cadeias comprovadas

### Carga de ramal

```text
Ramais!B9/B10
  --FORMULA--> Ramais!C13
  --CONSTANT--> 0.85, 0.5268
  --FORMULA--> carga combinada
```

Fórmula observada: `=B9*0.85+B10*0.5268`. A unidade física e o significado dos dois operandos permanecem PARTIAL.

### Seleção de carga QDT

```text
LADO 1!K13, L13, CH5
  --FORMULA--> LADO 1!M13
```

Fórmula: `=IF($CH$5="SIM",K13,L13)`. Estado: READY como regra isolada; cadeia completa dos operandes: PARTIAL.

### Validação lógica QDT

```text
LADO 1!D13, H13, D14, H9
  --FORMULA--> LADO 1!I13
  --RESULT--> OK ! / Erro 02
```

Fórmula: `=IF(OR(D13="",H13=""),"",IF(OR(D13=0,H13=0),"Erro 02",IF(AND(H13>=H9,D14<=D13),"OK !","Erro 02")))`.

### CQTS acumulada

```text
CQTS LADO 1[PONTO/TRECHO/CARGA]
  --STRUCTURED_REFERENCE + SUMIF--> ACUMULADA
  --LOOKUP--> PROJ1/GERAL
```

O padrão `SUMIF([TRECHO],[@PONTO],[ACUMULADA])+[@[TOTAL DO TRECHO]]` foi observado. A generalização para todos os lados ainda depende da expansão das tabelas e ordem de linhas.

### CQTS proteção

```text
Ib, In, Iz
  --FORMULA--> PROTECAO/status
  --RESULT--> OK / VERIFICAR
```

Regra isolada observada: `Ib <= In <= Iz`. READY como validação independente, não como prova da cadeia de cálculo de `Ib`.

## Saídas críticas e estado

| Saída | Precedentes conhecidos | Desconhecidos/broken | Status |
|---|---|---|---|
| carga direta | campos de ramal/ponto | unidade de alguns operandos | PARTIAL |
| carga acumulada | `PONTO`, `TRECHO`, `SUMIF`/tabelas | ordem completa, exceções | PARTIAL |
| kVA QDT | D/E/G/flags em `LADO` | cadeia completa e constantes | PARTIAL |
| Ib/In/Iz | condutor, fase, acumulada, ETA/ampacidade | unidades R/X/ETA e fórmulas QDT | BLOCKED |
| corrente QDT | carga, tensão, fase | fórmula crítica não fechada | BLOCKED |
| tensão/queda QDT | carga, condutor, comprimento, parâmetros | fórmula e precedentes | BLOCKED |
| carregamento | corrente, limites | cadeia completa | BLOCKED |
| proteção/status | `Ib`, `In`, `Iz` | produtores de Ib/In/Iz | PARTIAL |
| resultado por trecho/ponto/lado | todos os anteriores | dependências indiretas | BLOCKED |

## Referências quebradas

| Origem | Fórmula/dependência | Impacto | Classificação |
|---|---|---|---|
| QDT `Base de Dados!M2:N22` e adjacentes | `INDEX/HLOOKUP/VLOOKUP(#REF!, ...)` | pode afetar parâmetros | UNKNOWN / potencialmente CRITICAL |
| QDT `FML!E5`, `E11` | `SUM(#REF!)` | fatores | UNKNOWN / potencialmente CRITICAL |
| CQTS nomes `RAMA_PROJ1`, `RAMA_PROJ2`, `RAMAIS_PROJ1` | `#REF!` | projetos/ramais | HIGH |
| QDT `PF e Prestação de Serviço` | referência externa `DecInv` | relatório e possível cadeia indireta | UNKNOWN até fechamento |

## Grafo textual de alto nível

```text
ENTRADAS
  -> Ramais / RAMAL / PROJ1 / pontos / trechos / condutores
  -> carga direta
  -> carga acumulada
  -> kVA / demanda
  -> corrente / Ib / In / Iz
  -> tensão / queda
  -> carregamento / proteção / status
  -> resultados por trecho, ponto, circuito e lado

PARÂMETROS AUXILIARES
  -> Base de Dados / DB / nomes definidos / tabelas
  -> [#REF!] ou UNKNOWN em pontos ainda não resolvidos

DecInv
  -> PF e Prestação de Serviço
  -> destino elétrico: UNKNOWN
```

## Critério de fechamento

`BLOCK-QDT-004` permanece OPEN. Para resolver, o extrator deve materializar cada fórmula crítica, precedentes diretos, nomes, tabelas, externos, constantes e erros, e provar que todos os nós até as saídas elétricas são resolvidos.