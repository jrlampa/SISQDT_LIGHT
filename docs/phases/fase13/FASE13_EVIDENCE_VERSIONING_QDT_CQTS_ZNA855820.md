# Fase 13 — Versionamento da evidência

## Identidade

`EVIDENCE_VERSION = EVIDENCE-1-PARTIAL` representa a infraestrutura e os relatórios P1 existentes, não um snapshot completo dos quatro workbooks.

Uma futura extração deverá registrar:

- `EvidenceVersion`;
- `SourceArtifactHash`;
- `ExtractorVersion`;
- `SchemaVersion`;
- `ExtractionTimestamp`;
- política de exclusão do protótipo.

A nova extração não deve sobrescrever silenciosamente a anterior.

## Estado atual

Evidence Store histórico não foi localizado. `application.db` não existe na árvore do projeto. As evidências da Fase 9 permanecem documentais/testadas, e a aba `Analise Ponto a Ponto` permanece `EXCLUDED_PROTOTYPE`.
