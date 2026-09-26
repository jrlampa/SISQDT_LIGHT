# Fase 22 — Convergência Conceitual e Matemática QDT × CQTS

## 1. Premissa Unificada
A premissa central é que **QDT e CQTS não são dois modelos físicos divergentes**, mas instâncias do mesmo modelo matemático elétrico fundamental:
- **QDT Histórico:** Representa circuitos de BT organizados em ramos lineares clássicos (`LADO 1`, `LADO 2`), com coeficientes de queda ($C_q$) pré-tabelados em temperatura estática.
- **CQTS:** Representa a topologia em árvore direcionada real (com derivações, subderivações e cargas locais nos nós intermediários), corrigindo dinamicamente a resistência para a temperatura de regime contínuo ($T_{\text{cabo}}$).

---

## 2. Equivalência Física do Coeficiente Unitário de Queda
Na aba `Coeficiente Unitário` (presente em todos os workbooks de QDT e CQTS):
$$C_q = \frac{\sqrt{R^2 + X^2}}{V^2} \times 10000 = \frac{Z}{V^2 / 100}$$
Na coluna `BJ` do CQTS:
$$BJ = \frac{\sqrt{R(T)^2 + X^2}}{V^2 / 100}$$

As duas formulações são **identicamente equivalentes**:
$$\Delta V\% = M \cdot C_q \cdot L$$
No CQTS, a única extensão técnica é que $R$ é avaliado na temperatura de operação calculada ($T_{\text{cabo}}$), enquanto no QDT simplificado $R$ é tabelado a 20°C ou 70°C.

---

## 3. Demonstração dos Dois Casos

### 3.1 Caso Linear (`TR -> LID -> P1 -> P2 -> P3`)
Quando não há bifurcações a jusante:
1. **Acumulação de Carga:**
   $$E(P_i \rightarrow P_{i+1}) = \sum_{k=i+1}^{n} S_{\text{local}}(P_k)$$
   A carga acumulada do CQTS coincide com a soma acumulada de jusante do QDT.
2. **Queda de Tensão:**
   $$\Delta V\%_{\text{acumulado}}(P_n) = \Delta V\%_{\text{inicial}} + \sum_{i=1}^{n-1} \Delta V\%_{\text{trecho}}(P_i \rightarrow P_{i+1})$$
   A queda acumulada no terminal é exatamente a soma sequencial de cada vão, validando a paridade 1:1 entre o motor em árvore e a tabela linear.

### 3.2 Caso com Ramificação
```text
          ┌── P1 ── ... (Lado 1)
TR ── LID ─┤
          └── P13 ── ... (Lado 2)
```
1. **No Trecho Tronco (`TR -> LID`):**
   - Transporta a soma das cargas de todos os ramos a jusante mais a carga local de `LID`.
   - $M_{\text{tronco}}$ reflete a demanda total do transformador.
2. **Nos Ramos Filhos (`LID -> P1` e `LID -> P13`):**
   - Cada ramo transporta apenas a sua subárvore correspondente.
   - A queda de tensão não se soma entre ramos irmãos. O ramo $P1$ acumula a partir de $LID$, e o ramo $P13$ acumula a partir de $LID$ de forma estritamente independente.

---

## 4. Matriz de Convergência

| Grandeza | QDT Clássico | CQTS Topológico | Motor SISQDT_LIGHT Unificado |
| :--- | :--- | :--- | :--- |
| **Topologia** | Linear bipartida | Árvore direcionada | Grafo em árvore com suporte a linhas simples e derivações |
| **Carga Acumulada** | Linear | Recursiva em árvore | $E = S_{\text{local}} + \sum E_{\text{filhos}}$ |
| **Carga Escolhida $M$**| Carga de trecho | Regra com pisos regulatórios | `CandidateEndLoadSelectionRule` |
| **Temperatura Condutor**| Não considerada | Regime contínuo | `CandidateCableTemperatureRule` |
| **Queda de Trecho** | $M \cdot C_q \cdot L$ | $M \cdot BJ \cdot L_{\text{equiv}} \cdot k_{\text{fase}}$ | `CandidateSegmentVoltageDropRule` |
| **Queda Acumulada** | Soma sequencial | Soma pelo caminho raiz-nó | `CandidateAccumulatedVoltageDropRule` |
