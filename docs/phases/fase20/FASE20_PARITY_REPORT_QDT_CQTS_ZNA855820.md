# Fase 20 — Relatório de Paridade

## 1. Execução da Suíte de Testes Automatizada

A suíte completa foi compilada e executada via SDK .NET 8:
- **Total de Testes:** 56 testes (3 novos testes adicionados para a regra `CQTS.REAL_PROJECT.END_LOAD_SELECTION`)
- **Aprovados:** 56
- **Falhas:** 0
- **Ignorados:** 0
- **Diagnósticos:** 0
- **Status:** Sucesso Total

## 2. Cobertura da Nova Regra

No projeto `QdtCqts.Tests.Domain`, foram validados os seguintes cenários para `CQTS.REAL_PROJECT.END_LOAD_SELECTION`:
1. `CandidateEndLoadSelectionReproducesProj7AndProj4`:
   - PROJ 7 ($D=47, E=74.448\text{ kVA}, G=1 \rightarrow M=74.448\text{ kVA}$) $\rightarrow$ Aprovado.
   - PROJ 4 ($D=48, E=75.68658823529411\text{ kVA}, G=1 \rightarrow M=75.68658823529411\text{ kVA}$) $\rightarrow$ Aprovado.
2. `CandidateEndLoadSelectionAppliesConsumerFloors`:
   - Piso monofásico ($D=1, E=1.88, G=1 \rightarrow M=4.0\text{ kVA}$) $\rightarrow$ Aprovado.
   - Piso bifásico/trifásico ($D=2, E=3.0, G=1 \rightarrow M=8.0\text{ kVA}$) $\rightarrow$ Aprovado.
3. `CandidateEndLoadSelectionBlocksMissingOrInvalidInputs`:
   - Ausência de entradas obrigatórias $\rightarrow$ Status `Blocked`.
   - Incompatibilidade de unidades elétricas $\rightarrow$ Status `Unknown`.

## 3. Cobertura Geral
Todos os módulos centrais (`Domain`, `Calculation.Cqts`, `Calculation.Qdt`, `Topology`, `Import`, `Parity`) permanecem com cobertura contínua e íntegra.
