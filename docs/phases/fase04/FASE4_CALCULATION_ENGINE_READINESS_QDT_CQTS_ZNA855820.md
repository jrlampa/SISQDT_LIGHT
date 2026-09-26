# Fase 4 — Calculation Engine Readiness Report

**Projeto:** ZNA855820  
**Data:** 2026-09-26  
**Base:** auditoria, especificacao Fase 2 e matriz Fase 3  
**Decisao:** **NO-GO para motor QDT/CQTS de producao**. **GO restrito** para SQLite, dominio, importador somente leitura, evidencia, precedentes, harness e testes de infraestrutura. O fechamento da Fase 5 esta em [FASE5_GATE_DECISION_QDT_CQTS_ZNA855820.md](FASE5_GATE_DECISION_QDT_CQTS_ZNA855820.md).

## 1. Sumario executivo

A evidencia atual permite separar o VBA operacional do motor matematico, mas ainda nao permite declarar que todas as dependencias criticas do calculo QDT estao conhecidas. O link `DecInv` permanece indisponivel e e usado diretamente por `PF e Prestação de Serviço`. O QDT contem `#REF!` em formulas da `Base de Dados` e `FML`. Os nomes `solver_*` estao presentes no CQTS, mas nenhuma chamada explicita ou metadado textual de Solver foi localizado no pacote; isso reduz o risco, mas nao prova que uma execucao anterior do Solver nao tenha alterado caches. O golden dataset possui sete identificadores, porem quatro casos sao parciais e tres bloqueados.

Nao ha base tecnica para liberar implementacao de formulas eletricas de producao. Ha base suficiente para implementar a infraestrutura que tornara a proxima investigacao reproduzivel.

## 2. Status final dos bloqueios

| ID | Tema | Status | Classificacao final | Impacto | Evidencia | Proxima acao |
|---|---|---|---|---|---|---|
| BLOCK-QDT-001 | `DecInv` | **BLOCKED** | `BLOCKED — SOURCE UNAVAILABLE` | alto/bloqueante se o fluxo administrativo participar do escopo QDT | unidade `J:` indisponivel; arquivo nao encontrado localmente; formulas diretas em `PF e Prestação de Serviço!D2:D12`, `D14:D15`, `K54:K55`, `Q58`; 2 partes externalLink | obter workbook ou isolar formalmente `PF` fora do escopo eletrico |
| BLOCK-QDT-002 | VBA | **RESOLVED — NON-CALCULATION** | `VBA-CALC-STATUS: NO EVIDENCE OF CALCULATION` | baixo para matematica; medio para UX/importacao | 15 componentes extraidos; rotinas observadas tratam abertura, selecao, protecao, filtros, layout e formulario; nenhuma rotina eletrica identificada | encerrar como bloqueio matematico; portar apenas comportamento seguro se necessario |
| BLOCK-QDT-003 | Solver | **INVESTIGATING** | `REQUIRES LIMITED VERIFICATION` | alto ate provar que caches nao dependem de otimizacao | nomes `solver_*` em `RAMAL`/`PROJ1`; nenhuma chamada `SolverOk/SolverAdd/SolverSolve` encontrada; nenhum artefato textual Solver localizado no pacote | inspecao controlada de Solver pelo Excel ou aceitar formalmente que sao nomes residuais |
| BLOCK-QDT-004 | Precedentes criticos | **OPEN** | dependencia critica nao fechada | alto/bloqueante | `#REF!`, `INDEX/HLOOKUP/VLOOKUP`, structured references, external links e escopos de nomes | gerar grafo normalizado para saidas criticas |
| BLOCK-QDT-005 | Unidades | **INVESTIGATING** | criticas parcialmente confirmadas | alto | kVA, MVA, kV, A, °C e m confirmados por cabecalhos; R/X/ETA/R CORR e algumas conversoes ainda pendentes | concluir catalogo por formula e caso golden |
| BLOCK-QDT-006 | Golden dataset | **OPEN** | insuficiente para gate | bloqueante | 7 IDs; 3 `blocked`, 4 `partial`; nenhum caso completo dos quatro workbooks com inputs e intermediarios fechados | congelar snapshots completos |

### Decisao de reclassificacao do VBA

```text
VBA-CALC-STATUS = NO EVIDENCE OF CALCULATION
VBA nao constitui bloqueio do motor matematico.
VBA continua sendo evidencia operacional e de seguranca.
```

Essa conclusao nao afirma que cada byte foi semanticamente comparado entre ATUAL e PROJ; afirma que as rotinas extraidas e relevantes para o fluxo observado nao implementam calculo eletrico. Diferencas binarias entre os projetos continuam registradas como metadado, nao como bloqueio de formula.

## 3. Relatorio definitivo DecInv

### 3.1 Origem e tipo

- **Nome lógico:** `DecInv`.
- **Arquivo:** `NOVA PLANILHA DO CÁLCULO DA PARTICIPAÇÃO FINANCEIRA (A + B) RAMAL C LI_2016-2017 07-11-2016.xlsm`.
- **Caminho:** `J:\Celeridade 2016_3_SET\1Celeridade - LESTE\PLANILHAS\`.
- **Aba externa:** `07_nov_2016 a 06_nov_2017`.
- **Classificação:** dependência legada externa; suas células parecem fornecer dados de participação financeira/prestação de serviço, não foram demonstradas como cálculo elétrico.
- **Disponibilidade:** unidade `J:` inexistente no ambiente; busca local pelo nome não encontrou arquivo.

### 3.2 Ocorrencias verificadas

| Origem QDT | Dependencia |
|---|---|
| `PF e Prestação de Serviço!D2:D12` | B25, C25, C10:C17 da aba externa |
| `PF e Prestação de Serviço!D14:D15` | C21:O21 e C20:O20 da aba externa |
| `PF e Prestação de Serviço!K54:K55` | B25 da aba externa em regra condicional |
| `PF e Prestação de Serviço!Q58` | B25 da aba externa em regra condicional |

Os dois XLSM possuem `externalLink1.xml` e `externalLink2.xml`. O conteúdo e os valores retornados não podem ser determinados sem o workbook externo.

### 3.3 Teste de impacto

O teste de impacto conclusivo não pode ser executado sem a fonte. Não é permitido substituir a fonte por zero, cache ou valor inventado. A hipótese operacional atual é:

```text
DecInv -> PF e Prestação de Serviço -> possivelmente relatório/participação financeira
```

Não foi demonstrada uma cadeia `DecInv -> corrente/tensão/queda/carregamento` nas fórmulas críticas amostradas. Assim, `DecInv` é **bloqueante para paridade do workbook completo**, mas pode ser **não bloqueante para o núcleo elétrico** somente após um grafo de dependências comprovar que `PF` não alimenta `LADO 1/2`, `ANÁLISE PONTO A PONTO` ou resultados elétricos.

## 4. Parecer VBA

### Evidencias

| Componente | Rotinas | Natureza |
|---|---|---|
| `Módulo1` | `Orgao_DI`, `Dropdown21_Alteração`, `Filtrar`, `Mostrar`, `Assina_*` | layout, ocultação, controles, proteção e seleção |
| `EstaPasta_de_trabalho` | `Workbook_Open` | navegação para `Menu` |
| `Módulo3` | `Macro2`, `Macro3` | seleção/desproteção |
| `UserForm1` | `DesprotegeVBA`, `CopyFile`, `CommandButton1_Click` | manipulação de arquivo binário/proteção; risco de segurança |
| classes `Plan*` | vazias na extração | sem regra observada |

Nenhuma rotina extraída calcula potência, corrente, impedância, tensão, queda, carregamento ou status elétrico. O VBA não será portado como motor. `DesprotegeVBA` e `CopyFile` não serão portados.

### Impacto futuro

- `Workbook_Open` pode virar navegação inicial da UI.
- filtros e ocultação podem virar comandos seguros de visualização.
- proteção deve virar controle de permissões/edição, não senha Excel.
- nenhuma dessas adaptações deve alterar valores de cálculo.

## 5. Relatorio Solver

### Evidencia positiva

Existem nomes com escopo de aba, entre eles `solver_adj`, `solver_cvg`, `solver_lhs1..6`, `solver_rhs1..6`, `solver_rel1..6`, `solver_opt`, `solver_val`, `solver_typ`, `solver_tol`, `solver_itr`, `solver_tim`, `solver_mni`, `solver_pre` e outros em `RAMAL`/`PROJ1`.

### Evidencia negativa

- Nenhuma fórmula inspecionada contém `SolverOk`, `SolverAdd` ou `SolverSolve`.
- Nenhuma macro extraída chama o Solver.
- Nenhum texto `SolverOk`, `SolverAdd` ou `SolverSolve` foi localizado nos componentes XML pesquisáveis dos dois XLSM.
- As formulas encontradas em `PROJ1`/`RAMAL` são `SUMIF`, `IFERROR`, acumulacao e consultas de tabelas.

### Conclusao

```text
Solver = REQUIRES LIMITED VERIFICATION
```

O problema matemático não pode ser especificado sem objetivo, variáveis, restrições, método e saída verificáveis. A hipótese mais forte é configuração residual ou resultado de uma ação externa não capturada; não é suficiente para liberar um substituto.

### Ação final necessária

Executar inspeção controlada da configuração Solver em uma cópia de trabalho, sem salvar os arquivos originais, ou obter evidência documental de que os nomes são apenas resíduos. Caso não exista modelo ativo e os resultados das fórmulas não mudem, reclassificar como `RESOLVED — NON-CALCULATION/RESIDUAL CONFIGURATION`.

## 6. Relatorio de precedentes críticos

### Cadeia QDT/CQTS a fechar

```text
Entrada de ponto/trecho/ramal/carga/condutor
  -> carga direta e acumulada
  -> corrente / Ib / In / Iz
  -> tensão e queda
  -> carregamento e proteção
  -> status por trecho/ponto
  -> consolidação por lado/circuito
```

### Tipos de precedente identificados

| Tipo | Evidencia | Tratamento |
|---|---|---|
| Direto | referências de célula entre abas | resolver por endereço |
| Nome definido | `T_LINHA_MONO`, `T_LINHA_TRF`, `DMDI`, `CABOS` | resolver com escopo global/aba |
| Estruturado | `GERAL[CONDUTOR]`, `DIR_ATUAL[Ib]` | materializar tabela, linha e coluna |
| Externo | `PF` e `DecInv` | preservar como dependência faltante |
| Dinâmico | pesquisar `INDIRECT`, `OFFSET`, referências variáveis | classificar como não suportado até prova |
| Quebrado | `#REF!` em `Base de Dados`, `FML`, nomes CQTS | gerar erro de evidência, nunca substituir |

### Estado

`BLOCK-QDT-004` permanece `OPEN` porque não há grafo completo que prove que nenhum precedente externo/quebrado influencia uma saída elétrica crítica.

## 7. Catalogo de unidades e precisão

| Grandeza | Unidade Excel confirmada | Unidade interna | Conversão | Estado |
|---|---|---|---|---|
| Potência do transformador | MVA | MVA | 1 | READY |
| Impedância do transformador | % | percentual como valor Excel | 1 | READY para armazenamento; fórmula ainda a validar |
| Tensão de linha | kV | kV | 1 | READY |
| Carga no trecho/ponto | kVA | kVA | 1 | READY |
| Demanda | kVA | kVA | 1 | READY para armazenamento |
| Comprimento do trecho | m | m | 1 | READY |
| Corrente `Ib`, `In`, `Iz`, carregamento | A | A | 1 | READY |
| Temperatura | °C | °C | 1 | READY |
| Fase | `MONO`/`BIF`/`TRI` | enum textual | nenhuma | READY |
| R/X de ramal/cabo | `Ohms` no QDT; escopo pode ser por km | decimal, unidade anexada ao catálogo | depende da fórmula | PARTIAL |
| `R CORR` | cabeçalho sem unidade física explícita | decimal pendente | depende de temperatura/comprimento | PARTIAL |
| `ETA` | fator sem unidade | decimal adimensional | 1 | PARTIAL |
| `QT %` | percentual | decimal preservando valor Excel | somente quando fórmula confirmar | PARTIAL |
| Coordenadas UTM | X/Y | metros no CRS declarado | 1 | PARTIAL: CRS/zona pendentes |
| Seção do condutor | texto/nome de catálogo | `Conductor.catalog_key` | nenhuma | READY como identificação; área física pendente |

### Precisão

- Valores devem ser armazenados como `double`/`REAL` sem arredondamento de apresentação.
- Não usar o texto formatado como entrada de outro cálculo.
- Fórmulas amostradas não aplicam `ROUND`; isso não prova que nenhuma outra regra aplique arredondamento.
- A tolerância final permanece `PARTIAL` até o golden dataset completo. Comparações textuais/status são exatas; números devem preservar a ordem das operações e só então aplicar tolerância justificada.

## 8. Golden dataset mínimo e estado

| Caso | Cobertura | Estado |
|---|---|---|
| QDT ATUAL LADO 1 | transformador, carga, logica, kVA por consumidor | PARTIAL |
| QDT PROJ LADO 1/2 | diferencas de caches ATUAL/PROJ | BLOCKED |
| CQTS ATUAL LADO 1 linhas 8-10 | cadeia `TR,LID -> LID,P2 -> P2,P3`, acumulada, corrente, proteção | PARTIAL |
| CQTS PROJ PROJ1/GERAL PROJ | condutores, pontos, sobrecarga e metragem | PARTIAL |
| Caso simples de um trecho | ainda não isolado em snapshot completo | BLOCKED |
| Alteração de carga | diferença observada, entrada completa ausente | BLOCKED |
| Alteração de condutor | valores observados, experimento controlado ausente | BLOCKED |
| Alteração de comprimento | não congelado | BLOCKED |
| Múltiplos ramos | topologia inferida, snapshots completos ausentes | BLOCKED |

**Conclusão:** o conjunto não atende ao mínimo de GO. É suficiente para orientar o extrator e a estrutura de testes, não para validar o motor.

## 9. Validação cruzada QDT x CQTS

| Cálculo | Classificação | Justificativa |
|---|---|---|
| Carga direta/acumulada em kVA | **PARCIALMENTE EQUIVALENTE** | ambos possuem carga por ponto/trecho e acumulada, mas fórmulas e fatores diferem |
| Corrente por fase | **PARCIALMENTE EQUIVALENTE** | CQTS explicita caminho mono/trifásico; QDT usa colunas/regras próprias ainda não fechadas |
| Condutor e ampacidade | **PARCIALMENTE EQUIVALENTE** | ambos consultam catálogos/limites, mas catálogos e contexto podem diferir |
| Tensão nominal | **PARCIALMENTE EQUIVALENTE** | grandezas de tensão existem nos dois, sem prova de mesma transformação |
| Queda de tensão | **NÃO COMPARÁVEL** | fórmulas QDT completas ainda não fechadas; CQTS usa cadeia própria e fallback |
| Carregamento/proteção | **PARCIALMENTE EQUIVALENTE** | ambos produzem limites/status, mas critérios não estão provados iguais |
| Centro de carga | **DIFERENTE** | CQTS usa `SUMPRODUCT` específico; não há equivalente QDT comprovado |
| Topologia radial | **EQUIVALENTE COMO ESTRUTURA** | QDT linear pode ser representado como caso degenerado da árvore; isso não implica fórmula igual |
| Relatórios administrativos/participação | **NÃO COMPARÁVEL** | QDT depende de `DecInv`; CQTS não apresenta a mesma dependência |

O CQTS pode servir como referência estrutural e como teste parcial de acumulação/limites, nunca como oráculo substituto do QDT.

## 10. Arquitetura confirmada

```text
Topology Model
  -> TopologyValidator / ordem topologica
  -> CalculationRequest
  -> QDT Engine ou CQTS Engine
  -> resultados por nó, trecho, circuito, ramo e ponta
  -> CalculationRun / SQLite
```

A topologia não contém fórmulas elétricas. `Node`, `Edge`, `Parent`, `Children` e `Load` representam conexões e entradas; corrente, tensão, queda e carregamento pertencem aos motores.

A arquitetura-base permanece `.NET + WPF + SQLite3 + Domain Model + Calculation Engine`. Não há evidência para reabrir a decisão tecnológica.

## 11. Calculation Engine Readiness Matrix

| Regra | Origem | Fórmula/regra | Entrada | Unidade | Precisão | Teste | Status |
|---|---|---|---|---|---|---|---|
| Composição de carga | QDT `Ramais`, CQTS `RAMAL` | `a*0.85+b*0.5268` | duas entradas de carga | kVA | interna Excel | QDT/CQTS ramal | PARTIAL |
| KVA por consumidor | QDT `LADO 1!K13` | condicional por `D`, `E`, `G` | consumidores, fator, grupo | kVA | interna Excel | QDT LADO1 | PARTIAL |
| Seleção carga K/L | QDT `LADO 1!M13` | `IF(CH5="SIM",K,L)` | flag e cargas | kVA | exata | QDT LADO1 | READY como fórmula isolada |
| Validação lógica | QDT `LADO 1!I13` | `OK !` ou `Erro 02` | D/H/D14/D13 | enum/status | exata | QDT LADO1 | READY como fórmula isolada |
| Acumulada CQTS | `CQTS LADO 1`, `PROJ1` | `SUMIF` por `PONTO/TRECHO` | pontos, montante, cargas | kVA | interna Excel | CQTS linha 8-10 | PARTIAL |
| Corrente CQTS | `CQTS LADO 4!X8` | ramo por fase com `SQRT(3)` | acumulada, tensão, ETA, fase | A | interna Excel | CQTS LADO | PARTIAL |
| Proteção CQTS | `CQTS LADO 1!AA8` | comparar `Ib <= In <= Iz` | Ib/In/Iz | A/status | exata | CQTS LADO | READY como regra isolada |
| Queda QDT | QDT `LADO 1/2` | não fechada | carga, condutor, trecho, fase | pendente | pendente | QDT | BLOCKED |
| Queda CQTS | `CQTS LADO 4!L8` | fallback acumulada/coordenada | acumulada, condutor, coordenada, fase | pendente | interna Excel | CQTS | PARTIAL |
| Impacto DecInv | QDT `PF` | referencias externas | workbook ausente | pendente | pendente | DecInv | BLOCKED |
| Solver | nomes `solver_*` | objetivo/restrições desconhecidos | desconhecidas | pendente | pendente | Solver | UNKNOWN/PARTIAL |
| Tolerância final | quatro workbooks | ainda não calibrada | golden completo | por variável | pendente | parity suite | BLOCKED |

## 12. Gate do motor

### Critérios avaliados

- [x] VBA sem evidência de cálculo elétrico oculto nas rotinas extraídas.
- [ ] Não existem dependências matemáticas críticas desconhecidas.
- [ ] `DecInv` resolvido ou formalmente isolado do escopo elétrico.
- [ ] Solver matematicamente especificado ou formalmente reclassificado como residual.
- [ ] Precedentes críticos rastreados.
- [ ] Unidades críticas confirmadas, incluindo R/X/ETA/queda.
- [ ] Golden dataset mínimo completo.
- [ ] Precisão e arredondamentos críticos definidos.
- [ ] Critérios de tolerância definidos por variável.

### Parecer

```text
NO-GO para implementação do Calculation Engine de produção.
GO restrito para infraestrutura, importador, domínio, topologia, evidência e testes.
```

Motivos determinantes: `DecInv` indisponível, grafo de precedentes incompleto, Solver sem semântica comprovada, unidades críticas parciais e golden dataset insuficiente.

## 13. Backlog pós-Fase 4

1. Obter o workbook `DecInv` ou aprovar formalmente o isolamento de `PF e Prestação de Serviço` fora do escopo elétrico.
2. Implementar o extrator de precedentes no SQLite, preservando `#REF!`, nomes e referências estruturadas.
3. Fazer inspeção limitada do Solver em cópia e registrar objetivo/variáveis/restrições ou classificar resíduos.
4. Fechar R/X/ETA/R CORR, CRS UTM, percentuais e arredondamentos com fórmulas e catálogo.
5. Capturar datasets completos dos quatro workbooks com entradas, intermediários e resultados.
6. Criar apenas testes unitários de regras já `READY` e testes de topologia; não criar ainda o engine elétrico completo.
7. Reexecutar este gate após os itens 1-5.

## 14. Resposta objetiva

**Temos conhecimento suficiente para implementar o motor QDT sem depender do Excel?** Não.

**Temos conhecimento suficiente para implementar infraestrutura, topologia independente, repositórios, importador e harness?** Sim.

**O VBA é bloqueio matemático?** Não. Classificação: `VBA-CALC-STATUS = NO EVIDENCE OF CALCULATION`.

**O CQTS substitui o QDT como oráculo?** Não. Serve como referência parcial e estrutural.

**A arquitetura muda?** Não. `.NET + WPF + SQLite3 + Domain Model + Calculation Engine` permanece adequada.
