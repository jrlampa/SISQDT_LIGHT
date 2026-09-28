# FASE 27A — AUDITORIA DA INTERFACE WPF, CONTRATOS E ESPECIFICAÇÃO DO UNIFILAR OPERACIONAL

> **Data:** 2026-09-28  
> **Status:** Concluído — Diagnóstico e Especificação Arquitetural  
> **Branch:** `dev`  
> **Papéis:** Tech Lead, Arquiteto de Software, Engenheiro de Domínio Elétrico, Especialista WPF/MVVM, Auditor Técnico.  
> **Regra de Escopo:** Auditoria, diagnóstico e especificação exclusivamente. **NÃO implementar a Fase 27B**.  

---

## 1. Baseline e Verificação Inicial do Repositório

Antes de qualquer diagnóstico, a auditoria executou a verificação estrita da baseline de trabalho:

| Item | Esperado | Encontrado | Conformidade |
|---|---|---|---|
| **Branch** | `dev` | `dev` | Conforme |
| **HEAD Commit** | `77ba13d` (release 0.7.0) | `77ba13d` chore(governance): register phase 26 release | Conforme |
| **Commit Funcional** | `29c46ad` | `29c46ad` fix(protection): enforce evidence-gated assessment | Conforme |
| **Versão Manifestada** | `0.7.0` | `0.7.0` em `VERSION_MANIFEST.json` | Conforme |
| **Baseline Elétrica** | `CONFIRMED_PARITY` | `CONFIRMED_PARITY` | Conforme |
| **Compilação (`dotnet build`)** | 0 erros, 0 avisos | 0 erros, 0 avisos (15 assemblies compilados) | Conforme |
| **Suíte de Testes (`dotnet test`)** | 149 testes, 0 falhas | 149 testes aprovados, 0 falhas, 0 ignorados (~1.5s) | Conforme |
| **Alteração Pré-existente** | `QdtCqts.Desktop.Wpf_dwkfellp_wpftmp.csproj` staged como deletado | Preservada exatamente como encontrada; não tocada | Conforme |

---

## 2. Diagnóstico da Interface WPF Existente

### 2.1 Estrutura e Componentes Atuais
O projeto `src/QdtCqts.Desktop.Wpf` possui os seguintes componentes:
- **`MainWindow.xaml` / `MainWindow.xaml.cs`**:
  - Layout monolítico em `Grid` vertical de 5 linhas:
    1. Header Bar (`Row 0`): logotipo vetorial PNG e título do sistema.
    2. Controls & Actions (`Row 1`): ComboBox estático de seleção de projeto (`SelectedProject`), Botão de execução assíncrono (`ExecuteCalculationCommand`) e badge de status (`CalculationStatus`, `StatusColor`).
    3. Metrics Cards (`Row 2`): 4 cartões com métricas agregadas globais (Queda de Tensão Máxima %, Icc 3φ Máximo, Icc 1φ Mínimo e Proteção/Dispositivo).
    4. Segments DataGrid (`Row 3`): Tabela única com 11 colunas detalhando os trechos calculados (`SegmentDisplayModel`).
    5. Footer Bar (`Row 4`): Mensagem de status, `CorrelationId`, tempo de execução em milissegundos e `OutputHash` SHA-256.
- **`ViewModels/MainViewModel.cs`**:
  - 464 linhas (dentro do limite ideal de 500 linhas);
  - Controla o ciclo de vida via `CalculationUiState` (`Idle`, `Calculating`, `Success`, `ValidationError`, `CalculationError`, `EvidenceBlocked`);
  - Constrói o modelo de teste programaticamente em `BuildSelectedProjectVersion()`;
  - Invoca o serviço `CalculationService.ExecuteCalculation` em background via `Task.Run`.
- **`ViewModels/RelayCommand.cs`**:
  - Implementações puras de `RelayCommand` e `AsyncRelayCommand` com prevenção de reentrância (`_isExecuting`).

### 2.2 Avaliação do Princípio "Thin Frontend / Smart Backend"
A auditoria avaliou rigorosamente se a WPF realiza algum recálculo elétrico indevido:

| Verificação | Ocorrência na WPF | Detalhe / Evidência |
|---|---|---|
| Recálculo de Demanda | **NÃO** | Consome `EndLoadSelectedKva` do relatório do backend. |
| Recálculo de Demanda Acumulada | **NÃO** | Consome `DownstreamAccumulatedLoadKva` do relatório. |
| Recálculo de Queda de Tensão (QT / ΔV%) | **NÃO** | Consome `SegmentVoltageDropPercent` e `AccumulatedVoltageDropPercent`. |
| Recálculo de Corrente de Projeto ($I_b$) | **NÃO** | Consome `OperatingCurrentAmperes` do backend. |
| Recálculo de Curto-Circuito ($I_{\text{cc}3\phi}$ / $I_{\text{cc}1\phi}$) | **NÃO** | Consome `ShortCircuit3PhaseAmperes` e `ShortCircuit1PhaseAmperes`. |
| Recálculo de Impedância de Linha ($Z$) | **NÃO** | Consome parâmetros do relatório. |
| Duplicação da lógica do `UnifiedCalculationPipeline` | **NÃO** | Todo o pipeline executa no motor CQTS. |
| **Achado Arquitetural #1: `OverloadStatus`** | **SIM (Parcial)** | Em `SegmentDisplayModel.cs` (linha 44), a propriedade `OverloadStatus` avalia `OperatingCurrentAmperes > RatedAmpacityAmperes ? "SOBRECARGA" : "OK"`, duplicando a condição do backend, que já computa `seg.IsOverloaded`. |
| **Achado Arquitetural #2: Acoplamento de Mock/Fixture** | **SIM** | O método `BuildSelectedProjectVersion()` reside diretamente dentro da `MainViewModel`, misturando definição de modelo de dados com controle de apresentação da UI. |
| **Achado Arquitetural #3: Ausência de Dados de Nós e Transformadores na UI** | **SIM** | Embora `NetworkCalculationReport` contenha `Nodes` e `Transformers`, a UI projeta apenas `Segments` no DataGrid, ocultando informações vitais de nós, postes e barramento do trafo. |

---

## 3. Auditoria de Contratos e Modelo Topológico

### 3.1 `Domain.Topology.cs`
- O `TopologyValidator` valida circuito, detecta auto-loops, endpoints faltantes, ciclos orientados, nós órfãos, múltiplos pais e comprimentos negativos.
- Retorna `TopologyValidationResult` com listas ordenadas: `OrderedNodeIds`, `OrderedEdgeIds`, `RootNodeIds`, `TerminalNodeIds`.
- **Diagnóstico para Unifilar:**
  - O grafo gerado pela validação topológica fornece a hierarquia em árvore radial rigorosa necessária para um algoritmo de posicionamento unifilar.
  - A entidade `Node` possui propriedades `X` e `Y` (`double?`), além de `ParentNodeId`.
  - **Limitação identificada:** No fixture atual dos projetos reais (PROJ 7 e PROJ 4), `Node.X` e `Node.Y` contêm apenas índices discretos de sequência (ex.: `0, 0`, `1, 0`, `2, 0`...). Em dados geográficos reais, eles virão como coordenadas UTM planas (2.5D SIRGAS2000). Para renderizar um **Unifilar Elétrico Esquemático Operacional**, a interface necessita de uma estratégia de layout esquemático (projeção esquemática unifilar) com espaçamento ortogonal legível para engenharia, independente de coordenadas de mapa.

### 3.2 `CalculationOutputModels.cs` e `NetworkCalculationReport`
A estrutura do relatório atual entrega:
1. `Transformers`: lista de `TransformerCalculationResult` (potência nominal, carga de operação, impedância %, queda interna e MT);
2. `Segments`: lista de `SegmentCalculationResult` (36 grandezas elétricas e físicas completas por trecho);
3. `Nodes`: lista de `NodeCalculationResult` (`NodeId`, `ExternalKey`, `LocalConsumers`, `LocalLoadKva`, `AccumulatedVoltageDropPercent`);
4. `Protection`: `ProtectionCalculationResult` (corrente de projeto, limites de curto, suportabilidade térmica, status de evidência).

**Lacunas Contratuais para o Unifilar:**
1. `NodeCalculationResult` não expõe a **tensão resultante calculada no nó em Volts** ($V_{\text{nó}} = V_{\text{base}} \cdot [1 - CA\% / 100]$), apenas a porcentagem de queda $CA\%$.
2. `NodeCalculationResult` não expõe o **nível de curto-circuito na barra/nó** ($I_{\text{cc}3\phi}$ e $I_{\text{cc}1\phi}$), que atualmente está armazenado no trecho (`SegmentCalculationResult.ShortCircuit3PhaseAmperes`). Para o operador, a falta é inspecionada no barramento ou poste.
3. Não há mapeamento direto no relatório do papel topológico de cada nó (Raiz/Trafo, Derivação/LID, Passagem, Terminal/Carga).

---

## 4. Princípio Arquitetural do Unifilar: Projeção Visual Unificada

O Unifilar **NÃO** deve ser um modelo de dados elétricos paralelo. O fluxo arquitetural estrito deve ser:

```text
┌────────────────────────────────────────────────────────┐
│                      NetworkModel                      │
│ (Nós, Arestas, Cargas, Transformadores, Parâmetros)    │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│                    TopologyValidator                   │
│   (Árvore Radial, Ordem Topológica, Raiz, Terminais)   │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│               UnifiedCalculationPipeline               │
│ (Acumulação, Cargas M, Térmico, Queda, Curto, Proteção)│
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│                NetworkCalculationReport                │
│    (Resultados Físicos Imutáveis e Determinísticos)    │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│               UnifilarPresentationBuilder              │
│ (Gera Presentation Models a partir de Model + Report)  │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│                 WPF COCKPIT OPERACIONAL                │
│   ┌────────────────────────────────────────────────┐   │
│   │ HUD de Métricas Globais (Cards Superiores)     │   │
│   ├────────────────────────┬───────────────────────┤   │
│   │ Diagrama Unifilar      │ Painel de Detalhes    │   │
│   │ Interativo (Canvas/    │ Contextual Lateral    │   │
│   │ Vetorial com Zoom/Pan) │ (Elemento Selecionado)│   │
│   ├────────────────────────┴───────────────────────┤   │
│   │ Grid Tabular Sincronizada (Trechos/Nós/Trafos) │   │
│   ├────────────────────────────────────────────────┤   │
│   │ Rodapé de Rastreabilidade e Auditoria          │   │
│   └────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────┘
```

---

## 5. Especificação Arquitetural para a Fase 27B

### 5.1 Presentation Models Necessários
Para garantir o isolamento estrito e respeitar o limite de 500 linhas por arquivo, criaremos no namespace `QdtCqts.Desktop.Wpf.ViewModels.Unifilar`:

1. **`UnifilarDiagramViewModel.cs`**:
   - Agrega `ObservableCollection<UnifilarNodeViewModel>` e `ObservableCollection<UnifilarEdgeViewModel>`;
   - Controla viewport, nível de zoom (25% a 400%), pan (deslocamento X/Y) e modo de heatmap ativo;
   - Mantém `SelectedElement` para sincronização bidirecional.
2. **`UnifilarNodeViewModel.cs`**:
   - Posição no Canvas (`CanvasX`, `CanvasY`);
   - Metadados de identificação (`NodeId`, `ExternalKey`, `NodeType`: Trafo, Derivação, Poste, Terminal);
   - Grandezas de cálculo projetadas: $CA\%$ (Queda acumulada), $V_{\text{resultante}}$ (Volts), $I_{\text{cc}3\phi}$ (A), $I_{\text{cc}1\phi}$ (A), Carga local (kVA), Consumidores locais;
   - Estado visual: `IsSelected`, `IsHovered`, `SeverityColor` (baseada na tolerância de queda: verde se $\le 3\%$, âmbar se entre $3\%$ e $5\%$, vermelho se $> 5\%$).
3. **`UnifilarEdgeViewModel.cs`**:
   - Coordenadas de conexão (`X1, Y1` até `X2, Y2`);
   - Metadados do trecho (`EdgeId`, `FromNodeId`, `ToNodeId`, `ConductorKey`, `LengthMeters`);
   - Grandezas de cálculo projetadas: $I_b$ (A), $I_z$ (A), Carregamento % ($I_b / I_z$), Temperatura (°C), $\Delta V\%$ no trecho, $I_{\text{cc}3\phi}$ (A), $I_{\text{cc}1\phi}$ (A), `IsOverloaded`;
   - Estado visual: `IsSelected`, `IsHovered`, `StrokeColor`, `StrokeThickness`.
4. **`SelectedElementDetailViewModel.cs`**:
   - Modelo detalhado para o Painel de Detalhes Lateral;
   - Organizado em grupos de inspeção de engenharia:
     - *Identificação e Topologia:* IDs, endpoints, cota/comprimento, tipo;
     - *Cálculo Elétrico:* Carga montante $E$, carga $M$, corrente $I_b$, queda $\Delta V\%$, queda acumulada $CA\%$;
     - *Térmico e Condutor:* Seção mm², catálogo, temperatura calculada, resistência $R_{\text{ca}}$, reatância $X$, impedância $Z$, ampacidade admissível $I_z$;
     - *Curto-Circuito e Proteção:* Impedância Thévenin acumulada, $I_{\text{cc}3\phi}$, $I_{\text{cc}1\phi}$, tempo admissível Onderdonk $t_{\text{adm}}$, adequação do dispositivo;
     - *Rastreabilidade:* Proveniência de fórmulas, evidências vinculadas, hashes determinísticos.
5. **`UnifilarLayoutEngine.cs`**:
   - Motor puramente geométrico de posicionamento;
   - Se o `NetworkModel` contiver coordenadas válidas, normaliza e projeta na escala de tela;
   - Se as coordenadas forem nulas ou colineares, aplica algoritmo determinístico de árvore radial hierárquica (Sugiyama-light): raiz no topo/esquerda, derivações abrindo em níveis com espaçamento ortogonal uniforme.

### 5.2 Estrutura da Interface WPF (Cockpit Operacional)
A interface deve evoluir para um layout split-view balanceado e ergonômico:
- **Painel Superior:** Barra de comandos compacta (seleção de projeto, executar cálculo, botão de centralizar diagrama, seletor de heatmap: "Normal", "Queda de Tensão", "Carregamento Térmico", "Curto-Circuito");
- **Área Central (Splitter Horizontal):**
  - **Lado Esquerdo (65%):** Diagrama Unifilar Interativo em `ScrollViewer` com `Canvas` vetorial, suporte a Mouse Wheel (Zoom) e arraste (Pan), com nós e linhas clicáveis;
  - **Lado Direito (35%):** Painel de Detalhes do Elemento Selecionado, com abas limpas e organizadas;
- **Painel Inferior (Colapsável):** Abas tabulares (`DataGrid` de Trechos, `DataGrid` de Nós, `DataGrid` de Transformadores);
- **Rodapé:** Barra de observabilidade estruturada mantida e expandida.

---

## 6. Riscos, Conformidade e Diretrizes Não Negociáveis

1. **Zero Custo a Todo Custo:** Uso exclusivo de recursos nativos do WPF (.NET 8 Windows Desktop SDK: `Canvas`, `Path`, `Ellipse`, `Line`, `Border`, `MatrixTransform`). Nenhuma biblioteca gráfica de terceiros (como Syncfusion, Telerik, SciChart ou Infragistics) será necessária.
2. **2.5D e Half-way BIM:** O unifilar operará como projeção ortogonal 2D do modelo, preservando cotas altimétricas $Z$ e tipos construtivos de infraestrutura como metadados de engenharia no Painel de Detalhes.
3. **100% pt-BR:** Toda a terminologia de interface (rótulos, tooltips, unidades, diagnósticos) permanecerá estritamente em português do Brasil.
4. **Limites de Código:** Cada novo arquivo/classe será mantido abaixo de 500 linhas (ideal), respeitando a modularidade estrita.
5. **Branch e Governança:** O trabalho deve ser commitado na branch `dev`. A alteração pré-existente (`QdtCqts.Desktop.Wpf_dwkfellp_wpftmp.csproj`) foi preservada intocada.

---

## 7. Parecer Técnico do Tech Lead e Próximos Passos

- **Status da Auditoria (Fase 27A):** **APROVADA COM SUCESSO (GO)**.
- O diagnóstico comprovou que os motores elétricos e o relatório imutável contêm todos os dados necessários para o Cockpit.
- A especificação arquitetural estabelece a separação limpa entre o cálculo do backend e a projeção visual da UI, garantindo que o unifilar não recalculará grandezas elétricas.
- A implementação prática do Cockpit, Unifilar e Painel de Detalhes fica formalmente especificada e pronta para execução na **Fase 27B**.
