# ARQUITETURA DE RASTREABILIDADE DE CÁLCULO — CALCULATION TRACE

> **Status:** ATIVO E COMPROVADO (Fase 22.2)  
> **Data:** 2026-09-26  
> **Artefato:** `docs/architecture/CALCULATION_TRACE.md`  

---

## 1. Princípio Fundamental

> **"O cálculo não deve apenas retornar um número; ele deve ser capaz de explicar determinística e matematicamente como chegou a esse número."**

O **Calculation Trace** difere do log de aplicação por ser focado na física e na matemática: ele captura a tupla $(M, Z, L, \dots)$, as unidades físicas tipadas (`UnitCode`), os passos intermediários, a fórmula simbólica e a versão exata da regra que gerou a saída.

---

## 2. Modos de Trace: Normal vs. Diagnóstico

| Modo | Finalidade | Informações Capturadas |
| :--- | :--- | :--- |
| **`Normal`** | Operação regular de produção | `CalculationId`, `CorrelationId`, `RuleId`, `RuleVersion`, `RuleStatus`, `Status`, `InputHash`, `OutputHash`, `OutputValue`, `OutputUnit`, `DurationMs` |
| **`Diagnostic`** | Engenharia reversa, auditoria, testes e perícia | Todos os campos do modo Normal + lista completa de insumos tipados (`Inputs`), operações intermediárias e variáveis passo a passo (`Steps`), fórmula textual e referência de evidência celular original |

---

## 3. Hashes Determinísticos Canônicos

Para assegurar reprodutibilidade estrita:
$$\text{mesmos inputs} + \text{mesma versão} + \text{mesma regra} = \text{mesmo InputHash}$$

### 3.1 Algoritmo de Canonicalização (`DeterministicHashing.cs`)
1. **Ordenação:** Os insumos são ordenados alfabeticamente pelo nome da propriedade (ex: `AP`, `H`, `L`, `M`, `R`, `V`, `X`).
2. **Formatação de Valores:**
   - Cultura invariante (`CultureInfo.InvariantCulture`);
   - Separador decimal ponto (`.`);
   - Ponto flutuante formatado em formato lossless `G17`;
   - Listas e matrizes delimitadas por colchetes determinísticos `[v1,v2,...]`;
   - Valores nulos codificados explicitamente como `<null>`.
3. **Unidades Tipadas:** A unidade `UnitCode` é anexada entre colchetes (ex.: `M=74.448[Kva]`).
4. **Digest:** O texto canônico resultante é convertido em UTF-8 e submetido a SHA-256 gerando string hexadecimal de 64 caracteres.

---

## 4. Estrutura de Trace e Contratos

```csharp
public sealed record CalculationTraceDetail(
    string CalculationId,
    string CorrelationId,
    string RuleId,
    string RuleVersion,
    RuleStatus RuleStatus,
    TraceMode Mode,
    string InputHash,
    string OutputHash,
    IReadOnlyList<RuleInput> Inputs,
    IReadOnlyList<RuleTrace> Steps,
    object? OutputValue,
    UnitCode OutputUnit,
    CalculationStatus Status,
    string? FormulaString,
    string? EvidenceSource,
    double DurationMs,
    DateTimeOffset Timestamp);
```
