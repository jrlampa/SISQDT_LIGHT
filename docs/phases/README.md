# Histórico e Decisões de Gate das Fases (QDT + CQTS)

Este diretório contém os relatórios e registros de evidência matemática de cada fase do processo de engenharia reversa e reconstrução da ferramenta SISQDT_LIGHT.

---

## Sumário Cronológico das Fases

| Fase | Diretório | Foco Principal | Gate de Decisão | Testes |
|---|---|---|---|---|
| **Fase 03** | [fase03/](fase03/) | Mapeamento de Bloqueios UX e Arquiteturais | `NO-GO MOTOR PRODUÇÃO` / `GO RESTRITO FUNDAÇÃO` | - |
| **Fase 04** | [fase04/](fase04/) | Prontidão do Motor de Cálculo (Engine Readiness) | `NO-GO MOTOR PRODUÇÃO` | - |
| **Fase 05** | [fase05/](fase05/) | Fechamento de Bloqueadores Críticos | `GO RESTRITO FUNDAÇÃO & HARNESS` | - |
| **Fase 06** | [fase06/](fase06/) | Arquitetura Unificada Desktop (.NET 8 + SQLite) | `GO ARQUITETURA APROVADA` | - |
| **Fase 07** | [fase07/](fase07/) | Fundação SQLite, Importador Somente Leitura e Paridade P0/P1 | `GO FUNDAÇÃO IMPLEMENTADA` | - |
| **Fase 08** | [fase08/](fase08/) | Hardening de Infraestrutura, Grafo de Precedência P1 | `GO HARDENING CONCLUÍDO` | - |
| **Fase 09** | [fase09/](fase09/) | Especificação de Subconjunto Matemático e Regras Isoladas | `GO REGRAS ISOLADAS VALIDADAS` | - |
| **Fase 10** | [fase10/](fase10/) | Semântica de Cargas, Chained Golden e Status DecInv | `GO PARCIAL REGRAS DE CARGA` | - |
| **Fase 11** | [fase11/](fase11/) | Exclusão do Protótipo e Reconciliação de Precedência | `GO EXCLUSÃO DE PROTÓTIPO` | - |
| **Fase 12** | [fase12/](fase12/) | Reconciliação do Baseline e Identidade de Artefatos Oficiais | `GO RECONCILIAÇÃO OFICIAL` | - |
| **Fase 13** | [fase13/](fase13/) | Política de Versionamento Formal (Manifestos e Hashes) | `GO VERSIONING ESTABLISHED` | 45 |
| **Fase 14** | [fase14/](fase14/) | Catálogo Histórico, Matriz de Reconstrução e Topologia | `GO MATRIZ DE RECONSTRUÇÃO` | 45 |
| **Fase 15** | [fase15/](fase15/) | Primeira Regra Matemática Candidata: Ramal R/X (`Ramais!C13`) | `GO FIRST MATH RULE CANDIDATE` | 45 |
| **Fase 16** | [fase16/](fase16/) | Corpus de Projeto Real (CQT PROJ 7) e Cadeia de Paridade | `GO CORPUS REAL IDENTIFICADO` | 45 |
| **Fase 17** | [fase17/](fase17/) | Reconstrução Assistida pelo Usuário: Temperatura de Cabo (`BX13`) | `GO FIRST REAL PROJECT RULE` | 45 |
| **Fase 18** | [fase18/](fase18/) | Validação da Temperatura em Múltiplos Projetos Reais (CQT 7 e CQT 4) | `GO RESTRICTED - TEMP VALIDATED / CURRENT ORIGIN UNRECONCILED` | 53 |
| **Fase 19** | [fase19/](fase19/) | Origem da Amperagem AN13 (Ampacidade do Condutor) e Fechamento da Cadeia Térmica | `GO RESTRICTED - AMPACITY CONFIRMED / THERMAL CHAIN CLOSED` | 53 |

---

## Diretrizes de Consulta

1. Cada fase possui seu respectivo relatório de decisão (`FASE*_GATE_DECISION_*.md`) e relatórios de paridade ou validação.
2. Nenhuma regra anterior deve ser modificada retroativamente sem que haja nova decisão de gate documentada.
