# Fase 20 — Cadeia Real e Topologia Ramificada

## 1. Descoberta de Ramificação Real

O projeto real `CQT PROJ 7 REV2` revelou uma rede com **múltiplas ramificações** distribuídas entre as abas `LADO 1`, `LADO 2` e `LADO 3`:

```text
                  [ Transformador TR7 ]
                            │ (D=47, M=74.448 kVA)
                            ▼
                     [ Chave LID ]
                    ╱             ╲
                   ╱ (D=38, 53.28 kVA) ╲ (D=8, 14.51 kVA)
                  ▼                     ▼
             [ Ponto P1 ]          [ Ponto P13 - LADO 2 ]
                  │                     │
             [ Ponto P2 ]          [ Ponto P14 ]
             ╱          ╲               │
   (LADO 1) ╱            ╲ (LADO 3)   [ P18 / Ramal ]
           ▼              ▼
      [ Ponto P3 ]   [ Ponto P8 ]
           │              │
      [ Ponto P7 ]   [ Ponto P12 ]
```

### Análise da Ramificação:
- **Na Chave `LID`:**
  - O trecho tronco `TR7, LID` transporta a carga de **47 consumidores** ($74.448\text{ kVA}$).
  - Ramal 1 (`LID, P1`): atende **38 consumidores** ($53.2792\text{ kVA}$).
  - Ramal 2 (`LID, P13` em `LADO 2`): atende **8 consumidores** ($14.5136\text{ kVA}$).
  - Consumidor local em LID: 1 consumidor ($6.6552\text{ kVA}$).
  - Total: $38 + 8 + 1 = 47$ consumidores.
- **No Ponto `P2`:**
  - `P1, P2` transporta a carga de **36 consumidores** ($51.0232\text{ kVA}$).
  - Ramal `LADO 1` (`P2, P3`): atende **16 consumidores** ($26.9216\text{ kVA}$).
  - Ramal `LADO 3` (`P2, P8`): atende outros **16 consumidores** ($26.9216\text{ kVA}$).
  - Consumidores locais no poste P2: 4 consumidores.
  - Total: $16 + 16 + 4 = 36$ consumidores.

## 2. Conexão Completa da Cadeia Física

Com a Carga Escolhida $M$ formalizada, a cadeia física do CQTS até a temperatura final do cabo está fechada:

```text
Consumidores a Jusante (D) + Carga Acumulada (E) + Fator Diversidade (G)
                       ↓
         [ Regra de Seleção de Carga ]
      CQTS.REAL_PROJECT.END_LOAD_SELECTION
                       ↓
        Carga Escolhida no Trecho (M)
                       ↓
   [ Regra Térmica de Temperatura de Cabo ]
     CQTS.REAL_PROJECT.CABLE_TEMPERATURE
     (usando Tensão BX6, Ampacidade AN13 e Comprimento AP13)
                       ↓
        Temperatura Final do Cabo (BX13)
```

## 3. Paridade Comprovada

A conexão foi reproduzida em C# com **zero desvio**:
- `PROJ 7`: $D=47, E=74.448 \rightarrow M=74.448\text{ kVA} \rightarrow T = 43.6308\text{ °C}$
- `PROJ 4`: $D=48, E=75.686588... \rightarrow M=75.686588...\text{ kVA} \rightarrow T = 43.8576\text{ °C}$
