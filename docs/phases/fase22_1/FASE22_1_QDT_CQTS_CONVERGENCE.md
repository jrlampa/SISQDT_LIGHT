# FASE 22.1 — CONVERGÊNCIA QDT × CQTS E TOPOLOGIA UNIFICADA

> **Status:** CONVERGÊNCIA CONFIRMADA  
> **Data:** 2026-09-26  
> **Artefato:** `FASE22_1_QDT_CQTS_CONVERGENCE.md`  

---

## 1. Princípio Unificador

> **A topologia determina quais trechos existem e como as cargas e quedas se propagam; a física matemática determina como cada trecho individual é calculado.**

Não existem "duas físicas elétricas". O QDT e o CQTS convergem para a mesma expressão matemática básica:
$$\Delta V\% = M \cdot C_q \cdot L$$

---

## 2. Convergência no Caso Linear

### 2.1 QDT (Aba `Coeficiente Unitário`)
No QDT histórico, a queda de tensão percentual é formulada através de um coeficiente unitário $C_q$ tabelado por condutor:
$$C_q = \frac{Z}{V^2} \times 100 \quad \left[\frac{\%}{\text{kVA}\cdot\text{m}}\right]$$
onde $Z$ é a impedância do condutor e $V$ é a tensão nominal de linha ($220\text{ V}$).
Para um trecho com demanda $M$ (kVA) e comprimento $L$ (m):
$$\Delta V\%_{\text{QDT}} = M \times C_q \times L$$

### 2.2 CQTS (Coluna `BJ` e `BZ`)
No CQTS:
$$BJ = \frac{\sqrt{R(T)^2 + X^2}}{V^2 / 100} = \frac{Z}{V^2 / 100} = \frac{Z}{V^2} \times 100 \equiv C_q$$
E a fórmula da célula `BZ`:
$$\Delta V\%_{\text{CQTS}} = M \times BJ \times \left(\frac{L}{AP}\right) \times k_{\text{fase}}$$
Quando a topologia for linear ($AP=1$, $k_{\text{fase}}=1$ para $3\phi$):
$$\Delta V\%_{\text{CQTS}} = M \times BJ \times L = M \times C_q \times L \equiv \Delta V\%_{\text{QDT}}$$

### 2.3 Prova por Teste Automatizado
O teste unitário `Test07_LinearTopologyConvergenceCqtsAndQdt` em `IsolatedRuleTests.cs` executa ambas as rotinas independentes e valida paridade estrita até a 10ª casa decimal (`Assert.Equal(qdtDeltaV, (double)cqtsResult.OutputValue!, 10)`).

---

## 3. Comportamento no Caso Ramificado

Quando a rede possui bifurcações (ex.: nó `P2` dividindo em `P3` no `LADO 1` e `P8` no `LADO 3` em `CQT PROJ 7 REV2`):

```text
               ┌── P3 (Lado 1: BZ = 0.3865%, CA = 6.1902%)
... ── P2 ─────┤
 (CA = 5.8036%) └── P8 (Lado 3: BZ = 0.2174%, CA = 6.0210%)
```

1. **Acumulação de Carga a Montante:**
   O trecho a montante da bifurcação (`P1 -> P2`) transporta a soma de todas as cargas alimentadas a jusante:
   $$E(P1 \rightarrow P2) = \text{CargaLocal}(P2) + E(P2 \rightarrow P3) + E(P2 \rightarrow P8)$$
2. **Isolamento de Queda de Tensão:**
   A queda de tensão ao longo do caminho até $P3$ depende **apenas dos ancestrais diretos** ($\text{TR} \rightarrow \text{LID} \rightarrow \text{P1} \rightarrow \text{P2} \rightarrow \text{P3}$):
   $$CA(P3) = CA(P2) + BZ(P2 \rightarrow P3) = 5.8036\% + 0.3865\% = 6.1902\%$$
   A queda de tensão até $P8$ depende apenas de sua própria linhagem:
   $$CA(P8) = CA(P2) + BZ(P2 \rightarrow P8) = 5.8036\% + 0.2174\% = 6.0210\%$$
   **A queda em $P3$ não se soma à queda em $P8$.** Irmãos não somam quedas entre si.

---

## 4. Conclusão

A engine unificada utiliza:
1. Grafo direcionado em árvore para topologia (CQTS);
2. Equação física unificada de queda de tensão ($M \cdot BJ \cdot L_{\text{equiv}} \cdot k_{\text{fase}}$), que coincide com a formulação clássica do QDT no caso em série.
3. Propagação DFS/Topológica a jusante para cálculo dos perfis de tensão nodais ($CA$).
