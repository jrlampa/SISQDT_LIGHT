# Fase 19 — Origem da Amperagem AN13

## 1. Contexto

Na Fase 18, a regra de temperatura de cabo (`CQTS.REAL_PROJECT.CABLE_TEMPERATURE`) foi validada no CQT PROJ 7 REV2 e no CQT PROJ 4 REV1. Em ambos, a variável `AN13 = 430 A` participava diretamente do denominador térmico `((I * L) / (90 - 30))`.

O objetivo desta fase foi investigar se `AN13` representava corrente de carga calculada, ampacidade de catálogo ou preenchimento externo.

## 2. Teste de Sanidade Teórico

Para a carga observada no CQT PROJ 7:
- Carga: `M13 = 74.448 kVA`
- Tensão: `BX6 = 220 V`
- Fases: `H13 = 3`

Cálculo da corrente nominal teórica de carga:
$$I_{teórica} = \frac{74.448 \times 1000}{220 \times \sqrt{3}} \approx 195.38\text{ A}$$

Comparação com `AN13`:
- $I_{teórica} = 195.38\text{ A}$
- $AN13 = 430.00\text{ A}$

Divergência: $AN13$ é aproximadamente $2.2$ vezes superior à corrente de carga do trecho. O valor não coincide nem decorre da potência $M13$.

## 3. Confirmação do Especialista / Usuário

Questionado diretamente sobre a procedência de `AN13 = 430 A`, o usuário confirmou:
> "É a ampacidade/capacidade de condução do cabo (ex: 185 Al) obtida de tabela/catálogo da LIGHT ou lookup de condutores."

## 4. Evidência Celular e Estrutural nos Arquivos Reais

A inspeção da aba `LADO 1` no CQT PROJ 7 REV2 confirmou:
- `AN11`: Rótulo explícito *"Corrente do cabo para a condição"*.
- `AM10`/`AM11`: *"Cabo de B.T. - Seção [ mm2 ]"*.
- `AN13`: Valor estático numérico `430`, sem fórmula.
- A aba `Tabela` do workbook referencia catálogos de cabos da Light e normas (*"Ampacidade dos Cabos Subterrâneos.xls"*, *NBR 5410*), onde cabos de 185 mm² Al possuem capacidade compatível com 430 A dependendo das condições de instalação.

## 5. Conclusão

`AN13` **não é uma corrente elétrica calculada ($I_b$)**, mas sim a **ampacidade/capacidade admissível ($I_z$)** do condutor para a condição operacional/instalação.
