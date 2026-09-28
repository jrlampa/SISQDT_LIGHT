# Fase 30C — Reconciliação de Identidade Física × Lógica

**Caso:** ZNA855820 — Rede Invertida
**Baseline:** `d816f217` (`dev`, igual a `origin/dev`)
**Status:** `GO PARA PRÓXIMA DECISÃO — identidade ainda não determinada`
**Escopo:** investigação somente leitura; sem CAD/LISP novo no produto e sem associação automática.

## 1. Objetivo

Determinar, com evidência do caso local real, se os identificadores das planilhas QDT/CQTS e do DWG permitem reconciliar `Node` ↔ poste físico e `Edge` ↔ geometria física. O objetivo é bloquear uma futura exportação CAD caso ela dependesse de uma associação inventada.

## 2. Fontes Investigadas

| Fonte | Evidência consultada |
|---|---|
| `REDE_INVERTIDA_ZNA855820.dwg` e `.bak` | DWG analisado em cópia temporária pelo AutoCAD Core Console; o original estava aberto na sessão AutoCAD e não foi alterado. |
| `CQTS__ZNA855820_ATUAL.xlsx` e `CQTS__ZNA855820_PROJ.xlsx` | Metadados e células OpenXML, principalmente `COORDENADAS`, `LADO 1`, `LADO 2`, `LADO 1 PROJ` e `LADO 2 PROJ`; nenhuma macro foi executada. |
| `1.QDT/QDT_ZNA855820_ATUAL.xlsm` e `..._PROJ.xlsm` | Abas e células OpenXML, incluindo `Base de Dados`, `LADO 1`, `LADO 2` e `ANÁLISE PONTO A PONTO`; nenhuma macro foi executada. |
| `2.CALC_ESF/REDE_INVERTIDA_ZNA855820_01.xlsm` a `..._11.xlsm` | Inventário das planilhas auxiliares e abas `Ponto (01)` a `Ponto (11)`. |
| `ZNA855820.pdf` | Presença e tipo do documento; não usado como chave de identidade. |
| `aux cad/cqt/cqt_cad_extractor.lsp` | Seleção de linhas, cálculo de comprimento, letras de trechos e montante. |
| `aux cad/ambiental/cad2kmz.lsp` e `cad_to_kmz_exporter.py` | Extração de INSERT/POINT e LINE/POLYLINE para JSON; regras de ID e campos exportados. |
| `aux cad/mapa_postes/mapa_postes_exec.lsp` | Rotina de inserção de exemplo de poste/PONTO. |
| `aux cad/cqt/cqt_excel_manager.py` | Consumo do JSON de trechos e aplicação à planilha CQT. |
| `C:\Temp\cad2kmz_input.json` | JSON existente; título `AN_A057152931_DESENHO`, não é o caso ZNA855820. |
| `C:\Temp\kmz_points_input.json` | JSON existente com IDs `Bloco_*U729_n`, não é o caso ZNA855820. |
| Código sisQDT_LIGHT | `DomainTypes.cs`, `PhysicalGeometryJsonImporter.cs`, `ExcelEvidenceImporter.cs` e `MainViewModel.BuildSelectedProjectVersion`. |

### 2.1 Inventário do Caso Real

Encontrados no diretório `REDE_INVERTIDA_ZNA855820`:

- DWG, backup `.bak` e arquivos de lock `.dwl/.dwl2`;
- QDT `ATUAL` e `PROJ` (`1.QDT`);
- CQTS `ATUAL` e `PROJ`;
- 11 planilhas auxiliares de cálculo (`2.CALC_ESF`);
- PDF `ZNA855820.pdf`.

Não foram encontrados nesse projeto LISP, JSON, KMZ/KML, CSV ou arquivo de mapeamento físico↔lógico. O acervo `aux cad` contém 32 LISP, 16 JSON, um KMZ, seis XLSX, um XLSM, quatro DWG e um DXF; não contém CSV/KML. Esses arquivos são ferramentas/configurações/amostras do acervo e não foram tratados como dados ZNA sem identificação explícita.

## 3. Identidade Física Encontrada

O DWG contém identificadores de apresentação em atributos tag `XX` de blocos `NUM_PLAN`. Entre os valores há `TR` e `P1` a `P11`, mas cada um desses 12 rótulos ocorre duas vezes no desenho. Não foi encontrado atributo `LID`; a ocorrência textual de “LID” está dentro de uma nota geral sobre o lado BT do transformador, não como identificador de entidade.

O `handle` CAD identifica uma entidade dentro daquele DWG, mas não aparece nos JSONs de intercâmbio nem nas chaves do CQTS. O nome `NUM_PLAN` é o tipo de bloco de numeração, não uma identidade única de poste. Portanto, a evidência confirma rótulos de planta, não uma chave física global e estável por poste.

O probe somente leitura encontrou 7.587 entidades selecionáveis no desenho. Entre as classes registradas: 1.308 INSERT/blocos, 186 atributos, 2.728 LINE, 542 LWPOLYLINE, 3 POLYLINE, 93 POINT, 1.202 TEXT e 568 MTEXT. Há geometria em layers como `BT_CONDUTORES`, `LDA 1`, `LDA 2`, `BT_EXISTENTE` e layers de simbologia. A extração contou 62 coincidências exatas entre vértices/pontas de geometria e inserções de blocos, mas nenhuma com os INSERTs `NUM_PLAN` portadores dos rótulos `XX`.

As inserções de `NUM_PLAN` não são coordenadas físicas confiáveis dos postes: para os rótulos consultados, a distância até o segmento mais próximo de `BT_CONDUTORES` variou aproximadamente de 9,8 m a 32,7 m. Esse levantamento é descritivo, não define tolerância e não associa o rótulo a um poste ou trecho.

## 4. Identidade Lógica Encontrada

No CQTS, os cabeçalhos distinguem `ID LIGHT`, `PONTO`, `PONTO MONTANTE` e `TRECHO (m)`. `ID LIGHT` contém pares de conexão, por exemplo `TR, LID`, `LID, P2` e `P2, P3`; `PONTO` e `PONTO MONTANTE` numeram linhas dentro da aba/cenário; `TRECHO (m)` registra comprimento lógico. As abas `LADO 1/2` e `ATUAL/PROJ` são contextos distintos: seus tokens/quantidades não podem ser fundidos por posição de linha.

As colunas UTM do CQTS, inclusive `X/Y` nas abas consultadas, estão zeradas no caso analisado. A aba `COORDENADAS` também não fornece coordenadas operacionais para os nós. Os comprimentos lógicos permanecem preenchidos; por exemplo, em `ATUAL/LADO 1`, `TR, LID` tem 4 m e `LID, P2` tem 40 m, enquanto em `PROJ/LADO 1` os valores correspondentes são 4 m e 18 m.

No domínio atual:

| Conceito | Chaves/relacionamentos existentes | Evidência de origem no caso |
|---|---|---|
| `Transformer` | `Id`, `ExternalKey` | Identificação do transformador nas planilhas; sem chave CAD demonstrada. |
| `Circuit` | `Id`, `ExternalKey`, `TransformerId`, `SideIndex` | Abas `LADO 1/2`; não há adaptador de workbook que gere essa relação no produto. |
| `Node` | `Id`, `ExternalKey`, `CircuitId`, `ParentNodeId` | CQTS informa `PONTO`/`PONTO MONTANTE`; o significado de `ExternalKey` não é normalizado por um importador do caso. |
| `Edge` | `Id`, `ExternalKey`, `CircuitId`, `FromNodeId`, `ToNodeId`, `Length` | CQTS `ID LIGHT`/`TRECHO (m)` e QDT `Trecho do Circuito`; sem chave para entidade CAD. |

`ExcelEvidenceImporter` somente captura células, fórmulas e metadados como evidência; não converte workbook em `NetworkModel`. A WPF cria em `BuildSelectedProjectVersion` um modelo hardcoded de exemplo `CQT PROJ 4/7` com chaves `TR`, `LID`, `P1...` e `TR-LID`; isso não é uma importação do ZNA855820 e não comprova relação com o DWG.

## 5. Convenções do Acervo CAD

- `cad2kmz.lsp` seleciona INSERT/POINT e entidades LINE/LWPOLYLINE/POLYLINE. Quando não encontra atributo em tags contendo `NUM`, `PONTO`, `N`, `Nº` ou `POSTE`, cria `P1`, `P2`, ... pela ordem da seleção. A tag observada no ZNA, `XX`, não está nessa lista.
- O JSON desse exportador guarda `id/block/x/y` para pontos e `layer/coords` para linhas; linhas não recebem `id`, handle, extremos lógicos ou relação `From/To`.
- `cqt_cad_extractor.lsp` nomeia trechos por letras `A` a `Z`, mede entidades selecionadas (ou aceita metragem manual) e pede ao operador o trecho montante. Seu JSON contém letra, montante, quantidade de vãos e comprimento; não guarda handles, coordenadas ou ID LIGHT.
- `mapa_postes_exec.lsp` é uma rotina fixa de exemplo: usa coordenadas literais, insere um bloco `PONTO` e grava `01` nos atributos/textos. Não estabelece convenção reutilizável para o projeto real.
- O `cad_to_kmz_exporter.py` converte coordenadas assumindo parâmetros elipsoidais WGS84; essa transformação e o fuso recebido não provam o CRS do DWG ZNA.

Os dois JSONs temporários ainda existentes são de outros contextos: um tem título `AN_A057152931_DESENHO` e 45 pontos/218 linhas; o outro contém oito IDs `Bloco_*U729_n`. Nenhum deles é uma extração identificada do ZNA855820.

## 6. Tentativa de Reconciliação

| Hipótese de vínculo | Classificação | Evidência e limite |
|---|---|---|
| Tag CAD `XX` com valor `P1...P11`/`TR` ↔ token textual de `ID LIGHT` | `PROVÁVEL` | Há sobreposição lexical, mas cada rótulo ocorre duas vezes; `LID` não existe como atributo de nó; não há mapa que selecione circuito/cenário/ocorrência. Não é chave determinística. |
| `Node.ExternalKey` ↔ nome do bloco | `NÃO DISPONÍVEL` | `NUM_PLAN` é tipo de bloco repetido; nomes como `S_BT` e `*U...` identificam simbologia/blocos, não `ExternalKey`. |
| `Node.ExternalKey` ↔ CAD handle | `NÃO DISPONÍVEL` | O handle existe no DWG, mas não é propagado por `cad2kmz` nem registrado no CQTS. |
| Ordem de nós/linhas ↔ ordem de seleção CAD | `INDETERMINADA` | O LISP atribui IDs pela ordem do `ssget`/seleção; não existe prova documental de ordenação coincidente com abas, `PONTO` ou montante. |
| Coordenada CAD ↔ linha de coordenadas CQTS | `NÃO DISPONÍVEL` | Coordenadas UTM do CQTS são zero; os atributos E/N do bloco `CO` não têm vínculo com `ID LIGHT` ou `PONTO`. |
| LINE/LWPOLYLINE CAD ↔ `Edge.ExternalKey`/`FromNodeId`/`ToNodeId` | `NÃO DISPONÍVEL` | O JSON do acervo emite layer e coordenadas, sem ID de linha; o LISP CQT registra somente metragem/letra. |
| Comprimento físico ↔ `Edge.Length` | `INDETERMINADA` | Há geometrias e comprimentos lógicos, mas nenhum par físico confiável de nós/trechos para comparação. |

O mecanismo mínimo exigido para confirmar `Node X ↔ poste Y` não foi encontrado: uma chave única que sobreviva à extração e relacione a ocorrência CAD, o circuito/cenário e o nó lógico.

## 7. Análise Geométrica

Não há par `P1 → P2` físico confiável que possa ser associado a uma aresta específica do CQTS. Por isso não foram calculados distância física, diferença absoluta/percentual, azimute ou direção lógica para qualquer `Edge`. Os comprimentos de `TRECHO (m)` são evidência lógica, não foram comparados com geometrias CAD escolhidas por proximidade, layer ou ordem.

Os atributos `E/N` de blocos `CO` contêm valores com magnitude de Easting/Northing, mas são rótulos de coordenadas sem referência a `Node`, `Edge` ou ID LIGHT. A presença de LINEs em layers de rede confirma geometria CAD; não confirma a identidade de seus extremos no modelo lógico.

## 8. CRS e Unidade

O DWG tem `MAPCSASSIGN=nil` e não contém a entrada `ACAD_GEOGRAPHICDATA` no dicionário consultado. As coordenadas/atributos E/N são compatíveis em magnitude com coordenadas projetadas, mas o arquivo não declara sistema, fuso, hemisfério, datum ou EPSG. Os JSONs temporários com fuso 23 são de outros desenhos; o LISP pergunta o fuso e usa 23 como default de ferramenta, não como metadado do ZNA. Assim, fuso 23S é no máximo uma hipótese externa, não uma referência espacial confirmada para este DWG.

`INSUNITS=4` (milímetros) e `MEASUREMENT=1` foram lidos no DWG. `INSUNITS` controla unidades de inserção do AutoCAD e não determina sozinho a unidade numérica das coordenadas de modelo. Como não foi encontrada declaração de unidade para as coordenadas do desenho, a unidade física continua indeterminada; o valor `INSUNITS` impede promover a suposição de metros a contrato sem reconciliação. Datum/EPSG permanecem desconhecidos; `EPSG:31983` segue hipótese preliminar da Fase 28, não adotada.

## 9. Auditoria da Fase 30B

`PhysicalPosition` guarda coordenada, unidade, referência espacial, formato, ID externo e nome do bloco; `PhysicalLineGeometry` guarda coordenadas, unidade, referência e proveniência. Esses tipos são suficientes para transportar geometria física com CRS parcial e origem, mas não tornam `SourceId` uma chave lógica: o acervo pode sintetizar IDs pela ordem, e o modelo não registra se o valor foi extraído de atributo, handle ou gerado. Os tipos existentes não devem ser promovidos a vínculo confiável sem um mapeamento explícito.

`PhysicalGeometryJsonImporter.cs` tem 335 linhas e permanece abaixo do limite ideal documentado de 500. Parsing, validação, normalização de unidades/CRS, aplicação opcional de mapas explícitos e diagnósticos são coesos no adaptador; a investigação não revelou fronteira arquitetural que justifique extração de componentes. Nenhuma refatoração foi feita. Nenhum teste novo foi adicionado, pois não foi comprovada uma regra automática de associação.

## 10. Conclusões Obrigatórias

- **É possível associar automaticamente?** Não, com as evidências atuais.
- **Qual é a chave determinística?** Nenhuma foi encontrada. O rótulo `XX=P1` é repetido e não seleciona uma ocorrência/circuito.
- **Associação Node:** Não determinada. Existe somente candidato textual provável, sem coordenada/ocorrência unívoca.
- **Associação Edge:** Não determinada. O JSON das linhas não tem ID e os LISP não preservam correspondência com `ID LIGHT`.
- **Comprimento físico compatível?** Não verificável; não há pares físicos confiáveis para comparar.
- **CRS suficientemente conhecido?** Não. Há valores E/N com aparência projetada, mas não há CRS/fuso/datum/EPSG declarado; unidade também requer confirmação.
- **Identidade física encontrada:** rótulo de planta no atributo `XX` de blocos `NUM_PLAN`; cada entidade também tem handle no DWG. Isso não equivale a identidade física única de poste.

## 11. Limitações e Próxima Decisão

1. O projeto local não contém JSON DWG identificado como ZNA855820; os JSONs encontrados são de outros desenhos.
2. Os rótulos `P1...P11` se repetem e `LID` não aparece como atributo de nó no DWG.
3. Não foi demonstrada correspondência entre símbolos de poste, linhas de rede, atributos `XX` e linhas do CQTS.
4. O workbook tem coordenadas nulas/zero e comprimentos lógicos, mas não resolve a posição física.
5. Unidade e CRS do DWG permanecem insuficientemente declarados.

Antes de aprovar implementação da Fase 31, o responsável de engenharia deve escolher e validar uma fonte de vínculo, por exemplo: arquivo de mapeamento explícito por circuito/cenário (`Node.ExternalKey` ↔ handle/atributo CAD), enriquecimento do JSON com handle/atributos e ID de LINE, ou associação interativa revisada pelo operador. O fluxo futuro deve também exigir unidade/CRS declarados e rejeitar registros ambíguos; esta fase não implementa nenhuma dessas alternativas.

### Arquitetura Conceitual

```text
QDT/CQTS
  ↓
NetworkModel
  ↓
Node / Edge
  ↓
PhysicalPosition / PhysicalLineGeometry
  ↓
[ identidade física ↔ lógica: INCERTA / NÃO DETERMINADA ]
  ↓
futuro CadExportModel
  ↓
futuro LISP
  ↓
AutoCAD
```

**Gate:** `GO PARA PRÓXIMA DECISÃO — identidade ainda não determinada`. Não implementar LISP, exportação CAD ou associação artificial até haver uma chave/mapa confirmado e CRS/unidade esclarecidos.

## 12. Validação do Repositório

Nenhuma alteração de código, fixture real ou `VERSION_MANIFEST.json` foi necessária nesta fase. A validação final da solução e do diff é registrada no commit da Fase 30C.