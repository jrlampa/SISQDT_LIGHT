# RAG MEMORY — SISQDT_LIGHT (QdtCqts)

> **Documento de Memória Persistente do Projeto**  
> **Última Atualização:** 2026-09-28  
> **Versão do manifesto:** 0.7.0 (último release registrado — Fase 26; Fase 30B em desenvolvimento)
> **Branch Ativa:** `dev`  

---

## 1. Contexto e Missão do Projeto

O **SISQDT_LIGHT** é a iniciativa de engenharia de software da IM3 Brasil voltada para a modernização das ferramentas de planejamento, dimensionamento e cálculo de rede elétrica de distribuição da concessionária **Light S.A.**. Historicamente, estes cálculos dependiam de duas grandes famílias de planilhas Excel/VBA:

1. **QDT (Queda de Tensão):** Planilhas complexas com topologia linear bipartida (`LADO 1` e `LADO 2`), cálculo de queda de tensão em circuitos secundários de transformadores, ramais de ligação e balanceamento de cargas.
2. **CQTS (Cálculo de Queda de Tensão Subterrânea / Malhas):** Planilhas com topologia em árvore direcionada (`PONTO`, `PONTO MONTANTE`), cálculo de corrente acumulada por trecho, verificação de ampacidade de condutores subterrâneos e proteção de rede.

### Objetivo
Substituir o legado instável por uma aplicação desktop de alta precisão, determinística, auditável e offline-first com persistência SQLite e posterior sincronização Supabase, garantindo paridade matemática rigorosa com dados reais antes de qualquer liberação em produção.

---

## 2. Regras Não Negociáveis (Non-negotiables)

1. **Apenas na branch `dev`:** Nenhuma alteração direta na branch principal sem passar por validação em `dev`.
2. **OBRIGATÓRIO:** Criar e manter atualizado o par `RAG/MEMORY.md` + `CAC.md` antes e após cada ação.
3. **NÃO usar dados mockados:** Trabalhar exclusivamente com dados reais extraídos de projetos elétricos ou gerados via lógica comprovada de geoprocessamento.
4. **Não usar 3D e sim 2.5D:** Modelagem topológica e espacial plana com coordenadas georreferenciadas (UTM) e cotas altimétricas tratadas como atributos escalares (metadados Half-way BIM).
5. **Modularidade e SRP:** Separação estrita de responsabilidades entre Domínio, Aplicação, Infraestrutura, Motores de Cálculo e Interface de Usuário.
6. **Segurança First:** Sanitização e blindagem de entradas; leitura não executável de arquivos externos; rejeição de macros VBA; banco com transações e integridade referencial.
7. **Clean Code & Otimização:** "Mais resultado em menos linhas"; clareza arquitetural e sem overhead desnecessário.
8. **Thin Frontend / Smart Backend:** A interface gráfica (WPF) atua estritamente como camada de projeção/visualização. Toda a lógica pesada de validação topológica e cálculo reside nos serviços de domínio e motores de cálculo.
9. **Versionamento Único e Propagado:** Controle formal de versão do código, esquema, regras e evidências em `VERSION_MANIFEST.json` e `ARTIFACT_MANIFEST.md`.
10. **Full Suite de Testes:** Cobertura 100% para os 20% do código com 80% do impacto de negócio (domínio, regras de cálculo, topologia e paridade); cobertura mínima >=80% para os demais.
11. **Half-way BIM:** Manter e evoluir a estrutura de metadados de engenharia (condutores, materiais, coordenadas georreferenciadas e parâmetros de catálogo).
12. **Docker First:** Manter `Dockerfile`, `docker-compose.yml`, `.dockerignore` e `.gitignore` sempre funcionais e atualizados.
13. **Supabase First:** Arquitetura desenhada para integração e sincronização com Supabase (Auth, Postgres, Storage, Realtime, Edge Functions).
14. **Domain-Driven Design (DDD):** Modelagem orientada a domínios ricos, isolamento de Aggregates (`Project`, `ProjectVersion`, `NetworkModel`, `CalculationRun`), Value Objects e Invariantes.
15. **Interface 100% pt-BR:** UI/UX e mensagens para o usuário final integralmente em português do Brasil.
16. **Zero Custo a Todo Custo:** Uso exclusivo de ferramentas, bibliotecas e APIs de código aberto ou gratuitas.
17. **Limites de Tamanho de Código por Arquivo:**
    - **Ideal:** até 500 linhas
    - **Soft Limit:** até 750 linhas
    - **Hard Limit Absoluto:** até 1000 linhas (apenas quando modularização for tecnicamente inviável).
18. **Finalização Obrigatória de Task:**
    1. Executar suíte de testes (`dotnet test`);
    2. Verificar cobertura e integridade;
    3. Realizar o commit na branch `dev`;
    4. Atualizar `RAG/MEMORY.md` e `CAC.md`.

---

## 3. Histórico e Cronologia das Fases de Engenharia Reversa

- **Fases 01 a 05:** Auditoria inicial dos workbooks legados (`QDT_ZNA855820`, `CQTS__ZNA855820`). Mapeamento de fórmulas corrompidas (`#REF!`), links externos bloqueantes (`DecInv` em unidade de rede corporativa `J:\`), catálogo de precisão e unidades, e definição dos níveis do Harness de Paridade (P0 a P4). Decisão: **NO-GO para motor de produção; GO restrito para infraestrutura e testes**.
- **Fase 06:** Definição da arquitetura unificada da aplicação desktop em .NET 8, WPF, SQLite e assemblies desacoplados.
- **Fase 07:** Implementação da fundação de dados em SQLite, importador de evidência celular somente leitura (OpenXML) e harness de paridade P0/P1.
- **Fase 08:** Hardening de infraestrutura e construção do grafo formal de precedência de células.
- **Fase 09:** Formalização do subconjunto matemático isolado (`F9-RULESET-1`) e primeiros testes de regras unitárias independentes de células.
- **Fase 10:** Fechamento semântico das regras de carga e mapeamento de dependências entre QDT e CQTS.
- **Fase 11:** Identificação e exclusão definitiva de abas protótipo dos workbooks que distorciam os dados reais de cálculo.
- **Fase 12:** Reconciliação do baseline e catálogo de artefatos oficiais.
- **Fase 13:** Estabelecimento formal da governança de versionamento criptográfico (SHA-256) em `ARTIFACT_MANIFEST.md` e `VERSION_MANIFEST.json`.
- **Fase 14:** Catálogo histórico de evidências, matriz de reconstrução matemática e validação do modelo topológico de grafo dirigido.
- **Fase 15:** Implementação e validação fora do Excel da primeira regra candidata (`QDT.RAMAL.CANDIDATE_RX_COMBINATION`), reproduzindo exatamente `Ramais!C13` nos candidatos convergentes.
- **Fase 16:** Identificação de corpus de projeto real em uso pela engenharia (`CQT PROJ 7 REV2`) e identificação da cadeia condicional em `LADO 1!P13` e `BX13`.
- **Fase 17:** Sessão com usuário especialista que revelou o significado físico dos campos no projeto real: `AM13` é o condutor (`185 Al - MX`), `AN13` é a corrente em Amperes (`430 A`), e `P13/BX13` não é tensão, mas sim a **Temperatura do Condutor [°C]**. A regra `CQTS.REAL_PROJECT.CABLE_TEMPERATURE` foi implementada e testada com sucesso.
- **Fase 18:** Validação cruzada da fórmula de temperatura do cabo em múltiplos projetos reais (`CQT PROJ 7` e `CQT PROJ 4`). Confirmação de que `AN13` é input manual/externo nos snapshots. Suíte com 53 testes aprovados, 0 falhas.
- **Fase 19:** Investigação da origem de `AN13 = 430 A`. Confirmado pelo usuário e pelas evidências textuais da planilha (*"Corrente do cabo para a condição"*) que `AN13` é a **ampacidade/capacidade admissível ($I_z$)** do condutor de catálogo e não uma corrente de carga ($I_b$). Teste de sanidade comprovou que a corrente nominal de carga ($195.38\text{ A}$) diverge de $430\text{ A}$. Cadeia intermediária da temperatura fechada com integridade. Gate: `GO RESTRICTED - AMPACITY CONFIRMED / THERMAL CHAIN CLOSED`. 53 testes aprovados, 0 falhas.
- **Fase 20:** Reconstrução de `M13` através da consulta ao OneNote do usuário (`anotações GERAIS.one`) e análise de projetos reais (`CQT PROJ 7 REV2` e `CQT PROJ 4 REV1`). `M13` é a "Carga no fim do trecho escolhida", combinando a carga acumulada a jusante ($E13$), Fator de Diversidade ($G13$) e pisos de carga de 4 kVA (monofásico) e 8 kVA (bifásico/trifásico). Comprovada ramificação real da rede nas abas `LADO 1`, `LADO 2` e `LADO 3`. Implementada a regra `CQTS.REAL_PROJECT.END_LOAD_SELECTION`. Gate: `GO RESTRICTED - M13 RECONSTRUCTED / LOAD RULE CLOSED`. Suíte expandida para 56 testes aprovados, 0 falhas.
- **Fase 21:** Reconstrução das cargas terminais e acumulação radial a montante no CQTS. Identificação analítica das três cargas unitárias elementares dos consumidores: Padrão 1 ($1.4664\text{ kVA}$), Padrão 2 ($1.8048\text{ kVA}$) e Ramal de Ligação ($1.88\text{ kVA}$), validadas em `CQT PROJ 7 REV2`, `CQT PROJ 4 REV1` e `QDT_ZNA855820_PROJ`. Reconstrução da topologia em árvore radial e da regra de acumulação a montante para trechos lineares e bifurcações ($E_{\text{trecho}} = S_{\text{local}} + \sum E_{\text{filhos}}$ e $D_{\text{trecho}} = N_{\text{local}} + \sum D_{\text{filhos}}$). Comprovação do fechamento exato no nó de bifurcação `LID` ($74.4480\text{ kVA}$ em PROJ 7 e $75.6866\text{ kVA}$ em PROJ 4). Implementadas as regras candidatas `CQTS.REAL_PROJECT.CONSUMER_LOAD_AGGREGATION` e `CQTS.REAL_PROJECT.RADIAL_LOAD_ACCUMULATION`. Gate: `GO`. 60 testes aprovados, 0 falhas.
- **Fase 22:** Consolidação da topologia CQTS + QDT e reconstrução da queda de tensão. Modelo unificado em grafo em árvore: `TR`, `LID`, `PONTO`, `TRECHO`, `MONTANTE` e `RL` (terminal de carga). Comprovada a fórmula exata de queda percentual no trecho ($\Delta V\% = \frac{M \cdot Z \cdot L_{\text{equiv}}}{V^2 / 100} \cdot k_{\text{fase}}$) e acumulação ao longo dos caminhos da árvore ($CA(v) = CA(\text{pai}) + \Delta V\%_{\text{trecho}}$), com paridade analítica 1:1 e convergência total entre o fator $BJ$ do CQTS e o coeficiente $C_q$ da aba `Coeficiente Unitário` do QDT ($C_q = BJ = \frac{Z}{V^2 / 100}$). Implementadas as regras `CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP` e `CQTS.REAL_PROJECT.ACCUMULATED_VOLTAGE_DROP`. Gates: `TOPOLOGY_GATE = GO`, `VOLTAGE_DROP_GATE = GO`, `QDT_CQTS_CONVERGENCE_GATE = GO`. 70 testes aprovados, 0 falhas.
- **Fase 22.1:** Auditoria corretiva matemática e paridade estrita. Resolução definitiva de `AP` (cabos em paralelo) e `AQ` (comprimento real em metros). Demonstração do cancelamento dimensional exato ($10^3\text{ kVA} \times 10^{-3}\text{ km} = 1$). Comprovação literal dos fatores de fase ($3\phi \rightarrow 1$, $2\phi \rightarrow 2$, $1\phi \rightarrow 6$). Fechamento da resistência CA térmica com fator $K^*$ (`CandidateThermalResistanceRule`) e da queda interna do transformador $BV4$ (`CandidateTransformerVoltageDropRule`). Rastreamento da queda de MT ($CV105$). 7 casos Golden auditados e promovidos a `OFFICIAL_GOLDEN`. Gates: 6 gates aprovados com `GO`. 80 testes aprovados, 0 falhas.
- **Fase 22.2:** Observabilidade, logging estruturado e rastreabilidade de cálculo. Separação estrita de 3 camadas (Application Log, Calculation Trace e Audit Trail). Taxonomia formal de Event IDs (`APP`, `DB`, `IMPORT`, `CALC`, `RULE`, `TOPO`, `PARITY`, `EVID`, `GOLDEN`). Escopos assíncronos de `CorrelationId` e `CalculationId` (`CorrelationContext`). Hashes determinísticos canônicos (`DeterministicHashing`). Modos de trace `Normal` e `Diagnostic`. Higienização de dados sensíveis e PII (`SensitiveDataRedactor`). Rotação diária de logs (10MB, retenção de 30 dias). Projeto isolado `QdtCqts.Infrastructure.Observability` desacoplando Serilog das regras de negócio. Suíte expandida para 95 testes aprovados, 0 falhas. Gates: 5 gates aprovados com `GO`.
- **Fase 23:** Integração Ponta a Ponta da Cadeia de Cálculo + Identidade Visual Oficial (sisQDT_LIGHT). 122 testes automatizados aprovados, 0 falhas. 14 gates aprovados com `GO`.
- **Fase 24 (Fechada):** Reconstrução da Cadeia de Curto-Circuito (Icc 3φ e Icc 1φ) e Análise de Evidências de Proteção. 134 testes aprovados. Gates: todos `GO` exceto `PROTECTION_EVIDENCE_GATE = BLOCKED_BY_MISSING_EVIDENCE`.
- **Fase 25 (Concluída):** Consolidação do Motor de Cálculo, Integração WPF Real e Normalização de Contratos.
  - *WS-A — Auditoria de Contratos*: `CalculationOutputModels.cs` auditado; todos os records aprovados sem duplicação.
  - *WS-B — Contract Mismatch F24.1-A Resolvido*: `ProtectionCalculationResult` normalizado com campos `IsRatedCurrentAdequate` e `IsThermalWithstandAdequate`; construção via nomeação explícita de parâmetros.
  - *WS-C — Pipeline*: `UnifiedCalculationPipeline.cs` (582 linhas) auditado; dentro do Soft Limit; cadeia matemática inalterada.
  - *WS-D/E — WPF*: Enum `CalculationUiState` introduzido (`Idle`, `Calculating`, `Success`, `ValidationError`, `CalculationError`, `EvidenceBlocked`). `MainViewModel` com separação de tipos de erro, exposição de `InputHash`, `IsRatedCurrentAdequate`, `IsThermalWithstandAdequate`.
  - *WS-F — Rastreabilidade*: `InputHash` e `OutputHash` expostos e testados na ViewModel.
  - *WS-G — Proteção*: Curvas NH e Disjuntores continuam `BLOCKED_BY_MISSING_EVIDENCE`.
  - *WS-I/J — Testes*: 12 novos testes de integração e E2E. Suíte expandida para **146 testes aprovados, 0 falhas**.
  - Gates: `CONTRACT_GATE=GO`, `PIPELINE_GATE=GO`, `WPF_ARCHITECTURE_GATE=GO`, `WPF_INTEGRATION_GATE=GO`, `END_TO_END_GATE=GO`, `PARITY_REGRESSION_GATE=GO`, `OBSERVABILITY_GATE=GO`, `DOMAIN_ISOLATION_GATE=GO`, `BUILD_GATE=GO`, `TEST_GATE=GO`, `DOCUMENTATION_GATE=GO`, `PROTECTION_SCOPE_GATE=GO`.

- **Fase 26 (Concluída — fail-closed de proteção):** Removidos defaults NH-1600 A/0,1 s, fallback de seção, catálogos sintéticos de condutor e `TR_DEF`; ausência de entradas de catálogo bloqueia com diagnóstico. Evidência de dispositivo passou a ter contrato e status próprios (`Available/Missing/Invalid`, `Pass/Fail/EvidenceBlocked`). `Ib`, Icc3φ/Icc1φ e `t_adm` com proveniência identificada permanecem calculáveis; a WPF mantém resultados elétricos e sinaliza `EvidenceBlocked` quando não há curva. Golden Icc foi reclassificado como `TOLERANCE_PARITY` com erro absoluto máximo de `1e-9 A`; fluxo WPF é `BEHAVIORAL`. Evidence Store histórico segue ausente e o SHA-256 externo não é verificado contra arquivo. Commit funcional `29c46ad`; release `0.7.0` registrada no `VERSION_MANIFEST.json` pelo commit de governança.
- **Fase 27A (Concluída — Auditoria WPF, Contratos e Unifilar Operacional):** Auditoria aprofundada da interface WPF existente, validação do princípio Thin Frontend e mapeamento das lacunas de contrato para a evolução em Cockpit Operacional. Confirmado que a WPF não recalcula grandezas elétricas. Identificados achados de apresentação (duplicação da verificação de sobrecarga em `SegmentDisplayModel` e falta de exposição de nós/trafos na UI). Mapeadas as lacunas de `NodeCalculationResult` (tensão nodal em Volts e Icc no barramento do nó). Especificada a arquitetura completa da projeção visual unificada e os Presentation Models para a Fase 27B (`UnifilarDiagramViewModel`, `UnifilarNodeViewModel`, `UnifilarEdgeViewModel`, `SelectedElementDetailViewModel`, `UnifilarLayoutEngine`). Relatório oficial em `docs/phases/fase27/FASE27A_AUDITORIA_WPF_E_UNIFILAR.md`.
- **Fase 27B (Concluída — Contratos de Apresentação e Projeção do Grafo para o Unifilar):** Estabelecida a camada de apresentação do diagrama unifilar (`QdtCqts.Desktop.Wpf.ViewModels.Unifilar`), desacoplada de regras elétricas e renderização XAML definitiva. Criados contratos com preservação de identificadores de domínio: `UnifilarNodeViewModel`, `UnifilarEdgeViewModel`, `UnifilarTransformerViewModel`, `UnifilarDiagramViewModel`, `SelectedElementDetailViewModel` e motor geométrico `HierarchicalTreeLayoutEngine`. Sanada a duplicação de `OverloadStatus` em `SegmentDisplayModel`, consumindo `IsOverloaded` direto do motor. 7 novos testes de unidade e integridade de grafo adicionados em `UnifilarProjectionTests.cs`. Suíte expandida para **156 testes aprovados, 0 falhas, 0 warnings**.
- **Fase 27C (Concluída — Unifilar Visual WPF + Correção da Identidade Git):** Implementada a primeira versão funcional e interativa do Unifilar Operacional em WPF (`MainWindow.xaml` e `MainWindow.xaml.cs`) sobre os contratos da Fase 27B. Renderização vetorial de transformadores, trechos (arestas) e nós (barras); suporte a zoom (25% a 400%), pan contínuo e ajuste à tela (*Fit to View*); painel lateral categorizado com dados de engenharia e Half-way BIM; sincronização de seleção entre o diagrama e o DataGrid de trechos; suporte a redes com bifurcações (1 → N); correção da identidade Git local (`Jonatas`) sem reescrever histórico. Suíte expandida para **161 testes aprovados, 0 falhas, 0 warnings**.
- **Fase 27D (Concluída — Navegação Avançada, Trace Elétrico, Filtros Visuais e Otimização Controlada do Unifilar):**
  - **Zero Fallbacks Sintéticos (27D-AUDIT-01):** Eliminada a criação artificial de entidades de domínio na apresentação; seleção com ID inexistente cancela com diagnóstico determinístico (`SelectedElement = null`, `SelectedDetail = null`).
  - **Trace Elétrico Topológico da Raiz à Ponta (Fase 27D-A):** Algoritmo ascendente $O(\text{profundidade})$ rastreando o caminho da seleção até o transformador de alimentação sem recalcular física elétrica. Isolamento perfeito de bifurcações (seleção de um ramo não destaca ramos irmãos).
  - **Filtros Visuais de Condição Elétrica (Fase 27D-B):** Enum `UnifilarVisualFilter` (Todos, Sobrecarga, QuedaTensaoLimite, EvidenceBlocked, Selecionados, Trace). Atenuação visual (dimming 18%) via triggers WPF sem destruir coleções observáveis ou recalcular geometria. Consumo estrito de `IsOverloaded` e `EvidenceBlocked` do backend e threshold explícito configurável de $\Delta V$ (default 5.0%).
  - **FitToView Robusto (27D-AUDIT-02):** Centralização de métricas em `UnifilarVisualMetrics` e proteção total contra NaN/Infinity e viewports degenerados.
  - **Indexação $O(1)$ (27D-AUDIT-03):** Otimização de lookups por ID para grafos de grande escala.
  - **Suíte de Testes:** 25 novos testes dedicados em `UnifilarTraceAndNavigationTests.cs`. Suíte canônica expandida de 161 para **186 testes aprovados, 0 falhas, 0 warnings**.

- **Fase 28 (Concluída — Engenharia Reversa QDT/CQTS Real + Arquitetura Preliminar CAD/LISP):**
  - **Engenharia Reversa ZNA855820:** Rastreamento ponta a ponta da obra real de Rede Invertida. Extraídas fórmulas exatas das planilhas legadas de corrente ($I_b$), temperatura ($T$), resistência térmica ($R_{ca}$), queda de trecho ($\Delta V$), queda acumulada percentual ($CA\%$) e tensões nodais ($V_{127}, V_{220}$).
  - **Auditoria VBA:** Confirmado que o código VBA tem função exclusiva de automação de UI e ordenação de tabelas; a física de engenharia é 100% suportada pelas fórmulas de cálculo, eliminando dependência de macros legadas.
  - **Matriz de Paridade:** Status `CONFIRMADO` para grandezas elétricas e térmicas nos motores do `sisQDT_LIGHT`. Mapeadas lacunas de tensões nominais nodais explícitas em Volts e suporte nativo a cenários `ATUAL` vs `PROJETADO`.
  - **Arquitetura CAD/LISP:** Especificado o fluxo `sisQDT_LIGHT → CAD Export Model → LISP Generator → AutoCAD` e a separação entre espaço lógico e físico. SIRGAS 2000 / EPSG:31983 foi hipótese preliminar, não adotada como contrato; a Fase 30A não determinou datum/EPSG. Avaliadas as alternativas LISP e recomendada a Alternativa B.

- **Fase 30A (Concluída — fonte de geometria física):** DWG é a fonte primária; ferramenta existente do acervo extrai JSON de postes `{id, block, x, y, fuso}` e linhas com coordenadas. Unidade metro e fuso 23 foram identificados; datum/CRS completo não foi determinado; vínculo entre ID CAD e `ExternalKey` lógico continua ausente. Relatório: `docs/phases/fase30a/FASE30A_FONTE_GEOMETRIA_FISICA.md`.
- **Fase 30B (Atual — domínio espacial/importação JSON):** `PhysicalPosition` e `PhysicalLineGeometry` são separados de `LayoutX/LayoutY`; adaptador JSON valida e preserva IDs/blocos. Associação somente por mapas explícitos; sem vínculo, geometria permanece não associada. CRS UTM/fuso é preservado; datum/hemisfério/EPSG não têm defaults. Sem leitura DWG, transformação CRS ou mapa interativo.

---

## 4. Estado Atual do Código e Arquitetura

### 4.1 Projetos da Solução
- `src/QdtCqts.Domain`: Entidades centrais (`Transformer`, `Circuit`, `TopologyNode`, `TopologyEdge`, `Load`, `Conductor`), tipos de versão, taxonomia de eventos, correlação e sanitização de dados.
- `src/QdtCqts.Application`: Serviços orquestradores de casos de uso (`CalculationService`, `CalculationAuditService`, `CalculationTraceRecorder`, `ProjectService`, `ParityService`).
- `src/QdtCqts.Calculation.Abstractions`: Interfaces de motor (`ICalculationEngine`), requisições (`CalculationRequest`), resultados (`CalculationResult`), modelos de saída (`SegmentCalculationResult`, `NodeCalculationResult`, `TransformerCalculationResult`, `NetworkCalculationReport`), trilhas e hashing determinístico canônico (`DeterministicHashing`).
- `src/QdtCqts.Calculation.Qdt`: Regras comprovadas QDT e motor QdtCalculationEngine.
- `src/QdtCqts.Calculation.Cqts`: Motor CqtsCalculationEngine, pipeline unificado de cálculo (`UnifiedCalculationPipeline`) e regras matemáticas do CQTS (temperatura de cabo, seleção de carga M, agregação de cargas, acumulação radial E, queda de tensão no trecho, queda acumulada, resistência térmica CA, queda no transformador e curto-circuito trifásico/monofásico).
- `src/QdtCqts.Infrastructure.Observability`: Provedor de logging estruturado (Serilog), políticas de rotação de arquivos e registrador de startup.
- `src/QdtCqts.Infrastructure.Sqlite`: Banco de dados relacional e repositórios de dados SQLite.
- `src/QdtCqts.Infrastructure.ExcelEvidence`: Importador seguro OpenXML e construção de grafo de precedência celular.
- `src/QdtCqts.Infrastructure.Geometry`: Adaptador isolado do JSON físico produzido pelo acervo; sem parser DWG.
- `src/QdtCqts.Infrastructure.Parity`: Mecanismos de comparação numérica e paridade com tolerância de engenharia.
- `src/QdtCqts.Desktop.Wpf`: Aplicação desktop WPF para operadores de engenharia da Light com identidade visual, ícone oficial, cockpit operacional com diagrama unifilar interativo, trace elétrico determinístico, filtros visuais e painel de detalhes.

### 4.2 Métricas de Teste
- Total de testes automatizados no checkout da Fase 30B: **212 aprovados, 0 falhas, 0 ignorados**; `dotnet test` completo aprovado.
- Falhas: **0**.
- Duração da execução: **~1.7 segundos**.

---

## 5. Roadmap

- Fase 24: fechada no escopo definido; curvas NH/disjuntor, coordenação e seletividade permanecem bloqueadas por falta de evidência.
- Fase 25: concluída; integração WPF ao `CalculationService` consolidada.
- Fase 26: concluída; governança, fail-closed de proteção e paridade com tolerância formalizada.
- Fase 27A: concluída; auditoria completa da interface WPF, contratos e especificação do Unifilar Operacional.
- Fase 27B: concluída; contratos de apresentação, layout geométrico e projeção do grafo para o unifilar.
- Fase 27C: concluída; renderização visual do unifilar no Canvas WPF, zoom/pan/fit, seleção sincronizada e painel de detalhes.
- Fase 27D: concluída; navegação avançada, trace elétrico determinístico da raiz à ponta, filtros visuais (dimming) e zero fallbacks sintéticos.
- Fase 28: concluída; engenharia reversa do caso real ZNA855820 e arquitetura CAD/LISP preliminar.
- Fase 30A: concluída; fonte DWG/JSON, unidade/fuso e lacunas de datum/identidade documentadas.
- Fase 30B: atual; domínio espacial mínimo e importação de geometria JSON.
- Próxima fase oficial: `NEXT_PHASE_NOT_DEFINED`.



