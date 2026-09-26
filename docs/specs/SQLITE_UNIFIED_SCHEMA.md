# SQLite unificado QDT + CQTS

## Decisão

Um único `application.db` contém QDT e CQTS. `model_kind`/`calculation_mode` distinguem regras e resultados, não bancos.

## Núcleo

```text
projects
  -> project_versions
      -> network_models
          -> transformers
          -> circuits
          -> topology_nodes
          -> topology_edges
          -> branches
          -> loads
          -> conductors
          -> electrical_parameters
```

## Execução e evidência

```text
calculation_runs
  -> calculation_inputs
  -> calculation_results
  -> validations
  -> trace_steps

source_artifacts
  -> source_cells
  -> formula_references

golden_cases
  -> golden_observations
```

## Ajustes sobre o DDL da Fase 2

1. Renomear conceitualmente `nodes`/`edges` para `topology_nodes`/`topology_edges` ou manter nomes físicos com camada de repositório; a semântica deve ser topológica.
2. Adicionar `network_models` para impedir que `ProjectVersion` misture diretamente fontes QDT/CQTS.
3. Adicionar `calculation_mode` e `algorithm_version` a cada run.
4. Adicionar `calculation_inputs` imutáveis por run, além do hash.
5. Adicionar `trace_steps` para `RuleId`, `FormulaId`, `EvidenceId` e paths.
6. Manter `source_artifacts`, `source_cells` e `formula_references` separados do domínio.

## DDL lógico

```sql
projects(id, code, name, created_at, updated_at)
project_versions(id, project_id, label, model_kind, source_hash, state, parent_version_id)
network_models(id, version_id, kind, root_node_id, schema_version)
transformers(id, network_model_id, external_key, power_mva, impedance_percent, line_voltage_kv, demand_kva)
circuits(id, network_model_id, transformer_id, external_key, side_index, mode)
topology_nodes(id, network_model_id, circuit_id, external_key, parent_node_id, x, y, crs, is_source)
topology_edges(id, network_model_id, circuit_id, from_node_id, to_node_id, length_m, conductor_id, phase, installation)
branches(id, network_model_id, circuit_id, root_node_id, external_key)
loads(id, network_model_id, node_id, branch_id, kind, external_key, clients, kva, factor)
conductors(id, version_id, catalog_key, name, ampacity_a, resistance, reactance, units_json)
electrical_parameters(id, version_id, key, value_numeric, value_text, unit, certainty, source_ref)
calculation_runs(id, version_id, network_model_id, calculation_mode, algorithm_version, input_hash, status, started_at, completed_at)
calculation_inputs(id, run_id, entity_type, entity_id, path, value_text, value_numeric, unit, source_ref)
calculation_results(id, run_id, scope, entity_id, path, value_text, value_numeric, unit, status, rule_id, formula_id, evidence_id)
validations(id, run_id, version_id, code, scope_type, scope_id, severity, status, message, source_ref)
trace_steps(id, run_id, step_id, rule_id, formula_id, evidence_id, input_paths, output_path, operands_json, value_numeric, unit, status)
source_artifacts(id, path, file_name, file_type, sha256, captured_at)
source_cells(id, artifact_id, sheet_name, address, formula, cached_value_text, cached_value_numeric, classification)
formula_references(id, source_cell_id, precedent_ref, ref_kind, resolution_status)
golden_cases(id, case_id, model_kind, source_artifact_id, source_hash, input_snapshot_hash, status, tolerance_policy)
golden_observations(id, case_id, path, source_cell_id, expected_text, expected_numeric, unit, formula_id, comparison_mode)
```

## Integridade

- `PRAGMA foreign_keys=ON`.
- FKs compostas ou validação transacional garantem que edge e endpoints pertencem ao mesmo `network_model/circuit`.
- `CHECK` para `from <> to`, status e mode.
- índices em version, circuit, node parent, edge from/to, run, source cell e golden case.
- WAL, transaction per import/version/run, backup API e restauração testada.
- `UNKNOWN/BLOCKED` é estado persistido, não exceção descartada.

Não criar tabelas por aba ou por célula.