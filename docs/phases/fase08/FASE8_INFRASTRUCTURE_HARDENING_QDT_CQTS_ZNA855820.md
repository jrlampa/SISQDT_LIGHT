# Fase 8 — Infrastructure Hardening

**Baseline:** build/test Fase 7 aprovados; workbooks preservados.  
**Resultado:** hardening parcial concluído; não libera engines matemáticos.

## Implementado/testado

- FK real: parent inexistente rejeitado por SQLite.
- Transação: rollback de artifact/cell deixa zero registros parciais.
- Commit válido: EvidenceStore persiste em transação.
- Backup/restore: backup com source artifact/cell reabre e preserva dados.
- `PRAGMA integrity_check`: `ok` através de `SqliteDatabase.IsHealthy()`.
- `PRAGMA foreign_key_check`: sem linhas através de `IsHealthy()`.
- WAL e `foreign_keys=ON` continuam ativos.
- Migration inicial repetida sem duplicação.
- EvidenceStore rejeita artifact duplicado em vez de sobrescrever.
- SHA-256 determinístico já implementado.

## Limitações

- Ainda há somente migration version 1; infraestrutura para versões futuras existe, mas não há migração incremental real.
- FK composta para garantir que edge/endpoints pertencem ao mesmo circuito ainda depende de validação transacional/topológica.
- Backup com todas as entidades de domínio ainda deve ser ampliado; evidence foi validada com dados reais de teste.

## Estado

`SQLite = GO RESTRICTED`. Integridade básica está comprovada; hardening adicional de schema e migrations permanece para próximo incremento.