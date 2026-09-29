# Fase 30F — Reconciliação DWG ↔ Crosswalk Físico-Lógico

**Caso:** ZNA19165 — Robustez BT / LDA CARMETIBA
**Data:** 2026-09-28
**Baseline:** `aeb796e` (`dev`, igual a `origin/dev`)
**Status:** `NO-GO — IDENTIDADE FÍSICO-LÓGICA AINDA NÃO DETERMINADA`
**Escopo:** investigação somente leitura; nenhum exportador, DTO, regra de associação ou alteração de domínio/produto.

## 1. Objetivo

Determinar se os pontos lógicos `P2`, `P24`/`P24 - TRAFO` e `P25`, já cruzados entre CQTS, CSV, KMZ e `.srua`, podem ser associados de forma objetiva a uma entidade física específica do DWG ZNA19165. O teste decisivo é a última seta: identidade lógica/geográfica → INSERT, ponto, anotação ou geometria CAD com uma ocorrência única e reproduzível.

## 2. Baseline

- Branch `dev`; `HEAD == origin/dev == aeb796e0363b9e09f95d24dcdc6af98bfa6fe83f`.
- Commit-base: `docs(fase30e): rastrear origem do crosswalk`.
- Identidade Git: `Jonatas <jonatas.lampa@im3brasil.com.br>`.
- Suíte do baseline: 212 testes aprovados. Suíte reexecutada nesta fase: 212 aprovados, 0 falhas, 0 ignorados.
- A exclusão staged preexistente `src/QdtCqts.Desktop.Wpf/QdtCqts.Desktop.Wpf_dwkfellp_wpftmp.csproj` foi preservada fora do commit.
- `VERSION_MANIFEST.json` permaneceu intacto. Nenhum commit foi feito em `main` e nenhum histórico foi reescrito.

## 3. Governança e Salvaguardas

- `git status`, branch, HEAD, `origin/dev`, autor e email foram confirmados antes da investigação.
- O DWG principal e os dois BAKs foram copiados para `C:\Temp\phase30f` e lidos pelo AutoCAD Core Console 2026. O original nunca foi aberto pelo scanner nem salvo.
- O Core Console abriu as cópias como somente leitura. O único comando de diagnóstico foi `P30SCAN`; não foram executados comandos de edição CAD, macros VBA, LISP de produção, inserção, limpeza, alteração de UCS ou atribuição de CRS.
- O scanner `.phase30f_dwgscan.lsp/.scr` é descartável e não é API de produto; será removido antes do commit. Resultados não são promovidos a contrato.
- CSV, KMZ/KML, `.srua` e workbooks foram lidos sem alteração; VBA não foi executado.

## 4. Fontes Investigadas

1. DWG principal ZNA19165 e dois BAKs encontrados na pasta do projeto.
2. CQTS `ATUAL` e workbook CQTS sem sufixo `ATUAL` (contextos de abas/cenários distintos).
3. QDT `ATUAL` e três workbooks `PROJETADO` (sem sufixo, `_01`, `_02`).
4. CSV `_utm_23s`, KMZ e KML interno `doc.kml` da área LDA CARMETIBA.
5. Snapshot `ZNA19165 - ROBUSTEZ DE BT.srua`.
6. `ZNA 19165 ESTRUTURAS & CLIENTES.xlsx`, `ZNA19165.xls` e planilhas auxiliares `CÁLCULO_ESFORÇO...PONTO_24/25.xlsm`.
7. Ferramentas locais `cad2kmz.lsp`, `cad_to_kmz_exporter.py`, `cqt_cad_extractor.lsp`, `kmz_pontos_importer.py/.lsp` e importador CSV UTM.
8. Convenção genérica do acervo `aux cad/mapa_cad_paleta`: `DomainId` lógico, `Numero` visual, `Handle` físico e XData versionado. É outro módulo/projeto, não evidência de que essa convenção foi usada neste DWG.
9. SISQDT_LIGHT: resultados/documentação 30A–30E, `DomainTypes.cs`, `PhysicalGeometryJsonImporter.cs` e `ExcelEvidenceImporter.cs`.

## 5. Inventário, Versões e Proveniência

Os caminhos abaixo são relativos à pasta local `LIGHT\PROJETOS\ZNA19165 - ROBUSTEZ DE BT`. Timestamps servem apenas para inventário, não provam causalidade ou revisão aprovada.

| Artefato | Tamanho | Modificado | Hash SHA-256 abreviado | Contexto observado |
|---|---:|---|---|---|
| `ROBUSTEZ BT – ZNA19165/ROBUSTEZ_BT_ZNA1916_PROJETO.dwg` | 1.569.298 B | 2026-04-27 15:46 | `1B1CA5C0…B636929A` | DWG principal escolhido pelo nome da pasta/arquivo; lido em cópia. |
| `ROBUSTEZ BT – ZNA19165/ROBUSTEZ_BT_ZNA1916_PROJETO.bak` | 1.559.036 B | 2026-04-27 15:06 | `B913C2A7…5506C122` | BAK de mesmo basename; leitura indica snapshot quase idêntico, não outro projeto independente comprovado. |
| `ZNA-19165 - JUC - ROBUSTEZ DE BT.bak` | 1.382.155 B | 2026-04-22 18:05 | `B7A4F242…7547B2011` | BAK de nome/contexto distinto; geometria e conteúdo divergem do principal, não assumido como revisão equivalente. |
| `ZNA19165 - ROBUSTEZ DE BT-CQTS-ATUAL.xlsx` | 370.693 B | 2026-04-22 16:11 | `F8E907CA…9160D31F` | CQTS ATUAL; leitura OpenXML. |
| `ZNA19165 - ROBUSTEZ DE BT-CQTS.xlsx` | 366.722 B | 2026-04-22 17:45 | `E4A00C12…373C086A` | CQTS sem sufixo; contexto distinto, não fundido ao ATUAL. |
| `ROBUSTEZ BT – ZNA19165/QDT_ROBUSTEZ_BT_ZNA19165_ATUAL.xlsm` | 775.358 B | 2026-04-22 16:10 | `F81EE300…2419604D` | QDT ATUAL. |
| `.../QDT_ROBUSTEZ_BT_ZNA19165_PROJETADO.xlsm` | 771.702 B | 2026-04-22 17:24 | `01AA1101…9591834A` | QDT PROJETADO, sem índice. |
| `.../QDT_ROBUSTEZ_BT_ZNA19165_PROJETADO_01.xlsm` | 771.994 B | 2026-04-22 17:29 | `E310F24C…1DCE4B48` | QDT PROJETADO_01, artefato distinto. |
| `.../QDT_ROBUSTEZ_BT_ZNA19165_PROJETADO_02.xlsm` | 770.128 B | 2026-04-27 15:04 | `55B932A1…3D755AB5` | QDT PROJETADO_02, artefato distinto e posterior. |
| `ROBUSTEZ BT – ZNA19165/LDA CARMETIBA ZNA 19165/LDA CARMETIBA - ZNA 19165_utm_23s.csv` | 1.826 B | 2026-04-20 10:04 | `C8ED2FAD…2829BE20` | CSV, colunas `X,Y,Z,Name`, 36 linhas. |
| `.../LDA CARMETIBA - ZNA 19165.kmz` | 3.606 B | 2026-04-11 15:22 | `375B4DB2…029AD0D9` | KMZ com KML interno; não há KML separado na pasta. |
| `ZNA19165 - ROBUSTEZ DE BT.srua` | 9.937 B | 2026-04-20 08:10 | `20E943D9…963229F` | Snapshot de estado; 36 postes BT. |
| `ROBUSTEZ BT – ZNA19165/LDA CARMETIBA ZNA 19165/ZNA 19165 ESTRUTURAS & CLIENTES.xlsx` | 126.605.474 B | 2026-04-12 04:44 | `AB2B64B0…FF18CD` | Planilha geral de estruturas; título interno “ZNA 19165 - RUA ELANDE - SEPETIBA RJ”. |
| `ROBUSTEZ BT – ZNA19165/CÁLCULO_ESFORÇO.../CÁLCULO_ESFORÇO_ZNA19165_PONTO_24.xlsm` | 110.340 B | 2026-04-22 17:11 | `A56A840D…4D757096` | Apesar do caminho/nome, a aba `Ponto (1)` diz “CACUIA - RUA PEDREIRA - NOVA IGUAÇU”; não é evidência utilizável do P24 ZNA19165. |
| `.../CÁLCULO_ESFORÇO_ZNA19165_PONTO_25.xlsm` | 110.262 B | 2026-04-22 17:11 | `EF5B33D7…6C13B497` | Mesmo título interno de outro projeto; não usado para identidade ZNA19165. |
| `ZNA19165.xls` | 57.856 B | 2026-04-04 20:14 | `E7DD2112…766C10D8` | Workbook legado inventariado; não parseado nesta fase. |

Há 29 arquivos auxiliares de cálculo nomeados `PONTO_01` a `PONTO_29`; nome/caminho não garantem conteúdo do projeto, como mostram os workbooks PONTO_24/25. Não foi encontrado PDF, DXF ou documento de procedimento dentro do conjunto ZNA19165 pesquisado.

## 6. Método de Leitura

- Scanner AutoLISP descartável exportou, para cada entidade selecionável, handle, tipo, layer, layout, owner handle, nome/effective name, posição/rotação/escala, texto, atributos, propriedades dinâmicas, aplicações/contagem XData, chaves diretas do extension dictionary, reactors e campos DXF relevantes.
- O scanner enumerou o DWG principal inteiro em Model e Layout1; atributos foram lidos pela cadeia INSERT→ATTRIB. O scanner não transformou coordenadas nem calculou associações elétricas.
- As duas cópias BAK foram varridas com o mesmo procedimento. O Core Console emitiu aviso de custom objects/proxies; classes visíveis foram enumeradas, mas estruturas proprietárias não foram decodificadas.
- Workbooks `.xlsx/.xlsm` foram lidos em modo OpenXML somente leitura (`data_only`); nenhuma fórmula VBA foi executada.
- Coincidência numérica, camada, nome, owner ou proximidade não foram usados como regra de associação.

## 7. Inventário do DWG Principal

O DWG principal contém 13.219 entidades selecionáveis: 1.129 INSERT, 3.060 LINE, 2.602 LWPOLYLINE, 42 POINT, 3.554 TEXT, 730 MTEXT, 352 LEADER, 141 DIMENSION, 942 CIRCLE, 442 HATCH, 133 ARC, 42 ELLIPSE, 35 SPLINE, 9 POLYLINE, 5 VIEWPORT e 1 ACAD_TABLE. Há 10.701 entidades em Model e 2.518 em `Layout1`.

As 281 inserções `NUM_PLAN` com tag `XX` estão no espaço Model, divididas entre layers `0` (148), `BT_CONDUTORES` (125) e `LDA 1` (8). Os candidatos `XX=P2`, `XX=24` e `XX=25` estão todos em `BT_CONDUTORES`, Model; não são duplicações causadas por viewports de paper space.

## 8. Blocos e Candidatos de Estrutura

- `NUM_PLAN`: 281 inserções com atributo `XX`; o nome identifica um bloco de numeração, não um tipo de poste nem ID persistente.
- Outros blocos de contexto incluem `TRAFO` (7 inserções), `TRAFO_EXCLUSIV` (4), `CAIXA` (25), `N_PONTO` (8, layer `rede_mt_aerea_13kv_existente`), `S_BT2` (38), `XX` (77) e `XX2` (46). `RELOC_POSTE` aparece uma vez; não há um conjunto de INSERTs com `P24`/`P25` como nome semântico de bloco.
- As 8 tags `NUM=01…08` pertencem aos INSERTs `N_PONTO` de MT existente, não constituem a chave BT do CQTS. As 24 tags `TIIPO=CS` pertencem a `CAIXA`.
- Os `NUM_PLAN` candidatos têm `effectiveName=NUM_PLAN`; os atributos são visíveis (`isocop`, altura 2,2 no desenho consultado). Atributos não contêm `P24` nem `P25` literal.
- O scanner não recebeu propriedades dinâmicas de INSERT. Existem definições anônimas `*U...`, mas a anonimização não lhes atribui semântica elétrica; proxies/custom objects permanecem parcialmente opacos.

## 9. Atributos e Chaves Textuais

Foram contadas 313 referências ATTRIB anexas aos INSERTs: 281 `XX`, 24 `TIIPO` e 8 `NUM`. Não foram encontrados outros nomes de tag, como `ID`, `PONTO`, `PONTO_MONTANTE`, `LID`, `LIGHT`, `COD`, `CODIGO`, `POSTE`, `ESTRUTURA`, `TAG` ou `REF` nas entidades examinadas.

Distribuição relevante de `NUM_PLAN.XX` no DWG principal:

| Valor de `XX` | Ocorrências | Contexto | Consequência |
|---|---:|---|---|
| `P2` | 18 | Model; layers `0` e `BT_CONDUTORES` | Token textual não seleciona uma ocorrência. |
| `P1` | 13 | Model; múltiplas posições/layers | Também repetido. |
| `TR` | 12 | Model; múltiplas posições/layers | Não identifica transformador único. |
| `24` | 3 | Model; `BT_CONDUTORES` | Nenhum valor literal `P24`; três handles candidatos. |
| `25` | 3 | Model; `BT_CONDUTORES` | Nenhum valor literal `P25`; três handles candidatos. |
| `P24` / `P25` | 0 | — | Nenhuma tag com a grafia lógica completa. |

Em dois pares, `XX=24` coincide no ponto de inserção com outro `NUM_PLAN` de valor `TR`, e `XX=25` coincide com outro `NUM_PLAN` de valor `P2`. Ambos estão no mesmo layer e no mesmo Model space. É evidência de rótulos sobrepostos/representações repetidas, não critério para declarar qual é a identidade correta. O terceiro `XX=24` e o terceiro `XX=25` também permanecem candidatos separados.

## 10. XDATA, Extension Dictionaries, Reactors e Object Data

- XData attached: **0/13.219 entidades**. Nenhuma entidade examinada contém `AUXCAD_POSTE`, `AUXCAD_VAO`, `DomainId`, `VaoId` ou GUID de projeto em XData.
- O desenho registra 59 nomes APPID, entre eles `ADE`, `SDSK_POINT`, `AcDbDynamicBlockGUID`, `AcDbDynamicBlockTrueName` e `AcDbBlockRepBTag`. APPID cadastrado sem XData não é vínculo de negócio nem prova de que esse aplicativo gravou identidade nas entidades.
- Foram observadas 368 referências a extension dictionaries: 351 com chave direta `AcDbBlockRepresentation`, 4 `ASDK_XREC_ANNOTATION_SCALE_INFO`, 2 `AcDbContextDataManager` e 11 sem chave interpretável pelo scanner direto. Nenhuma chave direta retornada continha identificador PONTO/ID LIGHT.
- 211 entidades exibem grupo `ACAD_REACTORS`, principalmente anotações/simbologia. Os três conjuntos ATTRIB candidatos `XX=P2/24/25` não possuem XData, extension dictionary com chave reconhecida nem reactor referenciado como associação lógica.
- Não apareceu classe de entidade `MLEADER`; os 352 `LEADER` são objetos CAD presentes, mas os candidatos lógicos não estão ligados a eles por handle. Nenhum dos 352 leaders possui referência de anotação group 340 preenchida no inventário.
- O Core Console relatou custom objects. Não foi possível decodificar dados internos proprietários/Object Data; ausência de vínculo nas estruturas DXF enumeradas não prova que todo payload de custom object seja semanticamente vazio. Esse ponto fica explicitamente limitado, não convertido em “chave inexistente” absoluta.

## 11. Layers, Texto e Contexto de Representação

Layers com simbologia/candidatos incluem `BT_CONDUTORES`, `BT_EXISTENTE`, `LDA 1`, `LDA 2`, `LAD 1_PROJ`, `EQEXISTTXT`, `TOPOGRAFIA`, `COTAS_UTM` e `rede_mt_aerea_13kv_existente`. A combinação `(layer, blockName, XX)` ainda não é única para `P2`, `24` ou `25`.

O desenho contém 3.554 TEXT e 730 MTEXT, mas nenhum texto com o token completo `P24` ou `P25` foi encontrado. `P2`/`TR` aparecem em diversas anotações; MTEXTs também contêm tokens `P2` em descrições de cabo (`...QX`), que não significam necessariamente o poste lógico `P2`. Não há associação persistida de texto/leaders aos pontos CQTS.

## 12. Handles e Persistência

Os 13.219 handles do scan principal são únicos dentro desse DWG. Para os INSERTs candidatos, o owner é o BlockTableRecord de Model space; o handle identifica a entidade na base daquele arquivo, não seu significado elétrico nem um ID durável entre desenhos.

O BAK de mesmo basename contém 13.220 entidades: 13.218 handles/conteúdos comuns ao principal sem diferença nos campos comparados, um INSERT presente só no principal e dois só no BAK (`S_BT2`). Isso indica snapshots muito próximos; não demonstra qual handle representa um PONTO.

O BAK `ZNA-19165 - JUC - ROBUSTEZ DE BT.bak` contém 13.155 entidades: 13.141 handles comuns, 78 exclusivos do DWG principal, 14 exclusivos do JUC BAK e 1.627 handles comuns com conteúdo/posição diferente. O nome, timestamp e geometria caracterizam outro contexto/revisão candidata. Os totais `XX=P2/24/25` coincidem, mas as posições mudam; nomes de arquivo e tokens não bastam para vincular esse BAK aos artefatos CSV/KMZ/CQTS.

## 13. Duplicidades e Critério de Entidade Física

As duplicidades observadas não se restringem a diferentes layouts. As ocorrências de P#/TR/24/25 estão no Model space e em grupos/camadas distintos. O scan mostra `XX=24` e `XX=25` cada um em três INSERTs `NUM_PLAN` no mesmo layer; duas ocorrências de cada valor estão exatamente sobrepostas a outro `NUM_PLAN` com valor diferente (`TR` ou `P2`).

Não há um campo que declare “estrutura física”, “detalhe”, “existente/projetado”, vista ou ocorrência canônica para esses candidatos. Existem múltiplos símbolos `TRAFO` e `TRAFO_EXCLUSIV`; sua coexistência com um rótulo próximo é apenas contexto gráfico. O critério `layer + block + atributo` não produz chave única e nenhum handle foi selecionado como a entidade correta.

## 14. CRS e Unidade

No DWG principal: `INSUNITS=0`, `INSUNITSDEFSOURCE=4`, `INSUNITSDEFTARGET=4`, `MEASUREMENT=0`, `MAPCSASSIGN` vazio, `ACAD_GEOGRAPHICDATA` ausente; `UCSORG=(0,0,0)` e eixos UCS coincidem com WCS. O BAK de mesmo basename e o JUC BAK também indicam `INSUNITS=0` e `MAPCSASSIGN` vazio.

`INSUNITSDEFSOURCE/TARGET=4` não declara a unidade numérica da geometria corrente. O CSV usa `_utm_23s`; KMZ e `.srua` guardam longitude/latitude. A comparação de números brutos não demonstra transformação para WCS: a faixa X do CSV e a extensão WCS do DWG são disjuntas, e não há CRS/datum/transformação que explique o deslocamento. Não foi adotado EPSG, datum, zona, hemisfério ou fator de escala para reconciliar o desenho. Unidade do DWG permanece `INDETERMINADA`.

## 15. Três Traces Obrigatórios

Os elos anteriores ao DWG foram reproduzidos nesta fase: cada linha CSV selecionada é única; seu nome e lon/lat coincidem com o placemark KML e com `.srua.title`; as coordenadas UTM casam com as coordenadas inteiras CQTS no contexto indicado. Valores de coordenadas reais foram omitidos; os hashes/fontes e referências de aba/linha abaixo permitem inspecionar os mesmos registros localmente.

| Ponto lógico/contexto | CSV | KMZ | `.srua` | Candidatos no DWG e método de seleção | Outra entidade igualmente candidata? | Resultado DWG |
|---|---|---|---|---|---|---|
| `PONTO=2`, `LADO 1 PROJ2!A8:E8`, montante 0 | `Name=P2`, uma linha | `Placemark/name=P2`, coordenada igual ao `.srua` | `id=P1`, `title=P2` | 18 INSERTs `NUM_PLAN` com `XX=P2`; busca literal do valor ATTRIB em todos os INSERTs. Nenhum desses registros carrega referência ao CSV, KMZ ou CQTS. | Sim: 18 inserções; várias layers/posições. | `INDETERMINADO` |
| `PONTO=24`, `LADO 1!A8:E8`, montante 0 | `Name=P24 - TRAFO`, uma linha | `Placemark/name=P24 - TRAFO`, coordenada igual ao `.srua` | `id=P31`, `title=P24 - TRAFO` | Três INSERTs `NUM_PLAN` com `XX=24`; nenhum `P24` literal. Existem múltiplos blocos `TRAFO`; nenhuma relação por handle ou annotation pointer. | Sim: três tags `XX=24`; duas coincidem com `XX=TR`; há vários símbolos de trafo. | `INDETERMINADO` |
| `PONTO=25`, `LADO 1!A9:E9`, montante 24 | `Name=P25`, uma linha | `Placemark/name=P25`, coordenada igual ao `.srua` | `id=P32`, `title=P25` | Três INSERTs `NUM_PLAN` com `XX=25`; nenhum `P25` literal. Busca semântica de bloco/layer e posição não define seleção aprovada. | Sim: três tags `XX=25`; duas coincidem com `XX=P2`; sem metadado de ocorrência canônica. | `INDETERMINADO` |

**Como os candidatos foram escolhidos:** busca exaustiva das tags/valores existentes no inventário de INSERTs, não por seleção manual, ordem, primeiro match ou nearest-neighbor. **Como não foram escolhidos:** nenhum candidato foi elevado a vínculo, pois todos mantêm alternativas sem regra de desambiguação.

## 16. Matriz de Evidências

| Evidência | Chave única? | Relaciona-se ao DWG? | Força/classificação |
|---|---|---|---|
| CQTS `PONTO` + aba/cenário + coordenadas | No contexto da planilha/aba; não global | Não possui handle nem revisão DWG | `CONFIRMADA` como identidade lógica/contextual; `INDETERMINADA` até DWG. |
| CSV `Name` + X/Y | P2/P24/P25 aparecem uma vez neste CSV; globalmente há `P15` duplicado e `.` repetido | Nenhum handle/campo de revisão | `CONFIRMADA` na crosswalk local; `NÃO DISPONÍVEL` para handle. |
| KMZ placemark `name` + lon/lat | Sem ID XML/ExtendedData | Nenhum handle | `CONFIRMADA` como conteúdo geográfico correspondente; sem ponte CAD. |
| `.srua` `id`/`title`/lat/lng | `id` é único só neste snapshot; difere de `title` | Sem atributo/entity handle ou hash de DWG | `CONFIRMADA` no snapshot; `INDETERMINADA` como chave persistente. |
| DWG `NUM_PLAN.XX=P2` | Não; 18 ocorrências | Rótulo gráfico, sem ID lógico ou crosswalk | `CONFIRMADA` como texto CAD; vínculo `INDETERMINADO`. |
| DWG `NUM_PLAN.XX=24/25` | Não; 3 ocorrências de cada | Não carrega literal P24/P25, ID LIGHT ou ID trafo | `CONFIRMADA` como rótulo numérico; vínculo `INDETERMINADO`. |
| Handle DWG | Único no desenho amostrado | Não existe nos CSV/KMZ/CQTS/.srua | `CONFIRMADA` como identidade técnica intrarquivo; relação lógica `NÃO DISPONÍVEL`. |
| XData `DomainId`/`VaoId` | — | XData attached = 0 | `NÃO DISPONÍVEL` nesta revisão. |
| Coordenada CAD bruta ↔ CSV | — | Sem CRS/unidade compatíveis declarados; sem igualdade numérica direta | `INDETERMINADA`; proximidade não prova identidade. |
| QDT ATUAL/PROJETADO | Sem ocorrência textual de P24/P25 nas quatro versões lidas | Não guarda handle nem ponte aos pontos examinados | `NÃO DISPONÍVEL` para os três traces. |
| Aux `PONTO_24/25.xlsm` | Nome de arquivo sugere pontos; conteúdo diz outro projeto | Não contém associação ao DWG ZNA19165 | `CONTRADITÓRIA` com a suposição de que path/nome comprova contexto ZNA19165. |
| Convenção genérica `aux cad` `DomainId`/Handle | Especificada em outro módulo | Não há os XData correspondentes no DWG escaneado | `PROVÁVEL` como opção futura de software, não evidência deste projeto. |

## 17. Fluxo Operacional/Humano e Ferramentas

Não foi encontrado checklist, ata ou instrução específica para o projetista escolher um INSERT e associá-lo a `PONTO`/`ID LIGHT` no ZNA19165. A planilha geral mapeia `PONTO` a `POSTE`/tipo de estrutura, mas não a handle/coordenada CAD. Seu título também difere do título interno de algumas QDTs; rótulos coexistentes não resolvem revisão.

As rotinas locais não documentam a associação usada para produzir este DWG/KMZ:

- `cad2kmz.lsp` varre INSERT/POINT e aceita tags numéricas conhecidas; caso não obtenha tag reconhecida, cria `P1…` pela ordem da seleção. Não conserva handle no JSON e procura tags como `NUM/PONTO/N/POSTE`, não o atributo `XX` observado. Não há log de execução nem JSON de entrada que o identifique como produtor destes artefatos.
- `cad_to_kmz_exporter.py` recebe esse JSON e converte UTM para KML/KMZ; isso descreve um candidato de fluxo, não prova de proveniência da amostra.
- `cqt_cad_extractor.lsp` pede projeto, transformador, cenário, seleção manual de linhas, letras/montantes e comprimentos; o JSON de saída não guarda handles/extremos lógicos. Não foi demonstrado que gerou o CQTS analisado.
- `kmz_pontos_importer.py/.lsp` é fluxo KMZ→AutoCAD; não prova que o KMZ tenha sido produzido pelo desenho atual.
- O contexto geral de `aux cad/mapa_cad_paleta` distingue corretamente `DomainId` persistente, número visual e Handle. A busca do scanner não encontrou esse XData no DWG ZNA19165; não há evidência de que o plugin criou ou gravou o desenho.

Resultado humano: um operador pode reconhecer padrões gráficos, mas nenhum procedimento reproduzível nos artefatos consultados seleciona a ocorrência canônica. Portanto, `REGRA OPERACIONAL EXTERNA NÃO DISPONÍVEL`.

## 18. Resultado e Limitações

**Resultado C — IDENTIDADE DWG ↔ CROSSWALK NÃO DETERMINADA.** A identidade mostrada em CSV/KMZ corresponde a nomes e coordenadas; no CQTS, pontos têm chave contextual e coordenadas compatíveis. No DWG, o elo se reduz a rótulos `P2`, números `24/25`, símbolos e handles locais. Há múltiplos candidatos e nenhuma persistência lógica que escolha um.

Limitações explícitas:

- O desenho contém custom objects/proxies que o scanner não decodificou; não se afirma que todo storage proprietário é vazio.
- Coordenadas/CRS/unidades do DWG não foram declaradas. Sem transformação comprovada, semelhança/proximidade não é usada para identidade.
- O BAK com nome JUC tem conteúdo e extensão espacial diferentes; relação exata com obra/revisão não foi demonstrada.
- O `.xls` legado foi inventariado, mas não parseado; os QDT/CQTS lidos foram processados sem VBA.
- Não há PDF, DXF ou documento operacional identificado no diretório de projeto pesquisado.
- O inventário de pastas `aux cad` não demonstra que qualquer rotina foi executada sobre essa revisão DWG.

## 19. Decisão de Gate

`NO-GO — IDENTIDADE FÍSICO-LÓGICA AINDA NÃO DETERMINADA`

Não se encontrou uma regra observável que seja única, reproduzível para múltiplos pontos, resistente às duplicidades, independente de proximidade/ordem e capaz de escolher uma entidade física específica. Os três traces terminam em conjuntos candidatos, não em uma entidade confirmada. Fase 31 não aprovada.

## 20. Recomendação para Próxima Decisão

Solicitar à engenharia, para uma revisão DWG explicitamente identificada, um destes artefatos autoritativos:

1. Crosswalk aprovada `Project/Scenario/Circuit/Side/PONTO ou ID LIGHT → DWG hash/revision + Handle + EntityType`, com operador e data; ou
2. Regravação controlada da fonte CAD com `DomainId` GUID persistido por entidade física e atributos/relacionamentos lógicos validados, mantendo Handle apenas como referência da revisão; ou
3. Procedimento manual reproduzível de seleção/validação que resolva todos os candidatos duplicados e seja assinado pelo responsável técnico.

Em qualquer alternativa, documentar a semântica de `PONTO`, `XX`, `P#`, trafo/poste, cardinalidade, layout/circuito/cenário, tratamento de cópias/revisões, unidade, CRS/datum quando aplicável e auditoria de aprovação. Não converter rótulo `XX`, proximidade, sequência ou handle isolado em `PhysicalEntityId` automaticamente.

Nenhuma alteração de código, fixture, modelo, pipeline, LISP de produção ou `VERSION_MANIFEST.json` foi feita. Interromper no gate 30F e aguardar nova decisão antes da Fase 31.