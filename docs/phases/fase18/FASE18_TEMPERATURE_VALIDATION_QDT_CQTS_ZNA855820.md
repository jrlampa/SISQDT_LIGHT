# Fase 18 — Validação da temperatura

## Projetos reais

| Projeto | M13 | BX6 | H13 | AN13 | AP13 | Excel BX13 | Nativo |
|---|---:|---:|---:|---:|---:|---:|---:|
| CQT PROJ 7 REV2 | 74.448 | 220 | 3 | 430 | 2 | 43.630837053053675 | 43.6308370530537 |
| CQT PROJ 4 REV1 | 75.68658823529411 | 220 | 3 | 430 | 2 | 43.857612714067045 | 43.857612714067 |

Fórmula comum:

```text
M / (V * SQRT(3) / 1000)
/ ((I * L) / (90 - 30))
+ 30
```

Classificação nos dois casos: `NUMERICALLY_REPRODUCED`. A regra permanece candidata, não baseline oficial.
