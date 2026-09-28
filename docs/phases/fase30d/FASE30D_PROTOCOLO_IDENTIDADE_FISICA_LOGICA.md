# Fase 30D — Protocolo de Identidade Física-Lógica

**Baseline:** `a045cbe` (`dev`, igual a `origin/dev`)
**Status:** `IDENTIDADE NÃO DETERMINADA — REQUISITO EXTERNO NECESSÁRIO`
**Escopo:** investigação somente leitura; sem alteração de código, macros, DWG original ou `VERSION_MANIFEST.json`.

## 1. Objetivo

Investigar como artefatos reais relacionam elementos CAD a `PONTO`, `ID LIGHT`, trechos e entidades do `NetworkModel`. Distinguir relação determinística de correspondência provável, assistida ou não demonstrada.

## 2. Baseline

- Branch `dev`; `HEAD` e `origin/dev` em `a045cbe`.
- Identidade Git: `Jonatas <jonatas.lampa@im3brasil.com.br>`.
- Exclusão staged preexistente `src/QdtCqts.Desktop.Wpf/QdtCqts.Desktop.Wpf_dwkfellp_wpftmp.csproj` preservada, fora do commit.
- `VERSION_MANIFEST.json` não foi modificado.

## 3. Fontes Analisadas

| Fonte | Método/escopo |
|---|---|
| ZNA855820 | DWG em cópia via AutoCAD Core Console; CQTS/QDT por OpenXML; macros não executadas. Reaproveitadas as evidências e ressalvas da Fase 30C. |
| ZNA19165 | QDT ATUAL/PROJETADO, CQTS ATUAL e workbook CQTS, DWG em cópia temporária, CSV `_utm_23s`, KMZ e `ZNA 19165 ESTRUTURAS & CLIENTES.xlsx`. |
| ZNA23479 | `CQTsimplificado` ATUAL, CQT ATUAL/PROJ, DWG/DXF, CSV e KMZ. |
| Rede Invertida ZNA17846 | CQTS PROJ, QDT, DWG, CSV e KMZ. |
| ZNA402202 | QDT horizontal/vertical/projetado, DWG, CSV/KMZ e planilhas por poste. |
| BTZERO_ZNA48495 e CLANDESTINO_ZNA33649 | DWG, workbooks e CSV/KMZ; inventário estrutural, sem presumir vínculo com ZNA855820. |
| `aux cad/cqt/cqt_cad_extractor.lsp` | Seleção de linhas, letras de trecho, montante e JSON. |
| `aux cad/ambiental/cad2kmz.lsp`, `cad_to_kmz_exporter.py` | Regras de seleção, atributos, nomes e saída KMZ. |
| `aux cad/mapa_postes/mapa_postes_exec.lsp` | Rotina fixa de exemplo de inserção de bloco/PONTO. |
| SISQDT_LIGHT | `DomainTypes.cs`, `PhysicalGeometryJsonImporter.cs`, `ExcelEvidenceImporter.cs` e modelo WPF hardcoded. |

O inventário de `LIGHT\PROJETOS` encontrou 47 pastas de primeiro nível; 37 tinham ao menos um DWG/DXF e workbook, e 25 também tinham CSV/JSON/KMZ/KML. Esses números descrevem o conteúdo, não o estado “finalizado/em andamento”. A análise detalhada foi amostrada por disponibilidade de fontes; não se presumiu que arquivos parecidos pertencessem ao mesmo projeto.

Excel foi lido por OpenXML, sem executar VBA. O DWG ZNA19165 foi lido em cópia. O `.xls` legado ZNA19165 foi inventariado, mas não parseado: não havia provider OLEDB Jet/ACE enumerável e havia sessões Excel ativas; não foi aberto nelas.

## 4. Processo Real Observado

Não foi encontrado procedimento escrito que diga como o projetista seleciona uma entidade CAD e a associa a `PONTO`/trecho. As planilhas registram topologia e, em alguns projetos, coordenadas; CSV/KMZ preservam nomes de placemark; DWG contém rótulos. Não foi encontrado manifesto de associação, log de decisão do operador ou handle CAD em CSV/QDT.

No pacote ZNA19165 é plausível que o operador leia o rótulo `P#`, localize o `PONTO` numérico na aba/circuito e confira coordenadas UTM. Esse é um fluxo candidato sustentado pelos artefatos, não um processo humano comprovado. Rótulos duplicados exigem contexto/revisão.

## 5. Identidade Física Encontrada

### ZNA855820

No DWG, `NUM_PLAN.XX` contém `TR`/`P1...P11`, com rótulos repetidos. Não foi encontrado atributo `LID` de nó ou crosswalk CAD↔CQTS. As coordenadas CQTS estão nulas/zero; conclusão histórica registrada na Fase 30C.

### ZNA19165

A cópia do DWG continha 13.219 entidades selecionáveis; o probe classificou 1.129 INSERT, 313 atributos, 3.060 LINE, 2.602 LWPOLYLINE, 42 POINT, 3.554 TEXT e 730 MTEXT. Entre os blocos: `NUM_PLAN`, `N_PONTO`, `CAIXA`, `TRAFO` e simbologia dinâmica.

`NUM_PLAN` tem 281 tags `XX`; os valores `P1...P12`/`TR` aparecem em múltiplos INSERTs, layers e posições. `P1` ocorre 13 vezes, `P2` 18 vezes e `TR` 12 vezes no conjunto consultado. Textos `P1...P11` também se repetem em `EQEXISTTXT`. Esses são rótulos de planta, não IDs globais únicos por handle.

Tags adicionais: `NUM=01...08` em oito blocos `N_PONTO` no layer `rede_mt_aerea_13kv_existente`; `TIIPO=CS` em 24 blocos `CAIXA` no layer `BT_CONDUTORES`. Não correspondem à chave dos nós BT CQTS amostrados.

O CSV/KMZ `LDA CARMETIBA ZNA 19165` contém 36 pontos/placemarks: 31 valores distintos em `Name`, cinco registros “.” e duas posições diferentes com `P15`. O CSV tem `X,Y,Z,Name`; o KML tem os mesmos 36 nomes. Nenhum guarda handle DWG, versão de desenho, cenário ou circuito.

## 6. Identidade Lógica Encontrada

- **CQTS ZNA19165:** `PONTO`, `PONTO MONTANTE`, `COMPRIMENTO (m)` e `COORDENADAS UTM` (`X/Y`). Lado/cenário variam entre `LADO 1/2`, ATUAL e abas PROJ. Essa chave é contextual, não deve ser usada sem circuito/cenário.
- **Estruturas & Clientes ZNA19165:** a tabela geral tem cabeçalhos `PONTO`/`POSTE` e associa números de ponto a tipo/esforço de estrutura. Não associa ponto a handle ou coordenada física.
- **QDT ZNA19165:** `Trecho do Circuito` usa rótulos como `TRAF.1`, `P-2`, `P-3`; Lado 1/2 e ATUAL/PROJETADO são contextos distintos.
- **CQTS ZNA855820:** `ID LIGHT` contém pares como `TR, LID` e `LID, P2`; coordenadas nulas/zero.
- **Outros projetos:** ZNA402202 usa `TR`, `P-2`, `P-3` no QDT e nomes numéricos no CSV; ZNA23479 usa `01`, `02`, `A` e rótulos descritivos; ZNA17846 tem CQTS/CSV, mas as coordenadas amostradas não coincidiram.

No domínio, `Node.Id` é interno; `Node.ExternalKey` é texto sem semântica/proveniência de importação. `Circuit.SideIndex` contextualiza o circuito. `Edge.Id/ExternalKey` e `FromNodeId/ToNodeId` definem o trecho lógico. `Transformer.ExternalKey` não possui chave CAD associada. `ExcelEvidenceImporter` captura células/fórmulas, não constrói `NetworkModel`; o modelo WPF hardcoded não é importação de projeto real.

## 7. Hipóteses Testadas

| Hipótese | Resultado | Evidência |
|---|---|---|
| `NUM_PLAN.XX` é chave física global única | `REFUTADA` | P#/TR repetem em diversas inserções/posições/layers. |
| `NUM`/`TIIPO` identifica o poste BT do CQTS | `REFUTADA` nos dados observados | `NUM` pertence a `N_PONTO` de MT existente; `TIIPO=CS` pertence a `CAIXA`. |
| `CSV.Name`/KMZ placemark ↔ `CQTS.PONTO` no ZNA19165 | `CONFIRMADA` entre esses artefatos na precisão inteira armazenada | 39 ocorrências em seis abas/cenários encontraram exatamente um ponto CSV após normalizar às coordenadas inteiras gravadas no CQTS; os 39 sufixos P# também coincidiram com `PONTO`. Residual máximo observado menor que `0,000026` coordenada. Não prova vínculo a handle DWG. |
| `CSV.Name` é único | `REFUTADA` | `P15` está em duas posições; “.” aparece cinco vezes no CSV/KMZ. |
| Handle CAD ↔ CSV/CQTS | `REFUTADA` como relação persistida nas fontes atuais | CSV/KMZ/workbooks não guardam handle ou revisão DWG. |
| `cad2kmz` preserva necessariamente a tag `XX` | `REFUTADA` para o LISP analisado | O filtro aceita `NUM`, `PONTO`, `N`, `Nº`, `POSTE`; `XX` não está na lista. Sem tag reconhecida, cria IDs sequenciais pela ordem de seleção. Não há prova de que esse LISP gerou os CSVs amostrados. |
| Ordem de seleção/linha de planilha é chave | `REFUTADA` como regra demonstrada | `cqt_cad_extractor.lsp` seleciona entidades, soma comprimentos e pede letra/montante; não documenta alinhamento com o CQTS. |
| Inserções, POINT ou endpoints DWG coincidem com CSV no ZNA19165 | `REFUTADA` nas classes CAD varridas | Zero coincidências exatas ou arredondadas entre CSV e INSERT, POINT, LINE/LWPOLYLINE capturados. |
| Proximidade determina poste | `INDETERMINADA` | Não há regra nem tolerância documental; `TOLERÂNCIA NÃO DETERMINADA`. |
| `Edge` ↔ LINE/PLINE e `FromNodeId/ToNodeId` | `INDETERMINADA` | CSV é de pontos; linhas não têm ID/handle; LISP CQT grava letra/montante/comprimento, sem endpoints persistidos. |
| `P24 - TRAFO` ↔ `Transformer.ExternalKey` | `INDETERMINADA` | É candidato local; roots mudam entre abas/cenários e não há chave de entidade/trafo propagada. |
| Regra `PONTO`↔P# generaliza aos projetos | `REFUTADA` | ZNA19165 tem P#/coords; ZNA855820 tem ID LIGHT e coordenadas ausentes; ZNA23479/ZNA17846 não reproduziram o join amostrado; ZNA402202 usa rótulos/CSV diferentes. |

## 8. Evidências

### 8.1 Correspondência entre CQTS e CSV/KMZ — ZNA19165

- Fontes: `ZNA19165 - ROBUSTEZ DE BT-CQTS-ATUAL.xlsx`, `ZNA19165 - ROBUSTEZ DE BT-CQTS.xlsx`, `LDA CARMETIBA - ZNA 19165_utm_23s.csv` e `LDA CARMETIBA - ZNA 19165.kmz`.
- Em seis abas/lados/cenários, 39 ocorrências de nós com coordenadas não nulas tiveram correspondência única com CSV após normalizar à precisão inteira registrada no CQTS. Nenhuma foi ambígua; todos os nomes `P<n>` coincidiram com o número de `PONTO` (com sufixo `- TRAFO` no ponto identificado como trafo).
- X/Y dos CSV são floats e os do CQTS são inteiros; igualdade binária foi zero. O residual entre registros correspondentes foi menor que `3,8e-7` em X e `2,6e-5` em Y. Isso é evidência de duas tabelas alinhadas na precisão gravada, não regra de tolerância aprovada para produto.
- A planilha Estruturas & Clientes nomeia o projeto “ZNA 19165 - RUA ELANDE”; o QDT traz `RUA ELANDE & OUTRAS` e `LDA CARMETIBA`. Esses rótulos coexistem no mesmo workbook/pacote, mas não definem por si só a revisão de CAD do CSV.

### 8.2 DWG e dados persistentes — ZNA19165

- O CAD contém tags e textos P#, mas múltiplas ocorrências por valor; não existe `ID LIGHT` nas tags/textos procurados.
- CSV não coincide por XY com os 1.129 INSERT, 42 POINT ou endpoints/vértices das 3.060 LINE e 2.602 LWPOLYLINE capturadas. Não há link espacial direto do registro CSV a um handle nessa amostra.
- `INSUNITS=0`, `MAPCSASSIGN=nil`, `ACAD_GEOGRAPHICDATA` ausente. 59 APPID registrados, XDATA=0. Foram vistos 368 extension dictionaries; 351 entradas diretas `AcDbRepData`, duas `ACDB_ANNOTATIONSCALES`, quatro sem chave retornada; nenhuma chave direta contém identificador de nó/poste/trecho.
- `NUM_PLAN` tem inserções em escalas/espelhamentos diferentes; handles pertencem à revisão do DWG e não são exportados pelo CSV/KMZ.

### 8.3 Variação no Corpus

- **ZNA855820:** coordenadas CQTS ausentes; `ID LIGHT` usa pares textuais; nenhum JSON de intercâmbio identificado no projeto.
- **ZNA17846:** em dez ocorrências coordenadas amostradas (cinco por lado), nenhuma coincidiu com os 16 pontos do CSV pela comparação inteira testada.
- **ZNA23479:** CSV tem dez nomes distintos (`01/02/...`, `A` e rótulo descritivo); não houve correspondência inteira nas coordenadas CQTS não nulas examinadas.
- **ZNA402202:** QDT amostrado usa `TR`, `P-2`, `P-3`; CSV usa nomes numéricos e `X`. O workbook não oferece a mesma crosswalk de coordenadas observada no ZNA19165.
- **BTZERO_ZNA48495 / CLANDESTINO_ZNA33649:** CSVs também usam `X/Y/Z/Name`, mas os nomes, cardinalidade e workbooks variam; não considerados regra comum.

## 9. Relações Confirmadas

1. `PONTO`/`PONTO MONTANTE`/comprimento definem a topologia lógica CQTS por aba/circuito/cenário.
2. No ZNA19165, CSV e KMZ compartilham os mesmos 36 nomes.
3. No ZNA19165, CSV ↔ registros CQTS coordenados forma uma crosswalk única para 39 ocorrências na precisão inteira registrada; número de `PONTO` e sufixo P# coincidem.
4. O DWG contém rótulos P# em atributos/textos, mas são múltiplos por valor e por posição.

A relação 3 não prova qual entidade/handle CAD originou cada placemark nem que o KMZ/CSV foi exportado da mesma revisão do DWG. A cadeia determinística completa até `Node` não foi confirmada.

## 10. Relações Refutadas

- `P1=P1` como chave universal/suficiente sem contexto de projeto, circuito, cenário e ocorrência.
- Ordem de enumeração/seleção CAD como identidade lógica.
- Nome de bloco, handle ou tag `XX` como chave compartilhada única nas fontes atuais.
- Fallback sequencial do `cad2kmz.lsp` como identidade lógica.
- Geometria WPF `LayoutX/LayoutY` como posição CAD.
- Uma única nomenclatura de PONTO aplicável ao corpus inteiro.

## 11. Relações Indeterminadas

- Ocorrência CAD/handle que representa cada ponto CSV/KMZ no ZNA19165.
- Qual revisão DWG originou o CSV/KMZ; esses arquivos não carregam hash, handle ou referência ao desenho-fonte.
- Relação entre entidade física de trecho e `Edge` lógico.
- Identidade da entidade transformador com `Transformer.ExternalKey`.
- Se os 39 joins CQTS↔CSV representam o método usado pelo projetista, e não apenas alinhamento dos datasets.
- Tolerância para proximidade/interseção/continuidade: `TOLERÂNCIA NÃO DETERMINADA`.

## 12. Fonte Canônica

**FONTE CANÔNICA FÍSICO-LÓGICA NÃO ENCONTRADA.**

O CSV/KMZ ZNA19165 é uma fonte auxiliar candidata para nomes/posições; a planilha Estruturas & Clientes mapeia `PONTO` a tipo de poste. Nenhuma fonte guarda simultaneamente projeto/cenário, handle DWG e chave lógica, nem documenta revisão/proveniência do link.

## 13. Processo Humano

Nenhuma instrução/checklist/ata descreve a associação do projetista. Os rótulos P# e as coordenadas tornam plausível uma conferência visual/assistida, mas isso não foi registrado como processo real. Como o mesmo P15 tem duas posições e CAD contém múltiplas ocorrências por rótulo, a pessoa teria de selecionar/revisar o contexto; não há evidência de qual ocorrência é aceita.

## 14. CRS

- ZNA19165: CSV nomeado `_utm_23s`; CQTS declara `COORDENADAS UTM`, e os valores correspondem após normalização à precisão inteira. O DWG não declara `MAPCSASSIGN` nem contém `ACAD_GEOGRAPHICDATA`; datum/EPSG não determinados.
- ZNA855820: Fase 30C registrou `MAPCSASSIGN=nil` e ausência de dicionário geográfico; os JSONs temporários fuso 23 eram de outros desenhos.
- Não presumir `SIRGAS2000`, `WGS84`, `SAD69` ou `EPSG:31983`; CRS do CSV/KMZ não é automaticamente o CRS do DWG.

## 15. Unidade

- ZNA19165 DWG declara `INSUNITS=0` (unitless); isso não define unidade numérica do modelo.
- ZNA855820 DWG declara `INSUNITS=4` (unidade de inserção mm); isso também não determina sozinho a unidade das coordenadas do modelo.
- CSV `_utm_23s` e cabeçalho UTM são evidência de nomenclatura/unidade esperada dos dados de intercâmbio; não há metadado que a vincule ao handle DWG. Não promover a metros/UTM como contrato global.

## 16. Implicações para o Domain Model

1. `Node.Id` segue interno; `Node.ExternalKey` só pode receber `PONTO` após especificar escopo por projeto, cenário e circuito.
2. `Edge.Id` e `(CircuitId, FromNodeId, ToNodeId)` identificam trecho lógico; `Edge.ExternalKey` não pode vir da ordem CAD.
3. `PhysicalPosition.SourceId/SourceBlock` não guardam hoje hash/revisão DWG, handle, ocorrência ou método de associação. `PhysicalLineGeometry.SourceId` é opcional e o JSON de origem pode omitir IDs.
4. Cardinalidade geral Node↔poste é desconhecida. O ZNA19165 tem joins 1:1 entre registros CQTS/CSV no recorte; CAD contém rótulos repetidos. Elementos físicos sem lógico e lógicos sem posição são possíveis.
5. A relação Transformer↔símbolo e Edge↔LINE não foi demonstrada; alteração do domínio seria especulativa nesta fase.
6. Associação obsoleta requer revisão/versionamento da fonte; contrato atual não registra isso.

## 17. Contrato Recomendado (Somente Especificação)

Não alterar o domínio. Se engenharia validar a associação assistida, o artefato de mapeamento deverá registrar:

`ProjectKey + Scenario + Circuit/Side + LogicalType/Id + SourceFileHash + DWGRevision + Handle/EntityType + AttributeTag/Value + CSVName + Coordinates + CRS/Unit/Precision + MatchMethod + OperatorApproval + Timestamp`.

Rejeitar silenciosamente duplicidade/ambiguidade; exigir aprovação humana quando houver vários handles para o mesmo nome; registrar CRS/unidade e precisão declarados; nunca inferir pelo índice de seleção.

## 18. Cenário A/B/C/D

**C — Identidade não existe como contrato canônico do conjunto; requisito externo necessário.**

O ZNA19165 fornece um crosswalk corroborado CQTS↔CSV/KMZ e um candidato de associação assistida por P#/coordenadas. Falta vínculo/proveniência CSV↔handle DWG e não há procedimento humano documentado. Os demais projetos mostram formatos diferentes. Não há prova de duas fontes contraditórias para a mesma entidade/revisão (Cenário D).

## 19. Limitações

- 47 pastas inventariadas; investigação aprofundada amostrada nos casos listados, não em todos os projetos.
- Workbook legado `.xls` ZNA19165 não parseado por falta de provider OLEDB enumerável e presença de sessões Excel ativas.
- Core Console informou custom objects; o scanner leu as classes enumeradas, não decodificou objetos proprietários.
- Não há tolerância aprovada, processo humano documentado ou confirmação do responsável de engenharia.
- O CAD original não foi alterado; macros não foram executadas; nenhuma coordenada real foi adicionada a fixtures.

## 20. Decisão de Engenharia

**IDENTIDADE NÃO DETERMINADA — REQUISITO EXTERNO NECESSÁRIO.**

A evidência ZNA19165 é suficiente para registrar um crosswalk entre pontos CQTS e registros CSV/KMZ na precisão inteira do workbook, mas não para dizer “este `Node` é aquele handle/poste CAD” em todas as revisões e projetos. Nenhuma associação automática deve ser implementada nesta fase.

## 21. Pré-requisitos para Futura Implementação CAD

1. Engenharia confirma semântica e escopo de `PONTO`, `P#`, `ID LIGHT`, projeto/circuito/cenário.
2. Identificar a revisão DWG que originou CSV/KMZ e preservar handle/GUID da entidade; ou aprovar formalmente fluxo humano de mapeamento.
3. Declarar CRS, datum quando conhecido, unidade real e precisão; sem tolerância implícita.
4. Validar cardinalidade Node↔poste, Transformer↔símbolo e Edge↔LINE/POLYLINE em mais de um projeto e revisão.
5. Definir auditoria de associação, desambiguação e invalidação quando uma revisão do DWG mudar.

**Gate:** Fase31 não aprovada. Sem esses pré-requisitos, manter importação física não associada.
