# Fase 13 — Versionamento e configuração

## Política estabelecida

O projeto passa a manter dimensões independentes:

| Dimensão | Valor atual |
|---|---|
| `CODE_VERSION` | `0.1.0` |
| `CODE_BASELINE` | `BASELINE-CODE-F12` |
| `SCHEMA_VERSION` | `1` |
| `EVIDENCE_VERSION` | `EVIDENCE-1-PARTIAL` |
| `BASELINE_VERSION` | `UNRECONCILED` para o modelo elétrico |
| `RULESET_VERSION` | `F9-RULESET-1` |
| `GOLDEN_DATASET_VERSION` | `NOT_ESTABLISHED` |
| `MODEL_VERSION` | `NOT_ESTABLISHED` |
| `CALCULATION_RUN_VERSION` | `NOT_ESTABLISHED` |

SemGit: o workspace não é um repositório Git. Foi adotado manifesto local; nenhum histórico ou commit foi fabricado.

## Governança

Alterações de cálculo, domínio, importação, evidência, precedence, regras ou golden exigem artefato, hash, versão, fonte, teste, fase e gate. A aba protótipo permanece `EXCLUDED_PROTOTYPE` versão `PROTOTYPE-EXCLUSION-1`.

O modelo elétrico continua separado da linha do código e não foi promovido.
