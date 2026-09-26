# Fase 17 — Cadeia do projeto real

## CQTS

`CQT PROJ 7 - CLANDESTINO - AV PADRE DECAMINADA.xlsm`  
SHA-256: `9315E8AE152C225613988A04568C62749C3D586B569FF900B71584F237FF7762`

## Cadeia fechada

```text
M13 = 74.448 kVA
BX6 = 220 V
H13 = 3 fases
AN13 = 430 A
AP13 = 2 m
AO13 = AN13 * AP13 = 860
        |
        v
P13/BX13 = temperatura do cabo
```

Fórmula reproduzida para o ramo trifásico:

```text
M13 / (BX6 * SQRT(3) / 1000)
/ ((AN13 * AP13) / (90 - 30))
+ 30
```

Resultado nativo: `43.6308370530537`  
Resultado Excel: `43.630837053053675`  
Delta: representação numérica IEEE-754, dentro da comparação de 12 casas decimais.

A regra é específica do ramo real validado; não generaliza automaticamente para outras fases, condutores ou temperaturas-limite.
