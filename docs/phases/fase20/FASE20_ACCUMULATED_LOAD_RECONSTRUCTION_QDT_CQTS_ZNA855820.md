# Fase 20 — Reconstrução da Potência Acumulada M13

## 1. Identificação de M13
Nos projetos reais analisados (`CQT PROJ 7 REV2` e `CQT PROJ 4 REV1`), a variável `M13` possui cabeçalho formal na linha 10 da aba `LADO 1`:
- **Nome da Coluna:** *"Carga no fim do trecho escolhida"*
- **Unidade:** kVA
- **Valores observados no trecho raiz (`TR, LID`):**
  - CQT PROJ 7: `M13 = 74.448 kVA` (para 47 consumidores acumulados)
  - CQT PROJ 4: `M13 = 75.68658823529411 kVA` (para 48 consumidores acumulados)

## 2. Relação e Precedentes Celulares
A análise das fórmulas da linha 13 e subsequentes revelou a arquitetura de cálculo:
- `C13`: Trecho (`TR7, LID`)
- `D13`: Nº Consumidores no final do trecho ($N_{cons} = 47$)
- `E13`: Carga acumulada total no trecho ($E = 74.448\text{ kVA}$)
- `G13`: Fator de Diversidade ($FDIV = 1$)
- `K13`: Carga com regra de piso operacional:
  ```excel
  IF(OR(C13="",D13="",H13="",E13="",G13="",I13="Erro !",AM13="",AR13=""),"",
     IF(D13>2, E13*G13, IF(D13=2, 8*G13, 4*G13)))
  ```
- `L13`: Carga direta sem piso:
  ```excel
  IF(OR(C13="",D13="",H13="",E13="",G13="",I13="Erro !",AM13="",AR13=""),"", E13*G13)
  ```
- `M13`: Seleção da carga:
  ```excel
  IF($CH$5="SIM", K13, L13)
  ```

## 3. Comportamento Matemático Comprovado
1. Quando $D13 > 2$:
   $$M13 = E13 \times G13$$
2. Quando $D13 = 2$:
   $$M13 = 8 \times G13\text{ kVA (Piso bifásico/trifásico)}$$
3. Quando $D13 = 1$:
   $$M13 = 4 \times G13\text{ kVA (Piso monofásico)}$$

No caso de trechos principais ($D > 2$ e $G = 1$), $M$ é exatamente a carga acumulada $E$.

## 4. Propagação ao Longo do Circuito
Na progressão de jusante do `CQT PROJ 7 REV2`:
| Linha | Trecho | Consumidores ($D$) | Carga ($E$) [kVA] | Carga Escolhida ($M$) [kVA] | Condutor ($AM$) |
|---|---|---|---|---|---|
| 13 | `TR7, LID` | 47 | 74.4480 | 74.4480 | 240 Cu |
| 14 | `LID, P1`  | 38 | 53.2792 | 53.2792 | 185 Al - MX |
| 15 | `P1, P2`   | 36 | 51.0232 | 51.0232 | 185 Al - MX |
| 16 | `P2, P3`   | 16 | 26.9216 | 26.9216 | 185 Al - MX |
| 17 | `P3, P4`   | 15 | 25.4552 | 25.4552 | 185 Al - MX |
| 18 | `P4, P5`   | 14 | 23.9888 | 23.9888 | 185 Al - MX |
| 19 | `P5, P6`   | 12 | 21.0560 | 21.0560 | 185 Al - MX |
| 20 | `P6, P7`   | 4  | 7.2944  | 7.2944  | 70 Al - MX |
| 21 | `P8, RL`   | 1  | 1.8800  | 1.8800  | 16 Al_CONC_Tri |

A carga decresce estritamente em direção às pontas da rede à medida que os nós atendem blocos de consumidores.
