# Contratos de cálculo QDT + CQTS

## Enums

```text
CalculationMode = QDT | CQTS
CalculationStatus = READY | PASS | FAIL | UNKNOWN | BLOCKED
ResultScope = TRANSFORMER | CIRCUIT | BRANCH | NODE | EDGE | LOAD | POINT | GLOBAL
```

## ICalculationEngine

```text
CalculationResult Calculate(CalculationRequest request)
```

Pré-condições: versão válida, topologia válida para o modo, inputs e unidades resolvidos, regras críticas conhecidas. Caso contrário, retorna `BLOCKED` com diagnósticos; nunca usa default silencioso.

## CalculationRequest

```text
CalculationRequest
  project_version_id
  calculation_mode
  network_model
  transformer_inputs
  circuit_inputs[]
  topology
  loads[]
  conductors[]
  electrical_parameters[]
  evidence_context
  algorithm_version
```

## CalculationResult

```text
CalculationResult
  run_id
  calculation_mode
  algorithm_version
  input_hash
  status
  transformer_results[]
  circuit_results[]
  branch_results[]
  node_results[]
  edge_results[]
  load_results[]
  point_results[]
  validations[]
  trace_steps[]
  diagnostics[]
```

Cada item de resultado contém `scope`, `entity_id`, `value`, `unit`, `status`, `rule_id`, `formula_id?`, `evidence_id?`, `run_id` e `golden_case_id?`.

## Engines

### QDTCalculationEngine

Recebe somente regras QDT comprovadas e preserva a ordem das operações Excel. Queda QDT, dependências `#REF!`, `DecInv` ou unidade crítica ausente produzem `BLOCKED`.

### CQTSCalculationEngine

Recebe a mesma rede e usa regras CQTS comprovadas: acumulação por montante, fases, condutor e proteção conforme evidência. Não substitui fórmulas QDT.

## TraceStep

```text
TraceStep
  step_id
  rule_id
  formula_id?
  input_paths[]
  operation
  operands[]
  output_path
  output_value
  unit
  source_refs[]
  status
```

## Bloqueios

```text
CalculationBlockedException
  code: UNKNOWN_RULE | MISSING_INPUT | MISSING_UNIT |
        BROKEN_REFERENCE | EXTERNAL_DEPENDENCY | UNSUPPORTED_MODE
  source_refs[]
  message
```

A aplicação pode converter a exceção em resultado estruturado, mas não pode capturá-la e retornar zero.