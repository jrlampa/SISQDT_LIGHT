# Fase 10 — Relatório de parity

## Baseline

Build OK e **42 testes aprovados**. A verificação de hashes falhou como gate de evidência: nenhum workbook disponível corresponde aos hashes Fase 8.

## Regras

- `QDT.LADO1.SELECT_KL_TO_M13`: parity isolada da Fase 9 permanece válida.
- `QDT.LADO1.VALIDATION_I13`: parity isolada da Fase 9 permanece válida.
- `CQTS.PROTECTION.IB_IN_IZ`: parity isolada da Fase 9 permanece válida.
- `QDT.RAMAL.LOAD_COMPOSITION`: `BLOCKED`; não há golden oficial nem comparação Excel/native.
- cadeia encadeada QDT: `BLOCKED`; não há intermediário real comprovado.

Nenhuma tolerância foi adicionada. A expressão observada em cópia produz `1.13926612` para `C11=1.0903` e `C12=0.4034`, mas esse resultado não é golden do baseline e não será promovido.
