# Fase 3 — Resolucao dos bloqueios tecnicos e especificacao UX

**Projeto:** ZNA855820  
**Data:** 2026-09-26  
**Base:** `AUDITORIA_QDT_CQTS_ZNA855820.md` e `ESPECIFICACAO_QDT_CQTS_ZNA855820.md`  
**Parecer:** **NO-GO para o motor eletrico de producao**; **GO restrito para fundacao SQLite, catalogo de evidencia, importador somente leitura, harness de paridade e prototipo UX sem calculo de producao**. O fechamento da Fase 4 esta em [FASE4_CALCULATION_ENGINE_READINESS_QDT_CQTS_ZNA855820.md](FASE4_CALCULATION_ENGINE_READINESS_QDT_CQTS_ZNA855820.md).

## 1. Resumo do parecer

A Fase 3 resolveu parte relevante da incerteza estrutural, mas nao atingiu o gate de paridade. O link `DecInv` alimenta diretamente `PF e Prestação de Serviço`; o arquivo externo nao foi fornecido. O QDT possui formulas `#REF!` em `Base de Dados` e `FML`. O VBA foi extraido, mas os binarios ATUAL e PROJ diferem e ainda nao houve comparacao normalizada modulo a modulo. Os nomes `solver_*` foram confirmados no CQTS, mas nenhuma chamada explicita de Solver, objetivo ou restricao foi localizada nas formulas inspecionadas. Os golden cases ainda sao snapshots parciais, nao datasets completos de entrada/intermediario/resultado.

Essas lacunas podem alterar resultados. Portanto, nao e aceitavel marcar os bloqueios como `ACCEPTED RISK` para liberar o motor.

## 2. Matriz de bloqueios

Estados permitidos: `OPEN`, `INVESTIGATING`, `RESOLVED`, `ACCEPTED RISK`, `BLOCKED`.

| ID | Bloqueio | Impacto | Evidencia atual | Acao exigida | Estado |
|---|---|---|---|---|---|
| BLOCK-QDT-001 | Link externo `DecInv` | alto/bloqueante | Excel expoe caminho `J:\Celeridade 2016_3_SET\...xlsm`; formulas diretas em `PF e Prestação de Serviço!D2:D12`, `D14:D15`, `K54:K55`, `Q58` | obter o arquivo externo ou isolar todas as celulas dependentes e capturar valores/calculo em execucao controlada | **BLOCKED** |
| BLOCK-QDT-002 | VBA ATUAL x PROJ | baixo para matematica, medio para UX/importacao | `olevba` extraiu 15 componentes nominais; rotinas observadas sao operacionais, sem calculo eletrico | registrar `VBA-CALC-STATUS = NO EVIDENCE OF CALCULATION`; nao portar desprotecao; preservar diferenca binaria como metadado | **RESOLVED — NON-CALCULATION** |
| BLOCK-QDT-003 | Solver | alto | nomes `solver_*` em `RAMAL`/`PROJ1`; nenhuma chamada explicita `SolverOk`, `SolverAdd`, `SolverSolve` ou macro equivalente nas formulas/abas inspecionadas | extrair metadados de Solver do pacote/Excel e documentar objetivo, variaveis, restricoes e resultado; confirmar se e configuracao residual | **INVESTIGATING** |
| BLOCK-QDT-004 | Precedentes criticos | alto | formulas `#REF!`, `INDEX/HLOOKUP/VLOOKUP`, tabelas estruturadas, link externo e aliases por escopo | gerar grafo completo por formula normalizada e resolver precedentes de corrente, tensao, queda, carregamento e status | **OPEN** |
| BLOCK-QDT-005 | Unidades | alto | cabecalhos confirmam `[kVA]`, `[kV]`, `[A]`, `[°C]`, `(m)`, `[Ohms]`; algumas unidades de R/X/T/ETA permanecem incertas | validar cada campo contra formula, catalogo e valores; registrar conversao e precisao | **INVESTIGATING** |
| BLOCK-QDT-006 | Golden dataset incompleto | bloqueante | existem caches reais, mas nao snapshots completos de entradas, intermediarios e resultados para os quatro arquivos | congelar casos QDT/CQTS ATUAL/PROJ, por celula e por entidade de dominio | **OPEN** |

### Regra de transicao

- `RESOLVED` somente com evidencia reproduzivel anexada ao caso.
- `ACCEPTED RISK` nao e aplicavel aos bloqueios 001, 002, 004 e 006 sem demonstrar que a lacuna nao influencia os resultados criticos.
- `BLOCKED` significa que a evidencia faltante esta fora do workspace ou impede experimento seguro.

O parecer consolidado e a matriz atualizada de prontidao estao em [FASE4_CALCULATION_ENGINE_READINESS_QDT_CQTS_ZNA855820.md](FASE4_CALCULATION_ENGINE_READINESS_QDT_CQTS_ZNA855820.md).

## 3. Resolucao dos bloqueios

### 3.1 BLOCK-QDT-001 — `DecInv`

**Classificacao:** dependencia externa de dados/calculo, nao simples metadado.

Evidencias:

```text
QDT!PF e Prestação de Serviço!D2:D12
QDT!PF e Prestação de Serviço!D14:D15
QDT!PF e Prestação de Serviço!K54:K55
QDT!PF e Prestação de Serviço!Q58
```

As formulas referenciam a aba `07_nov_2016 a 06_nov_2017` do arquivo legado:

```text
J:\Celeridade 2016_3_SET\1Celeridade - LESTE\PLANILHAS\
[NOVA PLANILHA DO CÁLCULO DA PARTICIPAÇÃO FINANCEIRA (A + B)
RAMAL C LI_2016-2017 07-11-2016.xlsm]
```

O conteudo retornado por essas celulas nao foi comprovado sem o arquivo externo. O QDT tambem possui o nome/link `DecInv`. Nao e permitido substituir por hardcode, zero ou valor do cache sem preservar a origem e declarar a dependencia.

**Acoes:**

1. localizar o arquivo externo na unidade/rede autorizada;
2. registrar hash, abas e celulas referenciadas;
3. capturar valores de ATUAL e PROJ em modo somente leitura;
4. determinar se `PF e Prestação de Serviço` e requisito do motor QDT ou apenas relatorio administrativo;
5. se o arquivo permanecer indisponivel, bloquear qualquer paridade que atravesse esse fluxo e manter o recurso como `external_dependency_missing`.

### 3.2 BLOCK-QDT-002 — VBA

Inventario confirmado:

| Modulo | Evidencia | Efeito observado |
|---|---|---|
| `Módulo1` | `Orgao_DI`, `Dropdown21_Alteração`, `Filtrar`, `Mostrar`, `Assina_*` | ocultacao, filtros, controles, protecao e selecao |
| `EstaPasta_de_trabalho` | `Workbook_Open` | seleciona `Menu` |
| `Módulo3` | `Macro2`, `Macro3` | selecao/desprotecao |
| `UserForm1` | `DesprotegeVBA`, `CopyFile`, `CommandButton1_Click` | copia/abre arquivo binario e tenta desproteger VBA |
| classes `Plan*` | vazias na extracao | nenhum evento observado |

O mesmo conjunto nominal foi identificado em PROJ, mas o hash `vbaProject.bin` e diferente. Isso pode ser metadado, compressao, projeto/versao ou codigo. Ainda nao se deve concluir equivalencia.

**Portabilidade:** nao portar `DesprotegeVBA`, `CopyFile`, `Open`, `Put`, `Binary` nem quebra de senha. Portar apenas efeitos funcionais seguros como filtros e estado de visibilidade, caso sejam necessarios na UI.

### 3.3 BLOCK-QDT-003 — Solver

Os nomes encontrados incluem `solver_adj`, `solver_cvg`, `solver_lhs*`, `solver_rhs*`, `solver_rel*`, `solver_opt`, `solver_val`, `solver_typ`, `solver_tol`, `solver_itr`, `solver_tim`, `solver_mni`, entre outros, com escopo em `RAMAL` e `PROJ1`.

A inspecao das formulas em `DB`, `RAMAL` e `PROJ1` encontrou formulas de tabela, acumulacao e `SUMIF`, mas nao encontrou chamadas explicitas ao Solver. Portanto, a evidencia atual suporta duas hipoteses:

1. configuracao residual salva pelo Excel/ suplemento Solver; ou
2. Solver executado externamente por uma acao nao capturada nas formulas.

**Nao determinado:** objetivo, variaveis de decisao, restricoes, algoritmo, valores iniciais e se o resultado atual depende de uma execucao anterior.

**Acao:** obter a informacao de Solver pelo Excel/arquivo em copia de trabalho e capturar antes/depois de uma execucao autorizada, sem salvar os originais. Nao implementar otimizacao equivalente antes desse experimento.

### 3.4 BLOCK-QDT-004 — precedentes

Prioridade do grafo:

```text
entradas de ponto/trecho/carga/condutor
  -> acumulada e demanda
  -> corrente Ib/In/Iz
  -> tensao e queda
  -> limites/status
  -> consolidacao por lado e resultado final
```

O grafo deve preservar tipos de referencia: direta, nome definido, tabela estruturada, dinamica, externa e VBA. `#REF!` deve ser um no de erro, nunca silenciosamente removido. O importador deve materializar a celula origem, formula original, formula normalizada, precedentes e valor cacheado.

### 3.5 BLOCK-QDT-005 — unidades

Catalogo inicial, com certeza explicita:

| Campo/cabecalho | Unidade exibida | Unidade de dominio | Conversao | Certeza |
|---|---|---|---|---|
| `LADO!CARGA no fim do trecho` | kVA | kVA | 1 | confirmada |
| `LADO!Potência` | MVA | MVA | 1 | confirmada |
| `LADO!Tensão` | kV | kV | 1 | confirmada |
| `LADO!Temp. do cabo` | °C | °C | 1 | confirmada |
| `RAMAL!Carregamento` | A pelo contexto | A | 1 | confirmada pelo cabecalho |
| `RAMAL!R`, `X` | Ohms | ohm, escopo a determinar | possivel `/1000` em formulas | media |
| `CQTS!TRECHO` | m | m | 1 | confirmada |
| `CQTS!CARGA PONTO`, `ACUMULADO` | kVA | kVA | 1 | confirmada |
| `CQTS!Ib`, `In`, `Iz`, `AMP` | A pelo cabecalho/validacao | A | 1 | confirmada |
| `CQTS!FASE` | texto `MONO`, `BIF`, `TRI` | enum | nenhuma | confirmada |
| `CQTS!ETA` | fator | adimensional | 1 | campo confirmado, significado fisico pendente |
| `CQTS!R CORR` | parametro resistivo | ohm/unidade pendente | pendente | media |
| `QT %` | percentual | proporcao/percentual | fórmula especifica | media |
| `COORDENADAS X/Y` | UTM | metro no sistema declarado | 1 | sistema/zone a confirmar |

A regra e nao converter por intuicao. Cada conversao deve ser testada com a formula original e um caso golden.

## 4. Golden dataset — estado atual

Os casos abaixo sao reais, mas ainda parciais. O status `blocked` e intencional: caches observados nao constituem, sozinhos, um dataset completo de paridade.

| TEST-ID | Arquivo/aba | Entradas observadas | Intermediarios/resultados observados | Estado |
|---|---|---|---|---|
| TEST-QDT-ACTUAL-LADO1-001 | QDT ATUAL `LADO 1` | `C6=40`, `D6=20`, `E6=13.2`, `G6=2`, `H6=2` | `I13=OK !`, `K13=186.08282352941154`, `L13=186.08282352941154`, `M13=186.08282352941154` | blocked: faltam todas entradas dependentes e resultados por trecho |
| TEST-QDT-PROJ-LADO1-001 | QDT PROJ `LADO 1` | estrutura equivalente | valores cacheados diferem em `LADO 1/2` | blocked |
| TEST-CQTS-ACTUAL-LADO1-001 | CQTS ATUAL `LADO 1` | linha 8: `PONTO=1`, `TRECHO=0`, `D=4`, `H=22.5224`, `J=240 Cu`, `FASE=TRI`, `U=3`, `W=20` | `I=186.082823529412`, `K=162.780254933745`, `X=162.780254933745`, `Y=160`, `Z=430`, `AA=VERIFICAR` | partial |
| TEST-CQTS-ACTUAL-LADO1-002 | CQTS ATUAL `LADO 1` | linha 9: `PONTO=2`, `PONTO MONTANTE=1`, `D=40`, `H=91.5896`, `J=185 Al - MX` | `I=97.4552`, `X=85.2511908392861`, `Y=80`, `Z=355`, `AA=VERIFICAR` | partial |
| TEST-CQTS-ACTUAL-LADO1-003 | CQTS ATUAL `LADO 1` | linha 10: `PONTO=3`, montante 2, `D=28`, `H=7.2944`, `J=70 Al - MX` | `I=7.2944`, `X=6.38094515693455`, `Y=6`, `Z=202`, `AA=VERIFICAR` | partial |
| TEST-CQTS-PROJ-001 | CQTS PROJ `PROJ1`/`GERAL PROJ` | pontos projetados, `J2=SIM`, `M=50`, condutores `240 Cu`, `70 Al - MX`, `185 Al - MX` | `Q2=SOBRECARGA`, `T4=28`, `T5=80`, diferencas de metragem | partial |

**Para congelar:** exportar todas as entradas e dependencias do circuito, não somente as linhas exibidas, e registrar `SourceCell` para cada observação.

## 5. Auditoria UX baseada em evidencia

### 5.1 Imposicao de layout pelo Excel

| Aba | Print Area | Orientacao | Papel | Ajuste | Uso de tela inferido |
|---|---|---|---|---|---|
| `Ramais` | `B2:T80` | retrato | A4 (`PaperSize=9`) | 1 pagina de largura e altura | tabela compacta de catalogo/ramais |
| `Distrib. Cargas` | `B1:H29` | retrato | A4 | 1 x 1 | pequena grade de postes/cargas |
| `LADO 1/2` | `B1:CK53` | paisagem | A4 | 1 pagina de largura e altura | janela operacional larga, com colunas auxiliares ocultas |

`LADO` tem `UsedRange=A1:IE361`, mas a area de impressao termina em `CK53`; 209 linhas e 105 colunas estao ocultas no recorte auditado. Isso reforca que a tela do app deve priorizar o recorte operacional, com painel de detalhes sob demanda para colunas auxiliares.

Nao foi confirmado congelamento de paineis ativo nas abas auditadas. O app deve oferecer congelamento explicito de identificadores, mas nao afirmar que replica uma configuracao Excel inexistente.

### 5.2 Convencoes visuais

- Cabecalhos/secoes: azul escuro (`Interior.Color=12419407`) com texto branco.
- `LADO` possui cabecalhos por grupo com cores diferentes para entradas, logica, cargas e temperatura; `CARGA` aparece com preenchimento proprio e colunas de resultado sao bloqueadas/visualmente distintas conforme o recorte.
- `Distrib. Cargas` possui `Poste 1` destacado e postes seguintes editaveis; colunas calculadas aparecem bloqueadas e algumas usam preenchimento cinza/azul claro.
- `Ramais` combina catalogo de condutor/ampacidade e campos derivados; a visualizacao deve manter cabecalho forte e tabela densa.
- As cores sao evidencia de agrupamento, nao contrato semantico completo. A classificacao final entrada/calculo/resultado deve usar formula e propriedade de protecao conjuntamente.

## 6. Modelo UX

### 6.1 Janela principal

```text
+--------------------------------------------------------------------------------+
| Projeto: ZNA855820 | Versao: ATUAL | Modelo: QDT | [Salvar] [Calcular] [Validar]|
+----------------------+---------------------------------------------------------+
| Navegacao             | Barra de contexto: LADO 1 | status | ultima execucao       |
|                      +---------------------------------------------------------+
| > Projeto             | Grid operacional (prioridade de espaco)                 |
| > Lado                |                                                         |
| > Ramal               | colunas editaveis e calculadas, estilo planilha         |
| > Dist. de carga     |                                                         |
| > Rede / Arvore      |                                                         |
| > Resultados         |                                                         |
| > Evidencia          |                                                         |
+----------------------+---------------------------------------------------------+
| Status/validacoes: erros, avisos, fonte da celula, resultado stale             |
+--------------------------------------------------------------------------------+
```

A navegacao deve ser estreita e recolhivel; o grid ocupa o espaco dominante. Resultados criticos ficam em uma faixa inferior persistente, não em cards espalhados.

### 6.2 Tela `Lado`

Ordem preservada da planilha:

1. bloco de transformador/circuito;
2. bloco de identificacao e trecho;
3. demanda por consumidor e FDIV;
4. fases e tipo de trecho;
5. carga escolhida;
6. cabos, temperatura e parametros;
7. corrente, tensao, queda e status;
8. colunas auxiliares em painel secundario.

Interacao: edição direta no grid, com formula/celula calculada bloqueada. Ao selecionar uma célula calculada, o painel de evidencia mostra fórmula, precedentes e valor Excel/App.

### 6.3 Tela `Ramal`

- uma grade principal para ramais;
- identificacao do ramal, circuito/lado, ponto/trecho, condutor, clientes e dados de carga;
- resultados do ramal ao lado ou em colunas fixas;
- filtro por circuito/condutor/status;
- adicionar/duplicar/remover ramal por comandos de teclado;
- alteração cria versão de trabalho e marca resultados como `stale`.

A interface deve manter a relação Ramal -> circuito -> trecho/ponto visível no contexto da linha, sem obrigar o usuário a abrir modal.

### 6.4 Tela `Dist. de Carga`

- grade pequena e densa, próxima de `Poste`, carga, acumulada e total;
- entradas distinguíveis por desbloqueio e estilo;
- totais e fórmulas visíveis na mesma região;
- ligação por seleção para o trecho/ponto correspondente no `Lado` ou `Ramal`;
- mensagens de erro na própria linha, além do painel de validação.

A sequência `Lado -> Ramal -> Dist. de Carga` é tratada como fluxo de trabalho proposto, mas a dependência exata deve ser validada com usuário e grafo de precedentes.

### 6.5 Tela `Rede / Arvore`

Duas visões sincronizadas:

```text
[Tabela] [Arvore]

Tabela: Ponto | Montante | Trecho | m | Carga | Acumulada | Condutor | Status
Arvore: TRAFO -> P1 -> P2
                  \-> P3 -> P4
```

Selecionar uma linha destaca o nó/trecho na árvore; selecionar um nó destaca a linha. A árvore complementa, não substitui, o grid.

### 6.6 Resultados

A faixa persistente mostra: demanda, corrente máxima, tensão mínima, queda, carregamento, status e quantidade de erros/avisos. O detalhe expande para resultados por circuito, trecho, nó, ramo e ponta.

## 7. Mapa Excel -> WPF

| Excel | Tela/controle WPF | Observacao |
|---|---|---|
| `LADO 1`, `LADO 2` | `LadoView` + `LadoDataGrid` | mesma ordem operacional, colunas auxiliares recolhiveis |
| `Ramais` | `RamalView` + `RamalDataGrid` | editar sem navegar por telas |
| `Distrib. Cargas` | `DistribuicaoCargaView` | grid compacto de postes/cargas |
| `ANÁLISE PONTO A PONTO` | `AnalisePontoView` | evidencia e resultados por ponto |
| `COORDENADAS` | `CoordenadasView` | dados UTM e selecao de ponto |
| `LADO 1..4` CQTS | `CircuitoView`/`RedeView` | tabela + arvore |
| `PROJ1`, `GERAL PROJ` | `ProjetoComparacaoView` | ATUAL x PROJ e saturacao |
| `SUGESTAO_CORTES` | `ValidacaoView` | somente apos regra confirmada |
| `DB`, `Base de Dados` | `ParametrosView` | catalogo e metadados, nao grade crua |
| `Print_Area` | `PrintPreviewView` | referencia de conjunto, nao limite de edicao |

## 8. Interacao mouse + teclado

- `Tab` avanca para a proxima célula editável; células calculadas são puladas ou selecionáveis como leitura.
- `Enter` confirma e avança na mesma coluna; `Shift+Enter` volta.
- Setas navegam célula a célula.
- `Ctrl+C`/`Ctrl+V` preservam tipos e validam colunas de destino.
- `Ctrl+Z` desfaz alteração no draft; não reescreve versão congelada.
- `F2` edita célula; `Esc` cancela.
- `Ctrl+F` localiza ponto, ramal, trecho ou condutor.
- `Ctrl+G` abre navegação para célula/entidade equivalente.
- Duplo clique em resultado abre a cadeia de precedentes.
- Clique no status filtra `OK`, `VERIFICAR`, erro e stale.
- Ordenação só é permitida em tabelas cuja ordem não seja parte do cálculo; a ordem topológica deve permanecer protegida.

## 9. Criterios de aceitacao UX

1. Usuário localiza `LADO`, `RAMAL` e `DIST. DE CARGA` em no máximo uma ação a partir do projeto.
2. O grid mostra identificador, trecho/ponto e status sem scroll vertical para o primeiro conjunto operacional.
3. O usuário edita entradas sem conseguir alterar células calculadas acidentalmente.
4. Ao selecionar um resultado, a origem Excel ou regra futura aparece em até um painel, sem modal obrigatório.
5. O usuário consegue filtrar um ramal, acompanhar seu trecho e retornar ao circuito por seleção sincronizada.
6. A árvore e a tabela permanecem sincronizadas para pontos e trechos.
7. Demanda, corrente, tensão, queda, carregamento e status ficam visíveis na área de trabalho ou faixa persistente.
8. Alterar uma entrada marca resultados anteriores como `stale` antes de permitir exportação/relatório.
9. Copiar/colar uma faixa mantém validação, unidades e mensagens de erro.
10. O fluxo de teclado permite inserir uma sequência de linhas sem depender do mouse.
11. Print preview conserva o conjunto lógico das áreas `B1:CK53`, `B2:T80` e `B1:H29`, sem usar Excel como motor.
12. A interface não apresenta `#REF!` como resultado válido; mostra erro de evidência e origem.

## 10. Escopo autorizado apos o NO-GO

### Pode iniciar

- SQLite e migrações;
- entidades e validadores de domínio sem fórmulas elétricas finais;
- armazenamento de `SourceArtifact`, `SourceCell`, precedentes e golden cases;
- importador OOXML somente leitura;
- tela de inspeção de evidências;
- protótipo de `Lado`, `Ramal`, `Dist. de Carga` e árvore usando dados congelados, sem declarar cálculo de produção;
- comparação de versões e estado `stale`.

### Não pode iniciar

- `QdtCalculationModel` de produção;
- `CqtsCalculationModel` de produção;
- Solver equivalente;
- substituição de `DecInv`;
- exportação que declare paridade;
- portabilidade de macro de desproteção ou execução de VBA.

## 11. Proximas acoes para desbloqueio

1. Obter o workbook `DecInv` ou declarar formalmente quais resultados dependentes ficam fora do escopo.
2. Gerar diff normalizado de cada modulo VBA ATUAL/PROJ e verificar se houve mudança funcional.
3. Extrair metadados reais do Solver e reproduzir um caso antes/depois em copia.
4. Gerar grafo de precedentes criticos e classificar `#REF!`, dinamicas e externos.
5. Completar catalogo de unidades com casos numericos.
6. Congelar quatro golden cases completos, incluindo entradas e intermediarios.
7. Reavaliar o gate; manter `NO-GO` ate que os bloqueios 001, 002, 004 e 006 estejam resolvidos e 003/005 tenham evidencia suficiente.

## 12. Decisao

```text
NO-GO: motor eletrico de producao
GO: SQLite + evidencia + importacao somente leitura + harness de paridade + UX sem calculo final
```

A decisao e baseada em evidencias, nao em falta de implementacao. O aplicativo deve parecer a evolucao natural do QDT/CQTS, mas nao pode declarar paridade enquanto qualquer dependencia que possa mudar o resultado permanecer sem prova.
