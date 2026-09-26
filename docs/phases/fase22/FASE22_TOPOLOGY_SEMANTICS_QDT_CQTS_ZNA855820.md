# Fase 22 — Semântica Topológica Unificada (QDT + CQTS)

## 1. Princípio Arquitetural
A topologia determina **quais trechos existem e como estão interconectados**; a matemática determina **como cada trecho e nó são calculados**. O motor não deve construir dois sistemas duplicados, mas uma única representação unificada baseada em grafo direcionado acíclico (Árvore Radial) que comporta tanto a linearidade histórica do QDT quanto as ramificações e subderivações ricas do CQTS.

---

## 2. Semântica das Entidades e Atributos de Domínio

### 2.1 TR (Transformador)
- **Definição:** Raiz absoluta da árvore elétrica na Baixa Tensão (BT). Não possui nó montante no secundário.
- **Atributos:** Potência nominal ($S_{\text{trafo}}$ em kVA), impedância percentual ($Z\%_{\text{trafo}}$), tensão nominal secundária ($V_{\text{nominal}} = 220\text{ V}$).

### 2.2 LID (Lâmina / Chave Seccionadora / Taqueamento)
- **Definição:** Ponto de transição e manobra inicial na saída física do transformador.
- **Função Topológica:** Primeiro nó da rede BT após o trafo (`TR -> LID`). A partir de `LID`, a rede se bifurca nos circuitos principais (alimentadores / lados).

### 2.3 PONTO (Poste / Nó)
- **Definição:** Posicionamento físico da infraestrutura de suporte da rede (`TopologyNode`).
- **Atributos:** Identificador único (`P1`, `P2`, `P3`), coordenadas georreferenciadas 2.5D (UTM $X, Y$, cota $Z$ como metadado Half-way BIM) e lista de consumidores locais diretamente conectados ($N_{\text{local}}, S_{\text{local}}$).

### 2.4 TRECHO (Aresta Orientada)
- **Definição:** Conexão condutora entre dois nós adjacentes (`TopologyEdge` orientado de $u \rightarrow v$).
- **Atributos:**
  - `SourceNodeId` ($u$, a montante);
  - `TargetNodeId` ($v$, a jusante);
  - `ConductorKey` (coluna `AM`, ex: `185 Al - MX`, `240 Cu`);
  - `LengthMeters` (coluna `AQ`, $L$ em metros);
  - `ParallelCables` (coluna `AP`, número de cabos em paralelo por fase);
  - `EffectiveLength` (coluna `AR`, $L_{\text{equiv}} = L / AP$);
  - `PhaseCount` (coluna `H`, $3\phi, 2\phi, 1\phi$);
  - `Ampacity` (coluna `AN`, ampacidade de catálogo $I_z$);
  - `AccumulatedLoad` (coluna `E`, carga acumulada em kVA);
  - `SelectedLoad` (coluna `M`, carga escolhida com diversidade e pisos);
  - `SegmentVoltageDrop` (coluna `BZ`, queda percentual $\Delta V\%$ no trecho).

### 2.5 MONTANTE
- **Definição Operacional Comprovada:** O nó imediatamente anterior em direção à fonte (Pai no grafo direcionado).
- **Notação de Trecho:** Representado no formato `FROM, TO` na coluna `C` (ex: `TR7, LID`, `LID, P1`, `P1, P2`, `P2, P3`, `P2, P8`).
- **Regra:** Cada trecho possui exatamente um nó pai (montante) e um nó filho (jusante).

### 2.6 RL (Ramal de Ligação)
- **Definição:** Terminal de carga que conecta o poste da rede pública ao padrão de entrada do consumidor final mais desfavorável.
- **Papel no Cálculo:** Não é modelado como um poste convencional da infraestrutura viária, mas como um nó terminal de verificação normativa de queda de tensão até o medidor do cliente.
- **Validação:** Carga padrão de $1.88\text{ kVA}$ para ramal monofásico individual.

### 2.7 AM, AP e AN
- `AM`: Identificação do condutor do trecho (tipo de metal e seção transversal).
- `AP`: Número de condutores por fase no banco (cabos em paralelo).
- `AN`: Ampacidade admissível do condutor ($I_z$), fixada conforme norma de condutores subterrâneos/aéreos da concessionária.
