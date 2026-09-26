# Fase 19 — Cadeia Matemática em Projeto Real

## 1. Topologia da Cadeia Reconstruída

Com a semântica de `AN13` esclarecida, a cadeia de cálculo térmico observada nos projetos reais `CQT PROJ 7 REV2` e `CQT PROJ 4 REV1` conecta os seguintes parâmetros:

```text
[Entradas de Projeto / Catálogo]
M13  = Potência de Carga (kVA)
BX6  = Tensão de Fase/Linha (220 V)
H13  = Número de Fases (3)
AN13 = Ampacidade Admissível do Cabo (430 A)
AP13 = Comprimento do Trecho (2 m)

        ↓
[Cálculo Intermediário]
AO13 = AN13 * AP13 = 430 * 2 = 860 A.m

        ↓
[Cálculo Térmico do Condutor]
Elevação de Temperatura:
ΔT = [ M13 / (BX6 * √3 / 1000) ] / [ AO13 / (90 - 30) ]

Temperatura Final:
T_cabo = ΔT + 30 [°C]
Exibida em BX13 / P13
```

## 2. Parâmetros Comparados

| Projeto | M13 (kVA) | BX6 (V) | Fases | AN13 (A) | AP13 (m) | AO13 (A.m) | T_cabo (°C) |
|---|---|---|---|---|---|---|---|
| **CQT PROJ 7 REV2** | 74.448 | 220 | 3 | 430 | 2.0 | 860.0 | **43.6308** |
| **CQT PROJ 4 REV1** | 75.6866 | 220 | 3 | 430 | 2.0 | 860.0 | **43.8576** |

## 3. Isolamento da Regra F15 RX

A regra candidata `QDT.RAMAL.CANDIDATE_RX_COMBINATION` (Fase 15) calcula impedâncias de ramal ($R/X$) e não possui qualquer relação direta ou dependência de dados com a cadeia térmica de condutores de média/baixa subterrânea do CQTS. Ambas permanecem independentes.
