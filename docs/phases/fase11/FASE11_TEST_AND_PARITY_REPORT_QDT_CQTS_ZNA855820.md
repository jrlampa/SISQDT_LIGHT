# Fase 11 — Testes e parity

## Resultados

- Teste focado de importação/precedence após exclusão: **17 aprovados**.
- Build/teste anterior da solução: **42 aprovados**.
- `QDT.LADO1.SELECT_KL_TO_M13`: preservado.
- `QDT.LADO1.VALIDATION_I13`: preservado.
- `CQTS.PROTECTION.IB_IN_IZ`: preservado.
- Engines completos: continuam `ENGINE_NOT_READY`.

## Cobertura nova

1. Aba protótipo não entra no conjunto matemático válido.
2. Fórmula originada exclusivamente nela é `EXCLUDED_PROTOTYPE`.
3. Dependência produtiva para ela é `EXCLUDED_PROTOTYPE_DEPENDENCY`.
4. Ciclo exclusivo nela não contamina o grafo válido.
5. Reconhecimento funciona com e sem acentos.

## Limitações

Não há parity Excel/native nova porque os quatro workbooks oficiais e o `application.db` histórico não foram reconciliados. Os quatro hashes atuais são apenas `CURRENT_ARTIFACT_HASH`.
