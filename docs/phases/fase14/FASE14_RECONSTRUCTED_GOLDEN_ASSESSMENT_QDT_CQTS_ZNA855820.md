# Fase 14 — Avaliação de reconstructed golden

## Resultado

`NON_OFFICIAL_RECONSTRUCTED_GOLDEN = NONE`.

Os valores QDT-001 e CQTS-001 estão documentados como observações/caches e permitem testar as três regras isoladas, mas não satisfazem simultaneamente baseline oficial, inputs completos, unidades confirmadas e precedência completa. Portanto não foram promovidos a `RECONSTRUCTED_GOLDEN` nem a golden oficial.

## Regras isoladas

- M13: fórmula direta e resultado observado; parity isolada de teste, não parity do workbook oficial.
- I13: fórmula direta e textos exatos; snapshot de precedentes real incompleto.
- Ib/In/Iz: relação direta; fontes de obtenção de Ib/In/Iz bloqueadas.

Não há chained golden. `GOLDEN_DATASET_VERSION` permanece `NOT_ESTABLISHED`.
