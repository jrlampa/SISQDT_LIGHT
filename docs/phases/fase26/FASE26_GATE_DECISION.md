# Fase 26 — Integridade da Proteção, Fail-Closed e Governança

**Branch:** `dev`
**Baseline elétrico:** `CONFIRMED_PARITY`
**Fase anterior:** Fase 25 concluída (`5af1ac7`)
**Estado deste checkout:** implementação validada antes do commit autorizado
**Roadmap seguinte:** `NEXT_PHASE_NOT_DEFINED`

## Decisão

A ausência de evidência do dispositivo/curva não produz corrente nominal recomendada, tempo de fusão presumido ou conclusão `Pass`. O cálculo da rede mantém corrente de projeto, Icc3φ e Icc1φ; quando seção e temperatura possuem proveniência identificada, calcula também o limite térmico de Onderdonk. A conclusão dependente de curva fica `EvidenceBlocked`, sem converter o cálculo elétrico em erro.

A Fase 24 permanece fechada no escopo declarado: curto-circuito trifásico/monofásico, propagação de impedância, proteção térmica calculável e integração. Curvas NH/disjuntor, coordenação e seletividade continuam bloqueadas por ausência de evidência digital apropriada.

## Proveniência dos Dados

| Dado | Origem no cálculo | Proveniência registrada | Sem evidência |
|---|---|---|---|
| Corrente de projeto `Ib` | Carga do trecho/transformador e tensão `V` (`BX6`; cargas `BW13`) | Fórmula CQTS e referências celulares nos registros das Fases 24/25 | Pipeline bloqueia se tensão conhecida ou carga requerida faltar |
| Catálogo de condutor | Ampacidade e R/X associados ao ID/chave do trecho | Entrada explícita em `NetworkModel.Conductors` | Trecho sem condutor correspondente bloqueia; nenhum catálogo sintético é criado |
| Transformador | Potência, impedância e tensão nominal associados ao `TransformerId` do circuito | Entrada explícita em `NetworkModel.Transformers` e parâmetro `V` conhecido | Circuito sem transformador correspondente bloqueia; nenhum `TR_DEF` é criado |
| `Icc3φ` | Impedância equivalente e célula `CB` | `CandidateShortCircuitRule`; `CQTS.REAL_PROJECT.LADO1_COLS_CB_CC` | Não depende de curva |
| `Icc1φ` | Impedância de laço fase-neutro e célula `CC` | `CandidateShortCircuitRule`; `CQTS.REAL_PROJECT.LADO1_COLS_CB_CC` | Não depende de curva |
| Seção do condutor | Mapeamento de catálogo para `CT38:CU40` | `CQTS.REAL_PROJECT.LADO1_CT38_CU40` | Seção e `t_adm` nulos; avaliação bloqueada |
| Temperatura do condutor | Regra validada de temperatura em regime | `F17-CQTS-PROJ7-LADO1-P13`, propagado somente quando a regra passa | `t_adm` nulo; avaliação bloqueada |
| Corrente nominal do dispositivo | Não fornecida pelo pipeline | Campo obrigatório em `ProtectionDeviceEvidence`, junto de fonte, hash, fabricante, modelo e curva | Ausente; não há recomendação nominal |
| Pré-arco/fusão e tempo total de interrupção | Não fornecidos pelo pipeline | O contrato aceita tempo total de interrupção somente com evidência explícita | Ausentes; sem curva, conclusão bloqueada |
| I²t e curva tempo-corrente completa | Não consumidos nesta fase | Fora do contrato/cálculo atual | Não inferidos; coordenação/seletividade indisponíveis |
| Capacidade de interrupção | Não fornecida pelo pipeline | Campo obrigatório do contrato para avaliação explícita | Ausente; adequação nula e bloqueada |
| Fabricante, modelo, curva e condição de ensaio | Não fornecidos pelo pipeline | Metadados obrigatórios de `ProtectionDeviceEvidence` | Evidência ausente/inválida; conclusão bloqueada |

O Evidence Store histórico permanece ausente. O contrato carrega referência e SHA-256 sintaticamente validados, mas a aplicação não recupera o arquivo externo nem valida o hash contra seu conteúdo. O fixture de teste P10 não é evidência de fabricante nem golden elétrico.

## Paridade dos Goldens

| Corpus/ponto | `Icc3φ` (A) | `Icc1φ` (A) | Classificação |
|---|---:|---:|---|
| PROJ 7, TR→LID | 8098.066930449716 | 8182.488711140876 | `TOLERANCE_PARITY`, tolerância absoluta `1e-9 A` |
| PROJ 4, TR→LID | 8098.06418783237 | 8182.474839937445 | `TOLERANCE_PARITY`, tolerância absoluta `1e-9 A` |
| PROJ 7, LID→P1 e ponta | Oráculos completos preservados nos testes | Oráculos completos preservados nos testes | `TOLERANCE_PARITY`, tolerância absoluta `1e-9 A` |
| Fluxo WPF ponta a ponta | Presença/faixa nos segmentos | Presença de corrente positiva na ponta | `BEHAVIORAL`; fixture não equivale ao golden da linha 13 |

Os testes anteriores usavam arredondamento decimal de quatro casas. A tentativa de igualdade binária mostrou diferenças de poucos ULPs nos valores reconstruídos; o motor não foi alterado para forçar igualdade. A tolerância absoluta estreita substitui a alegação histórica de “bit-a-bit” para esses casos. A decomposição de impedância segue verificada com precisão decimal nos asserts próprios.

## Gates

| Gate | Estado | Evidência/limite |
|---|---|---|
| `PROTECTION_PROVENANCE_GATE` | `GO` restrito | Entradas calculáveis têm IDs de regra/célula; dispositivo ausente é explicitamente `Missing`. Evidence Store/hash externo não verificado pelo aplicativo. |
| `NO_SILENT_DEFAULT_GATE` | `GO` | Removidos defaults de fusível/tempo, fallback de seção, catálogos de condutor sintéticos e transformador `TR_DEF`; temperatura heurística não alimenta Onderdonk. |
| `FAIL_CLOSED_GATE` | `GO` | Evidência ausente/inválida produz `Blocked` na regra e `EvidenceBlocked` na avaliação; o resultado global do cálculo permanece `Pass`. |
| `SHORT_CIRCUIT_REGRESSION_GATE` | `GO` | Golden values e cálculo dos segmentos preservados sem curva. |
| `THERMAL_REGRESSION_GATE` | `GO` restrito | Onderdonk calculado quando seção e temperatura têm IDs identificáveis; `t_adm` nulo nos demais casos. |
| `WPF_EVIDENCE_GATE` | `GO` | WPF exibe `EvidenceBlocked`, não `CalculationError`, mantém segmentos/Icc e não sugere fusível. |
| `PARITY_GATE` | `GO` por tolerância | Goldens completos preservados; tolerância absoluta declarada de `1e-9 A`, não igualdade binária. |
| `DOCUMENTATION_GATE` | `GO` restrito | README/RAG/CAC/CHANGELOG/índice atualizados; manifesto permanece na release commitada Fase 25 até autorização. |
| `DOMAIN_ISOLATION_GATE` | `GO` | Tipos de evidência/status ficam no Domain; sem dependência nova de UI/infraestrutura. |
| `BUILD_GATE` | `GO` | `dotnet restore` e `dotnet build --no-restore` concluídos sem erro. |
| `TEST_GATE` | `GO` | `dotnet test --no-build --no-restore`: 149 total, 149 aprovados, 0 falhas, 0 ignorados. |

**Build:** 0 erros e 0 warnings. **Suíte:** 149 passed / 0 failed / 0 skipped.

## Git e Próxima Fase

O checkout inicial estava na branch `dev`, HEAD `6584042`, release `0.6.0`, com exclusão staged de `src/QdtCqts.Desktop.Wpf/QdtCqts.Desktop.Wpf_dwkfellp_wpftmp.csproj`. O arquivo contém referências locais a `obj/`, SDK e assemblies, sendo artefato gerado; a exclusão foi preservada sem alteração.

Nenhum commit foi criado. `VERSION_MANIFEST.json` ainda identifica a última release commitada (Fase 25); sua atualização e o commit de governança aguardam autorização explícita conforme a sequência definida para a Fase 26.

Não há roadmap oficial posterior à Fase 26: `NEXT_PHASE_NOT_DEFINED`.