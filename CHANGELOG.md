# Changelog

## Fase 20 — 2026-09-26

- Consulta ao OneNote do usuário (`anotações GERAIS.one`) identificou os critérios corporativos de levantamento de demanda, normas (ET 285/283) e condutores multiplexados na Light.
- Reconstrução de `M13`: identificado como a "Carga no fim do trecho escolhida", combinando a carga acumulada a jusante ($E13$), Fator de Diversidade ($G13$) e pisos de carga monofásica (4 kVA) e bifásica/trifásica (8 kVA).
- Comprovação da topologia ramificada real no CQT PROJ 7 REV2 através das abas `LADO 1`, `LADO 2` e `LADO 3`.
- Implementada a regra `CQTS.REAL_PROJECT.END_LOAD_SELECTION`.
- Testes expandidos de 53 para 56 testes, todos aprovados com 0 falhas.
- Gate: `GO RESTRICTED - M13 RECONSTRUCTED / LOAD RULE CLOSED`.

## Fase 19 — 2026-09-26

- Origem de `AN13` esclarecida: confirmada como a ampacidade admissível do cabo ($I_z = 430\text{ A}$ para o cabo `185 Al - MX`), obtida de tabela/catálogo da LIGHT ou entrada de condutores, e não corrente de carga ($I_b$).
- Teste de sanidade teórico comprovou que a corrente nominal de carga ($195.38\text{ A}$) diverge do valor nominal de catálogo ($430\text{ A}$).
- Inspeção estrutural da aba `LADO 1` e da aba `Tabela` corroborou referências às tabelas de ampacidade subterrânea da LIGHT.
- Cadeia intermediária da temperatura fechada com integridade. Nenhuma regra espúria de corrente foi implementada.
- Testes: 53 aprovados, 0 falhas.
- Gate: `GO RESTRICTED - AMPACITY CONFIRMED / THERMAL CHAIN CLOSED`.

## Fase 18 — 2026-09-26

- Temperatura do cabo validada em CQT PROJ 7 e CQT PROJ 4 com a mesma fórmula.
- `AN13` confirmado como input manual/externo no snapshot, não como corrente calculada.
- Nenhuma nova regra de corrente implementada.
- Testes: 53 aprovados, 0 falhas.
- Gate: `GO RESTRICTED - TEMPERATURE VALIDATED / CURRENT ORIGIN UNRECONCILED`.

## 0.3.0 — 2026-09-26

- **Phase:** FASE17
- **Change:** implementada a regra candidata `CQTS.REAL_PROJECT.CABLE_TEMPERATURE` para o ramo trifásico real do CQT PROJ 7.
- **Reason:** `P13/BX13` foi identificado pelo usuário como temperatura do cabo; a fórmula reproduziu exatamente o valor observado.
- **Evidence:** CQT PROJ 7 REV2, `LADO 1`, `M13=74.448`, `BX6=220`, `AN13=430`, `AP13=2`, `BX13=43.630837053053675`.
- **Tests:** 26 testes focados; suíte completa validada no encerramento.
- **Gate:** `GO RESTRICTED - FIRST REAL PROJECT RULE IMPLEMENTED`.
- **Scope:** regra candidata de projeto real; baseline elétrico e ruleset oficial permanecem inalterados.

## 0.2.0 — 2026-09-26

- **Phase:** FASE15
- **Change:** adicionada a regra candidata `QDT.RAMAL.CANDIDATE_RX_COMBINATION` para reproduzir `Ramais!C13` nos candidatos convergentes.
- **Reason:** fórmula, entradas, unidades de R/X e saída foram reproduzidas exatamente fora do Excel.
- **Evidence:** `CQT - ZERADO.xlsm` e `CQT - Light (Robusto).xlsm`, hashes registrados no relatório Fase 15.
- **Tests:** 24 testes focados aprovados; suíte completa validada no encerramento.
- **Gate:** `GO RESTRICTED - FIRST MATHEMATICAL RULE IMPLEMENTED`.
- **Scope:** regra candidata; `F9-RULESET-1` e baseline elétrico não foram alterados.

## 0.1.0 — 2026-09-26

- **Phase:** FASE13
- **Change:** estabelecido versionamento local formal para código, evidência, baseline, schema, precedence e ruleset.
- **Reason:** criar rastreabilidade sem promover o baseline elétrico não reconciliado.
- **Evidence:** estado validado da Fase 12; 45 testes aprovados; hashes críticos registrados em `ARTIFACT_MANIFEST.md`.
- **Tests:** build OK; 45 aprovados; 0 falhas; 0 diagnósticos.
- **Gate:** `GO RESTRICTED - VERSIONING ESTABLISHED / BASELINE STILL UNRECONCILED`.
- **Electrical model:** permanece `UNRECONCILED`; nenhum workbook candidato foi promovido.
