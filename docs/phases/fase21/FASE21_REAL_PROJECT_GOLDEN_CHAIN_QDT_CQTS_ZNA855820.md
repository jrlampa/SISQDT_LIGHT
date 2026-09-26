# Fase 21 — Cadeia Real Imutável de Casos de Ouro (Golden Chain)

## 1. Golden Case 1: CQT PROJ 7 - REV2
- **Arquivo:** `C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\AV PADRE DECAMINADA - ROBUSTEZ DE BT\CLANDESTINO - AV PADRE DECAMINADA - PARTE 1\OPO-26 - AVENIDA PADRE GUILHERME DECAMINADA_PARTE_1_REV2\1.CQT\CQT PROJ 7 - CLANDESTINO - AV PADRE DECAMINADA.xlsm`
- **Hash SHA-256:** `9315E8AE152C225613988A04568C62749C3D586B569FF900B71584F237FF7762`
- **Topologia do Nó LID:**
  - Ramo `LID, P1`: $D = 38$, $E = 53.2792\text{ kVA}$, $M = 53.2792\text{ kVA}$.
  - Ramo `LID, P13`: $D = 8$, $E = 14.5136\text{ kVA}$, $M = 14.5136\text{ kVA}$.
  - Consumidores Próprios em LID: $N = 1$, $S = 6.6552\text{ kVA}$.
  - Tronco `TR7, LID`:
    - Aba: `LADO 1`
    - Células: `D13 = 47`, `E13 = 74.448 kVA`, `G13 = 1`, `K13 = 74.448 kVA`, `L13 = 74.448 kVA`, `M13 = 74.448 kVA`.
- **Cadeia Reconstruída e Validada:**
  - Acumulação de $E$: $53.2792 + 14.5136 + 6.6552 = 74.4480\text{ kVA}$ (100% de paridade).
  - Seleção de $M$: $74.448 \times 1 = 74.4480\text{ kVA}$ (100% de paridade).
  - Temperatura Térmica do Cabo ($AN13 = 430\text{ A}, AP13 = 2\text{ m}, BX6 = 220\text{ V}$):
    $$T = 43.630837053053675^\circ\text{C} \text{ (100% de paridade bit-a-bit)}$$

---

## 2. Golden Case 2: CQT PROJ 4 - REV1
- **Arquivo:** `C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\AV PADRE DECAMINADA - ROBUSTEZ DE BT\CLANDESTINO - AV PADRE DECAMINADA - PARTE 1\OPO-26 - AVENIDA PADRE GUILHERME DECAMINADA_REV1\1.CQT\CQT PROJ 4 - CLANDESTINO - AV PADRE DECAMINADA.xlsm`
- **Hash SHA-256:** `A0C9D9472B7C6B2B765DA237858AD01703B317356A1C5300180912AB2D2D6E85`
- **Topologia do Nó LID:**
  - Ramo `LID, P1`: $D = 16$, $E = 26.9216\text{ kVA}$, $M = 26.9216\text{ kVA}$.
  - Ramo `LID, P7`: $D = 28$, $E = 41.9616\text{ kVA}$, $M = 41.9616\text{ kVA}$.
  - Consumidores Próprios em LID: $N = 4$, $S = 6.80338823529411\text{ kVA}$.
  - Tronco `TR4, LID`:
    - Aba: `LADO 1`
    - Células: `D13 = 48`, `E13 = 75.68658823529411 kVA`, `G13 = 1`, `M13 = 75.68658823529411 kVA`.
- **Cadeia Reconstruída e Validada:**
  - Acumulação de $E$: $26.9216 + 41.9616 + 6.80338823529411 = 75.68658823529411\text{ kVA}$ (100% de paridade).
  - Seleção de $M$: $75.68658823529411 \times 1 = 75.68658823529411\text{ kVA}$ (100% de paridade).
  - Temperatura Térmica do Cabo ($AN13 = 430\text{ A}, AP13 = 2\text{ m}, BX6 = 220\text{ V}$):
    $$T = 43.857612714067045^\circ\text{C} \text{ (100% de paridade bit-a-bit)}$$

---

## 3. Golden Case 3: QDT_ZNA855820_PROJ
- **Arquivo:** `C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\REDE INVERTIDA\REDE_INVERTIDA_ZNA855820\1.QDT\QDT_ZNA855820_PROJ.xlsm`
- **Valores Observados:**
  - Trecho $P2 \rightarrow P3$: $\Delta D = 2$, $\Delta E = 2.9328\text{ kVA} \implies 1.4664\text{ kVA/cons}$.
  - Trecho $P4 \rightarrow P5$: $\Delta D = 6$, $\Delta E = 8.7984\text{ kVA} \implies 1.4664\text{ kVA/cons}$.
  - Ramal de Ligação `PX, RL`: $D = 1$, $E = 1.88\text{ kVA}$.
  - Tronco Comum `TR, LID`: $D = 78$ consumidores, $E = 95.9176\text{ kVA}$, $M = 95.9176\text{ kVA}$.
