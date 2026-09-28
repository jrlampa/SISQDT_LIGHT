# Documentação do Projeto SISQDT_LIGHT (QdtCqts)

Bem-vindo à documentação centralizada do projeto de reconstrução e unificação das ferramentas de cálculo de rede elétrica da Light (QDT e CQTS).

---

## Estrutura da Documentação

```text
docs/
├── specs/       # Modelos conceituais, topologia, contratos de cálculo, DDL e matrizes
├── manifests/   # Manifestos de hashes SHA-256 e controle de integridade dos baselines
└── phases/      # Relatórios de engenharia reversa e portões de decisão (Fases 03 a 30C)
```

---

## Navegação Rápida

- **[Especificações Técnicas (specs/)](specs/README.md)**:
  - [Arquitetura da Aplicação Desktop](specs/APPLICATION_ARCHITECTURE.md)
  - [Modelo de Domínio Unificado](specs/DOMAIN_MODEL_QDT_CQTS.md)
  - [Modelo Topológico de Rede](specs/TOPOLOGY_MODEL_QDT_CQTS.md)
  - [Contratos de Cálculo](specs/CALCULATION_CONTRACTS_QDT_CQTS.md)
  - [Esquema do Banco SQLite](specs/SQLITE_UNIFIED_SCHEMA.md)
  - [Catálogo de Unidades e Precisão](specs/UNITS_PRECISION_CATALOG_QDT.md)
  - [Especificação Consolidada](specs/ESPECIFICACAO_QDT_CQTS_ZNA855820.md)

- **[Controle de Fases e Gates (phases/)](phases/README.md)**:
  - Registros de avanço técnico e paridade das fases 03 até a fase 18 (mais recente).

- **[Manifestos de Hashes (manifests/)](manifests/README.md)**:
  - Rastreabilidade estrita SHA-256 de artefatos de código e evidência.
