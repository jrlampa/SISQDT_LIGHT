# RAG MEMORY — SISQDT_LIGHT (QdtCqts)

> **Documento de Memória Persistente do Projeto**  
> **Última Atualização:** 2026-09-26  
> **Versão do Sistema:** 0.3.0 (Fase 21 Concluída)  
> **Branch Ativa:** `dev`  

---

## 1. Contexto e Missão do Projeto

O **SISQDT_LIGHT** é a iniciativa de engenharia de software da IM3 Brasil voltada para a modernização das ferramentas de planejamento, dimensionamento e cálculo de rede elétrica de distribuição da concessionária **Light S.A.**. Historicamente, estes cálculos dependiam de duas grandes famílias de planilhas Excel/VBA:
1. **QDT (Queda de Tensão):** Planilhas complexas com topologia linear bipartida (`LADO 1` e `LADO 2`), cálculo de queda de tensão em circuitos secundários de transformadores, ramais de ligação e balanceamento de cargas.
2. **CQTS (Cálculo de Queda de Tensão Subterrânea / Malhas):** Planilhas com topologia em árvore direcionada (`PONTO`, `PONTO MONTANTE`), cálculo de corrente acumulada por trecho, verificação de ampacidade de condutores subterrâneos e proteção de rede.

### Objetivo
Substituir o legado instável por uma aplicação desktop de alta precisão, determinística, auditável e offline-first com persistência SQLite e posterior sincronização Supabase, garantindo paridade matemática rigorosa com dados reais antes de qualquer liberação em produção.

---

## 2. Regras Não Negociáveis (Non-negotiables)

1. **Apenas na branch `dev`:** Nenhuma alteração direta na branch principal sem passar por validação em `dev`.
2. **OBRIGATÓRIO:** Criar e manter atualizado o par `RAG/MEMORY.md` + `CAC.md` antes e após cada ação.
3. **NÃO usar dados mockados:** Trabalhar exclusivamente com dados reais extraídos de projetos elétricos ou gerados via lógica comprovada de geoprocessamento.
4. **Não usar 3D e sim 2.5D:** Modelagem topológica e espacial plana com coordenadas georreferenciadas (UTM) e cotas altimétricas tratadas como atributos escalares (metadados Half-way BIM).
5. **Modularidade e SRP:** Separação estrita de responsabilidades entre Domínio, Aplicação, Infraestrutura, Motores de Cálculo e Interface de Usuário.
6. **Segurança First:** Sanitização e blindagem de entradas; leitura não executável de arquivos externos; rejeição de macros VBA; banco com transações e integridade referencial.
7. **Clean Code & Otimização:** "Mais resultado em menos linhas"; clareza arquitetural e sem overhead desnecessário.
8. **Thin Frontend / Smart Backend:** A interface gráfica (WPF) atua estritamente como camada de projeção/visualização. Toda a lógica pesada de validação topológica e cálculo reside nos serviços de domínio e motores de cálculo.
9. **Versionamento Único e Propagado:** Controle formal de versão do código, esquema, regras e evidências em `VERSION_MANIFEST.json` e `ARTIFACT_MANIFEST.md`.
10. **Full Suite de Testes:** Cobertura 100% para os 20% do código com 80% do impacto de negócio (domínio, regras de cálculo, topologia e paridade); cobertura mínima >=80% para os demais.
11. **Half-way BIM:** Manter e evoluir a estrutura de metadados de engenharia (condutores, materiais, coordenadas georreferenciadas e parâmetros de catálogo).
12. **Docker First:** Manter `Dockerfile`, `docker-compose.yml`, `.dockerignore` e `.gitignore` sempre funcionais e atualizados.
13. **Supabase First:** Arquitetura desenhada para integração e sincronização com Supabase (Auth, Postgres, Storage, Realtime, Edge Functions).
14. **Domain-Driven Design (DDD):** Modelagem orientada a domínios ricos, isolamento de Aggregates (`Project`, `ProjectVersion`, `NetworkModel`, `CalculationRun`), Value Objects e Invariantes.
15. **Interface 100% pt-BR:** UI/UX e mensagens para o usuário final integralmente em português do Brasil.
16. **Zero Custo a Todo Custo:** Uso exclusivo de ferramentas, bibliotecas e APIs de código aberto ou gratuitas.
17. **Limites de Tamanho de Código por Arquivo:**
    - **Ideal:** até 500 linhas
    - **Soft Limit:** até 750 linhas
    - **Hard Limit Absoluto:** até 1000 linhas (apenas quando modularização for tecnicamente inviável).
18. **Finalização Obrigatória de Task:**
    1. Executar suíte de testes (`dotnet test`);
    2. Verificar cobertura e integridade;
    3. Realizar o commit na branch `dev`;
    4. Atualizar `RAG/MEMORY.md` e `CAC.md`.

---

## 3. Histórico e Cronologia das Fases de Engenharia Reversa

- **Fases 01 a 05:** Auditoria inicial dos workbooks legados (`QDT_ZNA855820`, `CQTS__ZNA855820`). Mapeamento de fórmulas corrompidas (`#REF!`), links externos bloqueantes (`DecInv` em unidade de rede corporativa `J:\`), catálogo de precisão e unidades, e definição dos níveis do Harness de Paridade (P0 a P4). Decisão: **NO-GO para motor de produção; GO restrito para infraestrutura e testes**.
- **Fase 06:** Definição da arquitetura unificada da aplicação desktop em .NET 8, WPF, SQLite e assemblies desacoplados.
- **Fase 07:** Implementação da fundação de dados em SQLite, importador de evidência celular somente leitura (OpenXML) e harness de paridade P0/P1.
- **Fase 08:** Hardening de infraestrutura e construção do grafo formal de precedência de células.
- **Fase 09:** Formalização do subconjunto matemático isolado (`F9-RULESET-1`) e primeiros testes de regras unitárias independentes de células.
- **Fase 10:** Fechamento semântico das regras de carga e mapeamento de dependências entre QDT e CQTS.
- **Fase 11:** Identificação e exclusão definitiva de abas protótipo dos workbooks que distorciam os dados reais de cálculo.
- **Fase 12:** Reconciliação do baseline e catálogo de artefatos oficiais.
- **Fase 13:** Estabelecimento formal da governança de versionamento criptográfico (SHA-256) em `ARTIFACT_MANIFEST.md` e `VERSION_MANIFEST.json`.
- **Fase 14:** Catálogo histórico de evidências, matriz de reconstrução matemática e validação do modelo topológico de grafo dirigido.
- **Fase 15:** Implementação e validação fora do Excel da primeira regra candidata (`QDT.RAMAL.CANDIDATE_RX_COMBINATION`), reproduzindo exatamente `Ramais!C13` nos candidatos convergentes.
- **Fase 16:** Identificação de corpus de projeto real em uso pela engenharia (`CQT PROJ 7 REV2`) e identificação da cadeia condicional em `LADO 1!P13` e `BX13`.
- **Fase 17:** Sessão com usuário especialista que revelou o significado físico dos campos no projeto real: `AM13` é o condutor (`185 Al - MX`), `AN13` é a corrente em Amperes (`430 A`), e `P13/BX13` não é tensão, mas sim a **Temperatura do Condutor [°C]**. A regra `CQTS.REAL_PROJECT.CABLE_TEMPERATURE` foi implementada e testada com sucesso.
- **Fase 18:** Validação cruzada da fórmula de temperatura do cabo em múltiplos projetos reais (`CQT PROJ 7` e `CQT PROJ 4`). Confirmação de que `AN13` é input manual/externo nos snapshots. Suíte com 53 testes aprovados, 0 falhas.
- **Fase 19:** Investigação da origem de `AN13 = 430 A`. Confirmado pelo usuário e pelas evidências textuais da planilha (*"Corrente do cabo para a condição"*) que `AN13` é a **ampacidade/capacidade admissível ($I_z$)** do condutor de catálogo e não uma corrente de carga ($I_b$). Teste de sanidade comprovou que a corrente nominal de carga ($195.38\text{ A}$) diverge de $430\text{ A}$. Cadeia intermediária da temperatura fechada com integridade. Gate: `GO RESTRICTED - AMPACITY CONFIRMED / THERMAL CHAIN CLOSED`. 53 testes aprovados, 0 falhas.
- **Fase 20:** Reconstrução de `M13` através da consulta ao OneNote do usuário (`anotações GERAIS.one`) e análise de projetos reais (`CQT PROJ 7 REV2` e `CQT PROJ 4 REV1`). `M13` é a "Carga no fim do trecho escolhida", combinando a carga acumulada a jusante ($E13$), Fator de Diversidade ($G13$) e pisos de carga de 4 kVA (monofásico) e 8 kVA (bifásico/trifásico). Comprovada ramificação real da rede nas abas `LADO 1`, `LADO 2` e `LADO 3`. Implementada a regra `CQTS.REAL_PROJECT.END_LOAD_SELECTION`. Gate: `GO RESTRICTED - M13 RECONSTRUCTED / LOAD RULE CLOSED`. Suíte expandida para 56 testes aprovados, 0 falhas.
- **Fase 21:** Reconstrução das cargas terminais e acumulação radial a montante no CQTS. Identificação analítica das três cargas unitárias elementares dos consumidores: Padrão 1 ($1.4664\text{ kVA}$), Padrão 2 ($1.8048\text{ kVA}$) e Ramal de Ligação ($1.88\text{ kVA}$), validadas em `CQT PROJ 7 REV2`, `CQT PROJ 4 REV1` e `QDT_ZNA855820_PROJ`. Reconstrução da topologia em árvore radial e da regra de acumulação a montante para trechos lineares e bifurcações ($E_{\text{trecho}} = S_{\text{local}} + \sum E_{\text{filhos}}$ e $D_{\text{trecho}} = N_{\text{local}} + \sum D_{\text{filhos}}$). Comprovação do fechamento exato no nó de bifurcação `LID` ($74.4480\text{ kVA}$ em PROJ 7 e $75.6866\text{ kVA}$ em PROJ 4). Implementadas as regras candidatas `CQTS.REAL_PROJECT.CONSUMER_LOAD_AGGREGATION` e `CQTS.REAL_PROJECT.RADIAL_LOAD_ACCUMULATION`. Gate: `GO`. 60 testes aprovados, 0 falhas.
- **Fase 22 (Estado Atual):** Consolidação da topologia CQTS + QDT e reconstrução da queda de tensão. Modelo unificado em grafo em árvore: `TR`, `LID`, `PONTO`, `TRECHO`, `MONTANTE` e `RL` (terminal de carga). Comprovada a fórmula exata de queda percentual no trecho ($\Delta V\% = \frac{M \cdot Z \cdot L_{\text{equiv}}}{V^2 / 100} \cdot k_{\text{fase}}$) e acumulação ao longo dos caminhos da árvore ($CA(v) = CA(\text{pai}) + \Delta V\%_{\text{trecho}}$), com paridade analítica 1:1 e convergência total entre o fator $BJ$ do CQTS e o coeficiente $C_q$ da aba `Coeficiente Unitário` do QDT ($C_q = BJ = \frac{Z}{V^2 / 100}$). Implementadas as regras `CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP` e `CQTS.REAL_PROJECT.ACCUMULATED_VOLTAGE_DROP`. Gates: `TOPOLOGY_GATE = GO`, `VOLTAGE_DROP_GATE = GO`, `QDT_CQTS_CONVERGENCE_GATE = GO`. 70 testes aprovados, 0 falhas.

---

## 4. Estado Atual do Código e Arquitetura

### 4.1 Projetos da Solução
- `src/QdtCqts.Domain`: Entidades centrais (`Transformer`, `Circuit`, `TopologyNode`, `TopologyEdge`, `Load`, `Conductor`), tipos de versão e controle de invariantes.
- `src/QdtCqts.Application`: Serviços orquestradores de casos de uso (`ProjectService`, `TopologyService`, `CalculationService`, `ParityService`).
- `src/QdtCqts.Calculation.Abstractions`: Interfaces de motor (`ICalculationEngine`), requisições (`CalculationRequest`), resultados (`CalculationResult`) e trilhas (`TraceStep`).
- `src/QdtCqts.Calculation.Qdt`: Regras comprovadas QDT e regras candidatas isoladas.
- `src/QdtCqts.Calculation.Cqts`: Regras comprovadas CQTS, regra de temperatura de cabo, regra de seleção de carga no fim do trecho, regra de agregação de consumidores, regra de acumulação radial a montante, regra de queda de tensão no trecho e regra de queda de tensão acumulada.
- `src/QdtCqts.Infrastructure.Sqlite`: Banco de dados relacional e repositórios de dados SQLite.
- `src/QdtCqts.Infrastructure.ExcelEvidence`: Importador seguro OpenXML e construção de grafo de precedência celular.
- `src/QdtCqts.Infrastructure.Parity`: Mecanismos de comparação numérica e paridade com tolerância de engenharia.
- `src/QdtCqts.Desktop.Wpf`: Aplicação desktop WPF para operadores de engenharia da Light.

### 4.2 Métricas de Teste
- Total de testes automatizados: **70 testes**.
- Falhas: **0**.
- Duração da execução: **~1.5 segundos**.

---

## 5. Próximos Passos
1. Fase 23: Reconstrução das Correntes de Curto-Circuito ($I_{\text{cc}3\phi}$ e $I_{\text{cc}1\phi}$) e Coordenação de Proteção (Fusíveis NH / Disjuntores);
2. Conectar a interface WPF (MVVM) aos serviços de aplicação já implementados;
3. Manter a integridade de todos os testes e a documentação organizada no diretório `docs/`.

