# CAC — Context Architecture & Contracts

> **Conhecimento Arquitetural Compartilhado e Contratos de Componentes**  
> **Sistema:** SISQDT_LIGHT (QdtCqts)  
> **Status:** Ativo / Governança Técnica Estrita  
> **Data:** 2026-09-27  

---

## 1. Princípios Arquiteturais e Papéis

O **SISQDT_LIGHT** é orquestrado sob os seguintes papéis técnicos:

- **Tech Lead:** Governança técnica, integridade dos modelos e validação dos portões de decisão (Gates).
- **Dev Fullstack Sênior:** Implementação de código limpo, contratos de cálculo e integração de dados.
- **DevOps/QA:** Infraestrutura Docker, automação de testes contínuos, cobertura e manifestos criptográficos.
- **UI/UX Designer:** Experiência nativa Windows (WPF/MVVM), ergonomia para engenheiros eletricistas e padrão 100% pt-BR.
- **Estagiário:** Criatividade fora da caixa, descoberta de padrões e novos experimentos controlados.

---

## 2. Bounded Contexts e Separação de Responsabilidades

```text
┌──────────────────────────────────────────────────────────┐
│                   QdtCqts.Desktop.Wpf                    │
│            Thin Frontend: UI / Views / ViewModels        │
└─────────────┬──────────────────────────────┬─────────────┘
              │ consome                      │ registra ciclo de vida
┌─────────────▼──────────────────────────────┼─────────────┐
│                   QdtCqts.Application      │             │
│      ProjectService, CalculationService,   │             │
│      CalculationAuditService, TraceRecorder│             │
└──────────────┬─────────────────────────────┼─────────────┘
               │ orquestra                   │ invoca
┌──────────────▼─────────────┐ ┌─────────────▼─────────────┐
│      QdtCqts.Domain        │ │ Calculation.Abstractions  │
│ NetworkModel, Circuit,     │ │ ICalculationEngine, Trace │
│ EventIds, Redaction, Corr. │ │ DeterministicHashing      │
└──────────────▲─────────────┘ └─────────────┬─────────────┘
               │ persiste                    │ implementa
┌──────────────┴─────────────┐ ┌─────────────▼─────────────┐
│ Infrastructure.Sqlite      │ │ Calculation.Qdt           │
│ Repositórios, DDL, WAL     │ │ Calculation.Cqts          │
└────────────────────────────┘ └───────────────────────────┘
               ▲                             ▲
               │ importa/valida              │
┌──────────────┴─────────────┐ ┌─────────────┴─────────────┐
│ Infrastructure.ExcelEvid.  │ │ Infrastructure.Parity     │
│ OpenXML Reader, Precedence │ │ Comparador Numérico P0-P4 │
└────────────────────────────┘ └───────────────────────────┘
               ▲
               │ sinks, formatação e rotação (DI puro)
┌──────────────┴───────────────────────────────────────────┐
│           QdtCqts.Infrastructure.Observability           │
│ Serilog Provider, Rolling File 10MB/30d, StartupLogger   │
└──────────────────────────────────────────────────────────┘
```

### Regras de Dependência:
1. `Domain` e `Calculation.Abstractions` são **núcleos puros**: não possuem referências a UI, Excel Interop, SQLite ou bibliotecas externas.
2. Motores de cálculo (`Calculation.Qdt` e `Calculation.Cqts`) conhecem apenas `Domain` e `Calculation.Abstractions`.
3. A interface `Desktop.Wpf` nunca acessa diretamente bancos de dados ou rotinas de cálculo internas; comunica-se estritamente através dos serviços de `Application`.

---

## 3. Contratos de Dados e Engenharia

### 3.1 Contrato de Execução de Cálculo (`ICalculationEngine`)
```csharp
public interface ICalculationEngine
{
    CalculationResult Calculate(CalculationRequest request);
}
```
- **Pré-condições:** O modelo topológico deve estar aprovado pelo `TopologyValidator` (sem ciclos, sem órfãos, fonte única por circuito).
- **Tratamento de Exceções:** Ausência de dados de entrada ou grandezas sem unidade explícita **não geram fallbacks silenciosos ou valores zero**; produzem status `CalculationStatus.BLOCKED` com diagnósticos explicativos detalhados.

### 3.2 Topologia 2.5D e Half-way BIM
- **Layout lógico:** `Node.LayoutX/LayoutY` e Presentation Models WPF representam posições esquemáticas na tela; não são coordenadas CAD.
- **Geometria física (Fase 30B/30C):** `Node.PhysicalPosition` guarda Easting/Northing, unidade e referência espacial com CRS parcial. A Fase 30C não confirmou unidade/fuso no DWG ZNA855820 (`INSUNITS=4`, `MAPCSASSIGN=nil`, sem `ACAD_GEOGRAPHICDATA`); datum/EPSG e unidade das coordenadas permanecem desconhecidos até haver evidência explícita.
- **Half-way BIM:** A cota altimétrica ($Z$), tipo de estrutura (poste, poço de visita, galeria), material e ampacidade são armazenados como atributos de metadados de engenharia associados aos nós e trechos, dispensando a complexidade de renderizadores 3D pesados sem perda da precisão física.

### 3.3 Contrato de Curto-Circuito (`CandidateShortCircuitRule`)
- **Curto-Circuito Trifásico ($I_{\text{cc}3\phi}$):**
  $$I_{\text{cc}3\phi} = \frac{V_{\text{fn}}}{|\mathbf{Z}_{\text{up}} + \mathbf{Z}_{\text{bt, prior}} + \mathbf{Z}_{\text{seg}}|}$$
- **Curto-Circuito Monofásico ($I_{\text{cc}1\phi}$):**
  $$I_{\text{cc}1\phi} = \frac{V_{\text{fn}}}{\sqrt{(R_{\text{up}} + R_{\text{loop FN, prior}} + R_{\text{loop FN, trecho}})^2 + X_{\text{up}}^2}}$$
- **Isolamento de Domínio:** A regra de curto-circuito é pura e opera via `IIsolatedRule` sem acoplamento a Excel, I/O ou banco. As impedâncias a montante são decompostas fisicamente em fonte, MT e transformador referidas à tensão nominal secundária.

---

## 4. Persistência e Estratégia de Dados

- **SQLite Local (Offline First):**
  - Modo WAL (Write-Ahead Logging) ativo para concorrência e resiliência;
  - Integridade referencial ativada (`PRAGMA foreign_keys = ON;`);
  - Isolamento de transações em todas as escritas de versão e execuções de cálculo.
- **Supabase Ready (Cloud First):**
  - Mapeamento direto das entidades para esquemas PostgreSQL compatíveis;
  - Preparado para sincronização de projetos e autenticação federada com controle de acesso baseado em papéis (RBAC).

---

## 5. Rastreabilidade e Paridade Matemática

Toda execução de cálculo registra um `CalculationRun` imutável com os seguintes metadados:
- `project_version_id`: Identificador da versão do projeto;
- `calculation_mode`: `QDT` ou `CQTS`;
- `algorithm_version`: Versão do algoritmo implementado;
- `input_hash`: Hash SHA-256 do conjunto de entradas;
- `trace_steps`: Vetor de passos determinísticos executados com insumos, fórmulas e resultados intermediários.

### Regras Candidatas Reconstruídas no Motor:
1. `CQTS.REAL_PROJECT.CONSUMER_LOAD_AGGREGATION`: Agregação linear de cargas de consumidores no nó ($S_{\text{local}} = \sum N_g \cdot S_{\text{unit}, g}$).
2. `CQTS.REAL_PROJECT.RADIAL_LOAD_ACCUMULATION`: Acumulação a montante em árvore radial ($E_{\text{trecho}} = S_{\text{local}} + \sum E_{\text{filhos}}$ e $D_{\text{trecho}} = N_{\text{local}} + \sum D_{\text{filhos}}$).
3. `CQTS.REAL_PROJECT.END_LOAD_SELECTION`: Seleção de carga no fim do trecho $M$ com diversidade e pisos regulatórios ($D \le 2$).
4. `CQTS.REAL_PROJECT.CABLE_TEMPERATURE`: Cálculo de temperatura térmica de regime contínuo do condutor.
5. `CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP`: Cálculo de queda de tensão percentual no trecho com impedância térmica corrigida ($\Delta V\% = \frac{M \cdot Z \cdot L_{\text{equiv}}}{V^2 / 100} \cdot k_{\text{fase}}$).
6. `CQTS.REAL_PROJECT.ACCUMULATED_VOLTAGE_DROP`: Acumulação ao longo de caminhos independentes da árvore radial ($CA(v) = CA(\text{pai}) + \Delta V\%_{\text{trecho}}$).
7. `CQTS.REAL_PROJECT.THERMAL_RESISTANCE`: Resistência CA corrigida na temperatura de regime com efeito pelicular ($R_{\text{ca}}(T) = R_{\text{cc},20} \cdot [1 + \alpha_{20} \cdot (T - 20)] \cdot K^*$).
8. `CQTS.REAL_PROJECT.TRANSFORMER_VOLTAGE_DROP`: Queda interna percentual do transformador de distribuição ($\Delta V\%_{\text{trafo}} = \frac{M_{\text{trafo}}}{S_{\text{nom, trafo}}} \times Z\%_{\text{trafo}}$).

---

## 6. Governança de Código

- Arquivos fonte devem se manter preferencialmente abaixo de 500 linhas.
- Testes unitários obrigatórios para cada nova regra matemática adicionada ao motor.
- Nenhuma alteração de baseline elétrico é permitida sem reconciliação completa documentada em `docs/phases/`.

---

## 7. Observabilidade, Logging e Rastreabilidade (Fase 22.2)

### 7.1 Separação Rígida das Três Camadas de Rastreabilidade
1. **Application Log:** Registra o comportamento operacional da aplicação via `ILogger<T>` (`Microsoft.Extensions.Logging`). Integrado à biblioteca isolada `QdtCqts.Infrastructure.Observability` (Serilog com sinks de Console e Rolling Files diários em `logs/qdtcqts-YYYYMMDD.log`, 10MB máximo por arquivo, retenção de 30 dias).
2. **Calculation Trace:** Registra o passo a passo matemático estruturado (`CalculationTraceDetail`, `CalculationTraceRecorder`), suportando modos `Normal` e `Diagnostic`. Todas as grandezas possuem tipagem estrita com unidades (`UnitCode`), além de `InputHash` e `OutputHash` SHA-256 canônicos.
3. **Audit / Evidence Trail:** Registra a proveniência dos fatos geradores e histórico de auditoria (`CalculationAuditService`, `EvidenceTrailRecord`, `ParityTraceRecord`, `GoldenTraceRecord`). Permite responder com precisão: *quem, quando, qual versão, qual regra, quais inputs, qual fórmula, qual resultado, qual evidência e qual disparidade com o Excel/Golden*.

### 7.2 Isolamento Arquitetural de Providers
- Os projetos `Domain`, `Calculation.Abstractions`, `Calculation.Qdt`, `Calculation.Cqts` e `Application` **NÃO possuem qualquer acoplamento ou dependência do Serilog**.
- O Serilog é utilizado exclusivamente como provider de infraestrutura via injeção de dependência (`LoggingBootstrapper.AddQdtCqtsObservability`), respeitando o princípio de inversão de dependência (DIP).

### 7.3 Segurança First e Redação de Segredos
- Sanitização obrigatória de dados sensíveis (`SensitiveDataRedactor`) para impedir vazamento de senhas em strings de conexão, tokens `Bearer` e identificadores fiscais em logs.

---

## 8. Integração Ponta a Ponta e Identidade Visual (Fase 23)

### 8.1 Cadeia de Cálculo Unificada (`UnifiedCalculationPipeline`)
O pipeline de cálculo orquestra de ponta a ponta:
1. Agregação de cargas de consumidores nos nós ($S_{\text{local}}, D_{\text{local}}$);
2. Ordenação topológica e validação estrutural (`TopologyValidator`);
3. Acumulação radial a montante (pós-ordem: folhas $\rightarrow$ raiz) para determinar $E$ e $D$;
4. Seleção da carga no fim do trecho $M$ com fator de diversidade $G$ e piso regulatório $CH5$;
5. Dimensionamento da corrente $I_b$, capacidade do condutor $I_z \cdot AP$ e detecção de sobrecarga (`IsOverloaded`);
6. Cálculo térmico de regime permanente do cabo ($T$) e resistência CA corrigida ($R_{\text{ca}}$);
7. Impedância linear do trecho $Z$ e fator $BJ = Z / (V^2 / 100)$;
8. Queda de tensão percentual no trecho ($\Delta V\% = M \cdot BJ \cdot L_{\text{equiv}} \cdot k_{\text{fase}}$);
9. Queda interna do transformador ($\Delta V\%_{\text{trafo}}$) e média tensão ($\Delta V\%_{\text{MT}}$);
10. Acumulação ao longo dos caminhos da árvore (pré-ordem: raiz $\rightarrow$ folhas) para determinar $CA\%$ de cada nó com isolamento radial de caminhos.

### 8.2 Identidade Visual Oficial (`sisQDT_LIGHT`)
- **Marca Oficial:** `sisQDT_LIGHT` (grafia com sis minúsculo regular, QDT maiúsculo extra-bold azul e _LIGHT maiúsculo semi-bold âmbar).
- **Repositório Central de Assets:** `assets/brand/` contendo arquivos SVG vetoriais, PNGs de alta resolução para temas claro, escuro e monocromático, e `sisQDT_LIGHT.ico` multi-resolução com 7 camadas (16×16 a 256×256 em 32-bit RGBA).
- **Integração WPF:** Configuração nativa de `ApplicationIcon`, Resources e Window Icon no projeto `QdtCqts.Desktop.Wpf`.
- **Manual Oficial de Diretrizes:** Documentado em `docs/brand/README.md`.

---

## 9. Integridade da Avaliação de Proteção (Fase 26)

- `ProtectionDeviceEvidence` só pode ser consumida com referência de origem, SHA-256, fabricante, modelo, curva, corrente nominal, corrente do ponto de avaliação, tempo total de interrupção e capacidade de interrupção explícitos. O contrato atual não implementa curva completa, I²t, coordenação ou seletividade.
- `ProtectionEvidenceStatus` (`Available`, `Missing`, `Invalid`) representa a evidência; `ProtectionAssessmentStatus` (`Pass`, `Fail`, `EvidenceBlocked`) representa a conclusão. Ausência/invalidez não equivale a falha elétrica.
- A ausência de evidência não impede `Ib`, Icc3φ ou Icc1φ. Onderdonk requer seção catalogada (`CQTS.REAL_PROJECT.LADO1_CT38_CU40`) e temperatura de regime cuja regra passou (`F17-CQTS-PROJ7-LADO1-P13`); caso contrário, `t_adm` permanece nulo.
- É proibido inferir corrente nominal, tempo de fusão/atuação, capacidade de interrupção ou seção de condutor. Indicadores de adequação são anuláveis até uma evidência válida ser fornecida.
- O pipeline não sintetiza condutor nem transformador quando faltam no modelo: ampacidade, R/X, potência e impedância de catálogo devem estar presentes; caso contrário, o cálculo é bloqueado com diagnóstico explícito.
- O pipeline mantém o resultado elétrico geral `Pass` e a WPF sinaliza separadamente `EvidenceBlocked`; isso não deve ser convertido em `CalculationError` nem ocultar os resultados elétricos calculáveis.
- O identificador e o SHA-256 de uma evidência externa são metadados de proveniência. Como o Evidence Store histórico está ausente, o sistema não afirma validar o hash contra o conteúdo do arquivo.
- Estado de fases: Fases 24, 25, 26, 27A, 27B, 27C, 27D, 30A, 30B e 30C concluídas nos escopos documentados; Fase 30C encerra com identidade física/lógica ainda não determinada.

---

## 10. Arquitetura do Cockpit Operacional e Unifilar Elétrico (Fases 27A, 27B, 27C e 27D)

### 10.1 Princípio da Projeção Visual Unificada
- O Unifilar Elétrico é estritamente uma **projeção visual do modelo de rede (`NetworkModel`) e do relatório imutável de cálculo (`NetworkCalculationReport`)**.
- É **terminantemente proibido** criar uma segunda representação elétrica paralela ou recalcular qualquer grandeza na camada de apresentação (WPF).
- A WPF consome `Presentation Models` construídos de forma determinística por um builder especializado (`UnifilarPresentationBuilder`).

### 10.2 Contratos de Apresentação Implementados (Fase 27B)
1. `UnifilarDiagramViewModel`: Orquestrador visual com coleções observáveis de nós, arestas e transformadores, controle de seleção bidirecional sincronizada e metadados de rastreabilidade (`RunId`, `InputHash`, `OutputHash`, `AlgorithmVersion`, `UiState`).
2. `UnifilarNodeViewModel`: Projeção gráfica do nó com posicionamento esquemático ortogonal, identificação (`NodeId`, `ExternalKey`), tipo topológico derivado (`UnifilarNodeType`), queda acumulada $CA\%$, carga local kVA e consumidores locais.
3. `UnifilarEdgeViewModel`: Projeção gráfica da linha de distribuição com espessura proporcional ao condutor e grandezas elétricas prontas ($I_b$, $I_z$, $M$, $E$, $D$, $\Delta V\%$, $I_{\text{cc}3\phi}$, $I_{\text{cc}1\phi}$), consumindo o status de sobrecarga diretamente de `SegmentCalculationResult.IsOverloaded`.
4. `UnifilarTransformerViewModel`: Projeção gráfica do transformador de alimentação e queda interna/MT.
5. `SelectedElementDetailViewModel`: Projeção contextual no painel lateral de inspeção para o elemento atualmente selecionado (nó, trecho ou transformador), exibindo metadados Half-way BIM e rastreabilidade agrupados por categoria.
6. `HierarchicalTreeLayoutEngine`: Motor geométrico determinístico (`IUnifilarLayoutEngine`) que calcula coordenadas visuais $(X, Y)$ através de árvore radial hierárquica, 100% desacoplado de grandezas elétricas.

### 10.3 Implementação Visual do Cockpit Operacional (Fase 27C)
1. **Viewport Central com Zoom e Pan:** Implementado via WPF nativo (`Canvas`, `ScaleTransform`, `TranslateTransform`) suportando níveis de zoom de 25% a 400%, pan contínuo por arraste do mouse e ajuste à tela (*Fit to View* determinístico).
2. **Painel de Inspeção Lateral:** Integrado em split-view redimensionável com `GridSplitter`, exibindo títulos, subtítulos e categorias dinâmicas de dados técnicos (`DetailCategoryGroup` e `DetailItem`).
3. **Sincronização de Seleção Bidirecional:** Seleção cruzada entre o Unifilar e o DataGrid de Segmentos sem duplicação de dados de engenharia.
4. **Governança de Identidade Git:** Configuração local vinculada a `Jonatas` para novos commits sem reescrever histórico legado.

### 10.4 Trace Elétrico, Filtros Visuais e Resolução O(1) (Fase 27D)
1. **Zero Fallbacks Sintéticos:** Rejeição segura de seleções com IDs inválidos sem fabricar entidades de domínio na apresentação.
2. **Trace Elétrico Topológico da Raiz à Ponta:** Algoritmo determinístico $O(\text{profundidade})$ que percorre a árvore radial upstream a partir da seleção (`ToNodeId -> Edge -> FromNodeId -> ... -> Root`), destacando o caminho com isolamento estrito de ramos em bifurcações.
3. **Filtros Visuais de Condição Elétrica:** Enum `UnifilarVisualFilter` (`Todos`, `Sobrecarga`, `QuedaTensaoLimite`, `EvidenceBlocked`, `Selecionados`, `Trace`).
4. **Atenuação Visual por Hardware:** Propriedade `IsDimmed` associada a triggers XAML de opacidade (18%), preservando posições de tela e coleções sem causar redesenhos estruturais.
5. **Métricas Visuais e FitToView Centralizados:** `UnifilarVisualMetrics` e proteção total contra viewports nulos/degenerados e valores `NaN`/`Infinity`.
6. **Lookup Indexado:** Dicionários indexados por ID no orquestrador visual para acesso $O(1)$ sem degradação em grafos extensos.

---

## 11. Arquitetura Preliminar de Exportação CAD/LISP (Fase 28)

### 11.1 Princípio de Separação de Espaços
- **Espaço Lógico (WPF):** Árvore esquemática hierárquica otimizada para cognição humana, inspeção elétrica e rastreabilidade na tela do operador.
- **Espaço Físico (AutoCAD):** A futura representação CAD só pode consumir coordenadas quando unidade e CRS da fonte estiverem declarados e validados. A investigação ZNA855820 da Fase 30C não confirmou esses metadados. O layout de tela nunca é enviado como geometria física ao CAD.

### 11.2 Fluxo Semântico de Exportação
```
QDT/CQTS (Grafo Calculado) 
   ──► CAD Export Model (DTOs Neutros com UTM e Grandezas Elétricas)
   ──► LISP Generator (Template com Payload Estruturado - Alternativa B)
   ──► Área de Transferência ("Copiar para o CAD")
   ──► AutoCAD (Entidades nativas LINE, PLINE, BLOCK, MLEADER em Layers padronizados)
```

### 11.3 Sistema de Coordenadas e Entidades CAD
- **CRS (proposta preliminar da Fase 28):** SIRGAS 2000 / UTM Fuso 23S (`EPSG:31983`) foi uma hipótese arquitetural, não um CRS obrigatório do domínio. A Fase 30C não confirmou unidade, fuso, datum ou EPSG no DWG ZNA855820; consultar a seção 12 antes de consumir geometria.
- **Entidades Semânticas:**
  - Nós / Postes: `BLOCK` com atributos técnicos ou `POINT` com bloco inserido;
  - Trechos de Rede: `LINE` ou `PLINE` em layers de condutor com dados de faseamento e bitola;
  - Transformadores: `BLOCK` dedicado com identificador e potência nominal;
  - Callouts de Engenharia: `MLEADER` perpendicular ao trecho com indicação de corrente, $\Delta V\%$, tensões nominais nodais ($V_{127}, V_{220}$) e carregamento térmico.
- **Estratégia LISP:** Alternativa B (Híbrida com Payload de Dados Estruturado), garantindo portabilidade via Ctrl+V no console sem risco de buffer overflow nem dependência de arquivos temporários em disco.

---

## 12. Domínio Espacial e Importação de Geometria Física (Fase 30B)

- A fonte física é `DWG → ferramenta existente do acervo → JSON → sisQDT_LIGHT`. O backend não lê DWG e não executa conversão, reprojeção ou transformação de CRS.
- `Node.LayoutX/LayoutY` representam layout esquemático; `Node.PhysicalPosition` representa posição física com Easting/Northing, unidade, referência espacial e proveniência. `Edge.PhysicalGeometry` é opcional e recebe geometria linear somente com associação explícita.
- A referência espacial registra sistema, zona, hemisfério, datum e EPSG em campos independentes e anuláveis. O perfil DWG-JSON é UTM; o fuso vem do JSON. Datum, hemisfério e EPSG não são presumidos e `EPSG:31983` não é default.
- O adaptador da Fase 30B aceita o perfil em metros, mas a Fase 30C não confirmou a unidade das coordenadas do DWG ZNA855820. Não importar esse DWG como metros até a unidade ser confirmada; `INSUNITS=4` é configuração de unidade de inserção e não resolve sozinho a unidade numérica do modelo.
- O adaptador `QdtCqts.Infrastructure.Geometry` recebe JSON, preserva postes e linhas e retorna contagens, advertências e inválidos. A chave externa só é vinculada a `Node.Id`/`Edge.Id` por mapas explícitos; sem mapa, o elemento permanece importado e não associado.
- `Node.LayoutX/LayoutY` não recebem coordenadas físicas importadas; layout e Easting/Northing permanecem independentes.
- Linhas sem ID de origem permanecem sem ID sintético e sem vínculo a trecho lógico. Não se presume correspondência pela ordem, `ExternalKey`, proximidade ou sequência de extração.
- Evidência de origem: [Fase 30A](docs/phases/fase30a/FASE30A_FONTE_GEOMETRIA_FISICA.md). Decisões e limitações da Fase 30B: `docs/phases/fase30b/FASE30B_DOMINIO_ESPACIAL.md`.

### 12.1 Reconciliação Física/Lógica (Fase 30C)

- No DWG ZNA855820, os atributos `XX` em blocos `NUM_PLAN` carregam rótulos de planta `TR`/`P1...P11`, repetidos duas vezes; não foi encontrada chave única por poste nem atributo `LID` de nó.
- O CQTS usa `ID LIGHT`, `PONTO`, `PONTO MONTANTE` e `TRECHO (m)`, mas `ExcelEvidenceImporter` somente captura evidência de workbook e não constrói `NetworkModel`. O `MainViewModel` contém modelo hardcoded de exemplo, não import do ZNA.
- `cad2kmz.lsp` pode gerar IDs sequenciais pela ordem de seleção e as linhas JSON não têm ID/handle. Não usar ordem, bloco, proximidade ou rótulo repetido como associação automática.
- Gate da Fase 30C: `GO PARA PRÓXIMA DECISÃO — identidade ainda não determinada`. Unidade/CRS do DWG também não foram confirmados. Nenhuma arquitetura de Fase 31 foi aprovada; relatório: `docs/phases/fase30c/FASE30C_RECONCILIACAO_IDENTIDADE_FISICA_LOGICA.md`.




