# Fase 15 — Decisão de gate

## Gate

**A — GO RESTRICTED — FIRST MATHEMATICAL RULE IMPLEMENTED**

## Resultado final obrigatório

- **Candidato analisado:** `CQT - ZERADO.xlsm`, corroborado por `CQT - Light (Robusto).xlsm`; `base_jon_rev101.xlsm` foi o melhor candidato estrutural QDT, mas não forneceu cadeia nova fechada nesta ação.
- **Melhor evidência encontrada:** fórmula comum `Ramais!C13 = C11*0.85+C12*0.5268` em dois candidatos atuais.
- **Regra fechada:** `QDT.RAMAL.CANDIDATE_RX_COMBINATION`.
- **Fórmula:** `C11*0.85+C12*0.5268`.
- **Inputs:** C11=`1.0903` Ohm; C12=`0.4034` Ohm.
- **Output:** C13=`1.13926612` Ohm no contexto candidato.
- **Unidades:** C11/C12 confirmadas pelos cabeçalhos dos candidatos; C13 compatível no contexto R/X, não baseline oficial.
- **Reprodução:** `CANDIDATE_REPRODUCED / EXACT` em dois candidatos.
- **Implementação:** isolada, versionada `F15-CANDIDATE-1`, com trace e bloqueio de unidade desconhecida.
- **Testes:** 24 testes focados após a regra; suíte completa final: **51 aprovados, 0 falhas**.
- **Versão:** `CODE_VERSION=0.2.0`; `RULESET_VERSION=F9-RULESET-1`; `RECONSTRUCTION_VERSION=F14-RECON-1`.
- **Divergências:** fórmula catalogada antiga `B9*0.85+B10*0.5268` não foi substituída; status de carga continua `PARTIAL/BLOCKED`; candidatos não são baseline.
- **Próximo elo:** confirmar se a combinação R/X C13 possui dependentes elétricos válidos; depois procurar corrente com unidade e precedência completas.

## Restrições preservadas

`ELECTRICAL_BASELINE=UNRECONCILED`, `GOLDEN_DATASET_VERSION=NOT_ESTABLISHED`, engines completos `ENGINE_NOT_READY`, protótipo excluído. Fase 16 não iniciada.
