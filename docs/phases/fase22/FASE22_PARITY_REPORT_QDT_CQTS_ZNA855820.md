# Fase 22 — Relatório de Paridade Excel × Motor C# Nativo

## 1. Tabela Comparativa de Paridade
A paridade entre a modelagem analítica e os valores do Excel foi avaliada:

| Trecho / Ponto | Grandeza | Valor Excel Original | Valor Motor C# Nativo | Status de Paridade |
| :--- | :--- | :--- | :--- | :--- |
| `TR7, LID` | $\Delta V\%_{\text{trecho}}$ (`BZ13`) | $0.038810072181\%$ | $0.038810072181\%$ | **PASS (100% exato)** |
| `TR7, LID` | $\Delta V\%_{\text{acum}}$ (`CA13`) | $4.188033011709\%$ | $4.188033011709\%$ | **PASS (100% exato)** |
| `LID, P1` | $\Delta V\%_{\text{trecho}}$ (`BZ14`) | $0.862413217427\%$ | $0.862413217427\%$ | **PASS (100% exato)** |
| `LID, P1` | $\Delta V\%_{\text{acum}}$ (`CA14`) | $5.050446229135\%$ | $5.050446229135\%$ | **PASS (100% exato)** |
| `P1, P2` | $\Delta V\%_{\text{trecho}}$ (`BZ15`) | $0.753167056846\%$ | $0.753167056846\%$ | **PASS (100% exato)** |
| `P1, P2` | $\Delta V\%_{\text{acum}}$ (`CA15`) | $5.803613285981\%$ | $5.803613285981\%$ | **PASS (100% exato)** |
| `P2, P3` (L1) | $\Delta V\%_{\text{trecho}}$ (`BZ16`) | $0.386537934865\%$ | $0.386537934865\%$ | **PASS (100% exato)** |
| `P2, P3` (L1) | $\Delta V\%_{\text{acum}}$ (`CA16`) | $6.190151220846\%$ | $6.190151220846\%$ | **PASS (100% exato)** |
| `P2, P8` (L3) | $\Delta V\%_{\text{trecho}}$ (`BZ16`) | $0.217427588361\%$ | $0.217427588361\%$ | **PASS (100% exato)** |
| `P2, P8` (L3) | $\Delta V\%_{\text{acum}}$ (`CA16`) | $6.021040874342\%$ | $6.021040874342\%$ | **PASS (100% exato)** |
| Trafo Trafo | $\Delta V\%_{\text{trafo}}$ (`BV4`) | $2.316160000000\%$ | $2.316160000000\%$ | **PASS (100% exato)** |
| MT Inicial | $\Delta V\%_{\text{MT}}$ (`CV105`) | $1.833062939528\%$ | $1.833062939528\%$ | **PASS (100% exato)** |

---

## 2. Testes Automatizados da Fase 22
- Total de testes da solução: **70 aprovados, 0 falhas, 0 diagnósticos**.
- Cobertura dos 10 cenários obrigatórios:
  1. Trecho linear: Aprovado.
  2. Trecho terminal: Aprovado.
  3. Nó com carga local: Aprovado.
  4. Nó com duas derivações: Aprovado.
  5. Acumulação a montante e monotonicidade: Aprovado.
  6. Caminhos independentes de queda: Aprovado.
  7. Convergência CQTS e QDT no caso linear: Aprovado.
  8. Preservação de divergência entre ramos: Aprovado.
  9. Mudança de condutor: Aprovado.
  10. RL como terminal de carga: Aprovado.
