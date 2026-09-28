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
| **Fase 20** | [fase20/](fase20/) | Reconstrução de M13 (Carga Escolhida), Regra de Seleção de Carga e Ramificação Real | `GO RESTRICTED - M13 RECONSTRUCTED / LOAD RULE CLOSED` | 56 |
| **Fase 21** | [fase21/](fase21/) | Reconstrução das Cargas Terminais e Acumulação Radial a Montante no CQTS | `GO` | 60 |
| **Fase 22** | [fase22/](fase22/) | Consolidação da Topologia CQTS + QDT e Reconstrução de Queda de Tensão | `GO` (Topologia, Queda e Convergência) | 70 |
| **Fase 22.1** | [fase22_1/](fase22_1/) | Auditoria Corretiva Matemática e Paridade Estrita QDT + CQTS | `GO` (100% Paridade Bit-a-Bit / 6 Gates Aprovados) | 80 |
| **Fase 22.2** | docs/architecture/ | Observabilidade, Logging Estruturado e Rastreabilidade de Cálculo | `GO` (Logging, Trace, Audit Trail e Reprodutibilidade) | 95 |
| **Fase 23** | docs/brand/ | Integração Ponta a Ponta + Identidade Visual Oficial (sisQDT_LIGHT) | `GO` (Cadeia Ponta a Ponta 100% Validada + Brand Assets Aprovados) | 122 |
| **Fase 24** | [CHANGELOG.md](../../CHANGELOG.md) | Curto-circuito 3φ/1φ e análise delimitada de evidências de proteção | `GO` para curto-circuito; curvas bloqueadas por ausência de evidência | 134 |
| **Fase 25** | [CHANGELOG.md](../../CHANGELOG.md) | Consolidação do motor, integração WPF e contratos | `GO` nos gates declarados; curvas permanecem fora do escopo | 146 |
| **Fase 26** | [fase26/](fase26/) | Integridade da proteção, fail-closed e reconciliação de governança | `GO` restrito; 149 testes aprovados | 149 |
| **Fase 30A** | [fase30a/](fase30a/) | Fonte da geometria física DWG/JSON, unidade, fuso, CRS e identidade | Investigação concluída; datum/identidade não determinados | - |
| **Fase 30B** | [fase30b/](fase30b/) | Domínio espacial mínimo e importação física JSON | `GO` — 212 testes aprovados | 212 |
| **Fase 30C** | [fase30c/](fase30c/) | Reconciliação de identidade física/lógica no ZNA855820 | `GO PARA PRÓXIMA DECISÃO` — identidade e CRS/unidade não determinados | 0 novos; suíte 212 |
| **Fase 30D** | [fase30d/](fase30d/) | Protocolo de identidade física/lógica em corpus multi-projeto | `IDENTIDADE NÃO DETERMINADA — REQUISITO EXTERNO NECESSÁRIO` | 0 novos; suíte 212 |
| **Fase 30E** | [fase30e/](fase30e/) | Origem do crosswalk `.srua`/CSV/KMZ/CQTS e rastreio até DWG | Dados coerentes até CQTS; origem e handle DWG não determinados | 0 novos; suíte 212 |



---

## Diretrizes de Consulta

1. Cada fase possui registro de decisão e relatórios de paridade ou validação; Fases 24 e 25 foram registradas no changelog/RAG, sem diretórios individuais neste checkout.
2. Nenhuma regra anterior deve ser modificada retroativamente sem que haja nova decisão de gate documentada.
