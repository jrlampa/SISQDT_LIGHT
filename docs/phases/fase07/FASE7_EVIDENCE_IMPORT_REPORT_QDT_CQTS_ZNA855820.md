# Fase 7 — Relatório Evidence Store e Importer

## Implementado

`ExcelEvidenceImporter` lê XLSM/XLSX como ZIP/XML em memória, calcula SHA-256, lista sheets, resolve relações de workbook e extrai endereço, fórmula, cache e referências de externalLinks. `PrecedenceGraphExtractor` preserva referências de células, `#REF!` e referências externas. `EvidenceStore` persiste artifacts, cells e references no SQLite em transação.

Não executa macros, VBA, Solver, COM, links externos ou fórmulas. O conteúdo importado permanece dado literal.

## Testes

- importação do QDT XLSM;
- importação do CQTS XLSX;
- preservação de sheets e cells;
- preservação do hash e external links;
- extração de referências quebradas/externas;
- persistência transacional de evidência;
- ausência de dependência Microsoft Office.

## Limitações

Defined names, tables, print metadata e formula references estruturadas ainda precisam de expansão no próximo incremento; a versão atual é o primeiro extrator P0 e não declara normalização completa.