# Fase 8 — Golden Dataset Update

## Classes

- `ISOLATED_RULE_CASE`: permitido e usado para regras fechadas.
- `FULL_CALCULATION_CASE`: continua bloqueado enquanto cadeia, unidades, precedentes e tolerância não estiverem completos.

## Casos isolados autorizados

| Caso | Fonte | Estado |
|---|---|---|
| seleção K/L -> M13 | QDT `LADO 1!M13` | READY isolado |
| validação lógica -> I13 | QDT `LADO 1!I13` | READY isolado |
| composição de carga | QDT `Ramais`/CQTS `RAMAL` | PARTIAL: significado/unit dos operandos pendente |
| proteção `Ib <= In <= Iz` | CQTS `LADO` | READY isolado |

## Casos completos

QDT ATUAL, QDT PROJ, alteração de carga, condutor, comprimento, múltiplos trechos e CQTS múltiplos ramos permanecem `BLOCKED/PARTIAL` conforme o dataset da Fase 5. Nenhum valor foi inventado ou promovido a `FULL`.

## Regra de tolerância

`TOLERANCE = UNDEFINED`. O parity harness retorna `UNKNOWN` quando a tolerância, unidade ou evidência necessária não existe.

**Estado:** infraestrutura de golden preparada; dataset completo ainda não pronto para QDT Engine.