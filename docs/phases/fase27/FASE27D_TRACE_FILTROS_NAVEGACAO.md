# FASE 27D — NAVEGAÇÃO AVANÇADA, TRACE ELÉTRICO, FILTROS VISUAIS E OTIMIZAÇÃO CONTROLADA DO UNIFILAR

## 1. BASELINE

| Atributo | Estado Inicial Esperado | Estado Inicial Verificado | Status |
| :--- | :--- | :--- | :--- |
| **Branch** | `dev` | `dev` | Conforme |
| **HEAD Anterior** | `5004632051c1afb93125e759039dbec23847e797` | `5004632051c1afb93125e759039dbec23847e797` | Conforme |
| **Origin/dev Anterior**| `5004632051c1afb93125e759039dbec23847e797` | `5004632051c1afb93125e759039dbec23847e797` | Conforme |
| **Autor Git** | `Jonatas` | `Jonatas` (`jonathan@im3brasil.com.br`) | Conforme |
| **Build Inicial** | 0 erros / 0 avisos | 0 erros / 0 avisos | Conforme |
| **Suíte Canônica** | 161 testes canônicos | 161 testes (0 falhas) | Conforme |

---

## 2. AUDITORIA DA FASE 27C (ACHADOS)

| ID | Achado | Evidência | Correção Arquitetural | Status |
| :--- | :--- | :--- | :--- | :--- |
| **27D-AUDIT-01** | Fallback sintético de domínio em `SelectNode`, `SelectEdge`, `SelectTransformer` | `?? new Node(...)`, `?? new Edge(...)`, `?? new Transformer(...)` criavam dados artificiais caso o ID não existisse | Remoção total de fallbacks sintéticos. Rejeição da seleção com desmarcação e retorno determinístico (`SelectedElement = null`, `SelectedDetail = null`) | **CORRIGIDO** |
| **27D-AUDIT-02** | Métricas visuais hardcoded e clamp de zoom restritivo em `FitToView` | `~140 x 80` espalhados e `Math.Clamp(targetZoom, 0.25, 2.5)` sem parametrização explícita | Centralização em `UnifilarVisualMetrics` e documentação de `FitToViewMaxZoom = 2.5` como limite visual intencional de enquadramento | **CORRIGIDO** |
| **27D-AUDIT-03** | Buscas lineares repetidas $O(N)$ em coleções | `FirstOrDefault` repetido dentro dos métodos de seleção e iteração | Criação de dicionários indexados `_nodeVmById`, `_edgeVmById`, `_trafoVmById`, `_incomingEdgeByToNodeId` para resolução $O(1)$ | **CORRIGIDO** |
| **27D-AUDIT-04** | Ausência de destaque do caminho montante (Trace elétrico) | Seleção isolada sem indicar a árvore de alimentação | Implementação do algoritmo topológico determinístico `ComputeAndApplyTrace` da seleção até a raiz/transformador | **CORRIGIDO** |
| **27D-AUDIT-05** | Ausência de filtros visuais de condição elétrica e atenuação | UI exibia todos os elementos sem suporte a filtragem por sobrecarga, queda ou evidência | Criação de `UnifilarVisualFilter`, propriedade `IsDimmed` nas ViewModels e controle de opacidade via triggers WPF | **CORRIGIDO** |

---

## 3. PROBLEMAS ENCONTRADOS

1. **Criação de Entidades Sintéticas em Apresentação:** A camada de apresentação fabricava nós e arestas de domínio quando um ID desconhecido era passado, violando a integridade determinística e os dados reais.
2. **Hardcoded Offsets no Enquadramento:** As dimensões dos nós e transformadores estavam embutidas no cálculo do `FitToView` como valores mágicos (`-40`, `+150`, `-40`, `+90`), dificultando a manutenção e a precisão da centralização.
3. **Falta de Isolamento em Bifurcações no Trace:** Sem um mapa radial upstream indexado (`ToNodeId -> Edge`), percorrer o grafo exigiria buscas arbitrárias que poderiam vazar para ramos laterais não pertencentes ao caminho selecionado.

---

## 4. CORREÇÕES IMPLEMENTADAS

1. **Zero Fallbacks Sintéticos:**
   - Métodos `SelectNode`, `SelectEdge`, `SelectTransformer` agora validam contra `_domainNodesById`, `_domainEdgesById`, `_domainTrafosById`.
   - Se o elemento não existir no modelo real, nenhuma instância falsa é fabricada: a seleção é limpa e nenhum dado incorreto é projetado na interface.
2. **Centralização de Métricas (`UnifilarVisualMetrics`):**
   - Dimensões constantes (`NodeWidth = 74.0`, `NodeHeight = 48.0`, `TransformerWidth = 110.0`, `TransformerHeight = 68.0`, `DefaultMargin = 40.0`, `ViewportPaddingRatio = 0.92`, `MinZoom = 0.25`, `MaxZoom = 4.0`, `FitToViewMaxZoom = 2.5`).
   - Proteção estrita contra `NaN` e `Infinity` no cálculo de zoom e pan.
3. **Indexação para Alta Performance:**
   - Dicionários indexados por chave ordinal no construtor de `UnifilarDiagramViewModel`, permitindo trace e buscas em $O(1)$ por nó.

---

## 5. ARQUITETURA DO TRACE ELÉTRICO

- **Objetivo:** Rastrear e destacar visualmente o caminho elétrico completo da raiz/transformador até o elemento selecionado (nó terminal, nó de derivação ou trecho).
- **Algoritmo:**
  - Inicia no elemento selecionado:
    - Se for nó: marca o nó e inicia caminhada montante a partir de `nodeId`.
    - Se for aresta: marca a aresta e o nó de origem (`FromNodeId`), subindo a partir deste.
    - Se for transformador: marca o transformador e o nó raiz associado.
  - Caminhada a montante (Upstream Walk):
    - Consulta `_incomingEdgeByToNodeId` para encontrar a única aresta alimentadora daquele nó na topologia radial determinística.
    - Adiciona a aresta e seu nó de origem ao conjunto do trace.
    - Repete até encontrar o nó raiz (`dn.IsSource == true` ou presente em `_topology.RootNodeIds`).
    - Ao alcançar o nó raiz, inclui o transformador de alimentação do circuito.
- **Complexidade:** $O(H)$, onde $H$ é a profundidade máxima da árvore (para a rede de distribuição BT, $H \le 20$). Instantâneo (< 0.1 ms).
- **Isolamento de Bifurcações:**
  - Como a caminhada sobe estritamente pela aresta de entrada do nó selecionado (`edge.ToNodeId == currentWalkNodeId`), ramos irmãos nunca são visitados.
  - Selecionar `P1` na bifurcação `LID -> P1 / P2` destaca `P1 -> LID -> TR`, mantendo `P2` e seu trecho estritamente desmarcados.
- **Não-Recálculo Elétrico:** O trace é 100% de apresentação e topologia. Nenhuma corrente, potência, queda de tensão ou curto-circuito é recalculado.

---

## 6. ARQUITETURA DOS FILTROS VISUAIS

- **Modos de Filtro (`UnifilarVisualFilter`):**
  1. `Todos`: Nenhum elemento é atenuado (`IsDimmed = false`).
  2. `Sobrecarga`: Destaca trechos com `IsOverloaded == true` (consumido diretamente do `SegmentCalculationResult` do backend). Nós adjacentes a trechos sobrecarregados permanecem visíveis; demais elementos recebem `IsDimmed = true`.
  3. `QuedaTensaoLimite`: Destaca elementos cuja queda de tensão supera `VoltageDropThresholdPercent` (threshold de apresentação configurável, padrão 5.0% conforme norma de distribuição secundária BT).
  4. `EvidenceBlocked`: Destaca elementos quando o circuito possui bloqueio de evidência de proteção (`AssessmentStatus == ProtectionAssessmentStatus.EvidenceBlocked`).
  5. `Selecionados`: Foca estritamente no elemento atualmente selecionado.
  6. `Trace`: Foca estritamente nos elementos pertencentes ao caminho do trace elétrico ativo.
- **Mecanismo de Atenuação (Dimming):**
  - Implementado via propriedade `IsDimmed` (bool) nas ViewModels.
  - No XAML, disparado via `DataTrigger Binding="{Binding IsDimmed}" Value="True"` com `Setter Property="Opacity" Value="0.18"`.
  - Vantagem arquitetural: não destrói as coleções observáveis, não dispara reconstrução de layout e utiliza aceleração de hardware do WPF.

---

## 7. ESTADOS VISUAIS E HIERARQUIA DETERMINÍSTICA

| Estado | Prioridade Visual | Expressão no Trecho (Aresta) | Expressão no Nó | Expressão no Trafo |
| :--- | :--- | :--- | :--- | :--- |
| **Dimmed** | Modificador de Opacidade | `Opacity = 0.18` | `Opacity = 0.18` | `Opacity = 0.18` |
| **Normal** | Base | Traço Ciano (`#38BDF8`), 3.5px | Borda Cinza (`#475569`), 1.5px | Borda Azul (`#0284C7`), 2px |
| **Trace** | Topológico | Traço Dourado (`#FBBF24`), 5.5px | Borda Dourada (`#FBBF24`), 3px | Borda Dourada (`#FBBF24`), 3px |
| **Sobrecarga** | Elétrico Crítico | Traço Vermelho (`#EF4444`), 5px | Cor original (ou atenuado se filtrado) | Borda original |
| **Selecionado**| Foco de Interação | Traço Âmbar Forte (`#F59E0B`), 6px | Borda Branca (`#FFFFFF`), 3.5px | Borda Dourada (`#FBBF24`), 3.5px |

---

## 8. ESTRATÉGIA DE PERFORMANCE

1. **Indexação $O(1)$:** Eliminação de varreduras lineares com `FirstOrDefault` no grafo durante interações do usuário.
2. **Atenuação sem Redesenho:** Filtros operam por propriedade booleana `IsDimmed`, evitando recomputar o layout ortogonal e realocar elementos no `DiagramCanvas`.
3. **Thin Frontend:** WPF permanece estritamente como camada de visualização reativa. Toda a lógica de negócios e cálculos de grandezas elétricas permanecem no backend.

---

## 9. SUÍTE DE TESTES E COBERTURA

- **Arquivo Dedicado:** `tests/QdtCqts.Tests.Wpf/UnifilarTraceAndNavigationTests.cs` (571 linhas).
- **Cenários Testados:**
  1. `SelectNode_WithNonExistentId_DoesNotCreateSyntheticDomainEntity` (Zero fallbacks sintéticos)
  2. `SelectEdge_WithNonExistentId_DoesNotCreateSyntheticDomainEntity`
  3. `SelectTransformer_WithNonExistentId_DoesNotCreateSyntheticDomainEntity`
  4. `FitToView_ProducesFiniteZoomAndCoordinates_WithoutNaNOrInfinity` (1200x800, 3840x2160, 200x150)
  5. `FitToView_WithDegenerateViewport_ResetsToDefaultsSafely` (0x0, degenerado, NaN, Infinity)
  6. `FitToView_WithForkedNetwork_ComputesAccurateBounds`
  7. `Trace_TerminalNodeSelected_HighlightsFullPathToRootAndTransformer` (RL -> P5..P1 -> LID -> TR)
  8. `Trace_IntermediateNodeSelected_DoesNotHighlightDownstreamElements` (P3 -> montante OK, jusante desmarcado)
  9. `Trace_IntermediateEdgeSelected_HighlightsUpstreamPathAndEdgeItself`
  10. `Trace_RootNodeSelected_OnlyHighlightsRootAndTransformer`
  11. `Trace_ForkIsolation_SelectingBranchA_DoesNotHighlightBranchB` (Bifurcação LID -> P1 / P2 estritamente isolada)
  12. `Trace_ClearSelection_ClearsAllTraceFlagsDeterministically`
  13. `Trace_Determinism_ProducesIdenticalTraceAcrossRepeatedInvocations`
  14. `VisualFilter_Todos_KeepsAllElementsNonDimmed`
  15. `VisualFilter_Sobrecarga_DimsOnlyNonOverloadedElements` (Base real PROJ 4)
  16. `VisualFilter_QuedaTensaoLimite_FiltersAgainstConfigurableThreshold` (Threshold 4.0% em CQT PROJ 7)
  17. `VisualFilter_Selecionados_DimsAllNonSelectedElements`
  18. `VisualFilter_Trace_DimsAllElementsNotInTrace`
  19. `VisualFilter_EvidenceBlocked_PreservesEvidenceSemanticsDeterministically` (CQT PROJ 7 real e cenário Pass)
  20. `VisualFilter_SwitchBackToTodos_RestoresFullVisibilityForEveryElement`

---

## 10. RESULTADOS DA EXECUÇÃO

```text
Suíte de Testes Executada:
- QdtCqts.Tests.Topology:    6 aprovados (0 falhas)
- QdtCqts.Tests.Parity:      4 aprovados (0 falhas)
- QdtCqts.Tests.Domain:    106 aprovados (0 falhas)
- QdtCqts.Tests.Wpf:        53 aprovados (0 falhas)  [+25 novos testes]
- QdtCqts.Tests.Import:     17 aprovados (0 falhas)
---------------------------------------------------
Total:                     186 aprovados (0 falhas)
Taxa de Sucesso:           100%
Build:                     0 Erros / 0 Avisos
```

---

## 11. LIMITES OPERACIONAIS

1. **Topologias Suportadas:** Redes estritamente radiais ou em árvore hierárquica (padrão de distribuição BT da Light/ANEEL). Malhas fechadas continuam exigindo resolução prévia de abertura topológica.
2. **Threshold de Queda de Tensão:** Opera como threshold explícito de visualização na camada de apresentação (default 5.0%), pois o motor elétrico calcula a magnitude física de queda sem emitir veredito booleano em nível de trecho individual.

---

## 12. DÍVIDAS RESTANTES E PRÓXIMOS PASSOS

1. **Exportação Vetorial:** Possibilidade de exportar a visualização unifilar ativa (com trace e filtros destacados) em formato SVG ou PDF vetorial.
2. **Animação de Fluxo:** Implementação opcional de micro-animações (dash-offset) nas linhas do trace para indicar a direção do fluxo de potência no unifilar.
