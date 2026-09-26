# Fase 22 — Reconstrução Matemática da Queda de Tensão no CQTS

## 1. Origem da Fórmula no Excel

A investigação reversa das planilhas reais `CQT PROJ 7 REV2`, `CQT PROJ 4 REV1` e `QDT_ZNA855820_PROJ` revelou as seguintes fórmulas das colunas `BZ` (Queda de Tensão no Trecho) e `CA` (Queda de Tensão Acumulada):

### 1.1 Queda de Tensão no Trecho (`BZ13`)
$$\text{Fórmula: } = \text{IF}(\text{condição}, \text{""}, \text{IF}(H=3, BW \cdot BJ \cdot AR, \text{IF}(H=2, BW \cdot BJ \cdot AR \cdot 2, \text{IF}(H=1, BW \cdot BJ \cdot AR \cdot 6, 0))))$$

Onde:
- $BW = M$ (Potência no fim do trecho em kVA);
- $AR = L_{\text{equiv}} = \frac{AQ}{AP}$ (Comprimento em metros dividido pelo número de cabos em paralelo);
- $H$ é o número de fases ($3, 2, 1$);
- $BJ$ é o **fator unitário de queda de tensão**:
  $$BJ = \frac{\sqrt{R(T)^2 + X^2}}{V^2 / 100}$$

### 1.2 Parâmetros Físicos Envolvidos
- $V = 220\text{ V}$ (Tensão nominal entre fases da BT);
- $X$: Reatância indutiva do condutor em $\Omega/\text{km}$ (coluna `BB`);
- $R(T)$: Resistência do condutor na temperatura calculada $T_{\text{cabo}}$ em regime contínuo (coluna `BI`):
  $$R(T) = R_{20} \cdot [1 + \alpha_{20} \cdot (T_{\text{cabo}} - 20)] \cdot k_{\text{skin}}$$
  com $k_{\text{skin}} = \frac{R_{90,\text{catálogo}}}{R_{90,\text{calc}}}$;
- $Z = \sqrt{R(T)^2 + X^2}$ em $\Omega/\text{km}$.

### 1.3 Equação Física Unificada
$$\Delta V\%_{\text{trecho}} = \frac{M \cdot Z \cdot L_{\text{equiv}}}{V^2 / 100} \cdot k_{\text{fase}}$$
com $k_{\text{fase}} = \begin{cases} 1, & 3\phi \\ 2, & 2\phi \\ 6, & 1\phi \end{cases}$

---

## 2. Acumulação ao Longo dos Caminhos da Árvore (`CA`)

### 2.1 Ponto Inicial (`LID` — Linha 13)
$$CA13 = BZ13 + BV4 + CV105$$
Onde:
- $CV105 = \Delta V\%_{\text{MT}}$: Queda de tensão da Média Tensão refletida no secundário ($1.83306\%$);
- $BV4 = \Delta V\%_{\text{trafo}}$: Queda de tensão interna no transformador sob carga total:
  $$BV4 = \frac{M_{\text{trafo}}}{S_{\text{trafo}}} \cdot Z\%_{\text{trafo}} = \frac{74.448}{112.5} \cdot 3.5\% = 2.31616\%$$
- $BZ13$: Queda do trecho inicial $TR7 \rightarrow LID$ ($0.03881\%$);
- Queda acumulada inicial em `LID`:
  $$CA13 = 1.83306\% + 2.31616\% + 0.03881\% = 4.18803\%$$

### 2.2 Nós a Jusante (Linhas 14+)
Para qualquer nó $v$ alimentado a partir de seu nó pai $u$:
$$CA(v) = CA(u) + BZ(u \rightarrow v)$$
Exemplos comprovados em `CQT PROJ 7 REV2`:
- $P1$: $CA(P1) = CA(LID) + BZ(LID \rightarrow P1) = 4.18803 + 0.86241 = 5.05045\%$
- $P2$: $CA(P2) = CA(P1) + BZ(P1 \rightarrow P2) = 5.05045 + 0.75317 = 5.80361\%$
- $P3$ (Lado 1): $CA(P3) = CA(P2) + BZ(P2 \rightarrow P3) = 5.80361 + 0.38654 = 6.19015\%$
- $P8$ (Lado 3): $CA(P8) = CA(P2) + BZ(P2 \rightarrow P8) = 5.80361 + 0.21743 = 6.02104\%$

Essa formulação garante que a queda em cada nó reflete estritamente o somatório do caminho raiz $\rightarrow$ nó, sem interferência cruzada entre ramos irmãos.
