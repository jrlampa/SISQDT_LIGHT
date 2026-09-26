# Manifestos de Integridade e Baseline

Este diretório armazena os manifestos criptográficos e controles de integridade dos artefatos do SISQDT_LIGHT.

---

## Arquivos de Manifesto

| Arquivo | Formato | Descrição |
|---|---|---|
| [ARTIFACT_MANIFEST.md](ARTIFACT_MANIFEST.md) | Markdown | Tabela oficial de hashes SHA-256 dos artefatos críticos de código e documentação. |
| [BASELINE_MANIFEST.md](BASELINE_MANIFEST.md) | Markdown | Registro explicativo da versão do baseline de código e dados adotado. |
| [BASELINE_MANIFEST.json](BASELINE_MANIFEST.json) | JSON | Metadados legíveis por máquina do baseline formal do projeto. |

---

## Regra de Integridade

- Hashes de artefatos existentes **nunca** devem ser sobrescritos sem nova versão formal documentada no `CHANGELOG.md` e em relatório de fase com gate decision.
