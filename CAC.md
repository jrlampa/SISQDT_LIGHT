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
- **2.5D:** Os nós da rede contêm coordenadas geográficas bidimensionais projetadas ($X, Y$ em UTM/SIRGAS2000).
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



