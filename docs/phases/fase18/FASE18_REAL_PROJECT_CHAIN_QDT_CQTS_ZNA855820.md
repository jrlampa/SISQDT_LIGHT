# Fase 18 — Cadeia real

```text
M13 (kVA)
  + BX6 (V) + H13 (3 fases)
  + AN13 (A, entrada manual/externa)
  + AP13 (m)
        ↓
AO13 = AN13 * AP13
        ↓
CQTS.REAL_PROJECT.CABLE_TEMPERATURE
        ↓
P13/BX13 (°C)
```

A cadeia de temperatura foi validada em dois projetos reais. A origem matemática de AN13 permanece fora do trecho de fórmulas analisado. A regra F15 R/X não participa desta cadeia.
