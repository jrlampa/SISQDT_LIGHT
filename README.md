# SISQDT_LIGHT (QdtCqts)

**Sistema Unificado de Cálculo e Análise de Redes de Distribuição (QDT + CQTS)**

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/Tests-212%20passed-brightgreen.svg)]()
[![Database](https://img.shields.io/badge/Database-SQLite%203-lightgrey.svg)](https://www.sqlite.org/)
[![Status](https://img.shields.io/badge/Fase-30C%20Reconciliacao-orange.svg)]()

---

## 1. Visão Geral

O **SISQDT_LIGHT** é o projeto corporativo de engenharia reversa, unificação e modernização dos motores de cálculo de engenharia elétrica da **Light** (tradicionalmente implementados em planilhas Excel/VBA legadas de alta complexidade: **QDT** e **CQTS**).

O objetivo do sistema é substituir o legado por uma aplicação moderna, segura, determinística e de alta performance, construída sob os preceitos de **Domain-Driven Design (DDD)**, **Half-way BIM (2.5D)**, **Zero Custos**, **Thin Frontend / Smart Backend** e **100% em Português (pt-BR)**.

---

## 2. Estrutura da Solução e Arquitetura

O ecossistema é modularizado em bibliotecas e camadas com estrita separação de responsabilidades:

```text
SISQDT_LIGHT/
├── docs/                                  # Documentação completa e centralizada
│   ├── manifests/                         # Hashes SHA-256 e controle de integridade dos baselines
│   ├── phases/                            # Relatórios cronológicos e decisões de gate (Fases 03 a 30C)
│   └── specs/                             # Especificações de arquitetura, domínio, contratos e topologia
├── RAG/                                   # Base de conhecimento e memória persistente
│   └── MEMORY.md                          # Memória contextual viva do projeto
├── artifacts/                             # Artefatos externos e workbooks de referência
│   ├── cqts/                              # Workbooks CQTS
│   ├── evidence/                          # Evidências extraídas de planilhas
│   ├── external/                          # Dependências externas legadas
│   ├── golden/                            # Datasets dourados para testes de paridade
│   └── qdt/                               # Workbooks QDT
├── src/                                   # Código-fonte da aplicação
│   ├── QdtCqts.Application/               # Serviços de aplicação e orquestração de casos de uso
│   ├── QdtCqts.Calculation.Abstractions/  # Interfaces de cálculo, trace de passos e contratos
│   ├── QdtCqts.Calculation.Cqts/          # Motor de cálculo de regras CQTS (proteção, carga montante)
│   ├── QdtCqts.Calculation.Qdt/           # Motor de cálculo de regras QDT (queda, ramais, Lado 1/2)
│   ├── QdtCqts.Desktop.Wpf/               # Interface desktop WPF (Thin Frontend)
│   ├── QdtCqts.Domain/                    # Núcleo de domínio (Aggregates, Topologia, Tipos, Versionamento)
│   ├── QdtCqts.Infrastructure.ExcelEvidence/ # Importador de planilhas somente leitura e grafos
│   ├── QdtCqts.Infrastructure.Geometry/   # Adaptador de geometria física JSON (sem leitura DWG)
│   ├── QdtCqts.Infrastructure.Parity/     # Harness de comparação de paridade matemática
│   └── QdtCqts.Infrastructure.Sqlite/     # Repositórios e persistência relacional SQLite
├── tests/                                 # Suíte abrangente de testes automatizados (.NET xUnit)
│   ├── QdtCqts.Tests.Domain/              # Testes unitários do domínio e regras isoladas
│   ├── QdtCqts.Tests.Import/              # Testes de importação e validação de precedência
│   ├── QdtCqts.Tests.Parity/              # Testes de paridade com golden cases
│   └── QdtCqts.Tests.Topology/            # Testes do modelo de grafo e validações radiais
├── CAC.md                                 # Conhecimento Arquitetural Compartilhado e Contratos
├── CHANGELOG.md                           # Histórico de alterações por fase e versão
├── Dockerfile                             # Contêiner para compilação e execução de testes
├── docker-compose.yml                     # Orquestração local de desenvolvimento
├── QdtCqts.slnx                           # Descritor da solução .NET
└── VERSION_MANIFEST.json                  # Manifesto formal da versão corrente
```

---

## 3. Regras Não Negociáveis do Projeto

- **Branch Dev:** Todo o ciclo de desenvolvimento ativo reside na branch `dev`.
- **RAG & CAC:** Obrigatório consultar e atualizar [RAG/MEMORY.md](RAG/MEMORY.md) e [CAC.md](CAC.md) a cada evolução técnica.
- **Dados Reais (Zero Mock):** Cálculos e testes utilizam exclusivamente dados reais de projetos ou lógica algorítmica de geoprocessamento.
- **Topologia 2.5D (Sem 3D):** Modelagem espacial simplificada com coordenadas projetadas somente quando CRS/unidade forem declarados; datum/EPSG não são presumidos. Elevações/cotas permanecem atributos escalares (Half-way BIM).
- **Segurança First:** Sanitização e validação estrita em todas as portas de entrada de dados; execução em modo somente leitura para arquivos externos sem execução de macros.
- **Determinismo Absoluto:** Para entradas idênticas, o resultado do cálculo é 100% determinístico e auditável via trilha de passos (`TraceStep`).

---

## 4. Como Executar os Testes

Para restaurar, compilar a solução e rodar a suíte completa de testes:

```bash
# Executar todos os testes da solução
dotnet test

# Executar testes com relatório detalhado
dotnet test --logger "console;verbosity=normal"
```

Estado da suíte no checkout atual: **212 testes aprovados, 0 falhas, 0 ignorados**.

---

## 5. Estado de Engenharia

- Fases 24–28 e 30A–30C concluídas nos escopos documentados; a Fase 30C não determinou a associação física/lógica do ZNA855820.
- O domínio preserva `Node.PhysicalPosition` separado de `Node.LayoutX/LayoutY` e consome JSON gerado pela ferramenta do acervo.
- Icc trifásico e monofásico permanecem calculáveis e independentes das curvas de proteção.
- Sem evidência identificável de dispositivo/curva, a avaliação retorna `EvidenceBlocked`; a suportabilidade térmica só é calculada com seção e temperatura com proveniência identificada.
- Curvas NH/disjuntor, coordenação e seletividade não são implementadas nem inferidas.
- `VERSION_MANIFEST.json` identifica a release `0.7.0`/Fase 26; commit funcional `29c46ad`.
- Roadmap posterior: `NEXT_PHASE_NOT_DEFINED`.

## 6. Documentação Adicional

Para detalhes arquiteturais e especificações técnicas de engenharia, consulte o diretório [docs/](docs/README.md).
