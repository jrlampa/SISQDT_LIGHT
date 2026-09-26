# ARQUITETURA GLOBAL DE OBSERVABILIDADE — OBSERVABILITY

> **Status:** ATIVO E COMPROVADO (Fase 22.2)  
> **Data:** 2026-09-26  
> **Artefato:** `docs/architecture/OBSERVABILITY.md`  

---

## 1. Visão Geral e Topologia de Observabilidade

A observabilidade no **SISQDT_LIGHT** é projetada sob os princípios de arquitetura offline-first, alta precisão numérica e determinismo estrito. A topologia articula três camadas perfeitamente desacopladas:

```text
       ┌───────────────────────────────┐
       │   VERSION MANIFEST (Governo)  │
       └──────────────┬────────────────┘
                      │
                      ▼
       ┌───────────────────────────────┐
       │        APPLICATION LOG        │
       │ (ILogger<T> + Serilog Rolling)│
       └──────────────┬────────────────┘
                      │
         ┌────────────┴────────────┐
         ▼                         ▼
┌──────────────────┐      ┌──────────────────┐
│  CorrelationId   │      │  CalculationId   │
│ (Fluxo de Neg.)  │      │ (Execução Matem.)│
└────────┬─────────┘      └────────┬─────────┘
         │                         │
         └────────────┬────────────┘
                      ▼
       ┌───────────────────────────────┐
       │       CALCULATION TRACE       │
       │ (Insumos, Passos, Fórmulas)   │
       └──────────────┬────────────────┘
                      │
         ┌────────────┴────────────┐
         ▼                         ▼
┌──────────────────┐      ┌──────────────────┐
│   PARITY TRAIL   │      │   AUDIT TRAIL    │
│ (Excel vs Nativo)│      │ (Evidence Store) │
└──────────────────┘      └──────────────────┘
```

---

## 2. Diferenciação Crítica das Três Camadas

1. **Application Log:** Registra o comportamento operacional da aplicação (startup, shutdown, conexões, transações no SQLite, falhas de I/O, tempos de resposta).
2. **Calculation Trace:** Registra a cadeia física e matemática de dimensionamento (qual regra foi acionada, insumos canônicos, unidades tipadas, operações, resultado exato e hashes determinísticos).
3. **Audit / Evidence Trail:** Registra a proveniência dos fatos (de qual arquivo Excel real veio a fórmula, em qual aba, qual célula e com qual hash criptográfico SHA-256).

---

## 3. Startup e Metadados do Sistema

Ao inicializar, a aplicação emite o evento `APP-001` estruturado com o snapshot de metadados:
- `CodeVersion`: Versão do código executável;
- `Commit`: Hash de commit do repositório Git;
- `OS`: Descrição do sistema operacional Windows e arquitetura;
- `NetRuntime`: Versão do .NET 8 runtime;
- `SchemaVersion`: Versão do esquema do banco de dados SQLite;
- `RuleSetVersion`: Versão do conjunto de regras matemáticas ativas;
- `EvidenceVersion`: Versão do conjunto de evidências consolidadas;
- `LoggingProfile`: Perfil ativo (`Development`, `Test`, `Production`, `Diagnostic`).

---

## 4. Reprodutibilidade Estrita

Todo cálculo realizado pode ser integralmente reconstruído e auditado a posteriori a partir da tupla:
$$\langle \text{CalculationId}, \text{CorrelationId}, \text{RuleId}, \text{RuleVersion}, \text{InputHash}, \text{OutputHash} \rangle$$
garantindo auditoria e conformidade regulatória sem depender de planilhas Excel em tempo de execução.
