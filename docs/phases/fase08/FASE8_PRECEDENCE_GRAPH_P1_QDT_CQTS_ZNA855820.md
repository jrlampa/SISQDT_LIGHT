# Fase 8 — Precedence Graph P1

## Implementado

`PrecedenceGraphExtractor` reconhece e preserva:

- `CELL`;
- `BROKEN_REFERENCE` para `#REF!`;
- `EXTERNAL_REFERENCE` para fórmulas com workbook externo;
- fórmula original e célula fonte;
- status de resolução conservador (`unknown`/`missing`).

O importer anexa o conjunto de formula references ao workbook importado. A detecção de ciclos de fórmula é separada do `TopologyValidator`.

## Testes

- referência direta;
- referência quebrada;
- referência externa;
- ciclo `Sheet1!A1 -> B1 -> A1`;
- referências reais extraídas do QDT.

## Estado crítico QDT

As cadeias de carga, seleção K/L e validação lógica são reconhecíveis. Corrente, tensão, queda, carregamento e resultados finais ainda possuem precedentes `UNKNOWN/BROKEN/EXTERNAL` ou fórmula não fechada.

**Estado:** `GO RESTRICTED` P1 para evidência; `NO-GO` para grafo matemático crítico completo.