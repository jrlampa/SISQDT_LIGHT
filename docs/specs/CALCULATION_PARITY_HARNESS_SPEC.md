# Especificação do parity harness QDT + CQTS

**Status:** SPECIFICATION ONLY; execução bloqueada até golden dataset completo.

## Objetivo

Comparar uma implementação nativa futura com snapshots Excel sem executar Excel como runtime do produto.

```text
Workbook read-only
  -> Evidence extractor
  -> GoldenCase SQLite
  -> CalculationRequest
  -> Native engine
  -> Comparison
  -> divergence report
```

## Contrato lógico

```text
GoldenCase {
  case_id
  source_workbook
  source_hash
  model_kind
  inputs[]
  expected_observations[]
  tolerance_by_variable
}

Observation {
  path
  source_cell
  formula_source
  expected_text
  expected_numeric
  unit
  comparison_mode
}

ComparisonResult {
  case_id
  path
  pass_fail_unknown
  excel_value
  app_value
  absolute_delta
  relative_delta
  unit_match
  status_match
  reason
}
```

## Modos de comparação

- texto/status/identificador: `EXACT`;
- estrutura/ordem/topologia: `EXACT`;
- número: `EXACT_NUMERIC` quando a mesma ordem de operações permitir;
- número com diferença de representação: `TOLERANCE`, somente após tolerância derivada;
- unidade: `EXACT` contra catálogo normalizado;
- fórmula/origem: rastreabilidade obrigatória, não necessariamente igualdade textual após normalização.

Até fechar precisão:

```text
TOLERANCE = UNDEFINED
```

O harness deve falhar com `UNKNOWN`, não passar, quando a tolerância ou unidade estiver indefinida.

## Relatório de divergências

Cada divergência deve exibir:

```text
case_id
source_workbook / sheet / cell
path de domínio
fórmula Excel
precedentes
valor Excel
valor App
delta absoluto/relativo
unidade
status
classificação: CRITICAL | IMPORTANT | NON-CRITICAL
```

## Requisitos de execução

1. Não abrir Excel durante o cálculo nativo.
2. Não recalcular ou salvar os workbooks originais.
3. Golden snapshots devem ser imutáveis e possuir SHA-256.
4. Uma execução deve registrar `algorithm_version` e `input_hash`.
5. Resultados intermediários são obrigatórios para `carga`, `acumulada`, `kVA`, `Ib`, `In`, `Iz`, corrente, tensão, queda, carregamento e status quando existirem.
6. `#REF!`, external link ausente ou unidade desconhecida geram `UNKNOWN/BLOCKED`.
7. O harness precisa suportar QDT ATUAL/PROJ e CQTS ATUAL/PROJ separadamente.

## Portas de teste

- `P0`: leitura do golden case e hash.
- `P1`: comparação textual e de estrutura.
- `P2`: regras READY isoladas, como `Ib <= In <= Iz`.
- `P3`: engine QDT com casos completos.
- `P4`: regressão ATUAL×PROJ.
- `P5`: CQTS/topologia.

Estado atual: P0/P1 podem ser implementados; P2 é limitado a regras isoladas; P3/P4/P5 estão bloqueados para paridade final.