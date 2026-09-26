# CAC — Context Architecture & Contracts

> **Conhecimento Arquitetural Compartilhado e Contratos de Componentes**  
> **Sistema:** SISQDT_LIGHT (QdtCqts)  
> **Status:** Ativo / Governança Técnica Estrita  
> **Data:** 2026-09-26  

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
└────────────────────────────┬─────────────────────────────┘
                             │ consome
┌────────────────────────────▼─────────────────────────────┐
│                   QdtCqts.Application                    │
│      ProjectService, CalculationService, ParityService   │
└──────────────┬─────────────────────────────┬─────────────┘
               │ orquestra                   │ invoca
┌──────────────▼─────────────┐ ┌─────────────▼─────────────┐
│      QdtCqts.Domain        │ │ Calculation.Abstractions  │
│ NetworkModel, Circuit,     │ │ ICalculationEngine        │
│ Node, Edge, Load, Conductor│ │ CalculationRequest/Result │
└──────────────▲─────────────┘ └─────────────┬─────────────┘
               │ persiste                    │ implementa
┌──────────────┴─────────────┐ ┌─────────────▼─────────────┐
│ Infrastructure.Sqlite      │ │ Calculation.Qdt           │
│ Repositórios, DDL, WAL     │ │ Calculation.Cqts          │
└────────────────────────────┘ └───────────────────────────┘
               ▲
               │ importa/valida
┌──────────────┴─────────────┐ ┌───────────────────────────┐
│ Infrastructure.ExcelEvid.  │ │ Infrastructure.Parity     │
│ OpenXML Reader, Precedence │ │ Comparador Numérico P0-P4 │
└────────────────────────────┘ └───────────────────────────┘
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

