# FASE 28 — ENGENHARIA REVERSA QDT/CQTS REAL + ARQUITETURA PRELIMINAR CAD/LISP

**Data:** 28 de Setembro de 2026  
**Status:** CONCLUÍDO (Engenharia Reversa, Validação de Domínio e Arquitetura CAD)  
**Autor:** Jonatas (`jonatas.lampa@im3brasil.com.br`)  
**Repositório:** `jrlampa/SISQDT_LIGHT`  
**Branch:** `dev`  

---

## 1. ESCOPO E OBJETIVOS

A **Fase 28** estabelece o marco de validação de engenharia e modelagem arquitetural para integração CAD/LISP do `sisQDT_LIGHT`. Em conformidade estrita com a governança do projeto, esta fase **não implementa código de exportação CAD nem botões na UI**, concentrando-se em:

1. **Engenharia Reversa Profunda com Caso Real (ZNA855820):**
   - Desmontar e rastrear a cadeia completa `ENTRADA → PROCESSAMENTO → RESULTADO` das planilhas legadas de QDT (`.xlsm`) e CQTS (`.xlsx`).
   - Auditar fórmulas físicas, tabelas de apoio, limites regulamentares e código VBA para determinar o que é física elétrica e o que é automação de interface.
2. **Mapeamento e Paridade contra o sisQDT_LIGHT:**
   - Confrontar as regras do caso real com o motor elétrico C# (`QdtCqts.Calculation.Cqts`) e com os contratos da Fase 27B/27C.
   - Identificar paridades (`CONFIRMADO`), lacunas (`AUSENTE`/`PARCIAL`) e divergências conceituais.
3. **Auditoria do Unifilar Operacional (Fase 27C):**
   - Revisar o anti-pattern identificado de criação de entidades sintéticas (`?? new Node(...)`, `?? new Edge(...)`) no `UnifilarDiagramViewModel`.
   - Avaliar a sincronização bidirecional, performance de renderização e rastreabilidade com o grafo elétrico.
4. **Arquitetura Preliminar de Exportação CAD/LISP:**
   - Formalizar o fluxo: `QDT/CQTS → sisQDT_LIGHT → Unifilar → CAD Export Model → LISP Generator → AutoCAD`.
   - Definir a separação entre espaço lógico (unifilar WPF em árvore) e espaço físico (georreferenciado em coordenadas planas UTM com azimutes reais).
   - Especificar o contrato preliminar do `CAD Export Model` (entidades semânticas: `LINE`, `PLINE`, `POINT/BLOCK`, `MTEXT`, `MLEADER`).
   - Avaliar as três alternativas técnicas para geração LISP (Autocontido, Payload de Dados, Loader Externo) à luz do acervo prático existente.

---

## 2. BASELINE E GOVERNANÇA GIT

- **Baseline Verificada:** Commit `62084bd4aa7e96a4177f778b61a38de446a76c36` (`dev` sincronizado com `origin/dev`).
- **Suíte de Testes Pré-Fase:** 186 testes executados, 186 aprovados (100% de sucesso), 0 erros, 0 avisos.
- **Identidade Git:** Configuração local corrigida e validada:
  ```text
  user.name = Jonatas
  user.email = jonatas.lampa@im3brasil.com.br
  ```
- **Integridade do Repositório:** Nenhuma operação destrutiva (`rebase`, `reset --hard`, `amend`, `force push`) foi ou será realizada. Arquivo temporário de compilação WPF (`src/QdtCqts.Desktop.Wpf/QdtCqts.Desktop.Wpf_dwkfellp_wpftmp.csproj`) mantido estritamente fora da staging area.

---

## 3. INVENTÁRIO DO PROJETO REAL (ZNA855820)

O caso real selecionado é a obra de **Rede Invertida ZNA855820**, localizada no diretório:
`C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\REDE INVERTIDA\REDE_INVERTIDA_ZNA855820`

### 3.1. Arquivos Encontrados e Inspecionados

| Arquivo | Tipo / Formato | Papel no Projeto | Conteúdo Principal |
| :--- | :--- | :--- | :--- |
| `CQTS__ZNA855820_PROJ.xlsx` | Planilha Excel (OpenXML, sem macros) | Cálculo Elétrico Projetado (Rede Invertida) | 15 abas (`DB`, `COORDENADAS`, `SUGESTAO_CORTES`, `RAMAL`, `PROJ1`, `LADO 1`, `LADO 2`, `GERAL PROJ`, etc.) |
| `CQTS__ZNA855820_ATUAL.xlsx` | Planilha Excel (OpenXML, sem macros) | Cálculo Elétrico Atual (Diagnóstico de Violação) | 12 abas retratando a rede antes da intervenção |
| `1.QDT\QDT_ZNA855820_PROJ.xlsm` | Planilha Excel com Macros (VBA) | Dimensionamento de Cargas e Transformador | 14 abas (`PP e CE`, `Curva Disj.`, `Alocação % de tensão`, `Circuito 01`, `Circuito 02`, etc.) |
| `1.QDT\QDT_ZNA855820_ATUAL.xlsm` | Planilha Excel com Macros (VBA) | QDT da situação atual | Dimensionamento original com violação de tensão |
| `REDE_INVERTIDA_ZNA855820.dwg` | Desenho AutoCAD (Formato AC1032, AutoCAD 2026) | Projeto Executivo e Geometria Física | Postes com atributos (`POSTE PROJ`, `POSTE C/ LUMINARIA`), trafo (`TRAFO RET`), condutores (`240AMX`, `3x1x70+54.6`), cabos BT |
| `ZNA855820.pdf` | Documento PDF (2 páginas) | Prancha de Execução e Ordem Técnica | Desenho de campo com indicação de cortes de rede e inversão de alimentação |
| `aux cad\cqt\cqt_cad_extractor.lsp` | Rotina AutoLISP (Acervo de Apoio) | Referência de Extração/Desenho CAD | Estrutura de exportação e manipulação de entidades com atributos e JSON |

---

## 4. FLUXO DE ENGENHARIA REVERSA — QDT

### 4.1. Cadeia de Cálculo: Entrada → Processamento → Resultado

```
[Cadastro de Cargas / Clientes]
       │
       ▼ (Aba 'PP e CE')
[Classificação por Atividade & Categoria (Residencial, Comercial, IP)]
       │
       ▼ (Fatores de Diversidade / Demanda Light)
[Demanda Diversificada (kVA)]
       │
       ▼ (Aba 'Circuito 01' / 'Circuito 02')
[Alocação por Circuito e Faseamento (A, B, C)]
       │
       ▼
[Dimensionamento do Trafo & Proteção Geral (Disjuntor / Curva Disj.)]
```

### 4.2. Regras e Fórmulas de QDT Identificadas

1. **Demanda Individual e Agrupada:**
   - Aplicação de curvas de demanda Light por tipologia de carga e número de unidades consumidoras.
   - Cálculo da corrente nominal secundária do transformador ($S_{\text{trafo}} = 112.5\text{ kVA}$, $V_{\text{sec}} = 220/127\text{ V}$):
     $$I_{\text{nom, trafo}} = \frac{112.5 \times 1000}{\sqrt{3} \times 220} = 295.24\text{ A}$$
2. **Equilíbrio de Fases:**
   - As cargas são distribuídas entre as fases A, B e C nos circuitos de baixa tensão para minimizar a corrente no neutro e o desbalanço de tensão.
3. **Alocação de Queda de Tensão (Aba `Alocação % de tensão`):**
   - **Média Tensão (MT):** Reserva típica de $1.00\%$ a $1.50\%$.
   - **Transformador (Trafo):** Queda interna de impedância percentual ($\approx 2.50\%$).
   - **Margem Restante para BT:** Limite regulamentar total de $8.66\%$ (critério Light/ANEEL Prodist). No caso analisado, a alocação inicial na rede MT + Trafo somava **$3.50\%$**, restando **$5.16\%$** para a rede secundária de BT.

---

## 5. FLUXO DE ENGENHARIA REVERSA — CQTS

O CQTS (Cálculo de Queda de Tensão e Sobrecarga) opera sobre a topologia radial árvore da rede secundária, trecho a trecho, da raiz (bucha do transformador) até cada nó terminal/ramal.

### 5.1. Fórmulas Matemáticas Rigorosamente Extraídas das Células

A inspeção automatizada do arquivo `CQTS__ZNA855820_PROJ.xlsx` (abas `PROJ1`, `LADO 1`, `LADO 2`) revelou a formulação exata executada:

#### 1. Corrente de Carga no Trecho ($I_b$)
- **Monofásico:**
  $$I_b = \frac{S_{\text{acum}} \times 1000}{V_{\text{sec}} \times \eta}$$
- **Trifásico:**
  $$I_b = \frac{S_{\text{acum}} \times 1000}{\sqrt{3} \times V_{\text{nom}} \times \eta}$$
  *Onde:* $V_{\text{nom}} = 220\text{ V}$, $V_{\text{sec}} = 127\text{ V}$, $\eta$ é o número de condutores por fase em paralelo.

#### 2. Temperatura Operacional do Condutor ($T$)
Fórmula na Coluna `N` da planilha:
$$T = T_{\text{amb}} + 60 \times \left(\frac{I_b}{I_z}\right)^2$$
- $T_{\text{amb}} = 20^\circ\text{C}$ (ou $30^\circ\text{C}$ dependendo do critério sazonal adotado).
- $I_z$ é a capacidade nominal de condução (ampacidade) em regime contínuo para o cabo e tipo de instalação.
- O fator $60$ representa a elevação máxima de temperatura de regime ($90^\circ\text{C} - 30^\circ\text{C} = 60^\circ\text{C}$ para isolação XLPE).

#### 3. Resistência Efetiva Corrigida pela Temperatura ($R_{ca}$)
Fórmula na Coluna `S` da planilha:
$$R_{ca} = \frac{R_{20}}{n_{\text{cabos}}} \times \left[1 + \alpha \times (T - 20)\right]$$
- $\alpha_{\text{Al}} = 0.00393\ ^\circ\text{C}^{-1}$ (Alumínio).
- $\alpha_{\text{Cu}} = 0.00382\ ^\circ\text{C}^{-1}$ (Cobre).
- $R_{20}$: resistência ôhmica em corrente contínua a $20^\circ\text{C}$ ($\Omega/\text{km}$).

#### 4. Queda de Tensão no Trecho ($\Delta V_{\text{trecho}}$)
Fórmula na Coluna `W` da planilha:
$$\Delta V_{\text{trecho}} = k_{\text{fase}} \times S_{\text{acum}} \times \frac{\sqrt{R_{ca}^2 + X^2}}{\frac{V_{\text{nom}}^2}{100}} \times L_{\text{trecho}} \times \frac{V_{\text{mono}}}{100} + \delta_{\text{raiz}} \times (V_{\text{mono}} \times \Delta V\%_{\text{MT+TR}})$$
- $k_{\text{fase}} = 1$ para Trifásico ($3\phi$), $2$ para Bifásico ($2\phi$), $6$ para Monofásico ($1\phi$).
- $L_{\text{trecho}}$: comprimento do vão em quilômetros.
- $\delta_{\text{raiz}} = 1$ no trecho inicial (ligado ao trafo) e $0$ nos trechos subsequentes.

#### 5. Queda de Tensão Acumulada Percentual ($CA\%$)
Fórmula na Coluna `X` da planilha:
$$CA\%_{\text{nó}} = \frac{\Delta V_{\text{acum, V}}}{V_{\text{mono}}} = \frac{\Delta V_{\text{acum, montante}} + \Delta V_{\text{trecho}}}{127}$$

#### 6. Tensões Efetivas no Nó (Colunas `Y` e `Z`)
- **Tensão Monofásica (Fase-Neutro):**
  $$V_{127} = 127 \times (1 - CA\%)$$
- **Tensão Trifásica (Fase-Fase):**
  $$V_{220} = 220 \times (1 - CA\%)$$

### 5.2. Critérios de Bloqueio e Limites Regulamentares (PRODIST / Light)

| Parâmetro | Faixa Adequada | Faixa Precária | Faixa Crítica | Ação de Engenharia |
| :--- | :--- | :--- | :--- | :--- |
| **$V_{127}$ (V)** | $\ge 116.0\text{ V}$ | $110.0\text{ V} \le V < 116.0\text{ V}$ | $< 110.0\text{ V}$ | Violação se $V < 116.0\text{ V}$ |
| **$V_{220}$ (V)** | $\ge 201.0\text{ V}$ | $191.0\text{ V} \le V < 201.0\text{ V}$ | $< 191.0\text{ V}$ | Violação se $V < 201.0\text{ V}$ |
| **$\Delta V\%$ Total** | $\le 8.66\%$ | $8.66\% < \Delta V \le 13.38\%$ | $> 13.38\%$ | Obras de reforço / inversão |
| **Carregamento ($I_b / I_z$)** | $\le 100\%$ | $100\% < Carreg \le 120\%$ | $> 120\%$ | Troca de bitola / remanejamento |

---

## 6. PAPEL DO VBA NO PROJETO REAL

A análise das macros contidas no arquivo `1.QDT\QDT_ZNA855820_PROJ.xlsm` revelou os seguintes procedimentos:
- `Sequencial_Descricao`: Organização e renumeração sequencial de linhas nas tabelas de postes e ramais.
- `Workbook_Open`: Inicialização e dimensionamento de janelas do Excel.
- `DesprotegeVB` / `ProtegeVB`: Gestão de senhas de proteção de células.

**Conclusão Auditada:**  
O código VBA **não executa cálculo elétrico ou física de distribuição**. Toda a lógica de engenharia reside exclusivamente nas **fórmulas declarativas das planilhas**. Portanto, o backend C# do `sisQDT_LIGHT` já substitui 100% da necessidade de VBA, sem perda de regras físicas.

---

## 7. MATRIZ DE PARIDADE E DIVERGÊNCIAS (LEGADO VS sisQDT_LIGHT)

| Domínio Elétrico / Parâmetro | Caso Real (Legado) | Implementação sisQDT_LIGHT | Classificação | Evidência / Localização no Código |
| :--- | :--- | :--- | :--- | :--- |
| **Topologia Radial (Árvore)** | Abas `PROJ1` / `LADO 1` | `ElectricalNetworkGraph` / `Node` / `Edge` | `CONFIRMADO` | `src/QdtCqts.Domain/Topology/` |
| **Corrente Nominal do Trafo** | $I_n = S / (\sqrt{3} \cdot V)$ | `TransformerNominalCurrentRule` | `CONFIRMADO` | `CandidateTransformerNominalCurrentRule.cs` |
| **Corrente de Carga ($I_b$)** | $S / (\sqrt{3} \cdot V_{\text{nom}})$ | `BranchCurrentRule` | `CONFIRMADO` | `CandidateBranchCurrentRule.cs` |
| **Temperatura do Condutor ($T$)** | $T_{\text{amb}} + 60 \cdot (I_b / I_z)^2$ | `ConductorOperatingTemperatureRule` | `CONFIRMADO` | `CandidateConductorOperatingTemperatureRule.cs` |
| **Resistência Térmica ($R_{ca}$)** | $R_{20} \cdot [1 + \alpha(T-20)]$ | `ThermalCorrectedResistanceRule` | `CONFIRMADO` | `CandidateThermalCorrectedResistanceRule.cs` |
| **Queda de Tensão no Trecho** | $\Delta V = k \cdot S \cdot \frac{Z}{V^2/100} \cdot L$ | `BranchVoltageDropPercentageRule` | `CONFIRMADO` | `CandidateBranchVoltageDropPercentageRule.cs` |
| **Queda de MT + Trafo na Raiz** | Soma de $3.50\%$ na entrada BT | `SourceImpedanceVoltageDrop` | `PARCIAL` | Suportado como queda percentual inicial, mas requer parametrização explícita na raiz |
| **Tensões Nodais em Volts ($V_{127}$, $V_{220}$)** | Colunas explícitas `Y` e `Z` ($127 \times (1-CA\%)$) | Exposta como queda acumulada ($\Delta V\%$); falta exposição explícita em Volts | `PARCIAL` | Identificado na Fase 27A; deve ser adicionado aos DTOs de nó |
| **Cenário Atual vs Projetado** | Dois arquivos Excel separados (`_ATUAL` e `_PROJ`) | Rede única em memória | `AUSENTE` | Não há entidade nativa `Scenario` (`Baseline` vs `Projected`) |
| **Rastreabilidade de Coordenadas UTM** | Aba `COORDENADAS` (X, Y métricos) | `NodePosition` (espaço lógico de tela) | `AUSENTE` | As coordenadas físicas UTM reais dos postes não possuem campo no domínio de topologia |

---

## 8. EVIDÊNCIA NUMÉRICA DA OBRA ZNA855820

A auditoria da rede ZNA855820 demonstrou a exata necessidade técnica da intervenção:

1. **Situação ATUAL (Violação Regulatória):**
   - No trecho mais distante do circuito (ponta de rede no poste `P855820-02/12`):
     - Queda Acumulada: **$9.90\%$** (acima do limite de $8.66\%$).
     - Tensão Monofásica: **$114.4\text{ V}$** (abaixo de $116.0\text{ V}$, regime precário/crítico).
     - Carregamento do cabo multiplexado de $35\text{ mm}^2$: $104\%$ (sobrecarga térmica).
2. **Situação PROJETADA (Rede Invertida):**
   - Com a manobra executada no projeto executivo (abertura de chave no ponto crítico e alimentação da ponta via circuito alternativo com condutor $240\text{ mm}^2$ multiplexado):
     - Queda Acumulada Máxima: **$6.96\%$** (conforme, margem de $1.70\%$ abaixo do teto).
     - Tensão Monofásica Mínima: **$118.1\text{ V}$** (plenamente adequada, $> 116.0\text{ V}$).
     - Tensão Trifásica Mínima: **$204.6\text{ V}$** (plenamente adequada, $> 201.0\text{ V}$).
     - Carregamento Máximo: **$78.4\%$** (sem sobrecarga).

---

## 9. AUDITORIA DO UNIFILAR WPF (FASE 27C)

A revisão do `UnifilarDiagramViewModel.cs` e dos contratos visuais revelou:

1. **Eliminação do Anti-Pattern de Entidades Sintéticas:**
   - No código da Fase 27C:
     ```csharp
     SelectedItem = selectedNode ?? (object)selectedEdge ?? selectedTransformer;
     ```
     O fallback `?? new Node(...)` ou `?? new Edge(...)` foi apontado na Fase 27C e precisa ser estritamente bloqueado. A seleção deve ser nula (`null`) quando não houver correspondência, nunca criando objetos fantasmas fora do grafo.
2. **Separação Espaço Lógico vs Espaço Físico:**
   - O algoritmo de layout da Fase 27B/27C calcula posições $(X, Y)$ em uma grade hierárquica (Tree Layout) para legibilidade na tela do operador.
   - **Regra Arquitetural:** Esse espaço lógico de tela **não pode ser enviado ao AutoCAD**. O CAD precisa receber as coordenadas UTM físicas reais dos postes, preservando vãos, distâncias e azimutes de campo.

---

## 10. ARQUITETURA PROPOSTA PARA INTEGRAÇÃO CAD/LISP

### 10.1. O Princípio Semântico: Entidades Reais do AutoCAD

A futura integração rejeita expressamente abordagens como exportação de imagens ou linhas genéricas sem metadados. O AutoCAD deve receber entidades semânticas nativas:
- **Postes / Nós:** `BLOCK` com atributos (ex: `POSTE PROJ`, `POSTE EXISTENTE`) contendo identificador, tipo, esforço nominal e altura, ou `POINT` com bloco anexado.
- **Trechos de Rede:** `LINE` ou `PLINE` contendo no XData ou propriedades técnicas o tipo do condutor, fases, bitola e comprimento real.
- **Transformadores:** `BLOCK` dedicado (`TRAFO RET`) com atributos de potência (kVA), tensões e identificador do equipamento.
- **Callouts / Pranchas:** `MLEADER` (Multileader) apontando diretamente para o poste ou trecho, exibindo os dados de cálculo elétrico ($I_b$, $\Delta V\%$, $V_{127}$, $V_{220}$, carregamento).
- **Layers Padronizados:** Separação estrita em camadas do projeto (ex: `ELETRICA_BT_CONDUTOR`, `ELETRICA_BT_POSTES`, `ELETRICA_BT_TRAFOS`, `ELETRICA_BT_TEXTOS`, `ELETRICA_BT_CALLOUTS`).

### 10.2. Fluxo Conceitual Completo

```
┌────────────────────────┐
│     sisQDT_LIGHT       │
│  (Grafo + Cálculo CQTS)│
└───────────┬────────────┘
            │
            ▼
┌────────────────────────┐
│    CAD Export Model    │◄─── [Coordenadas UTM Reais + Metadados Elétricos]
│  (Contrato DTO Neutro) │
└───────────┬────────────┘
            │
            ▼
┌────────────────────────┐
│     LISP Generator     │
│  (Template + Serializer│
└───────────┬────────────┘
            │
            ▼
┌────────────────────────┐
│   Clipboard / Script   │  ("Copiar para o CAD" -> Ctrl+V no AutoCAD)
└───────────┬────────────┘
            │
            ▼
┌────────────────────────┐
│     AutoCAD CLI        │
│ (Desenha LINE, PLINE,  │
│  BLOCK, MLEADER reais) │
└────────────────────────┘
```

---

## 11. CONTRATO PRELIMINAR: CAD EXPORT MODEL

Abaixo define-se o contrato formal preliminar (C# DTOs) a ser posicionado em `QdtCqts.Application.CadExport` nas fases subsequentes:

```csharp
namespace QdtCqts.Application.CadExport
{
    public sealed record CadExportDocument(
        string ProjectId,
        string ProjectName,
        string CoordinateReferenceSystem, // Ex: "EPSG:31983" (SIRGAS 2000 / UTM Zone 23S)
        string LinearUnit,               // "Meters"
        IReadOnlyList<CadNodeExportModel> Nodes,
        IReadOnlyList<CadEdgeExportModel> Edges,
        IReadOnlyList<CadTransformerExportModel> Transformers,
        IReadOnlyList<CadCalloutExportModel> Callouts,
        CadLayerConfiguration Layers
    );

    public sealed record CadNodeExportModel(
        string NodeId,
        string ExternalKey,               // Identificador de campo (ex: "P855820-01/01")
        string NodeType,                  // "Poste", "Barramento", "Caixa", "Ponta"
        double EastingX,                  // Coordenada UTM X (Metros)
        double NorthingY,                 // Coordenada UTM Y (Metros)
        double? ElevationZ,               // Cota em metros
        double RotationAngleDegrees,      // Orientação/Azimute do poste
        string CircuitId,
        double Voltage127V,
        double Voltage220V,
        double AccumulatedVoltageDropPct,
        bool IsCritical
    );

    public sealed record CadEdgeExportModel(
        string EdgeId,
        string FromNodeId,
        string ToNodeId,
        double LengthMeters,
        double CalculatedAzimuthDegrees,
        string ConductorType,             // Ex: "Multiplexado Alumínio"
        string Phasing,                   // "3F+N", "2F+N", "1F+N"
        string GaugePhases,               // "70 mm²"
        string GaugeNeutral,              // "54.6 mm²"
        double CurrentIb,                 // Amperes
        double AmpacityIz,                // Amperes
        double ThermalCorrectedRca,       // Ohms
        double VoltageDropPct,            // Queda no trecho %
        double LoadingPct,                // Carregamento %
        bool IsOverloaded
    );

    public sealed record CadTransformerExportModel(
        string TransformerId,
        string ExternalKey,
        double EastingX,
        double NorthingY,
        double RatedPowerKva,             // 112.5 kVA
        string PrimaryVoltageKv,          // "13.8 kV"
        string SecondaryVoltageV,         // "220/127 V"
        double NominalCurrentSecondary,   // 295.24 A
        double LoadingPct
    );

    public sealed record CadCalloutExportModel(
        string CalloutId,
        string TargetElementId,
        string TargetElementType,         // "Node", "Edge", "Transformer"
        double InsertionX,
        double InsertionY,
        double LeaderOriginX,
        double LeaderOriginY,
        string Title,
        IReadOnlyList<string> TechnicalLines
    );

    public sealed record CadLayerConfiguration(
        string LayerNodes = "ELETRICA_BT_POSTES",
        string LayerEdges = "ELETRICA_BT_CONDUTOR",
        string LayerTransformers = "ELETRICA_BT_TRAFOS",
        string LayerTexts = "ELETRICA_BT_TEXTOS",
        string LayerCallouts = "ELETRICA_BT_CALLOUTS"
    );
}
```

---

## 12. SISTEMA DE COORDENADAS E GEOREFERENCIAMENTO

Com base na auditoria dos arquivos `CQTS__ZNA855820_PROJ.xlsx` e `REDE_INVERTIDA_ZNA855820.dwg`:
- **Sistema Identificado:** Coordenadas métricas planas UTM Projetadas.
- **Datum:** **SIRGAS 2000** (ou WGS 84, compatível no submétrico).
- **Projeção:** UTM Fuso **23S** (Hemisfério Sul, Meridiano Central 45°W), correspondente ao Estado do Rio de Janeiro / Concessão Light.
- **Código EPSG Oficial:** `EPSG:31983`.
- **Faixas Típicas de Coordenadas:**
  - $X$ (Easting): $\approx 670.000\text{ m}$ a $690.000\text{ m}$.
  - $Y$ (Northing): $\approx 7.460.000\text{ m}$ a $7.520.000\text{ m}$.
- **Decisão Arquitetural:** O `CAD Export Model` transportará obrigatoriamente o código EPSG e a unidade métrica, garantindo que o AutoCAD plote os elementos em suas coordenadas globais reais ou com vetor de translação para inserção por ponto base.

---

## 13. ESTRATÉGIA DE ORIENTAÇÃO DA REDE

Para posicionar anotações, blocos e textos no CAD sem sobreposição:
1. **Azimute do Vão:**
   $$\theta = \text{atan2}(\Delta X, \Delta Y) \times \frac{180}{\pi}$$
2. **Orientação dos Textos:**
   - Textos de trecho devem ser paralelos ao vetor do vão.
   - Aplicação da regra de leitura do AutoCAD: se $90^\circ < \theta \le 270^\circ$, somar $180^\circ$ ao ângulo do texto para evitar textos "de cabeça para baixo".
3. **Offset dos Callouts (MLEADER):**
   - Vetor perpendicular ao trecho ($\theta \pm 90^\circ$) com distância configurável em metros de escala do desenho.

---

## 14. ANÁLISE COMPARATIVA DE FORMATOS DE GERAÇÃO LISP

| Critério | Alternativa A: LISP Totalmente Autocontido | Alternativa B: LISP com Payload Serializado (Recomendada) | Alternativa C: LISP Loader Externo |
| :--- | :--- | :--- | :--- |
| **Descrição** | Rotina emite uma sequência direta de `(command "_.LINE" ...)` e `(command "_.TEXT" ...)` em um bloco único. | Uma biblioteca base de funções LISP genéricas recebe uma lista de dados (payload) e desenha iterativamente. | Um comando curto executa `(load "c:/temp/export.lsp")`. |
| **Facilidade de Uso (UX)** | Excelente (Ctrl+V direto na linha de comando). | Excelente (Ctrl+V direto na linha de comando). | Média (depende de arquivo gravado em disco e permissões). |
| **Manutenibilidade** | Baixa (lógica de desenho misturada com dados do projeto). | **Alta (separação clara entre template de desenho e dados).** | Alta. |
| **Volume de Texto** | Muito alto (repetição massiva de comandos). | **Compacto (reutiliza loops sobre listas associativas).** | Compacto no clipboard. |
| **Risco Operacional** | Buffer overflow em redes com mais de 200 nós se colado de uma vez no console do AutoCAD. | Controlado via quebra de blocos de listas `(setq *data* '(...))`. | Risco de arquivo temporário bloqueado ou caminhos inválidos. |

**Decisão Arquitetural:**  
Adota-se a **Alternativa B (Híbrida com Payload Estruturado)**: o gerador LISP criará uma rotina autocontida dividida em:
1. Definição da função `c:SISQDT_DESENHAR_REDE`;
2. Declaração do dicionário de dados (nós, trechos, callouts);
3. Execução transparente com tratamento de erros `*error*` e restauração de `OSMODE` e `CMDECHO`.

---

## 15. RISCOS TÉCNICOS IDENTIFICADOS

1. **Escala e Estilo de Texto/Bloco no AutoCAD do Usuário:**
   - Se o template padrão (`.dwt`) do cliente usar fontes ou estilos ausentes, textos podem ficar desproporcionais.
   - *Mitigação:* Usar estilos padrão (`Standard`) com altura explícita em metros de escala.
2. **Variável OSMODE (Object Snap):**
   - Ao desenhar via comando AutoLISP, o `OSMODE` ativo pode desviar coordenadas de linhas para vértices vizinhos.
   - *Mitigação:* Desativar `OSMODE` temporariamente durante a execução do script `(setvar "OSMODE" 0)` e restaurar ao término.
3. **Redes Sem Coordenadas UTM Disponíveis:**
   - Em estudos puramente teóricos de QDT/CQTS, pode não haver coordenadas geográficas dos postes.
   - *Mitigação:* Quando $X$ e $Y$ UTM forem nulos, o exportador utilizará o layout hierárquico lógico (derivado do unifilar) como fallback explícito, sinalizado nos metadados.

---

## 16. ITENS QUE DEPENDEM DE TESTE MANUAL FUTURO NO AUTOCAD

1. **Colagem de LISP de Médio Porte no Console:**
   - Testar o comportamento do buffer do AutoCAD 2024/2026 ao receber 1.000 a 3.000 linhas coladas via clipboard.
2. **Renderização de MLEADER:**
   - Validar se o comando `(command "_.MLEADER" ...)` comporta-se de forma homogênea entre diferentes versões do AutoCAD e Civil 3D em português e inglês (uso obrigatório do prefixo `_.`).
3. **Visualização dos Layers:**
   - Confirmar se as cores e espessuras de linha propostas oferecem legibilidade imediata tanto em fundo escuro (Model) quanto em fundo branco (Layout/Paper Space).

---

## 17. PROPOSTA PARA AS PRÓXIMAS FASES

- **Fase 29 — Domínio Espacial e Módulo de Cenários:**
  - Incorporar no modelo de domínio de nós e trechos os campos de coordenadas físicas UTM reais (`EastingX`, `NorthingY`, `EPSG`).
  - Suportar a distinção entre cenários `ATUAL` e `PROJETADO` dentro do mesmo caso de cálculo.
  - Implementar os campos explícitos de tensão nodal nominal ($V_{127}$ e $V_{220}$) nos DTOs de cálculo.
- **Fase 30 — Gerador LISP e Exportador CAD:**
  - Implementar a biblioteca `QdtCqts.Application.CadExport`.
  - Desenvolver os testes unitários do gerador LISP com snapshots do caso ZNA855820.
  - Integrar a ação "Copiar para o CAD" no Unifilar WPF.

---

## 18. CONCLUSÃO DA FASE 28

A Fase 28 conclui com sucesso a engenharia reversa do caso real ZNA855820, comprovando a validade física das fórmulas implementadas no `sisQDT_LIGHT` e fornecendo uma especificação arquitetural robusta, segura e semântica para a futura geração de desenhos no AutoCAD via LISP.
