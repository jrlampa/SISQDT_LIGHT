# Fase 13 — Versionamento do ruleset

`RULESET_VERSION = F9-RULESET-1`.

| RuleId | RuleVersion | Formula/regra | Status | IntroducedInPhase |
|---|---:|---|---|---|
| `QDT.LADO1.SELECT_KL_TO_M13` | 1 | `M13=IF(CH5="SIM",K13,L13)` | isolada/testada | Fase 9 |
| `QDT.LADO1.VALIDATION_I13` | 1 | fórmula textual I13 validada | isolada/testada | Fase 9 |
| `CQTS.PROTECTION.IB_IN_IZ` | 1 | `Ib <= In <= Iz` | isolada/testada | Fase 9 |

Cada alteração futura deverá criar nova `RuleVersion`, com FormulaId, EvidenceId, SourceArtifactHash, GoldenCaseId, motivo e teste. Não há alteração matemática nesta fase.

A exclusão `Analise Ponto a Ponto` é política versionada separadamente: `PROTOTYPE-EXCLUSION-1`.
