# Fase 13 — Política de versionamento de artefatos

## Estados

`DISCOVERED`, `CURRENT_CANDIDATE`, `HISTORICAL_COPY`, `OFFICIAL_BASELINE`, `DERIVED`, `RECOVERED`, `RECONSTRUCTED`, `REJECTED`, `SUPERSEDED`.

Candidatos Excel atuais permanecem `CURRENT_CANDIDATE / UNKNOWN_ORIGIN`. Não entram no Git nem são movidos.

## Hashes

Todo artefato disponível deve registrar SHA-256, tamanho, timestamp, caminho, classificação, origem e versão. Workbooks históricos ausentes são registrados como `NOT_FOUND`; hashes atuais nunca substituem hashes históricos.

## Estrutura lógica

As pastas `artifacts/qdt`, `artifacts/cqts`, `artifacts/external`, `artifacts/evidence` e `artifacts/golden` são reservadas a manifests/referências, não a cópias automáticas de binários proprietários. Os originais permanecem em seus locais.

## Segurança

Não executar macros/COM/VBA, não salvar workbooks e não registrar credenciais ou dados sensíveis.
