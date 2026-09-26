# Fase 22 — Cadeia Real Imutável de Casos de Ouro (Golden Chain)

## 1. Golden Case 1: CQT PROJ 7 REV2
- **Arquivo:** `C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\AV PADRE DECAMINADA - ROBUSTEZ DE BT\CLANDESTINO - AV PADRE DECAMINADA - PARTE 1\OPO-26 - AVENIDA PADRE GUILHERME DECAMINADA_PARTE_1_REV2\1.CQT\CQT PROJ 7 - CLANDESTINO - AV PADRE DECAMINADA.xlsm`
- **Hash SHA-256:** `9315E8AE152C225613988A04568C62749C3D586B569FF900B71584F237FF7762`
- **Valores Observados de Queda de Tensão no Trecho (`BZ`) e Acumulada (`CA`):**
  - Linha 13 (`TR7, LID`): $BZ13 = 0.038810072\%$, $CA13 = 4.188033012\%$
  - Linha 14 (`LID, P1`): $BZ14 = 0.862413217\%$, $CA14 = 5.050446229\%$
  - Linha 15 (`P1, P2`): $BZ15 = 0.753167057\%$, $CA15 = 5.803613286\%$
  - Linha 16 (`P2, P3` - Lado 1): $BZ16 = 0.386537935\%$, $CA16 = 6.190151221\%$
  - Linha 16 (`P2, P8` - Lado 3): $BZ16 = 0.217427588\%$, $CA16 = 6.021040874\%$
  - Linha 21 (`P8, RL`): $BZ21 = 0.260070018\%$, $CA21 = 7.811063301\%$
- **Cadeia Reconstruída:**
  - $M13 \rightarrow BZ13 \rightarrow CA13 \rightarrow CA14 \rightarrow CA15 \rightarrow CA16$:
  - Paridade de 10 a 16 dígitos com os valores gravados no Excel.

---

## 2. Golden Case 2: CQT PROJ 4 REV1
- **Arquivo:** `C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\AV PADRE DECAMINADA - ROBUSTEZ DE BT\CLANDESTINO - AV PADRE DECAMINADA - PARTE 1\OPO-26 - AVENIDA PADRE GUILHERME DECAMINADA_REV1\1.CQT\CQT PROJ 4 - CLANDESTINO - AV PADRE DECAMINADA.xlsm`
- **Hash SHA-256:** `A0C9D9472B7C6B2B765DA237858AD01703B317356A1C5300180912AB2D2D6E85`
- **Valores Observados:**
  - $TR4 \rightarrow LID$: $M13 = 75.686588235294\text{ kVA}$, $D13 = 48$.
  - Ramo 1 (`LID, P1`): $M14 = 26.9216\text{ kVA}$, $D14 = 16$.
  - Ramo 2 (`LID, P7`): $M14 = 41.9616\text{ kVA}$, $D14 = 28$.
  - Ramal de Ligação `P5, RL`: $D = 1$, $M = 1.88\text{ kVA}$.
- **Cadeia Reconstruída:**
  - Topologia com ramificação dupla em `LID` confirmada e consistente com o modelo em árvore.

---

## 3. Golden Case 3: QDT_ZNA855820_PROJ
- **Arquivo:** `C:\Users\jonat\OneDrive - IM3 Brasil\LIGHT\PROJETOS\REDE INVERTIDA\REDE_INVERTIDA_ZNA855820\1.QDT\QDT_ZNA855820_PROJ.xlsm`
- **Valores Observados:**
  - Mesma estrutura de cabeçalho, colunas e formulação de queda de tensão da família CQT/QDT unificada.
