# FASE 22.1 — UNIDADES, VARIÁVEIS E ANÁLISE DIMENSIONAL DEFINITIVA

> **Status:** FECHAMENTO DEFINITIVO  
> **Data:** 2026-09-26  
> **Artefato:** `FASE22_1_UNITS_AND_VARIABLES.md`  

---

## 1. Dicionário Definitivo das Colunas CQTS

| Coluna | Cabeçalho Linha 10/11/12 | Significado Físico / Engenharia | Unidade |
| :---: | :---: | :---: | :---: |
| **AM** | `Cabo de B.T. / Seção [mm²]` | Chave do condutor na tabela de cabos | Texto |
| **AN** | `Corrente do cabo para a condição` | Ampacidade nominal corrigida de catálogo para 1 condutor ($I_z$) | A |
| **AO** | `=AN*AP` | Ampacidade total da fase com condutores paralelos ($I_{z,\text{total}}$) | A |
| **AP** | `Nº de circuito no trecho` | Quantidade de cabos/circuitos em paralelo por fase | Adimensional ($n_{\text{paralelo}}$) |
| **AQ** | `Trecho` | Comprimento real do trecho | **metros (m)** |
| **AR** | `=AQ/AP` | Comprimento equivalente do trecho ($L_{\text{equiv}}$) | **metros (m)** |
| **AT** | `Rcc fase [ W / km ] 20°C` | Resistência em Corrente Contínua a 20°C | $\Omega/\text{km}$ |
| **AW** | `a` | Coeficiente térmico da resistência ($\alpha_{20}$) | $1/^\circ\text{C}$ ($0.00403\text{ Al} / 0.00393\text{ Cu}$) |
| **AX** | `Rcc fase [ W / km ] 90°C` | Resistência CC corrigida a 90°C (ou 70°C para PVC) | $\Omega/\text{km}$ |
| **AY** | `Rca fase [ W / km ] 90°C` | Resistência CA de catálogo a 90°C | $\Omega/\text{km}$ |
| **BB** | `XL fase [ W / km ]` | Reatância indutiva da fase | $\Omega/\text{km}$ |
| **BE** | `K*` | Fator de efeito pelicular/proximidade ($AY / AX$) | Adimensional |
| **BI** | `Rca fase 1 [ W / km ]` | Resistência CA calculada na temperatura $T_{\text{cabo}}$ | $\Omega/\text{km}$ |
| **BJ** | `K 1` | Coeficiente unitário de queda de tensão ($\frac{Z}{V^2/100}$) | $(\Omega/\text{km}) / \text{V}^2$ |
| **BW** | `Carga no fim do trecho [ kVA ]` | Potência aparente adotada no trecho ($M$) | **kVA** |
| **BX** | `Temp. do Cabo [ °C ]` | Temperatura de regime contínuo do cabo ($T_{\text{cabo}}$ de $P$) | $^\circ\text{C}$ |
| **BZ** | `Queda de Tensão % Trecho` | Queda de tensão percentual no trecho individual | **%** |
| **CA** | `Queda de Tensão % Acumulado` | Queda de tensão percentual acumulada desde a MT/trafo | **%** |

---

## 2. Prova do Cancelamento Dimensional na Queda de Tensão

A fórmula na célula `BZ` é:
$$\Delta V\%_{\text{trecho}} = \text{BW} \times \text{BJ} \times \text{AR} \times k_{\text{fase}}$$

Onde:
$$\text{BJ} = \frac{\sqrt{\text{BI}^2 + \text{BB}^2}}{V^2 / 100}$$

### Dedução Física e Cancelamento:
1. Queda de tensão de linha em Volts para sistema trifásico equilibrado:
   $$\Delta V_{\text{linha}} = \sqrt{3} \cdot I_b \cdot Z_{\text{trecho}}$$
2. Corrente de carga $I_b$ a partir de $M$ em kVA ($M \times 10^3\text{ VA}$):
   $$I_b = \frac{M_{\text{kVA}} \times 10^3}{\sqrt{3} \cdot V}$$
3. Impedância do trecho em Ohms a partir de $Z$ em $\Omega/\text{km}$ e $L_{\text{equiv}}$ em metros ($L_{\text{equiv}} \times 10^{-3}\text{ km}$):
   $$Z_{\text{trecho}} = Z_{\Omega/\text{km}} \cdot \left(L_{\text{equiv}} \times 10^{-3}\right)$$
4. Substituição direta:
   $$\Delta V_{\text{linha}} = \sqrt{3} \cdot \left(\frac{M_{\text{kVA}} \times 10^3}{\sqrt{3} \cdot V}\right) \cdot \left(Z_{\Omega/\text{km}} \cdot L_{\text{equiv}} \times 10^{-3}\right)$$
   $$\Delta V_{\text{linha}} = \frac{M_{\text{kVA}} \cdot Z_{\Omega/\text{km}} \cdot L_{\text{equiv}}}{V} \times \left(10^3 \cdot 10^{-3}\right) = \frac{M \cdot Z \cdot L_{\text{equiv}}}{V}$$
   **O fator $10^3$ da potência em kVA cancela com o fator $10^{-3}$ do comprimento em metros para $\Omega/\text{km}$!**
5. Expressando em percentual da tensão de linha $V$:
   $$\Delta V\% = \frac{\Delta V_{\text{linha}}}{V} \times 100 = \frac{M \cdot Z \cdot L_{\text{equiv}}}{V^2} \times 100 = \frac{M \cdot Z \cdot L_{\text{equiv}}}{V^2 / 100}$$

Para $V = 220\text{ V}$:
$$\frac{V^2}{100} = \frac{220^2}{100} = \frac{48400}{100} = 484$$

Portanto, a fórmula não precisa de conversões manuais adicionais porque a formulação da planilha foi dimensionada para aceitar $M$ em kVA, $L$ em metros e $Z$ em $\Omega/\text{km}$ com cancelamento exato das potências de 10.

---

## 3. Fatores de Fase ($k_{\text{fase}}$)

Na célula `BZ13`:
```excel
IF(H13=3, BW13*BJ13*AR13*1, IF(H13=2, BW13*BJ13*AR13*2, IF(H13=1, BW13*BJ13*AR13*6, 0)))
```
- **Trifásico ($H=3$):** $k_{\text{fase}} = 1.0$ (queda entre fases com carga equilibrada).
- **Bifásico ($H=2$):** $k_{\text{fase}} = 2.0$ (circuito a dois condutores fase-fase, loop duplo de ida e volta).
- **Monofásico ($H=1$):** $k_{\text{fase}} = 6.0$ (tensão de fase $V_{\text{fn}} = V/\sqrt{3}$, corrente $I_{1\phi} = \sqrt{3} M / V$, queda percentual referida a $V_{\text{fn}} \implies 2 \times (\sqrt{3})^2 = 6$).
