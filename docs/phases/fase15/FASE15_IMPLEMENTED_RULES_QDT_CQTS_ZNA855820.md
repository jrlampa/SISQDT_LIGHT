# Fase 15 — Regras implementadas

## Regra candidata

- `RuleId`: `QDT.RAMAL.CANDIDATE_RX_COMBINATION`
- `RuleVersion`: `1`
- `ImplementationVersion`: `F15-CANDIDATE-1`
- `FormulaId`: `QDT.RAMAL.CANDIDATE_RX_COMBINATION.FORMULA`
- `EvidenceId`: `F15-CANDIDATE-RAMAIS-C13`
- `ReconstructionVersion`: `F14-RECON-1`
- Entradas: C11/C12, ambas `Ohm`
- Saída: C13, `Ohm` no contexto candidato
- Status: `CANDIDATE_REPRODUCED`

A implementação é isolada, possui trace e bloqueia unidades desconhecidas. Não está integrada ao engine completo, não altera `QDT.RAMAL.LOAD_COMPOSITION`, não altera `F9-RULESET-1` e não promove o baseline elétrico.

As três regras Fase 9 permanecem inalteradas.
