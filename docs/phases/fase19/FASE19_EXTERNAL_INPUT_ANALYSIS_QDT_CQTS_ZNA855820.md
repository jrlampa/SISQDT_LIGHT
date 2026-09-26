# Fase 19 — Análise de Entrada Externa (AN13)

## 1. Classificação da Variável

No snapshot dos workbooks `CQT PROJ 7 REV2` e `CQT PROJ 4 REV1`, a célula `AN13` é classificada como:
- **Tipo:** `EXTERNAL_LOOKUP_OR_MANUAL_INPUT`
- **Semântica:** Ampacidade admissível do cabo ($I_z$) sob condição de projeto
- **Unidade:** Amperes ($A$)
- **Dependência de Fórmula:** Nenhuma fórmula interna direta no snapshot analisado

## 2. Relação com Tabelas e Catálogos

Na planilha `Tabela` (sheet15) do CQT PROJ 7:
- O condutor `185 Al - MX` está cadastrado na linha 27 com impedâncias $R = 0.2149\ \Omega/km$ e $X = 0.1178\ \Omega/km$.
- O cabeçalho na coluna H referencia fontes externas de ampacidade:
  - `Ampacidade dos Cabos Subterrâneos.xls`
  - `AMPACIDADE1 DE CABOS ARMADOS-revisão.xls`
  - `..\ARQ_2_ANEXOS\Ampacidade_PVC_XLPE_NBR_5410_2004.pdf`
  - `..\ARQ_2_ANEXOS\TABELA AMPACIDADADE_2 CIRCUITOS NO DUTO.doc`

## 3. Impacto Arquitetural

Como `AN13` representa a ampacidade de catálogo ($I_z$) e não uma corrente derivada da potência de carga ($I_b$), a cadeia de precedência física é:

```text
Condutor de Catálogo (ex: 185 Al - MX)
        ↓
Ampacidade Admissível (Iz = 430 A) [AN13]
        ↓
Comprimento do Trecho (L = 2 m) [AP13]
        ↓
Produto Amperagem x Comprimento (AO13 = 860)
        ↓
Elevação e Temperatura Final do Cabo (BX13 = 43.63 °C)
```

Isso consolida que o motor elétrico **não deve** tentar calcular `AN13` a partir de $M13$, preservando a integridade dos dados reais de projeto.
