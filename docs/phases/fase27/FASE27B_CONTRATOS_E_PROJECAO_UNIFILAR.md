# FASE 27B — CONTRATOS DE APRESENTAÇÃO E PROJEÇÃO DO GRAFO PARA O UNIFILAR

> **Data:** 2026-09-28  
> **Status:** Concluído com Sucesso (`GATE = GO`)  
> **Branch:** `dev`  
> **Commit da Fase:** `feat(fase27b): establish unifilar presentation contracts`  
> **Papéis:** Tech Lead, Arquiteto de Software, Engenheiro de Domínio Elétrico, Especialista WPF/MVVM  

---

## 1. Visão Geral e Propósito

A **Fase 27B** estabelece formalmente os contratos de apresentação e a camada de projeção determinística necessários para transformar o modelo elétrico (`NetworkModel`) e o relatório de cálculo imutável (`NetworkCalculationReport`) em um grafo apresentável para o futuro Unifilar Operacional do `sisQDT_LIGHT`.

Seguindo estritamente as diretrizes da fase:
- **Não foi implementado o Canvas definitivo, zoom/pan complexo ou estética de UX final.**
- **Não foi criada uma segunda topologia elétrica.**
- **A camada WPF não recalcula nenhuma grandeza elétrica.**
- **O achado de duplicação do `OverloadStatus` foi sanado.**
- **A integridade topológica 1:1 entre o `TopologyValidator` e o modelo de apresentação foi comprovada por testes automatizados.**

---

## 2. Decisões Arquiteturais e Evidências das Grandezas

| Questão Arquitetural | Decisão Formal | Justificativa / Evidência |
|---|---|---|
| **Decisão sobre "QT"** | `QT = não formalizado no contrato atual` | Auditoria rigorosa no repositório comprovou ausência de definição unívoca para "QT" no modelo atual. As grandezas existentes são $S_{\text{local}}$ (`LocalLoadKva`), $E_{\text{trecho}}$ (`DownstreamAccumulatedLoadKva`) e $M_{\text{trecho}}$ (`EndLoadSelectedKva`). Nenhum campo `QT` foi inventado. |
| **Decisão sobre Demanda Acumulada** | Propriedade do Trecho (`DownstreamAccumulatedLoadKva`) | No motor (`CandidateRadialLoadAccumulationRule` e `UnifiedCalculationPipeline`), a acumulação pós-ordem reside nos trechos a montante ($E$). Não foi duplicada no nó na camada de apresentação. |
| **Decisão sobre Icc no Nó** | Mantido no Trecho (`SegmentCalculationResult`) | O curto-circuito é calculado na ponta do segmento a jusante de sua impedância. Não foi copiado cegamente para o nó para evitar conflito com nós raiz sem aresta a montante. |
| **Decisão sobre Tensão no Nó** | Exposição estrita de `AccumulatedVoltageDropPercent` | Nenhuma fórmula nova de tensão em Volts foi implementada na WPF; a queda acumulada calculada no backend é a grandeza canônica entregue. |
| **Decisão sobre Tipo Topológico** | `UnifilarNodeType` derivado da Topologia | Classificação determinística (`Source`, `Transformer`, `PassThrough`, `Branch`, `Terminal`) calculada com base no `NetworkModel` e no `TopologyValidationResult`. |
| **Decisão sobre OverloadStatus** | Consumo direto de `seg.IsOverloaded` | Removida a duplicação `OperatingCurrentAmperes > RatedAmpacityAmperes` em `SegmentDisplayModel` e `UnifilarEdgeViewModel`. |
| **Decisão sobre Layout** | `HierarchicalTreeLayoutEngine` puramente geométrico | Posicionamento radial em árvore baseado unicamente na topologia estrutural, 100% desacoplado de grandezas elétricas. |

---

## 3. Contratos e Arquivos Criados

Todos os arquivos respeitam o limite ideal de 500 linhas:

1. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/UnifilarNodeType.cs` (23 linhas)
2. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/UnifilarElementKind.cs` (12 linhas)
3. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/UnifilarNodeViewModel.cs` (51 linhas)
4. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/UnifilarEdgeViewModel.cs` (105 linhas)
5. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/UnifilarTransformerViewModel.cs` (60 linhas)
6. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/SelectedElementDetailViewModel.cs` (178 linhas)
7. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/IUnifilarLayoutEngine.cs` (20 linhas)
8. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/HierarchicalTreeLayoutEngine.cs` (138 linhas)
9. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/UnifilarPresentationBuilder.cs` (168 linhas)
10. `src/QdtCqts.Desktop.Wpf/ViewModels/Unifilar/UnifilarDiagramViewModel.cs` (144 linhas)
11. `tests/QdtCqts.Tests.Wpf/UnifilarProjectionTests.cs` (225 linhas)

---

## 4. Testes e Validação

- Testes existentes mantidos: 149 testes aprovados.
- Novos testes adicionados na Fase 27B: 7 testes cobrindo projeção de nós, trechos, transformadores, integridade topológica, layout geométrico determinístico, seleção/detalhes e integração no `MainViewModel`.
- Total da suíte: **156 testes aprovados, 0 falhas, 0 ignorados**.
- Compilação: 0 erros, 0 avisos.
