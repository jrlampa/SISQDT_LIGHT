# Changelog

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
