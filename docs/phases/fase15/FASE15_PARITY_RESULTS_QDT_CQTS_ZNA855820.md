# Fase 15 — Resultados de parity dos candidatos

| Candidato | Célula | Fórmula | Inputs | Observado | Nativo | Resultado |
|---|---|---|---|---:|---:|---|
| `CQT - ZERADO.xlsm` | `Ramais!C13` | `C11*0.85+C12*0.5268` | 1.0903; 0.4034 Ohm | 1.13926612 | 1.13926612 | `EXACT` |
| `CQT - Light (Robusto).xlsm` | `Ramais!C13` | `C11*0.85+C12*0.5268` | 1.0903; 0.4034 Ohm | 1.13926612 | 1.13926612 | `EXACT` |

Não foi usada tolerância. A convergência é de candidatos atuais, não parity oficial do baseline.

Casos de unidade desconhecida produzem `UNKNOWN`; ausência de input produz `BLOCKED`. O inspetor temporário foi removido após a extração.
