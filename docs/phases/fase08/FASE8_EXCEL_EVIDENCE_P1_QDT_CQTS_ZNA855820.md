# Fase 8 — Excel Evidence P1

## Implementado

O importer OOXML read-only preserva:

- workbook hash;
- sheets e relações workbook/worksheet;
- cell address, fórmula literal e cached value;
- external link entries;
- defined names, escopo local/global e hidden;
- tabelas, sheet, range e colunas;
- structured reference tokens sem substituir a fórmula original;
- Print Area como metadado de evidência;
- formula references conservadoras.

Nenhum Excel, COM, VBA, Solver ou link externo é executado.

## Testes reais

Os testes importam QDT XLSM e CQTS XLSX fornecidos e verificam sheets, células, nomes, tabelas, structured refs, Print Area, hash e external links.

## Limitações P1

- Persistência das novas coleções P1 no SQLite ainda precisa de tabelas específicas para defined names/tables/print metadata.
- Page setup detalhado/page breaks ainda não é normalizado como entidade.
- Structured references são preservadas como tokens; resolução semântica completa de linha/tabela permanece `UNKNOWN`.

**Estado:** `GO RESTRICTED` para Evidence P1.