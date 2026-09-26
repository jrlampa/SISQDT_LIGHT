# Fase 9 — Decisão de gate

## Decisão

**GO RESTRICTED — FIRST CLOSED MATHEMATICAL SUBSET**

O gate autoriza somente as três regras isoladas documentadas na matriz da Fase 9, com trace e testes. Não autoriza cálculo de projeto, dimensionamento, fluxo ou qualquer engine completo.

## Estado dos engines

- QDT: `PARTIAL/RESTRICTED`; regras isoladas disponíveis, engine completo ainda `BLOCKED/ENGINE_NOT_READY`.
- CQTS: `NO-GO` para cálculo completo; regra isolada de proteção disponível.
- UI WPF: permanece design-only.
- Excel: permanece oracle/evidence-only; nenhum workbook foi alterado.

## Bloqueios mantidos

Composição de carga, Solver, DecInv, unidades/tolerâncias completas, casos golden completos e regras elétricas dependentes permanecem fora do gate.

## Validação executada

- Build/teste específico das regras: 18 aprovados.
- Suíte anterior: 30 aprovados antes da Fase 9.
- Próximo fechamento deve executar a suíte completa e verificar novamente os hashes dos quatro workbooks.
