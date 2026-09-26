# Fase 21 — Reconstrução das Cargas Terminais dos Consumidores

## 1. Contexto e Objetivo
A Fase 20 estabeleceu que a carga escolhida no fim do trecho ($M$) é obtida diretamente da carga acumulada ($E$) e do fator de diversidade ($G$), com regras de piso quando $CH5 = \text{"SIM"}$ e $D \le 2$:
$$M = \begin{cases} E \times G, & \text{se } D > 2 \text{ ou } CH5 = \text{"NÃO"} \\ 8 \times G, & \text{se } D = 2 \text{ e } CH5 = \text{"SIM"} \\ 4 \times G, & \text{se } D = 1 \text{ e } CH5 = \text{"SIM"} \end{cases}$$

O objetivo desta etapa foi determinar a origem exata das cargas individuais de cada consumidor e como elas nascem antes de entrar na agregação do trecho.

---

## 2. Evidência nos Projetos Reais

A investigação analítica reversa nos arquivos de projeto reais da Light (`CQT PROJ 7 REV2`, `CQT PROJ 4 REV1` e `QDT_ZNA855820_PROJ`) comprovou a existência de três grandezas unitárias de demanda de consumidor perfeitamente reproduzidas bit-a-bit:

### 2.1 Carga Unitária Padrão Residencial / Clandestino
- **Valor Unitário:** $1.4664\text{ kVA}$ ($1466.4\text{ VA}$)
- **Comprovação em `CQT PROJ 7 REV2`:**
  - Trecho $P3 \rightarrow P4$: $\Delta D = 1$, $\Delta E = 1.4664\text{ kVA}$ (1 consumidor).
  - Trecho $P4 \rightarrow P5$: $\Delta D = 1$, $\Delta E = 1.4664\text{ kVA}$ (1 consumidor).
  - Trecho $P5 \rightarrow P6$: $\Delta D = 2$, $\Delta E = 2.9328\text{ kVA} = 2 \times 1.4664\text{ kVA}$ (2 consumidores).
  - Trecho $P8 \rightarrow P9$ (Lado 3): $\Delta D = 3$, $\Delta E = 4.3992\text{ kVA} = 3 \times 1.4664\text{ kVA}$ (3 consumidores).
- **Comprovação em `CQT PROJ 4 REV1`:**
  - Trecho $P1 \rightarrow P2$: $\Delta D = 3$, $\Delta E = 4.3992\text{ kVA} = 3 \times 1.4664\text{ kVA}$ (3 consumidores).
  - Trecho $P2 \rightarrow P3$: $\Delta D = 3$, $\Delta E = 4.3992\text{ kVA} = 3 \times 1.4664\text{ kVA}$ (3 consumidores).
  - Trecho $P9 \rightarrow P10$: $\Delta D = 4$, $\Delta E = 5.8656\text{ kVA} = 4 \times 1.4664\text{ kVA}$ (4 consumidores).
- **Comprovação em `QDT_ZNA855820_PROJ`:**
  - Trecho $P2 \rightarrow P3$: $\Delta D = 2$, $\Delta E = 2.9328\text{ kVA} = 2 \times 1.4664\text{ kVA}$ (2 consumidores).
  - Trecho $P4 \rightarrow P5$: $\Delta D = 6$, $\Delta E = 8.7984\text{ kVA} = 6 \times 1.4664\text{ kVA}$ (6 consumidores).

### 2.2 Carga Unitária Diferenciada / Comercial / Maior Porte
- **Valor Unitário:** $1.8048\text{ kVA}$ ($1804.8\text{ VA}$)
- **Comprovação em `CQT PROJ 7 REV2`:**
  - Trecho $P14 \rightarrow P15$ (Lado 2): $\Delta D = 2$, $\Delta E = 3.6096\text{ kVA} = 2 \times 1.8048\text{ kVA}$.
  - Trecho $P15 \rightarrow P16$ (Lado 2): $\Delta D = 1$, $\Delta E = 1.8048\text{ kVA}$.
  - Trecho $P16 \rightarrow P17$ (Lado 2): $\Delta D = 1$, $\Delta E = 1.8048\text{ kVA}$.
  - Trecho $P10 \rightarrow P11$ (Lado 3): $\Delta D = 2$, $\Delta E = 3.6096\text{ kVA} = 2 \times 1.8048\text{ kVA}$.
- **Comprovação em `CQT PROJ 4 REV1`:**
  - Trecho $P3 \rightarrow P4$ (Lado 1): $\Delta D = 4$, $\Delta E = 7.2192\text{ kVA} = 4 \times 1.8048\text{ kVA}$.
  - Trecho $P12 \rightarrow P13$ (Lado 2): $\Delta D = 4$, $\Delta E = 7.2192\text{ kVA} = 4 \times 1.8048\text{ kVA}$.

### 2.3 Carga Terminal de Ramal de Ligação (RL)
- **Valor Unitário:** $1.88\text{ kVA}$ ($1880\text{ VA}$)
- **Comprovação Universal nos Projetos:**
  - Aparece de forma canônica nas linhas de ramal pontual terminal em todas as abas:
    - PROJ 7 Lado 1 (Linha 21): $D = 1, E = 1.88\text{ kVA}$.
    - PROJ 7 Lado 2 (Linha 20): $D = 1, E = 1.88\text{ kVA}$.
    - PROJ 7 Lado 3 (Linha 21): $D = 1, E = 1.88\text{ kVA}$.
    - PROJ 4 Lado 1 (Linha 19): $D = 1, E = 1.88\text{ kVA}$.
    - PROJ 4 Lado 2 (Linha 23): $D = 1, E = 1.88\text{ kVA}$.
    - ZNA855820 Lado 1 (Linha 19): $D = 1, E = 1.88\text{ kVA}$.
    - ZNA855820 Lado 2 (Linha 18): $D = 1, E = 1.88\text{ kVA}$.
- **Composição da Linha Terminal de Rede:**
  - Nos finais de rede atendendo 4 consumidores:
    $$E_{\text{terminal}} = 1.88 + 3 \times 1.8048 = 7.2944\text{ kVA}$$
    Identificado com igualdade exata de 10 dígitos decimais nos múltiplos projetos reais.

---

## 3. Modelo Matemático da Carga no Ponto
Para um ponto (poste) $v$, a carga local agregada de consumidores diretamente conectados é:
$$S_{\text{local}}(v) = \sum_{g} N_{g}(v) \cdot S_{\text{unit}, g}$$
onde $N_g(v)$ é o número de consumidores da classe $g$ conectados ao poste $v$, e $S_{\text{unit}, g}$ é a demanda unitária daquela classe.
