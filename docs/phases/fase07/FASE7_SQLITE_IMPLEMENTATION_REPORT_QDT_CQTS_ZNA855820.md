# Fase 7 — Relatório SQLite

## Implementado

`SqliteDatabase` cria um único `application.db`, ativa `foreign_keys`, `WAL` e `busy_timeout`, executa migration inicial idempotente e oferece backup por `BackupDatabase`.

O schema cobre projetos, versões, network models, transformadores, circuitos, nós, edges, branches, loads, condutores, parâmetros, runs, inputs, resultados, validações, trace, evidência e golden cases.

## Testes

- criação limpa;
- tabelas presentes;
- `PRAGMA foreign_keys=1`;
- `PRAGMA journal_mode=wal`;
- backup e abertura do backup;
- migration idempotente por `CREATE IF NOT EXISTS`/`INSERT OR IGNORE`.

Rollback/FK de inserção inválida e restore com dados ainda devem ser ampliados no próximo incremento.
