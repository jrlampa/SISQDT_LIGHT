# Changelog

## Fase 30F — Reconciliação DWG ↔ Crosswalk Físico-Lógico

- Varrido em cópia somente leitura o DWG principal ZNA19165 (13.219 entidades), o BAK de mesmo basename (13.220; conteúdo quase idêntico) e o BAK “JUC” (13.155; contexto/posições diferentes). Original e BAKs não foram alterados.
- A crosswalk `CQTS → CSV ↔ KMZ ↔ .srua` foi reproduzida para `P2`, `P24 - TRAFO` e `P25`; os candidatos DWG são conjuntos ambíguos: 18 `XX=P2`, três `XX=24` e três `XX=25`. Não há `P24/P25` literal nem handle compartilhado.
- DWG principal tem `INSUNITS=0`, `MAPCSASSIGN` vazio, sem `ACAD_GEOGRAPHICDATA`, XDATA=0; CRS e unidade seguem sem contrato. Handles são únicos no arquivo, mas só identificam entidades naquela revisão.
- Foram encontrados 59 APPIDs, 368 extension dictionaries, 211 reactors e custom objects semânticos não decodificados; nenhum dado de identidade lógica foi demonstrado nos candidatos.
- Planilhas auxiliares PONTO_24/25 declaram internamente outro projeto (“CACUIA - RUA PEDREIRA - NOVA IGUAÇU”); não foram usadas para associar postes do ZNA19165.
- Gate: `NO-GO — IDENTIDADE FÍSICO-LÓGICA AINDA NÃO DETERMINADA`. Sem alteração de produto, fixture, CAD/LISP ou `VERSION_MANIFEST.json`; Fase 31 não aprovada.
- Relatório: `docs/phases/fase30f/FASE30F_RECONCILIACAO_DWG_CROSSWALK.md`.

## Fase 30E — Rastreio da Origem do Crosswalk Físico-Lógico

- No ZNA19165, comparados 36 pontos do `.srua`, 36 placemarks do KMZ e 36 linhas do CSV: nome/lat-lon coincidem integralmente; conversão UTM documentada no importer local reproduz o CSV com arredondamento a 0,001 m.
- Três rastros confirmam correspondência local até linhas CQTS por PONTO/contexto e coordenadas. Isso é coerência de dados, não prova a direção de geração entre `.srua`, KMZ e CSV.
- O histórico de exportação dentro do `.srua` registra cinco postes e zero arestas, não os 36 do estado atual; o link `localhost` associado não foi localizado. Serializador/origem do snapshot continuam sem prova.
- CSV/KMZ/.srua/CQTS não carregam handle ou revisão DWG. Rótulos DWG se repetem e a varredura geométrica anterior não encontrou join dos pontos CSV com INSERT/POINT/linhas.
- Gate: `CROSSWALK DE DADOS CONFIRMADO ATÉ CQTS; ORIGEM E VÍNCULO AO HANDLE DWG NÃO DETERMINADOS`. Sem alteração de código, CAD/LISP, fixtures ou `VERSION_MANIFEST.json`; Fase 31 não aprovada.
- Relatório: `docs/phases/fase30e/FASE30E_RASTREAMENTO_ORIGEM_CROSSWALK.md`.

## Fase 30D — Protocolo de Identidade Física-Lógica

- Inventariadas 47 pastas de projetos locais: 37 com CAD+workbook e 25 que também contêm CSV/JSON/KMZ/KML; amostras mantidas identificadas por obra.
- No ZNA19165 foi demonstrado join único de 39 ocorrências CQTS→CSV/KMZ usando a precisão inteira gravada no CQTS, com correspondência numérica `PONTO`/sufixo P#. Isso não liga placemark/CSV a um handle do DWG.
- Identificadores físicos P#/TR no DWG repetem; CSV `P15` tem duas posições; não houve coincidência de pontos CSV com blocos, POINTs ou endpoints CAD capturados. `cqt_cad_extractor.lsp` pede montante/letra e `cad2kmz.lsp` pode gerar nomes por ordem quando não reconhece atributo.
- Fonte canônica e procedimento humano escrito não encontrados. Estado: `IDENTIDADE NÃO DETERMINADA — REQUISITO EXTERNO NECESSÁRIO`; Fase31 não aprovada.
- DWG ZNA19165 declara `INSUNITS=0`, `MAPCSASSIGN=nil`, sem `ACAD_GEOGRAPHICDATA`; CRS/unidade das coordenadas do modelo não confirmados. Sem código/fixture/testes de produto e sem alteração do `VERSION_MANIFEST.json`.
- Relatório: `docs/phases/fase30d/FASE30D_PROTOCOLO_IDENTIDADE_FISICA_LOGICA.md`.

## Fase 30C — Reconciliação de Identidade Física × Lógica

- Investigado o projeto local real ZNA855820: DWG, workbooks QDT/CQTS `ATUAL/PROJ`, 11 planilhas auxiliares e PDF; macros não foram executadas e o DWG original não foi alterado.
- Confirmado que `NUM_PLAN.XX` contém rótulos `TR`/`P1...P11` repetidos, enquanto o CQTS registra pares em `ID LIGHT`; não existe chave única ou importador que ligue esses dados a `Node.ExternalKey`/`Edge`.
- Os LISP existentes atribuem IDs por ordem de seleção ou registram letras/comprimentos manuais; não preservam handle/coordenadas ↔ trecho lógico.
- Requalificadas para o ZNA as afirmações anteriores de unidade/fuso: os JSONs temporários analisados pertencem a outros desenhos; `MAPCSASSIGN=nil`, sem `ACAD_GEOGRAPHICDATA`, `INSUNITS=4`; CRS/unidade seguem pendentes.
- Sem mudança de código, testes ou `VERSION_MANIFEST.json`; sem LISP/exportador CAD e sem associação artificial.
- **Gate:** `GO PARA PRÓXIMA DECISÃO — identidade ainda não determinada`; Fase 31 permanece não aprovada. Relatório: `docs/phases/fase30c/FASE30C_RECONCILIACAO_IDENTIDADE_FISICA_LOGICA.md`.

## Fase 30B — Domínio Espacial e Importação de Geometria Física

- Criados `PhysicalPosition`, `PhysicalLineGeometry` e `SpatialReference`; `Node.LayoutX/LayoutY` não são usados como coordenadas físicas e `Edge.PhysicalGeometry` é opcional.
- Criado `QdtCqts.Infrastructure.Geometry` para adaptar JSON de postes/linhas do acervo ao modelo interno, sem parser DWG.
- O adaptador exige unidade declarada pelo chamador, valida metro, IDs, duplicidades, coordenadas UTM e fuso; preserva origem (`DWG_JSON`, ID e bloco) e retorna contagens/avisos/erros.
- Associação para `Node.Id`/`Edge.Id` exige mapas explícitos. Sem mapeamento, elementos são preservados não associados; linhas sem ID não recebem ID sintético.
- CRS é representado com campos independentes/nuláveis. UTM é o perfil do adaptador, zona vem do JSON; hemisfério, datum e EPSG não recebem defaults. Fuso ausente é preservado como CRS parcial.
- Testes cobrem payload válido, X/Y, unidade, fuso 23, CRS parcial, associação explícita, identidade ausente, dados inválidos, duplicidades, ausência de linhas e independência do layout WPF.
- Limites: sem leitura DWG, transformação CRS, mapa interativo ou associação automática. Fixture de teste mínima anonimizada baseada no contrato evidenciado pela Fase 30A.
- **Testes:** 14 testes do importador e 1 teste WPF de isolamento do layout; `dotnet test --nologo`: **212 aprovados, 0 falhas, 0 ignorados**.

## Fase 28 — Engenharia Reversa QDT/CQTS Real + Arquitetura Preliminar CAD/LISP

- **Engenharia Reversa com Caso Real (ZNA855820):**
  - Desmontagem profunda da cadeia `ENTRADA → PROCESSAMENTO → RESULTADO` das planilhas legadas de QDT (`.xlsm`) e CQTS (`.xlsx`) da obra real de Rede Invertida ZNA855820.
  - Extração e validação das fórmulas exatas de corrente ($I_b$), temperatura do condutor ($T$), resistência corrigida ($R_{ca}$), queda de tensão no trecho ($\Delta V$), queda acumulada percentual ($CA\%$) e tensões nominais nodais ($V_{127}$ e $V_{220}$).
  - Análise de macros VBA (`Sequencial_Descricao`, `Workbook_Open`, `DesprotegeVB`): confirmado que o VBA exerce apenas automação de interface e ordenação de tabelas; 100% da engenharia elétrica reside nas fórmulas declarativas.
- **Matriz de Paridade e Diagnóstico:**
  - Status `CONFIRMADO` para corrente de carga, temperatura de condutor, resistência térmica e queda de tensão no trecho em relação aos motores de cálculo C# do `sisQDT_LIGHT`.
  - Identificada lacuna de exposição explícita das grandezas de tensão nominal em Volts ($V_{127}$ e $V_{220}$) nos DTOs de nós (atualmente concentradas em $\Delta V\%$).
  - Identificada necessidade de modelagem nativa de múltiplos cenários (`ATUAL` vs `PROJETADO`) dentro do mesmo caso de cálculo.
- **Arquitetura Preliminar de Exportação CAD/LISP:**
  - Definido o fluxo semântico: `QDT/CQTS → sisQDT_LIGHT → Unifilar → CAD Export Model → LISP Generator → AutoCAD`.
  - Estabelecida a separação rigorosa entre o espaço lógico (layout em árvore do unifilar WPF) e o espaço físico (georreferenciado em coordenadas planas UTM com azimutes reais).
  - Especificado o contrato preliminar do `CAD Export Model` com suporte a entidades nativas do AutoCAD (`LINE`, `PLINE`, `POINT/BLOCK`, `MTEXT`, `MLEADER`).
  - A arquitetura preliminar propôs SIRGAS 2000 / UTM Zone 23S (`EPSG:31983`); a investigação da Fase 30A não confirmou datum nem EPSG, portanto esse código não é um default nem contrato espacial.
  - Avaliação técnica das 3 alternativas LISP: recomendada a Alternativa B (Híbrida com Payload de Dados Estruturado).
- **Documentação de Engenharia:**
  - Elaborado documento exaustivo `docs/phases/fase28/FASE28_ENGENHARIA_REVERSA_REAL_E_ARQUITETURA_CAD_LISP.md`.

## Fase 27D — Navegação Avançada, Trace Elétrico, Filtros Visuais e Otimização Controlada do Unifilar

- **Zero Fallbacks Sintéticos de Domínio (27D-AUDIT-01):**
  - Eliminada totalmente a fabricação de entidades artificiais (`?? new Node(...)`, `?? new Edge(...)`, `?? new Transformer(...)`) em `UnifilarDiagramViewModel.SelectNode/SelectEdge/SelectTransformer`.
  - Passagem de ID inexistente cancela a seleção determinística (`SelectedElement = null`, `SelectedDetail = null`) sem inventar dados, garantindo fidelidade de modelo.
- **Trace Elétrico Topológico da Raiz à Ponta (Fase 27D-A):**
  - Implementado algoritmo de rastreamento ascendente `ComputeAndApplyTrace` conectando a ponta/trecho selecionado até o transformador e nó fonte na topologia radial real.
  - Complexidade $O(\text{profundidade})$, instantâneo e estritamente determinístico.
  - Isolamento estrito de bifurcações: selecionar um nó/ramo não destaca ramos irmãos.
  - Zero recálculo de grandezas elétricas (operação puramente de apresentação/topologia).
- **Filtros Visuais de Condição Elétrica (Fase 27D-B):**
  - Criado enum `UnifilarVisualFilter` com modos 100% pt-BR: `Todos`, `Sobrecarga`, `QuedaTensaoLimite`, `EvidenceBlocked`, `Selecionados`, `Trace`.
  - Mecanismo de atenuação visual (dimming) via propriedade `IsDimmed` (opacidade 18% em elementos filtrados) acionado via triggers de hardware no WPF, sem destruir coleções observáveis ou recalcular posições de layout.
  - Filtro de sobrecarga consome estritamente o status `IsOverloaded` calculado pelo motor de backend.
  - Filtro de queda de tensão opera com threshold explícito configurável de apresentação (`VoltageDropThresholdPercent`, default 5.0%).
  - Filtro de `EvidenceBlocked` preserva semântica canônica da Fase 26.
- **Robustez do FitToView e Métricas Visuais (27D-AUDIT-02):**
  - Centralizadas métricas de apresentação em `UnifilarVisualMetrics` (`NodeWidth`, `NodeHeight`, `TransformerWidth`, `TransformerHeight`, `DefaultMargin`, `ViewportPaddingRatio`, `MinZoom = 0.25`, `MaxZoom = 4.0`, `FitToViewMaxZoom = 2.5`).
  - Proteção completa contra viewport degenerado, ausência de elementos e valores `NaN` ou `Infinity`.
- **Indexação e Performance (27D-AUDIT-03):**
  - Índices $O(1)$ (`_nodeVmById`, `_edgeVmById`, `_trafoVmById`, `_incomingEdgeByToNodeId`) no orquestrador de apresentação do unifilar, eliminando varreduras lineares repetidas em grafos de grande escala.
- **Expansão da Suíte de Testes:**
  - Criado `UnifilarTraceAndNavigationTests.cs` com 25 novos testes unitários e de integração cobrindo trace, isolamento de bifurcações, filtros, dimming, robustez de viewport e zero fallbacks sintéticos.
  - Suíte canônica expandida de 161 para **186 testes aprovados, 0 falhas, 0 warnings**.

## Fase 27C — Unifilar Visual WPF + Correção da Identidade Git

- **Workstream A — Identidade Git:** Investigada a origem da identificação `Jonathan IM3` (originada da chave `user.name` na configuração local `.git/config`) e reconfigurada formalmente para `Jonatas`. Histórico de commits anteriores preservado sem rebase.
- **Workstream B — Unifilar Visual WPF:**
  - Implementada a área central do Unifilar Operacional no `MainWindow.xaml` e `MainWindow.xaml.cs` utilizando WPF nativo (Canvas, ItemsControl, DataTemplate, Line, Border).
  - Renderização esquemática e ortogonal de Transformadores, Trechos (com condutor e badge de ΔV) e Nós (com identificação da chave externa e queda acumulada CA%).
  - Interação gráfica completa de visualização: Zoom (25% a 400%), Pan por arrasto com clique do mouse e botão de ajuste à tela (*Fit to View* com centralização automática da bounding box).
  - Painel Lateral de Detalhes implementado como drawer contextual categorizado (Identificação/Topologia, Grandezas Elétricas, Térmico/Condutor, Curto-Circuito e Rastreabilidade).
  - Sincronização bidirecional de seleção: clicar em um trecho no diagrama seleciona a linha correspondente no DataGrid de Segmentos e vice-versa.
  - Testes automatizados expandidos para redes radiais com ramificações (1 → N) e bifurcações, operações de Zoom/Pan e estados visuais.
  - Suíte de testes expandida para **161 testes aprovados, 0 falhas, 0 warnings**.

## Fase 27B — Contratos de Apresentação e Projeção do Grafo para o Unifilar

- Estabelecida a camada de apresentação do diagrama unifilar (`QdtCqts.Desktop.Wpf.ViewModels.Unifilar`), desacoplada de regras elétricas e renderização XAML definitiva.
- Criados contratos de apresentação com preservação estrita de identificadores de domínio: `UnifilarNodeViewModel`, `UnifilarEdgeViewModel`, `UnifilarTransformerViewModel`, `UnifilarDiagramViewModel` e `SelectedElementDetailViewModel`.
- Implementado o motor de layout geométrico puro `HierarchicalTreeLayoutEngine` com interface `IUnifilarLayoutEngine`, garantindo posicionamento determinístico e livre de recálculos elétricos.
- Construtor determinístico `UnifilarPresentationBuilder` orquestrando a fusão de `NetworkModel`, `TopologyValidationResult` e `NetworkCalculationReport`.
- Corrigida a duplicação arquitetural do `OverloadStatus` em `SegmentDisplayModel` e `UnifilarEdgeViewModel`, que agora consomem diretamente o campo `IsOverloaded` calculado pelo motor no backend.
- Classificação de nós em `UnifilarNodeType` derivada estritamente da topologia estrutural (`Source`, `Transformer`, `PassThrough`, `Branch`, `Terminal`).
- Decisões de domínio fundamentadas: QT não formalizado no contrato atual (não inventado); demanda acumulada e curto-circuito preservados em seus contratos autoritativos do backend; nenhuma fórmula nova de tensão inserida na apresentação.
- Adicionados 7 novos testes de integração e integridade de grafo em `UnifilarProjectionTests.cs`, expandindo a suíte para **156 testes aprovados, 0 falhas, 0 warnings**.

## Fase 26 — Integridade da Proteção, Fail-Closed e Reconciliação de Governança

- Removidos da `CandidateProtectionRule` os defaults silenciosos de NH-1600 A e 0,1 s; seção sem mapeamento deixa de assumir 16 mm².
- Removidos também os fallbacks produtivos de catálogo de condutor e `TR_DEF`; sem condutor/transformador correspondente no modelo, o cálculo retorna diagnóstico `Blocked` em vez de sintetizar ampacidade, impedância ou parâmetros de transformador.
- Adicionados contratos explícitos `ProtectionDeviceEvidence`, `ProtectionEvidenceStatus` e `ProtectionAssessmentStatus` (`Pass`, `Fail`, `EvidenceBlocked`), com valores de adequação anuláveis quando a evidência não existe.
- A regra calcula `Ib` sem curva e calcula Onderdonk apenas quando seção catalogada e temperatura validada têm referências identificáveis. Evidência de dispositivo requer fonte, SHA-256, fabricante, modelo, curva, corrente nominal, ponto de avaliação, tempo total de interrupção e capacidade de interrupção.
- O pipeline passa a propagar a ausência de evidência sem bloquear Icc3φ/Icc1φ ou o relatório global do cálculo. Tensão nominal desconhecida bloqueia o pipeline em vez de usar 220 V como fallback.
- A WPF exibe `EvidenceBlocked` sem classificar como erro de cálculo, preserva os segmentos/Icc e não apresenta fusível recomendado sem dados do dispositivo.
- Evidência de temperatura propagada somente após sucesso de `CandidateCableTemperatureRule`; condutor inexistente no catálogo impede cálculo de seção/tempo térmico.
- Corrigidos testes legados que afirmavam adequação de NH sem curva. Incluídos testes para falta/invalidez de evidência, cálculo térmico, consumo de contrato explícito e estado WPF.
- Golden values completos mantidos. Os checks de curto-circuito são classificados `TOLERANCE_PARITY` com tolerância absoluta de `1e-9 A`; os fluxos WPF são `BEHAVIORAL`. A alegação anterior de “bit-a-bit” da Fase 24 foi reclassificada após constatação de diferenças de poucos ULPs, sem alterar o motor.
- README, CAC, RAG e índice de fases reconciliados. Release `0.7.0`/Fase 26 registrada em `VERSION_MANIFEST.json`, apontando ao commit funcional `29c46ad`.
- Roadmap posterior: `NEXT_PHASE_NOT_DEFINED`.
- **Gates:** `PROTECTION_PROVENANCE_GATE=GO (restrito ao que é consumido)`, `NO_SILENT_DEFAULT_GATE=GO`, `FAIL_CLOSED_GATE=GO`, `SHORT_CIRCUIT_REGRESSION_GATE=GO`, `THERMAL_REGRESSION_GATE=GO (restrito)`, `WPF_EVIDENCE_GATE=GO`, `PARITY_GATE=GO (tolerância)`, `DOCUMENTATION_GATE=GO`, `DOMAIN_ISOLATION_GATE=GO`, `BUILD_GATE=GO`, `TEST_GATE=GO`.
- **Validação final:** `dotnet restore` OK; `dotnet build --no-restore` OK (0 erros, 0 warnings); `dotnet test --no-build --no-restore`: **149 aprovados, 0 falhas, 0 ignorados**. `git diff --check` executado ao final.

## Fase 25 — 2026-09-28

- **Consolidação do Motor de Cálculo, Integração WPF e Contrato Normalizado.**
- **WS-A — Auditoria de Contratos de Saída:**
  - Verificação completa de `CalculationOutputModels.cs`: todos os campos de `SegmentCalculationResult`, `NodeCalculationResult`, `TransformerCalculationResult`, `ProtectionCalculationResult` e `NetworkCalculationReport` auditados e aprovados sem duplicação.
- **WS-B — Normalização do Contract Mismatch F24.1-A:**
  - Campos `IsCurrentAdequate` e `IsThermalAdequate` de `ProtectionCalculationResult` renomeados para `IsRatedCurrentAdequate` e `IsThermalWithstandAdequate`, alinhando com a nomenclatura canônica de `ProtectionAssessment` em `Domain`.
  - Construção de `ProtectionCalculationResult` em `UnifiedCalculationPipeline` convertida para nomeação explícita de parâmetros, eliminando mapeamento posicional silencioso.
  - Adicionado `<remarks>` de documentação no record para rastrear a decisão de design.
- **WS-C — Modularização Controlada do Pipeline:**
  - `UnifiedCalculationPipeline.cs` auditado (582 linhas): dentro do Soft Limit, sem violação. Responsabilidades bem definidas. Modularização adicional diferida para quando atingir Soft Limit com nova funcionalidade.
- **WS-D/E — WPF — Integração Real e Estados Explícitos:**
  - Enum `CalculationUiState` introduzido: `Idle`, `Calculating`, `Success`, `ValidationError`, `CalculationError`, `EvidenceBlocked`.
  - `MainViewModel` refatorado com separação estrita de erro de validação vs. erro de cálculo vs. ausência de evidência.
  - Adicionadas propriedades `InputHash`, `IsRatedCurrentAdequate`, `IsThermalWithstandAdequate` e `UiState` à ViewModel.
  - `ResetResults()` limpa todos os campos de rastreabilidade corretamente.
  - `ApplySuccessState()` e `ApplyCalculationErrorState()` como métodos privados nomeados para clareza de ciclo de vida.
- **WS-F — Resultado Auditável:**
  - `InputHash` e `OutputHash` agora expostos e populados na ViewModel após cálculo.
  - `CorrelationId` e `CalculationId` preservados e propagados corretamente.
- **WS-G — Proteção (Escopo Mantido):**
  - Curvas NH, curvas de disjuntores, coordenação e seletividade continuam classificados como `BLOCKED_BY_MISSING_EVIDENCE`. Nenhuma alteração neste escopo.
- **Limpeza:**
  - Arquivo `Class1.cs` (stub vazio) removido de `QdtCqts.Application`.
  - `algorithmVersion` default em `CalculationService` atualizado de `"23.0.0"` para `"25.0.0"`.
- **WS-I/J — Testes de Integração e End-to-End:**
  - 12 novos testes em `Fase25IntegrationTests.cs` cobrindo todos os 9 cenários do WS-I e 2 fluxos E2E (PROJ 7 e PROJ 4).
  - Todos os valores elétricos verificados contra oráculos reais das Fases 22–24.
  - Total da suíte expandido para **146 testes aprovados, 0 falhas, 0 warnings** relevantes.
- **Gates:** `CONTRACT_GATE=GO`, `PIPELINE_GATE=GO`, `WPF_ARCHITECTURE_GATE=GO`, `WPF_INTEGRATION_GATE=GO`, `END_TO_END_GATE=GO`, `PARITY_REGRESSION_GATE=GO`, `OBSERVABILITY_GATE=GO`, `DOMAIN_ISOLATION_GATE=GO`, `BUILD_GATE=GO`, `TEST_GATE=GO`, `DOCUMENTATION_GATE=GO`, `PROTECTION_SCOPE_GATE=GO`.

## Fase 24 — 2026-09-27

- **Reconstrução da Cadeia de Curto-Circuito (Icc 3φ e Icc 1φ) e Análise de Evidências de Proteção.**
- **Decomposição Auditável de Impedância Equivalente (Zeq) e Curto-Circuito Trifásico (Icc 3φ):**
  - Mapeamento analítico e computacional completo da fórmula `LADO 1!CB13`:
    `=($BX$6/SQRT(3))/IMABS(IMSUM($AS$8,BK13,BL13,$CA$8,$CF$8))`
  - Decomposição exata dos 12 parâmetros constituintes (`BX6`, `CF8`, `CG8`, `BW6`, `AS6`, `BB13`, `BI13`, `AR13`, `C6`, `D6`, `E6`, `G6`).
  - Demonstração analítica da impedância equivalente de Thévenin:
    $R_{\text{total}} = R_{\text{fonte}} (0) + R_{\text{MT}} (0.00039839) + R_{\text{trafo}} (0) + R_{\text{BT}} (0.00017742) = 0.000575805501 \;\Omega$.
    $X_{\text{total}} = X_{\text{fonte}} (0.000242) + X_{\text{MT}} (0.00019511) + X_{\text{trafo}} (0.01505778) + X_{\text{BT}} (0.0001794) = 0.015674288889 \;\Omega$.
    $|\mathbf{Z}_{\text{eq}}| = \sqrt{R_{\text{total}}^2 + X_{\text{total}}^2} = 0.015684861623 \;\Omega$.
  - Paridade originalmente reportada como bit-a-bit (reclassificada na Fase 26 como `TOLERANCE_PARITY` após auditoria IEEE-754):
    Excel `CB13` = `8098.066930449716 A`, Motor = `8098.066930449716 A`, Erro = $0.00 \times 10^0\text{ A}$ (IEEE-754).
- **Curto-Circuito Monofásico (Icc 1φ):**
  - Identificada e comprovada a fórmula da coluna `CC13`:
    `=($BX$6/SQRT(3))/IMABS(IMSUM($AS$8,BN13,BO13,$CA$8,$CF$8))`
  - Comprovada a resistência de laço fase-neutro: $BO13 = (R_{\text{fase}} + R_{\text{neutro}}) \times \frac{L_{\text{equiv}}}{1000}$.
  - Paridade confirmada em `CC13`:
    Excel = `8182.488711140876 A`, Motor = `8182.488711140876 A`, Erro = $0.00 \times 10^0\text{ A}$.
- **Validação Cruzada em Corpus Homologado (CQT PROJ 7 REV2 e CQT PROJ 4 REV1):**
  - Confirmação de que a estrutura de cálculo de curto-circuito é idêntica nos dois projetos reais.
  - CQT PROJ 4 `CB13`: Excel = `8098.06418783237 A`, Motor = `8098.06418783237 A`, Erro = $0.00 \times 10^0\text{ A}$.
  - CQT PROJ 4 `CC13`: Excel = `8182.474839937445 A`, Motor = `8182.474839937445 A`, Erro = $0.00 \times 10^0\text{ A}$.
- **Auditoria de Evidências de Proteção:**
  - Inspecionadas as abas `Curva NH` e `Curva Disj.`: constatado que contêm unicamente imagens estáticas WMF e referências textuais externas a documentos DOC/PDF (`..\ARQ_2_ANEXOS\Curvas de disjuntores de BT.doc` e `..\ARQ_2_ANEXOS\Curva disj trif_Trafo AP_ GES-6300B.pdf`), sem nenhuma tabela numérica ou curva digitalizada.
  - Coordenação/seletividade de curvas classificada rigorosamente como: `BLOCKED_BY_MISSING_EVIDENCE`.
  - Mapeadas as fórmulas existentes de proteção em `LADO 1`: corrente de projeto de raiz (`CH40 = 195.3753 A`), menor corrente monofásica de falta (`CH36 = 500.1808 A`) e tempo admissível de curto pelo critério de suportabilidade térmica do condutor mais crítico (Equação de Onderdonk em `CR37` e `CR38`).
- **Implementação e Testes:**
  - Regra isolada `CandidateShortCircuitRule` consolidada em `src/QdtCqts.Calculation.Cqts`.
  - Integração no `UnifiedCalculationPipeline` propagando impedâncias e calculando correntes por trecho da rede radial.
  - Suíte de testes expandida para **134 testes automatizados aprovados, 0 falhas, 0 warnings**.
  - Gates: `SHORT_CIRCUIT_3PH_GATE = GO`, `IMPEDANCE_DECOMPOSITION_GATE = GO`, `SHORT_CIRCUIT_1PH_GATE = GO`, `PROTECTION_EVIDENCE_GATE = BLOCKED_BY_MISSING_EVIDENCE (Curvas NH/Disjuntor sem dados numéricos)`, `PARITY_GATE = GO`, `TEST_GATE = GO`.

## Fase 23 — 2026-09-26

- **Integração Ponta a Ponta da Cadeia de Cálculo + Identidade Visual Oficial (sisQDT_LIGHT).**
- **Workstream A — Cadeia de Cálculo Ponta a Ponta:**
  - Implementação do pipeline unificado ponta a ponta (`UnifiedCalculationPipeline`) integrando:
    * *Cargas Individuais*: Agregação nos nós via `CandidateConsumerLoadAggregationRule`.
    * *Topologia Radial*: Ordenação topológica e validação invariante estrita via `TopologyValidator`.
    * *Acumulação a Montante*: Pós-ordem (folhas até a raiz) calculando $E$ e consumidores $D$ via `CandidateRadialLoadAccumulationRule`.
    * *Demanda e Fim de Trecho*: Seleção de carga $M$ com fator de diversidade $G$ e piso regulatório $CH5$ via `CandidateEndLoadSelectionRule`.
    * *Dimensionamento*: Corrente de projeto $I_b$, capacidade do condutor $I_z \cdot AP$ e detecção automática de sobrecarga (`IsOverloaded`).
    * *Análise Térmica*: Temperatura de regime permanente $T$ via `CandidateCableTemperatureRule` e resistência CA corrigida $R_{\text{ca}}(T)$ via `CandidateThermalResistanceRule`.
    * *Impedância e Queda no Trecho*: $Z = \sqrt{R_{\text{ca}}^2 + X^2}$, $BJ = Z / (V^2 / 100)$ e $\Delta V\%$ com multiplicador de fase via `CandidateSegmentVoltageDropRule`.
    * *Transformador e MT*: Queda interna $\Delta V\%_{\text{trafo}}$ via `CandidateTransformerVoltageDropRule` e parcela de média tensão $\Delta V\%_{\text{MT}}$.
    * *Queda Acumulada*: Pré-ordem (raiz até as folhas) calculando $CA\%$ com isolamento estrito de caminhos radiais via `CandidateAccumulatedVoltageDropRule`.
  - Novos modelos estruturados em `src/QdtCqts.Calculation.Abstractions/CalculationOutputModels.cs`: `SegmentCalculationResult`, `NodeCalculationResult`, `TransformerCalculationResult` e `NetworkCalculationReport`.
  - Orquestração de aplicação (`CalculationService`) gerenciando `CorrelationContext`, `CalculationId`, medição de duração, logging estruturado com `CalculationEventIds`, persistência de traces em `CalculationTraceRecorder` e hash canônico determinístico via `DeterministicHashing`.
- **Workstream B — Identidade Visual Oficial (sisQDT_LIGHT):**
  - Criação da marca e conceito gráfico original representando rede de distribuição radial, diagrama de circuito elétrico, nós técnicos e precisão matemática.
  - Tipografia técnica estrita: `sisQDT_LIGHT` (sis em caixa baixa regular, QDT em caixa alta extra-bold azul elétrico, _LIGHT em caixa alta semi-bold âmbar energia).
  - Geração de pacote completo de assets em `assets/brand/`:
    * `sisQDT_LIGHT.svg`, `sisQDT_LIGHT_dark.svg`, `sisQDT_LIGHT_light.svg`, `sisQDT_LIGHT_mono.svg`, `sisQDT_LIGHT_icon.svg`.
    * `sisQDT_LIGHT.png`, `sisQDT_LIGHT_dark.png`, `sisQDT_LIGHT_light.png`, `sisQDT_LIGHT_mono.png`, `sisQDT_LIGHT_icon.png`.
    * `sisQDT_LIGHT.ico`: Ícone Windows oficial multi-resolução contendo 7 tamanhos (16×16, 24×24, 32×32, 48×48, 64×64, 128×128, 256×256) em 32-bit RGBA com transparência pura.
  - Integração nativa no projeto `QdtCqts.Desktop.Wpf` (`ApplicationIcon`, Resources e Window Icon/Title no `MainWindow.xaml`).
  - Criação do manual de diretrizes de marca em `docs/brand/README.md`.
- **Suíte de Testes Automatizados:**
  - 27 novos testes unitários e de integração adicionados:
    * 23 testes em `BrandAssetTests.cs` validando existência física, headers de PNG, parsing de SVG, estrutura multi-resolução de ICO e espelhamento em WPF.
    * 4 testes em `Fase23EndToEndIntegrationTests.cs` validando paridade com CQT PROJ 7 REV2, CQT PROJ 4 REV1, detecção de sobrecarga e rejeição de ciclos topológicos.
  - Total da suíte expandido para **122 testes aprovados, 0 falhas, 0 warnings**.
- **Gates:** 14 gates avaliados e aprovados com `GO` (8 para Fase 23 e 6 para Identidade Visual).

## Fase 22.2 — 2026-09-26

- **Observabilidade, Logging Estruturado e Rastreabilidade de Cálculo.**
- **Separação estrita de 3 camadas de rastreabilidade:**
  1. *Application Log*: Comportamento operacional da aplicação via `ILogger<T>` com provedor Serilog isolado em `QdtCqts.Infrastructure.Observability`.
  2. *Calculation Trace*: Rastreamento granular da execução física e matemática com insumos tipados (`UnitCode`), passos intermediários, fórmula e hashes determinísticos.
  3. *Audit / Evidence Trail*: Proveniência de fatos derivados de Excel (arquivo, hash SHA-256, aba, célula, valor original).
- **Taxonomia formal de Event IDs:** Implementada em `CalculationEventIds` cobrindo `APP-xxx`, `DB-xxx`, `IMPORT-xxx`, `CALC-xxx`, `RULE-xxx`, `TOPO-xxx`, `PARITY-xxx`, `EVID-xxx` e `GOLDEN-xxx`.
- **Correlação e Identidade:** `CorrelationContext` com escopo ambiente `AsyncLocal` para correlação de fluxos operacionais (`CorrelationId`) e execuções matemáticas específicas (`CalculationId`).
- **Hashes Determinísticos Canônicos:** Utilitário `DeterministicHashing` implementando ordenação alfabética estrita, cultura invariante, formatação lossless `G17`, representação de nulos e SHA-256 para `InputHash` e `OutputHash`.
- **Modos de Trace:** Suporte aos modos `Normal` e `Diagnostic` em `CalculationAuditService` e `CalculationTraceRecorder`.
- **Segurança First e Redação de Dados Sensíveis:** Utilitário `SensitiveDataRedactor` aplicando máscara automática em tokens (`Bearer`), credenciais e PII (CPF).
- **Log File Rotation:** Política de rotação diária de arquivos em `logs/application-yyyyMMdd.log`, limite de 10 MB e retenção de 30 dias.
- **Metadados de Startup:** Evento estruturado `APP-001` emitido no início com versão do código, commit, runtime .NET 8, SO e versões dos manifestos.
- **Documentação de Arquitetura Criada:**
  - `docs/architecture/LOGGING.md`
  - `docs/architecture/CALCULATION_TRACE.md`
  - `docs/architecture/AUDIT_TRAIL.md`
  - `docs/architecture/OBSERVABILITY.md`
- **Suíte de Testes:** 15 novos testes cobrindo configuração, taxonomia de IDs, correlation, calculation ID, determinismo de hashes, paridade, golden cases, redaction, rotação, metadados e cenário de auditoria `CQT PROJ 7 TR -> LID`. Total de testes expandido para **95 testes aprovados, 0 falhas**.
- **Gates:** 5 gates aprovados com `GO` (`LOGGING_GATE`, `CALCULATION_TRACE_GATE`, `AUDIT_TRAIL_GATE`, `REPRODUCIBILITY_GATE`, `SECURITY_LOGGING_GATE`).

## Fase 22.1 — 2026-09-26

- **Auditoria Corretiva Matemática e Paridade Estrita QDT + CQTS.**
- Auditoria direta via OpenXML nas células, fórmulas, unidades e precedentes nos workbooks reais (`CQT PROJ 7 REV2` e `CQT PROJ 4 REV1`).
- **Resolução definitiva de AP vs AQ:** Comprovado pelos cabeçalhos das linhas 10/11/12 que `AP` é o número de circuitos/cabos em paralelo por fase ($n_{\text{paralelo}}$), `AQ` é o comprimento real em metros ($L$), e `AR = AQ / AP` é o comprimento equivalente em metros ($L_{\text{equiv}}$).
- **Cancelamento dimensional exato comprovado:** $M$ em kVA ($10^3\text{ VA}$) e $L$ em metros com $Z$ em $\Omega/\text{km}$ ($10^{-3}\text{ km}$) cancelam exatamente as potências de 10, explicando por que a fórmula direta $M \cdot BJ \cdot L_{\text{equiv}} \cdot k_{\text{fase}}$ resulta em $\Delta V\%$ sem fatores manuais artificiais.
- **Fatores de fase comprovados na fórmula:** $H=3 \implies 1$, $H=2 \implies 2$, $H=1 \implies 6$, identificados textualmente na fórmula `=IF(H=3, BW*BJ*AR, IF(H=2, BW*BJ*AR*2, IF(H=1, BW*BJ*AR*6, 0)))`.
- **Resistência CA térmica fechada:** Comprovada a fórmula de `BI` com correção térmica e fator de efeito pelicular $K^* = AY / AX$ (`CQTS.REAL_PROJECT.THERMAL_RESISTANCE`).
- **Queda interna do transformador formalizada:** Regra `CandidateTransformerVoltageDropRule` (`CQTS.REAL_PROJECT.TRANSFORMER_VOLTAGE_DROP`) implementando `BV4 = (M13 / AS6) * BW6`.
- **Rastreabilidade da queda de Média Tensão:** Célula `CV105 = (CU105 / (BX6 / SQRT(3))) * 100` rastreada até o bloco de MT (linhas 98-108).
- **Paridade numérica bit-a-bit:** 100% dos trechos auditados fecharam com erro absoluto $0.00\times 10^0$ (IEEE-754).
- **Reclassificação formal de Goldens:** 7 casos auditados promovidos a `OFFICIAL_GOLDEN`.
- **Suíte de testes:** Adicionado `Fase22_1ParityTests.cs` cobrindo os 10 casos obrigatórios. Suíte expandida para **80 testes aprovados, 0 falhas**.
- **Gates:** 6 gates aprovados (`TOPOLOGY_GATE = GO`, `VOLTAGE_DROP_GATE = GO`, `THERMAL_IMPEDANCE_GATE = GO`, `CA_ACCUMULATION_GATE = GO`, `QDT_CQTS_CONVERGENCE_GATE = GO`, `GOLDEN_GATE = GO`).

## Fase 22 — 2026-09-26

- **Consolidação da Topologia CQTS + QDT e Reconstrução da Queda de Tensão.**
- Consolidação do modelo topológico unificado em grafo em árvore (Tree Topology): `TR` (raiz), `LID` (lâmina/transição inicial), `PONTO` (nós/postes), `TRECHO` (arestas orientadas `FROM, TO`), `MONTANTE` (pai no grafo) e `RL` (terminal de carga do ramal mais distante).
- Reconstrução da fórmula física exata de queda de tensão no trecho (`BZ`):
  $\Delta V\%_{\text{trecho}} = \frac{M \cdot Z \cdot L_{\text{equiv}}}{V^2 / 100} \cdot k_{\text{fase}}$, com impedância corrigida pela temperatura de regime contínuo $T_{\text{cabo}}$ ($R(T)$ e $X$).
- Reconstrução da propagação acumulada ao longo dos caminhos da árvore (`CA`):
  $CA(v) = CA(\text{pai}) + \Delta V\%_{\text{trecho}}$, comprovando que ramos irmãos (ex: Lado 1 e Lado 3 a partir de P2) não somam quedas entre si.
- Comprovação da queda inicial em `LID`:
  $CA13 = \Delta V\%_{\text{MT}} (1.833\%) + \Delta V\%_{\text{trafo}} (2.316\%) + \Delta V\%_{\text{trecho}} (0.0388\%) = 4.18803\%$.
- Demonstração da convergência analítica 1:1 entre o fator unitário de queda $BJ$ do CQTS e o coeficiente $C_q$ da aba `Coeficiente Unitário` do QDT: $C_q = BJ = \frac{Z}{V^2 / 100}$.
- Implementadas as regras candidatas `CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP` e `CQTS.REAL_PROJECT.ACCUMULATED_VOLTAGE_DROP`.
- Testes expandidos de 60 para 70 testes automatizados, todos aprovados com 0 falhas.
- Gates: `TOPOLOGY_GATE = GO`, `VOLTAGE_DROP_GATE = GO`, `QDT_CQTS_CONVERGENCE_GATE = GO`.

## Fase 21 — 2026-09-26

- **Reconstrução das Cargas Terminais e Acumulação Radial a Montante no CQTS.**
- Identificação analítica das três cargas unitárias elementares dos consumidores: Padrão 1 ($1.4664\text{ kVA}$), Padrão 2 ($1.8048\text{ kVA}$) e Ramal de Ligação Terminal ($1.88\text{ kVA}$), validadas em `CQT PROJ 7 REV2`, `CQT PROJ 4 REV1` e `QDT_ZNA855820_PROJ`.
- Reconstrução da topologia em árvore radial e da regra de acumulação a montante para trechos lineares e pontos com ramificação/bifurcação ($E_{\text{trecho}} = S_{\text{local}} + \sum E_{\text{filhos}}$ e $D_{\text{trecho}} = N_{\text{local}} + \sum D_{\text{filhos}}$).
- Fechamento da cadeia matemática completa no nó de bifurcação `LID`:
  - `PROJ 7`: $53.2792\text{ kVA (Ramo 1)} + 14.5136\text{ kVA (Ramo 2)} + 6.6552\text{ kVA (Local)} = 74.4480\text{ kVA}$ ($D = 47$), reproduzindo $E13$ e $M13$ exatamente.
  - `PROJ 4`: $26.9216\text{ kVA (Ramo 1)} + 41.9616\text{ kVA (Ramo 2)} + 6.8034\text{ kVA (Local)} = 75.6866\text{ kVA}$ ($D = 48$), reproduzindo $E13$ e $M13$ exatamente.
- Implementadas as regras candidatas `CQTS.REAL_PROJECT.CONSUMER_LOAD_AGGREGATION` e `CQTS.REAL_PROJECT.RADIAL_LOAD_ACCUMULATION`.
- Testes expandidos de 56 para 60 testes, todos aprovados com 0 falhas e cobertura coletada.
- Gate: `GO`.

- Consulta ao OneNote do usuário (`anotações GERAIS.one`) identificou os critérios corporativos de levantamento de demanda, normas (ET 285/283) e condutores multiplexados na Light.
- Reconstrução de `M13`: identificado como a "Carga no fim do trecho escolhida", combinando a carga acumulada a jusante ($E13$), Fator de Diversidade ($G13$) e pisos de carga monofásica (4 kVA) e bifásica/trifásica (8 kVA).
- Comprovação da topologia ramificada real no CQT PROJ 7 REV2 através das abas `LADO 1`, `LADO 2` e `LADO 3`.
- Implementada a regra `CQTS.REAL_PROJECT.END_LOAD_SELECTION`.
- Testes expandidos de 53 para 56 testes, todos aprovados com 0 falhas.
- Gate: `GO RESTRICTED - M13 RECONSTRUCTED / LOAD RULE CLOSED`.

## Fase 19 — 2026-09-26

- Origem de `AN13` esclarecida: confirmada como a ampacidade admissível do cabo ($I_z = 430\text{ A}$ para o cabo `185 Al - MX`), obtida de tabela/catálogo da LIGHT ou entrada de condutores, e não corrente de carga ($I_b$).
- Teste de sanidade teórico comprovou que a corrente nominal de carga ($195.38\text{ A}$) diverge do valor nominal de catálogo ($430\text{ A}$).
- Inspeção estrutural da aba `LADO 1` e da aba `Tabela` corroborou referências às tabelas de ampacidade subterrânea da LIGHT.
- Cadeia intermediária da temperatura fechada com integridade. Nenhuma regra espúria de corrente foi implementada.
- Testes: 53 aprovados, 0 falhas.
- Gate: `GO RESTRICTED - AMPACITY CONFIRMED / THERMAL CHAIN CLOSED`.

## Fase 18 — 2026-09-26

- Temperatura do cabo validada em CQT PROJ 7 e CQT PROJ 4 com a mesma fórmula.
- `AN13` confirmado como input manual/externo no snapshot, não como corrente calculada.
- Nenhuma nova regra de corrente implementada.
- Testes: 53 aprovados, 0 falhas.
- Gate: `GO RESTRICTED - TEMPERATURE VALIDATED / CURRENT ORIGIN UNRECONCILED`.

## 0.3.0 — 2026-09-26

- **Phase:** FASE17
- **Change:** implementada a regra candidata `CQTS.REAL_PROJECT.CABLE_TEMPERATURE` para o ramo trifásico real do CQT PROJ 7.
- **Reason:** `P13/BX13` foi identificado pelo usuário como temperatura do cabo; a fórmula reproduziu exatamente o valor observado.
- **Evidence:** CQT PROJ 7 REV2, `LADO 1`, `M13=74.448`, `BX6=220`, `AN13=430`, `AP13=2`, `BX13=43.630837053053675`.
- **Tests:** 26 testes focados; suíte completa validada no encerramento.
- **Gate:** `GO RESTRICTED - FIRST REAL PROJECT RULE IMPLEMENTED`.
- **Scope:** regra candidata de projeto real; baseline elétrico e ruleset oficial permanecem inalterados.

## 0.2.0 — 2026-09-26

- **Phase:** FASE15
- **Change:** adicionada a regra candidata `QDT.RAMAL.CANDIDATE_RX_COMBINATION` para reproduzir `Ramais!C13` nos candidatos convergentes.
- **Reason:** fórmula, entradas, unidades de R/X e saída foram reproduzidas exatamente fora do Excel.
- **Evidence:** `CQT - ZERADO.xlsm` e `CQT - Light (Robusto).xlsm`, hashes registrados no relatório Fase 15.
- **Tests:** 24 testes focados aprovados; suíte completa validada no encerramento.
- **Gate:** `GO RESTRICTED - FIRST MATHEMATICAL RULE IMPLEMENTED`.
- **Scope:** regra candidata; `F9-RULESET-1` e baseline elétrico não foram alterados.

## 0.1.0 — 2026-09-26

- **Phase:** FASE13
- **Change:** estabelecido versionamento local formal para código, evidência, baseline, schema, precedence e ruleset.
- **Reason:** criar rastreabilidade sem promover o baseline elétrico não reconciliado.
- **Evidence:** estado validado da Fase 12; 45 testes aprovados; hashes críticos registrados em `ARTIFACT_MANIFEST.md`.
- **Tests:** build OK; 45 aprovados; 0 falhas; 0 diagnósticos.
- **Gate:** `GO RESTRICTED - VERSIONING ESTABLISHED / BASELINE STILL UNRECONCILED`.
- **Electrical model:** permanece `UNRECONCILED`; nenhum workbook candidato foi promovido.
