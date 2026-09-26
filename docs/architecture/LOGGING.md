# ARQUITETURA DE LOGGING — SISQDT_LIGHT

> **Status:** ATIVO E COMPROVADO (Fase 22.2)  
> **Data:** 2026-09-26  
> **Artefato:** `docs/architecture/LOGGING.md`  

---

## 1. Princípios de Logging Estruturado

O sistema não adota logging ingênuo (`Console.WriteLine` / texto plano não estruturado). Todos os eventos são registrados através da abstração padrão do .NET:
```csharp
Microsoft.Extensions.Logging.ILogger<T>
```
e implementados via provedor Serilog isolado na camada `QdtCqts.Infrastructure.Observability`. Nenhuma camada interna (`Domain`, `Calculation`, `Application`) possui acoplamento direto com o Serilog.

---

## 2. Níveis de Log

| Nível | Uso Pretendido | Ambiente Ativo |
| :--- | :--- | :--- |
| **Trace (Verbose)** | Detalhamento profundo de insumos, passos e variáveis intermediárias de cálculo | Diagnóstico Pontual |
| **Debug** | Diagnóstico de desenvolvimento e engenharia reversa | Desenvolvimento / Teste |
| **Information** | Eventos operacionais normais: startup, migração de banco, início/fim de cálculo, importação de workbook | Todos os Ambientes |
| **Warning** | Situação anômala recuperável: regra candidata em uso, evidência incompleta, fallback regulatório | Todos os Ambientes |
| **Error** | Operação falhou: cálculo bloqueado, invariante topológica violada, erro de importação | Todos os Ambientes |
| **Critical** | Falha grave com risco de corrupção ou violação sistêmica | Todos os Ambientes |

---

## 3. Taxonomia Formal de Event IDs

A taxonomia de identificadores de evento (`CalculationEventIds`) é rigorosamente particionada por domínio funcional:

| Prefixo | Faixa ID | Domínio | Exemplo |
| :--- | :--- | :--- | :--- |
| `APP-xxx` | 1001–1999 | Ciclo de vida da aplicação | `APP-001` (Startup), `APP-002` (Shutdown), `APP-003` (Unhandled Exception) |
| `DB-xxx` | 2001–2999 | Banco SQLite e migrações | `DB-001` (Opened), `DB-002` (Migration Started), `DB-003` (Migration Completed) |
| `IMPORT-xxx` | 3001–3999 | Ingestão e parsing de Excel | `IMPORT-001` (Started), `IMPORT-002` (Completed), `IMPORT-004` (Failed) |
| `CALC-xxx` | 4001–4999 | Execução de cálculo | `CALC-001` (Started), `CALC-002` (Rule Executed), `CALC-003` (Completed), `CALC-004` (Blocked) |
| `RULE-xxx` | 5001–5999 | Regras matemáticas granulares | `RULE-001` (Executed), `RULE-002` (Failed), `RULE-003` (Candidate Warning) |
| `TOPO-xxx` | 6001–6999 | Topologia e Invariantes | `TOPO-001` (Node Created), `TOPO-004` (Validated), `TOPO-005` (Invariant Violation) |
| `PARITY-xxx` | 7001–7999 | Confronto Excel × Nativo | `PARITY-001` (Match), `PARITY-002` (Mismatch) |
| `EVID-xxx` | 8001–8999 | Proveniência de Fatos | `EVID-001` (Captured), `EVID-002` (Missing or Incomplete) |
| `GOLDEN-xxx` | 9001–9999 | Casos de Ouro | `GOLDEN-001` (Pass), `GOLDEN-002` (Mismatch) |

---

## 4. Política de Armazenamento e Rotação de Arquivos (Rolling Policy)

- **Diretório:** `logs/` (configurável, relativo ao binário, sem caminhos absolutos de usuário).
- **Padrão de Nome:** `application-yyyyMMdd.log`.
- **Rotação:** Diária (`RollingInterval.Day`).
- **Tamanho Máximo por Arquivo:** 10 MB (`FileSizeLimitBytes = 10485760`).
- **Retenção:** 30 dias (`RetainedFileCountLimit = 30`).

---

## 5. Higienização e Proteção de Dados Sensíveis (Redaction)

O utilitário `SensitiveDataRedactor` sanitiza automaticamente:
- Tokens de autenticação (`Bearer ...`, `token=...`, `secret=...`);
- Credenciais em strings de conexão (`Password=...`, `User Id=...`);
- Dados cadastrais de consumidores (ex.: CPF no formato `***.***.***-**`).
