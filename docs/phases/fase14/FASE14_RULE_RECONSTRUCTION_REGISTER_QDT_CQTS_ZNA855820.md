# Fase 14 — Registro de reconstrução de regras

`ReconstructionVersion = F14-RECON-1`  
`RuleSetVersion permanece F9-RULESET-1`.

| RuleId | RuleVersion | ReconstructionVersion | EvidenceIds | Source documents | Confidence | Status | Implementável nesta fase |
|---|---:|---|---|---|---|---|---|
| `QDT.LADO1.SELECT_KL_TO_M13` | 1 | F14-RECON-1 | EVID-F9-M13-FORMULA | F9 subset/parity; F13 ruleset | DIRECT | RECONSTRUCTED | NÃO |
| `QDT.LADO1.VALIDATION_I13` | 1 | F14-RECON-1 | EVID-F9-I13-FORMULA | F9 implementation/subset | DIRECT | RECONSTRUCTED | NÃO |
| `CQTS.PROTECTION.IB_IN_IZ` | 1 | F14-RECON-1 | EVID-F9-PROTECTION-RANGE | F9 subset/parity; F13 ruleset | DIRECT | RECONSTRUCTED | NÃO |
| `QDT.RAMAL.LOAD_COMPOSITION` | não estabelecida | F14-RECON-1 | EVID-F10-RAMAIS-C13; EVID-F10-LOAD-CATALOG | F10 reports; candidates | PARTIAL_RECONSTRUCTION | PARTIAL/BLOCKED | NÃO |
| `CQTS.ACCUMULATED_LOAD` | não estabelecida | F14-RECON-1 | EVID-CQTS-ACCUMULATED-SUMIF | precedence/auditoria | PARTIAL_RECONSTRUCTION | PARTIAL | NÃO |

Nenhuma reconstrução altera o ruleset executável ou promove baseline/golden.
