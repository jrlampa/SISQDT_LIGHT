# ARQUITETURA DE TRILHA DE AUDITORIA E PROVENIÊNCIA — AUDIT TRAIL

> **Status:** ATIVO E COMPROVADO (Fase 22.2)  
> **Data:** 2026-09-26  
> **Artefato:** `docs/architecture/AUDIT_TRAIL.md`  

---

## 1. Princípio Fundamental

> **"O Excel é oráculo e repositório de evidência para engenharia reversa; ele não é o motor de execução em tempo de execução."**

O **Audit Trail** garante que cada afirmação, fórmula, parâmetro e resultado numérico produzido pelo sistema SISQDT_LIGHT possa ser auditado e confrontado com seu fato gerador na documentação da Light ou na evidência celular extraída.

---

## 2. Camadas da Trilha de Auditoria

```text
EVIDENCE STORE (OpenXML / Fatos Reais)
       │
       ▼
EVIDENCE TRAIL (Arquivo, Hash, Aba, Célula, Fórmula, Valor)
       │
       ▼
CALCULATION RUN (CalculationId, CorrelationId, InputHash, OutputHash)
       │
       ├──────────────► PARITY TRAIL (Confronto Excel × Nativo, Delta, Tolerância)
       │
       └──────────────► GOLDEN CASE TRAIL (GoldenCaseId, Status, Esperado, Obtido)
```

---

## 3. Contratos de Trilha de Auditoria

### 3.1 Proveniência de Fatos (`EvidenceTrailRecord`)
- `EvidenceId`: Identificador único do fato gerador (ex.: `F22-CQTS-PROJ7-LADO1-BZ13`);
- `SourceFile`: Nome do arquivo de projeto real de onde a evidência foi extraída;
- `SourceHash`: Hash SHA-256 criptográfico do arquivo fonte;
- `Sheet`: Aba do workbook (ex.: `LADO 1`);
- `Cell`: Coordenada celular exata (ex.: `BZ13`);
- `Formula`: Expressão original da célula Excel;
- `CachedValue`: Valor numérico pré-calculado na célula;
- `ExtractionVersion`: Versão do extrator que capturou o fato;
- `Timestamp`: Data/hora UTC da captura.

### 3.2 Registro de Paridade Numérica (`ParityTraceRecord`)
- `CalculationId`, `CorrelationId`;
- `RuleId`, `CaseId`;
- `Source` (`Excel`), `Target` (`NativeEngine`);
- `ExpectedValue`, `ActualValue`, `Delta`, `Tolerance`;
- `Status` (`MATCH` ou `MISMATCH`);
- `Timestamp`.

### 3.3 Registro de Validação de Casos de Ouro (`GoldenTraceRecord`)
- `GoldenCaseId` (ex.: `GoldenSegmentVoltageDrop_Proj7_Line13_TrToLid`);
- `GoldenVersion`;
- `SourceHash`;
- `ExpectedValue`, `ActualValue`, `Delta`, `Tolerance`;
- `Status` (`PASS` ou `FAIL`);
- `Timestamp`.
