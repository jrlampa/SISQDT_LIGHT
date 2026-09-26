# Fase 21 — Relatório de Paridade Excel × Motor C# Nativo

## 1. Resumo da Paridade
A paridade entre a modelagem analítica candidata e os valores gravados nas planilhas reais do Excel foi avaliada para todos os elos da cadeia:

| Etapa da Cadeia | Grandeza | Valor Excel Original | Valor Motor C# Nativo | Status de Paridade |
| :--- | :--- | :--- | :--- | :--- |
| **Carga Padrão 1** | $S_{\text{unit}, 1}$ | $1.4664\text{ kVA}$ | $1.4664\text{ kVA}$ | **PASS (100% exato)** |
| **Carga Padrão 2** | $S_{\text{unit}, 2}$ | $1.8048\text{ kVA}$ | $1.8048\text{ kVA}$ | **PASS (100% exato)** |
| **Ramal Terminal** | $S_{\text{RL}}$ | $1.8800\text{ kVA}$ | $1.8800\text{ kVA}$ | **PASS (100% exato)** |
| **Fim de Linha (4 cons)** | $E_{\text{terminal}}$ | $7.2944\text{ kVA}$ | $7.2944\text{ kVA}$ | **PASS (100% exato)** |
| **Trecho Linear P3-P4** | $E(P3 \rightarrow P4)$ | $25.4552\text{ kVA}$ | $25.4552\text{ kVA}$ | **PASS (100% exato)** |
| **Nó LID (PROJ 7)** | $E13$ | $74.4480\text{ kVA}$ | $74.4480\text{ kVA}$ | **PASS (100% exato)** |
| **Nó LID (PROJ 7)** | $M13$ | $74.4480\text{ kVA}$ | $74.4480\text{ kVA}$ | **PASS (100% exato)** |
| **Térmico Cabo (PROJ 7)**| $T_{\text{cabo}}$ | $43.630837053054^\circ\text{C}$ | $43.630837053054^\circ\text{C}$ | **PASS (12 dígitos)** |
| **Nó LID (PROJ 4)** | $E13$ | $75.686588235294\text{ kVA}$ | $75.686588235294\text{ kVA}$ | **PASS (10 dígitos)** |
| **Nó LID (PROJ 4)** | $M13$ | $75.686588235294\text{ kVA}$ | $75.686588235294\text{ kVA}$ | **PASS (10 dígitos)** |
| **Térmico Cabo (PROJ 4)**| $T_{\text{cabo}}$ | $43.857612714067^\circ\text{C}$ | $43.857612714067^\circ\text{C}$ | **PASS (12 dígitos)** |

---

## 2. Testes de Suíte Automatizados
- **Total de Testes:** 60 aprovados, 0 falhas, 0 diagnósticos.
- **Novas Regras Implementadas e Cobertas:**
  - `CQTS.REAL_PROJECT.CONSUMER_LOAD_AGGREGATION`
  - `CQTS.REAL_PROJECT.RADIAL_LOAD_ACCUMULATION`
- **Regras das Fases Anteriores Preservadas:**
  - `CQTS.REAL_PROJECT.END_LOAD_SELECTION` (Fase 20)
  - `CQTS.REAL_PROJECT.CABLE_TEMPERATURE` (Fase 18/19)
  - `QDT.RAMAL.CANDIDATE_RX_COMBINATION`
  - `QDT.LADO1.SELECT_KL_TO_M13`
  - `QDT.LADO1.VALIDATION_I13`
  - `PROTECTION.IB_IN_IZ`
