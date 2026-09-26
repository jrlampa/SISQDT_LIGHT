# Auditoria exploratoria e engenharia reversa: QDT + CQTS

**Projeto:** ZNA855820  
**Data da auditoria:** 2026-09-26  
**Escopo:** quatro arquivos fornecidos, somente leitura  
**Status:** Fase de levantamento concluida com lacunas explicitas. A consolidacao da Fase 2 esta em [ESPECIFICACAO_QDT_CQTS_ZNA855820.md](ESPECIFICACAO_QDT_CQTS_ZNA855820.md); este documento permanece como registro da auditoria inicial.

## 1. Conclusao executiva

1. **E possivel reconstruir o motor?** Sim, em principio. As planilhas contem formulas, tabelas, nomes definidos e caches numericos suficientes para reconstruir grande parte do fluxo. A paridade integral ainda depende da extração do VBA, dos links externos e da semantica completa das tabelas/colunas.
2. **QDT como referencia primaria:** confirmado. O QDT possui dois lados visiveis (`LADO 1`, `LADO 2`), calculos por linha/trecho, validacoes de logica, corrente, carga e queda de tensao.
3. **CQTS:** confirmado como modelo complementar baseado em pontos/trechos e tabelas estruturadas. As colunas `PONTO`, `TRECHO`, `ACUMULADA`, `CONDUTOR`, `FASE` e resultados por lado sustentam uma rede radial com derivacoes. O suporte a loops/multiplos caminhos ainda nao foi comprovado.
4. **ATUAL x PROJ:** as dimensoes e a maior parte das formulas permanecem iguais; as mudancas observadas sao majoritariamente entradas/topologia e valores calculados armazenados. Ha formulas alteradas no CQTS em `RAMAL` e `PROJ1`.
5. **VBA:** os XLSM possuem `xl/vbaProject.bin` de 67.072 bytes. O Excel COM, com acesso programatico ao projeto VBA nao exposto, retornou zero componentes. Portanto, a existencia de VBA e confirmada, mas seu conteudo e efeitos ainda sao **NAO DETERMINADOS**.
6. **Recomendacao:** nao implementar o aplicativo ainda. Primeiro fechar a extração OLE/VBA, os links externos, os nomes definidos completos, o mapeamento de colunas e uma bateria de casos de paridade baseada em valores calculados pelo Excel.

## 2. Metodologia e limites

- Leitura direta dos pacotes OOXML ZIP/XML, sem alterar os arquivos originais.
- Abertura pelo Excel COM em modo somente leitura, com `AutomationSecurity=3` e sem salvar.
- Nenhuma macro foi executada deliberadamente.
- Contagens de formulas XML representam nos `<f>` explicitos. Formulas compartilhadas podem ser expandidas pelo Excel; por isso as contagens COM e XML nao sao necessariamente iguais.
- Valores numericos foram observados como caches IEEE-754/Excel, por exemplo `186.08282352941154`; nao foram tratados como valores arredondados de apresentacao.
- Nao foi feita recalculacao forçada nem alteracao de links externos.

## 3. Inventario dos arquivos

| Rotulo | Arquivo | Tipo | Tamanho | Abas | Nomes | VBA | Tabelas ZIP | Links externos |
|---|---|---:|---:|---:|---:|---|---:|---:|
| QDT_ATUAL | `QDT_ZNA855820_ATUAL.xlsm` | XLSM | 769.937 bytes | 14 | 7 | sim | 9 | 2 |
| QDT_PROJ | `QDT_ZNA855820_PROJ.xlsm` | XLSM | 772.106 bytes | 14 | 7 | sim | 9 | 2 |
| CQTS_ATUAL | `CQTS__ZNA855820_ATUAL.xlsx` | XLSX | 403.883 bytes | 15 | 105 no XML / 107 no COM | nao identificado | 22 | 0 observado |
| CQTS_PROJ | `CQTS__ZNA855820_PROJ.xlsx` | XLSX | 406.135 bytes | 15 | 105 no XML / 107 no COM | nao identificado | 22 | 0 observado |

As datas de modificacao dos arquivos foram 2026-09-24. Os arquivos originais permanecem intocados.

## 4. Estrutura QDT

Abas e funcao inferida pela estrutura e formulas:

| Aba | Estado | Dimensao XML | Funcao observada |
|---|---|---|---|
| PP e CE | oculta | A1:Q254 | tabelas/parametros de perdas, padroes ou cargas |
| Base de Dados | oculta | A1:AK72 | base de parametros; 70 nos de formula |
| PF e Prestação de Serviço | oculta | A1:AI121 | parametros e regras de prestacao |
| Ramais | visivel | A1:X82 | entrada/calculo de ramais; combinacao de cargas |
| Distrib. Cargas | visivel | B1:I35 | distribuicao de cargas; 128 nos de formula |
| Coeficiente Unitário | oculta | B2:I18 | coeficientes |
| FML | oculta | D2:J12 | fatores/parametros |
| LADO 1 | visivel | A1:IE361 | calculo principal do lado 1 |
| LADO 2 | visivel | A1:IE361 | calculo principal do lado 2 |
| ANÁLISE PONTO A PONTO | oculta | A2:S304 | acumulacao/análise por ponto; tabela estruturada |
| Curva Disj. | oculta | A1:AG255 | curva de dispositivo |
| Curva NH | oculta | A1:M41 | curva/parametro NH |
| Alocação % de tensão | oculta | A1:AZ768 | alocacao percentual de tensao |
| Tabela | oculta | B1:P255 | tabelas auxiliares |

O Excel COM confirmou `LADO 1` e `LADO 2` como visiveis e 9 abas ocultas. O QDT usa tabelas Excel visiveis principalmente na analise ponto a ponto; varias outras estruturas sao intervalos nomeados ou referencias convencionais.

### Evidencias de entradas, intermediarios e resultados

- **Entradas:** campos sem formula em `Ramais`, `LADO 1`, `LADO 2`, `Distrib. Cargas` e tabelas auxiliares; entre os valores aparecem ponto/trecho, condutor, fase, comprimento, demanda e parametros do transformador.
- **Intermediarios:** colunas que calculam carga por consumidor, carga acumulada, corrente, impedancia corrigida, fatores e percentual de queda.
- **Resultados:** colunas com status (`OK !`, `Erro 02`, `Erro 05 !`), corrente, kVA, tensao e verificacoes de limite.
- **Dependencia de macro:** `LADO 1!AM1` e `LADO 2!AM1` exibem aviso para ativar a macro quando `B255 <> "SIM"`; `AM2` tambem depende de `B255`. Isso prova que uma condicao de funcionamento esta ligada a uma escrita/alteracao externa, provavelmente VBA, mas a rotina que escreve o valor permanece nao determinada.

## 5. Estrutura CQTS

| Aba | Estado | Dimensao XML | Funcao observada |
|---|---|---|---|
| DB | oculta | A1:AG416 | base de dados, condutores, trafos, coeficientes e parametros |
| COORDENADAS | visivel | B2:R68 | coordenadas dos pontos |
| SUGESTAO_CORTES | oculta | A1:AH220 | sugestao/avaliacao de cortes e resultados auxiliares |
| RAMAL | visivel | A1:AF217 | carga do ramal e consolidacao inicial |
| PROJ1 | visivel | A2:U454 | parametros do projeto e selecao de condutor/carga |
| LADO 1..4 | visiveis | A1:AH27 / AG36 | calculo dos lados atuais |
| GERAL PROJ | visivel | A2:S454 | consolidacao da rede projetada |
| LADO 1/2 PROJ | visiveis | A1:AG23/24 | calculo da rede projetada |
| GERAL PROJ2 | visivel | A2:R454 | segunda consolidacao de projeto |
| LADO 1/2 PROJ2 | visiveis | A1:AG24 | segunda variante projetada |

O Excel COM confirmou 5 tabelas em `DB`, 2 em `COORDENADAS`, 1 em `RAMAL`, 2 em `PROJ1`, e tabelas nos relatorios de lados/projetos. O CQTS nao contem `vbaProject.bin` no pacote.

## 6. Modelo de dados reconstruido

Modelo conceitual minimo evidenciado:

```text
Projeto
  +-- Transformador
  +-- Circuito/Lado
  +-- Ponto (node)
  +-- Trecho (edge dirigido)
  +-- Ramal
  +-- Carga/Cliente
  +-- Condutor
  +-- Parametros eletricos
  +-- Resultado por ponto/trecho/lado
```

| Entidade | Identificador/campos evidenciados | Entrada ou calculo |
|---|---|---|
| Transformador | `TR_ATUAL`, `QT_TR`, `T_LINHA_TRF`, tensao e demanda | parametro/entrada |
| Ponto | `PONTO`, referencias como `P3`, `P4`, `P5` | entrada/topologia |
| Trecho | `TRECHO`, pares como `P3, P4`; comprimento e acumulacao | entrada/topologia |
| Ramal | `RAMAL`, clientes e composicao de carga | entrada/calculo |
| Condutor | `CONDUTOR`, tabelas `CABOS[]`/nomes equivalentes | parametro |
| Carga | clientes, kVA por ponto, acumulada | entrada/calculo |
| Lado | `LADO 1` a `LADO 4` e variantes PROJ | estrutura de calculo |
| Resultado | `Ib`, `In`, `Iz`, status, queda, tensao, corrente | calculo |

A correspondencia exata de todos os campos e unidades ainda requer uma matriz completa de cabecalhos de cada tabela.

## 7. Modelo topologico

### QDT

O QDT organiza o calculo em dois lados lineares do transformador. A estrutura de `LADO 1` e `LADO 2`, com centenas de linhas e colunas fixas, indica uma sequencia de trechos/pontos por lado, nao um grafo generico no armazenamento da planilha.

### CQTS

O CQTS possui evidencias de uma lista de arestas orientadas:

- pontos identificados por valores como `P3`, `P4`, `P5`;
- pares de pontos armazenados em colunas de trecho, por exemplo `P3, P4`;
- referencias `PONTO` e `TRECHO` usadas em `SUMIF`;
- resultados acumulados propagados por referencias de linha e por consultas a `GERAL`;
- varios lados calculados separadamente.

Representacao recomendada para o futuro modelo:

```text
Node { id, coordinate, load, metadata }
Edge { id, parentNode, childNode, length, conductor, phase, parameters }
Circuit { sourceNode, side, edges }
```

**Conclusao atual:** o CQTS pode ser modelado como uma arvore dirigida radial, com um pai e zero ou varios filhos por no. Nao foi encontrada evidencia suficiente para afirmar suporte a ciclos, caminhos alternativos ou malhas; esses casos devem ser rejeitados ou marcados como `NAO DETERMINADO` ate teste especifico.

## 8. Formulas e regras matematicas evidenciadas

### 8.1 Composicao de carga do ramal

Origem observada em QDT `Ramais` e CQTS `RAMAL`:

```excel
=B9*0.85+B10*0.5268
```

Forma matematica:

```text
carga = entrada_1 * 0,85 + entrada_2 * 0,5268
```

Os valores `0,85` e `0,5268` sao hardcoded e devem ser tratados como constantes rastreaveis, nao como conhecimento externo. O significado fisico exato ainda e **NAO DETERMINADO**.

### 8.2 KVA por consumidor/ponto no QDT

Amostra `LADO 1!K13`:

```excel
=IF(OR(C13="",D13="",H13="",E13="",G13="",I13="Erro !",AM13="",AR13=""),"",IF(D13>2,E13*G13,IF(D13=2,8*G13,4*G13)))
```

A formula depende de tipo/quantidade em `D13`, fator em `E13`, grupo em `G13` e validacoes de preenchimento. Ela implementa ramos discretos para `D > 2`, `D = 2` e caso restante.

`LADO 1!L13`:

```excel
=IF(OR(C13="",D13="",H13="",E13="",G13="",I13="Erro !",AM13="",AR13=""),"",E13*G13)
```

`LADO 1!M13` seleciona a variante:

```excel
=IF($CH$5="SIM",K13,L13)
```

### 8.3 Validacao logica

Amostra `LADO 1!I13`:

```excel
=IF(OR(D13="",H13=""),"",IF(OR(D13=0,H13=0),"Erro 02",IF(AND(H13>=H9,D14<=D13),"OK !","Erro 02")))
```

Isso e uma regra de aprovacao/reprovacao, nao somente apresentacao.

### 8.4 Coordenadas/queda no CQTS

Amostra observada em `CQTS LADO 4!L8`:

```excel
=IFERROR(IFERROR(DIR_ATUAL2224[[#This Row],[ACUMULADA]]/((IF(DIR_ATUAL2224[[#This Row],[FASE]]="MONO",T_LINHA_MONO,IF(DIR_ATUAL2224[[#This Row],[FASE]]="BIF",T_LINHA_TRF,T_LINHA_TRF*SQRT(3)))/1000)*VLOOKUP(DIR_ATUAL2224[[#This Row],[CONDUTOR]],CABOS[],9,0))+30,W8+60*(X8/Z8)),"")
```

A formula possui pelo menos duas alternativas: uma baseada em acumulacao, tensao de linha, condutor e deslocamento `+30`; outra baseada em coordenadas `W8+60*(X8/Z8)`. A escolha exata e o significado fisico de cada coluna precisam ser documentados antes da implementacao.

### 8.5 Corrente no CQTS

Amostra `CQTS LADO 4!X8`:

```excel
=IFERROR(IF(DIR_ATUAL2224[[#This Row],[FASE]]="MONO",DIR_ATUAL2224[[#This Row],[ACUMULADA]]*1000/(T_LINHA_TRF*DIR_ATUAL2224[[#This Row],[ETA]]),DIR_ATUAL2224[[#This Row],[ACUMULADA]]*1000/(SQRT(3)*$R$7*DIR_ATUAL2224[[#This Row],[ETA]])),"")
```

A forma matematica e condicional por fase, com divisor monofasico ou trifasico e fator `ETA`.

### 8.6 Media ponderada por carga

Amostra `CQTS LADO 1!AF4`:

```excel
=IFERROR(SUMPRODUCT(I8:I23,F8:F23)/SUM(I8:I23),"INSERIR DADOS")
```

O resultado e uma media ponderada de `F` pelos valores de `I`, com fallback textual.

### 8.7 Regras de projeto

Amostras de `PROJ1` e `GERAL PROJ`:

```excel
=IF(OR(J9="",J9=0),"VAZIO",IF(J9>P3,"SOBRECARGA",""))
=SUMIF(GERAL[CONDUTOR],$S4,GERAL[M])
=R3-PROJ1!T4
```

Essas regras mostram que o projeto compara demanda com capacidade, consolida metragem por condutor e calcula diferencas entre metragem existente/projetada.

## 9. Precisao, arredondamento e constantes

- O Excel armazena caches com varias casas decimais; exemplos: `186.08282352941154`, `0.1060968302845441`.
- As formulas amostradas nao fazem `ROUND` explicito; isso nao prova que nao exista arredondamento em outras abas/VBA.
- A precisao interna deve ser mantida em dupla precisao equivalente ao Excel; formatacao visual deve ser separada.
- Constantes hardcoded confirmadas nas amostras: `0.85`, `0.5268`, `4`, `8`, `6`, `2`, `1`, `30`, `60`, `1000`, `SQRT(3)`, alem de parametros por nome como `QT_MTTR`, `T_LINHA_MONO`, `T_LINHA_TRF`, `DMDI`, `fator_potencia`.
- O significado, unidade e origem regulatoria de cada constante ainda nao estao completamente determinados.

## 10. Comparacao ATUAL x PROJ

### QDT

- Mesmas 14 abas, mesmas dimensoes XML, mesmos nomes e mesmas quantidades estruturais.
- Nenhuma formula diferente foi detectada na comparacao por coordenada XML.
- As diferencas de celula concentram-se nos valores cacheados de `LADO 1` e `LADO 2`; isso e consistente com entradas/topologia alteradas e formulas recalculadas.
- `PP e CE`, `Base de Dados`, `PF e Prestação de Serviço`, `Ramais`, `Distrib. Cargas`, `Coeficiente Unitário`, `FML`, `ANÁLISE PONTO A PONTO`, curvas, alocacao e tabela nao apresentaram diferencas de celula nessa comparacao.

### CQTS

Mudancas observadas por coordenada:

| Aba | Evidencia |
|---|---|
| `DB` | 8 valores, incluindo `K6`, `K7`, `K8`, `K10` e espelhos `T4:T7`; transformador/demanda parecem alterados |
| `RAMAL` | 57 mudancas; 2 formulas alteradas e mudancas em `AA26`, `AA28`, `AB36:AB39`, `U18:U23` |
| `PROJ1` | 143 mudancas; 3 formulas alteradas, pontos adicionados/removidos e parametros de projeto alterados |
| `GERAL PROJ` | 11 valores; pontos, clientes, demanda acumulada e diferencas de metragem alterados |
| `LADO 1` | 127 valores; novos pontos/condutores e varios status mudam de `OK` para `VERIFICAR` |
| `LADO 2` | 107 valores; cadeia de pontos muda de `LID,P4`/`P4,P5` para `LID,P7`/`P7,P8` etc. |
| `LADO 3` | 107 valores; condutores, pontos e verificacoes mudam |
| `LADO 4` | 13 valores; valores de ponta/demanda/corrente/queda mudam |
| `SUGESTAO_CORTES` | 349 valores recalculados |
| Demais abas | sem mudanca por coordenada na comparacao executada |

A classificacao preliminar e: `DB!K6:K10` = parametros/entradas derivadas; `PROJ1` e `RAMAL` = entrada/topologia e calculo; `LADO*`, `GERAL*` e `SUGESTAO_CORTES` = resultados/intermediarios. A classificacao final exige cabecalhos e regras de negocio completos.

## 11. VBA, links e dependencias externas

- Ambos os QDT contem `xl/vbaProject.bin`, 67.072 bytes descompactados.
- O pacote possui `externalLink1.xml` e `externalLink2.xml`; referencias externas tambem aparecem nas formulas XML.
- O Excel COM abriu os arquivos com macros desabilitadas, mas `VBProject.VBComponents.Count` retornou zero. Isso pode significar projeto protegido, acesso programatico desabilitado ou outra limitacao de automacao; nao significa ausencia de codigo.
- Nao executar macros nesta fase.
- Proxima evidencia necessaria: extrair o projeto OLE em copia temporaria usando ferramenta de leitura de VBA, registrar nomes de modulos, linhas, eventos e UDFs; depois comparar os dois `vbaProject.bin` sem executar o codigo.

## 12. Mapa de dependencias atual

```text
DB / Base de Dados / nomes definidos
        |
        +--> condutores, trafos, fatores, tensoes e limites
        |
Ramais / RAMAL / PROJ1 / entradas de pontos e cargas
        |
        +--> acumulacao por PONTO/TRECHO
        |
LADO 1..n / LADO 1..n PROJ
        |
        +--> corrente, impedancia, queda, carregamento e status
        |
GERAL / ANÁLISE PONTO A PONTO / SUGESTAO_CORTES
        |
        +--> consolidacao, verificacoes e relatorios
```

Dependencias ocultas identificadas: nomes definidos, tabelas estruturadas, celulas ocultas, caches, links externos e possivel escrita por VBA. Referencias indiretas e a lista completa de precedentes ainda precisam de exportacao automatizada.

## 13. Matriz de rastreabilidade inicial

| ID | Origem | Regra observada | Implementacao futura | Teste |
|---|---|---|---|---|
| CALC-001 | `QDT!Ramais`, `CQTS!RAMAL` | combinacao `0.85` e `0.5268` | `LoadAggregation` | PAR-001 |
| CALC-002 | `QDT!LADO 1!K13` | kVA condicional por quantidade | `QdtEngine` | PAR-002 |
| CALC-003 | `QDT!LADO 1!I13` | validacao `OK !`/`Erro 02` | `ValidationEngine` | PAR-003 |
| CALC-004 | `CQTS!LADO 1!AF4` | media ponderada por `SUMPRODUCT` | `CqtsEngine` | PAR-004 |
| CALC-005 | `CQTS!LADO 4!X8` | corrente por fase e acumulada | `CurrentCalculator` | PAR-005 |
| CALC-006 | `CQTS!LADO 4!L8` | queda/posicao com fallback | `VoltageDropCalculator` | PAR-006 |
| CALC-007 | `CQTS!PROJ1!Q2` | status vazio/sobrecarga | `ProjectValidation` | PAR-007 |
| CALC-008 | `QDT!LADO 1!AM1` | bloqueio/aviso dependente de macro | `MacroCompatibilityGate` | PAR-008 |

Esta matriz e inicial. Ela nao substitui a matriz completa celula-a-celula exigida para liberar a implementacao.

## 14. Casos de teste de paridade

Casos reais minimos a extrair e congelar:

| Caso | Fonte | Variacao | Saidas a comparar |
|---|---|---|---|
| PAR-001 | ATUAL | rede atual completa | cargas, corrente, tensao, status por trecho |
| PAR-002 | PROJ | pontos/condutores projetados | resultados de `PROJ1`, `GERAL PROJ`, lados |
| PAR-003 | ATUAL | carga zero em uma ponta | acumulacao e validacoes |
| PAR-004 | ATUAL | um trecho simples | queda e corrente |
| PAR-005 | PROJ | varios ramos | propagacao pai/filho |
| PAR-006 | PROJ | troca de condutor | `Ib`, `In`, `Iz`, status |
| PAR-007 | PROJ | troca de comprimento/coordenada | queda e coordenada |
| PAR-008 | ATUAL | macro desabilitada/habilitada | bloqueio e resultados |

Para cada caso, exportar entradas, formulas, valores cacheados e status por celula. O Excel deve ser oraculo; a comparacao nao pode limitar-se ao resultado final.

## 15. Criterio de paridade

Ainda nao e correto fixar uma tolerancia arbitraria. A proposta operacional e:

- texto/status: igualdade exata;
- inteiros e identificadores: igualdade exata;
- valores numericos: comparar valor interno, nao formatacao;
- tolerancia inicial de engenharia somente para diagnostico: `abs(app - excel) <= 1e-12 * max(1, abs(excel))`, a ser validada contra casos reais;
- qualquer divergencia acima da tolerancia deve guardar formula, operandos, ordem das operacoes, constantes e arredondamentos aplicados.

A tolerancia final deve ser calculada por familia de grandeza depois da extração dos valores Excel e de um teste de sensibilidade de ponto flutuante.

## 16. QDT x CQTS

| Aspecto | QDT | CQTS |
|---|---|---|
| Forma principal | dois lados lineares | pontos/trechos e varios lados |
| Fonte de calculo | formulas fixas por colunas/linhas | tabelas estruturadas e consultas por ponto |
| Topologia | sequencia por lado | arvore radial inferida |
| VBA | presente | nao encontrado no pacote |
| Links externos | presentes | nao observados |
| Validacao | erros textuais e limites | status `OK`/`VERIFICAR`, sobrecarga e limites |
| Compartilhamento | parametros/conceitos de carga, condutor, fase, corrente | deve compartilhar dominio, nao necessariamente formula |

QDT e CQTS podem compartilhar entidades de dominio e uma camada de parametros, mas os motores devem permanecer separados ate provar equivalencia numerica. O CQTS nao deve ser tratado como um QDT reduzido.

## 17. Arquitetura recomendada apos fechar lacunas

Tecnologia recomendada provisoriamente: **.NET 8 + WPF**, por ser Windows-first, offline, adequada a instalacao corporativa, interoperacao futura com Excel e separacao clara entre dominio e UI. A recomendacao fica condicionada a uma prova de que a engine propria reproduz o Excel sem depender de automacao em producao.

```text
WPF UI
  |
Application services / import-export / reports
  |
Domain model: Project, Transformer, Node, Edge, Circuit, Load, Conductor
  |
Calculation engines
  +-- QDT engine
  +-- CQTS radial engine
  +-- current / voltage-drop / loading / validation
  |
Persistence: versioned project + inputs + results + audit trail
  |
Excel adapter: import, export, oracle/parity harness
```

A automacao Excel deve ser ferramenta de importacao/oraculo, nao dependencia obrigatoria do calculo offline final, salvo se o VBA provar ser indispensavel e nao reproduzivel.

## 18. Requisitos classificados

### MUST HAVE

- paridade numerica com QDT, incluindo valores intermediarios;
- preservacao de constantes, ordem de operacoes e regras de status;
- modelo radial de nos/trechos do CQTS;
- rastreabilidade arquivo/aba/celula/formula/VBA/teste;
- importacao e exportacao controladas;
- funcionamento offline;
- testes ATUAL e PROJ;
- separacao entre precisao interna e apresentacao.

### SHOULD HAVE

- comparador Excel versus aplicativo por celula;
- versionamento do projeto;
- relatorio de precedentes e dependencias;
- visualizacao da arvore e destaque de trechos com erro;
- validacao de ciclos e trechos desconectados.

### NICE TO HAVE

- exportacao automatica para XLSX/XLSM;
- compatibilidade de layout com relatorios existentes;
- importacao de outros projetos da mesma familia;
- editor grafico de topologia.

### UNKNOWN

- lista e efeito de todos os modulos VBA;
- nomes e conteudo dos links externos;
- unidades exatas de todas as colunas;
- regra de cada constante hardcoded;
- tratamento de loops, fases incompletas e dados ausentes;
- arredondamentos fora das formulas amostradas;
- formulas matriciais/compartilhadas completas e precedentes indiretos;
- comportamento quando macros alteram ou limpam celulas.

## 19. Riscos e bloqueios

1. **VBA nao extraido:** risco alto de perder inicializacao, limpeza, solver, validacao ou escrita de celulas.
2. **Links externos:** risco alto de resultados dependerem de arquivos nao fornecidos.
3. **Caches:** valores armazenados podem refletir recalculacao anterior, macro ou Excel com outra configuracao.
4. **Formulas compartilhadas:** comparar apenas nos XML pode omitir formulas expandidas.
5. **Nomes duplicados/escopo:** o CQTS possui nomes repetidos e nomes de escopo potencialmente diferente.
6. **Unidades:** formulas misturam `/1000`, `SQRT(3)`, fatores e valores constantes; sem dicionario de unidades, a implementacao pode produzir resultado numericamente parecido mas semanticamente errado.
7. **Topologia:** a radialidade e forte indicio, mas ciclos e multiplos caminhos nao foram testados.
8. **Macro gate:** o aviso de ativacao pode indicar que a planilha nao funciona apenas com formulas.

## 20. Trabalho obrigatorio antes da implementacao

1. Extrair `vbaProject.bin` em copia temporaria e produzir inventario de modulos/eventos/UDFs.
2. Resolver `externalLink1.xml` e `externalLink2.xml`, identificando arquivos e celulas dependentes.
3. Exportar todas as tabelas, nomes definidos, cabecalhos, formatos numericos e validacoes.
4. Expandir formulas compartilhadas e gerar mapa de precedentes por celula.
5. Capturar uma execucao controlada no Excel para cada arquivo, registrando entradas antes/depois e valores intermediarios.
6. Classificar cada coluna por entidade, unidade, entrada/intermediario/resultado.
7. Montar os casos PAR-001 a PAR-008 com snapshots imutaveis.
8. So entao definir interfaces finais da engine e iniciar prototipo de calculo.

## 21. Resposta objetiva as perguntas de encerramento

- **O que entra no QDT?** Dados de ramais, pontos/trechos, cargas, condutores, fases, transformador e parametros auxiliares; o dicionario completo ainda falta.
- **O que calcula?** Carga/kVA, corrente, tensao/queda, acumulacao, verificacoes, carregamento e relatorios por lado/ponto.
- **Como calcula?** Por formulas condicionais e consultas a bases/tabelas, com regras especificas por linha e lado; varias formulas foram identificadas, mas nao todas.
- **Onde?** Principalmente `Ramais`, `Distrib. Cargas`, `LADO 1`, `LADO 2`, `ANÁLISE PONTO A PONTO` e abas auxiliares.
- **Quais macros participam?** Nao determinado; o binario existe e a planilha exibe gate de macro.
- **O que entra no CQTS?** Pontos, trechos, coordenadas, cargas, condutores, fases, parametros e projeto.
- **Como representa a rede?** Lista/tabela de pontos e arestas dirigidas, com acumulacao por `SUMIF`/referencias; forte indicio de arvore radial.
- **Como propaga resultados?** Por ponto/trecho, acumulacao e consultas a tabelas de consolidacao.
- **Como ATUAL difere de PROJ?** Principalmente entradas/topologia e resultados cacheados; CQTS tambem possui poucas formulas alteradas.
- **Quais regras compartilham?** Cargas, condutores, fases, transformador, corrente, queda e verificacoes conceituais; as implementacoes de formula nao devem ser presumidas equivalentes.
- **Motor independente da interface?** Sim, depois de fechar VBA/links/unidades e congelar testes.
- **Tecnologia?** Provisoriamente .NET 8/WPF, condicionada aos resultados finais da auditoria.
- **O que falta antes de programar?** VBA, links externos, precedentes completos, unidades, casos de paridade e regras nao determinadas listadas acima.

**Conclusao:** a auditoria estrutural e matematica inicial esta documentada, mas a condicao de encerramento da especificacao ainda nao foi atingida. A proxima etapa correta e completar as lacunas de VBA, links externos e rastreabilidade antes de qualquer codigo de producao.

## 22. Adendo da Fase 2

O VBA foi posteriormente extraido em copia somente leitura com ferramenta OLE. Foram identificados os componentes `Módulo1`, `Módulo2`, `Módulo3`, `EstaPasta_de_trabalho`, `UserForm1` e classes de planilha, totalizando 15 componentes nominais. As rotinas observadas atuam principalmente em abertura, selecao, ocultacao de linhas, filtros, protecao, controles e formulario; `UserForm1` contem `DesprotegeVBA`/`CopyFile`, que nao devem ser portados para o aplicativo.

O mesmo conjunto nominal aparece no PROJ, mas os binarios `vbaProject.bin` diferem: SHA-256 ATUAL `c65818d86fa8ea6381d47e3ae3c2f96e468df03fd5cc533baa142313865567c6`; PROJ `109141263097dea8c7ce5b09be2ab9d3077856d72afd2bd513a46fb8cbc8aa1b`. O impacto dessas diferencas no calculo ainda precisa de comparacao textual por modulo.

Tambem foram confirmados diretamente pelo Excel: o link externo `DecInv`, nomes CQTS com escopo de planilha, nomes `RAMA_PROJ1`/`RAMA_PROJ2`/`RAMAIS_PROJ1` com `#REF!`, e parametros `solver_*` em `RAMAL`/`PROJ1`. Esses pontos foram incorporados na especificacao consolidada e continuam bloqueios para congelar o motor de calculo.
