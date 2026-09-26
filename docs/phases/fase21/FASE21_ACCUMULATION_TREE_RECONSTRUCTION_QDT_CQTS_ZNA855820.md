# Fase 21 — Reconstrução da Árvore de Acumulação a Montante

## 1. Topologia Radial e Acumulação em Árvore
A rede de distribuição de Baixa Tensão (BT) do CQTS opera em topologia radial estrita (grafo acíclico orientado em árvore):
- **Raiz da Árvore:** Transformador / Lâmina Chave Seccionadora (`TR` $\rightarrow$ `LID`).
- **Nós (Vértices):** Postes e pontos de derivação (`TopologyNode`).
- **Arestas (Trechos):** Condutores que conectam dois postes adjacentes (`TopologyEdge`).

Cada trecho é orientado a partir da fonte em direção às cargas: $(u \rightarrow v)$, onde $u$ é o nó pai (a montante) e $v$ é o nó filho (a jusante).

---

## 2. Regras Matemáticas de Acumulação

### 2.1 Trecho Linear Simples (Sem Derivação a Jusante)
Se o nó $v$ alimenta apenas um segmento à frente $(v \rightarrow w)$:
$$D(u \rightarrow v) = D(v \rightarrow w) + N_{\text{local}}(v)$$
$$E(u \rightarrow v) = E(v \rightarrow w) + S_{\text{local}}(v)$$

### 2.2 Trecho Terminal (Folha da Árvore)
Se o nó $v$ não possui nenhum trecho a jusante:
$$D(u \rightarrow v) = N_{\text{local}}(v)$$
$$E(u \rightarrow v) = S_{\text{local}}(v)$$

### 2.3 Ponto com Ramificação / Bifurcação
Quando o nó $v$ deriva em múltiplos ramos a jusante $w_1, w_2, \dots, w_k$:
$$D(u \rightarrow v) = N_{\text{local}}(v) + \sum_{i=1}^{k} D(v \rightarrow w_i)$$
$$E(u \rightarrow v) = S_{\text{local}}(v) + \sum_{i=1}^{k} E(v \rightarrow w_i)$$

---

## 3. Comprovação Analítica da Ramificação de `LID` (PROJ 7)

No projeto real `CQT PROJ 7 REV2`:
1. **Ramo 1 (`LID -> P1`, Lado 1):**
   - $D = 38$ consumidores acumulados
   - $E = 53.2792\text{ kVA}$
2. **Ramo 2 (`LID -> P13`, Lado 2):**
   - $D = 8$ consumidores acumulados
   - $E = 14.5136\text{ kVA}$
3. **Carga Própria Local no Nó `LID`:**
   - $N_{\text{local}} = 1$ consumidor
   - $S_{\text{local}} = 6.6552\text{ kVA}$
4. **Tronco Comum a Montante (`TR7 -> LID`):**
   $$D(\text{TR7, LID}) = 38 + 8 + 1 = 47\text{ consumidores}$$
   $$E(\text{TR7, LID}) = 53.2792 + 14.5136 + 6.6552 = \mathbf{74.4480\text{ kVA}}$$

Essa comprovação analítica fecha com precisão exata o valor de $E13 = 74.448\text{ kVA}$ observado na planilha Excel real.

---

## 4. Comprovação Analítica da Ramificação de `LID` (PROJ 4)

No projeto real `CQT PROJ 4 REV1`:
1. **Ramo 1 (`LID -> P1`, Lado 1):**
   - $D = 16$ consumidores
   - $E = 26.9216\text{ kVA}$
2. **Ramo 2 (`LID -> P7`, Lado 2):**
   - $D = 28$ consumidores
   - $E = 41.9616\text{ kVA}$
3. **Carga Própria Local no Nó `LID`:**
   - $N_{\text{local}} = 4$ consumidores
   - $S_{\text{local}} = 6.80338823529411\text{ kVA}$
4. **Tronco Comum a Montante (`TR4 -> LID`):**
   $$D(\text{TR4, LID}) = 16 + 28 + 4 = 48\text{ consumidores}$$
   $$E(\text{TR4, LID}) = 26.9216 + 41.9616 + 6.80338823529411 = \mathbf{75.68658823529411\text{ kVA}}$$

---

## 5. Validação da Fórmula Lógica da Coluna I
A coluna `I` da planilha valida estruturalmente essa hierarquia:
$$\text{Fórmula I13: } = \text{IF}(\text{AND}(H13 \ge H9, D14 \le D13), \text{"OK !"}, \text{"Erro 02"})$$
Isso comprova que o número de consumidores no trecho à frente ($D14$) deve ser estritamente menor ou igual ao trecho a montante ($D13$), assegurando monotonicidade da propagação.
