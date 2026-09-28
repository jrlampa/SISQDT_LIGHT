# Fase 30E — Rastreio da Origem do Crosswalk Físico-Lógico

**Caso principal:** ZNA19165 — LDA CARMETIBA / Robustez BT
**Baseline:** `e9665a3` (`dev`, igual a `origin/dev`)
**Status:** `CROSSWALK DE DADOS CONFIRMADO ATÉ CQTS; ORIGEM E VÍNCULO AO HANDLE DWG NÃO DETERMINADOS`
**Escopo:** investigação documental e leitura somente; sem alteração de código, DWG, macros, artefatos de projeto ou `VERSION_MANIFEST.json`.

## 1. Pergunta

De onde vem a identidade mostrada no CSV/KMZ, como ela aparece no CQTS e se é possível seguir a mesma identidade até um objeto/handle no DWG. A análise separa equivalência observada entre dados de causalidade/proveniência, que precisa de registro independente.

## 2. Decisão Executiva

No ZNA19165, 36 registros de `state.btTopology.poles` no arquivo `.srua` têm correspondência exata de título e latitude/longitude com os 36 placemarks de ponto do KMZ. Aplicando a função e os parâmetros explícitos do `kmz_pontos_importer.py` (fuso 23, hemisfério sul, resultado UTM arredondado a 3 casas), os 36 também correspondem às coordenadas e nomes do CSV. Três exemplos rastreados individualmente chegam a linhas CQTS com o mesmo PONTO e coordenadas compatíveis.

Isso confirma que as quatro representações carregam uma crosswalk coerente neste recorte; **não demonstra qual arquivo gerou qual outro**. O `.srua` não tem handle, tipo de entidade CAD, revisão/hash DWG ou método de associação. O KMZ não tem IDs XML, `ExtendedData` nem handles. No DWG examinado, rótulos se repetem e as classes geométricas varridas não coincidem com os pontos do CSV. A associação exata a uma entidade/handle, e portanto a `Node`/`Transformer`/`Edge` do produto, continua indeterminada.

**Gate:** não aprovar associação automática nem Fase 31. Manter geometria física não associada até existir proveniência validada pela engenharia.

## 3. Baseline e Salvaguardas

- Branch `dev`; `HEAD` e `origin/dev` em `e9665a3451cb234119374d875fbf8eb180f4d0b3`.
- Identidade Git local: `Jonatas <jonatas.lampa@im3brasil.com.br>`.
- A exclusão staged preexistente `src/QdtCqts.Desktop.Wpf/QdtCqts.Desktop.Wpf_dwkfellp_wpftmp.csproj` foi preservada fora do escopo.
- Não executar VBA/LISP, não gravar nos arquivos de engenharia, não usar proximidade ou ordem como identidade e não alterar `VERSION_MANIFEST.json`.

## 4. Fontes e Método

| Fonte | Leitura/escopo |
|---|---|
| `ZNA19165 - ROBUSTEZ DE BT.srua` | JSON somente leitura; examinados topologia, resumos/histórico de exportação e metadados. |
| `LDA CARMETIBA - ZNA 19165.kmz` | KML interno; nomes, coordenadas, IDs XML e `ExtendedData`. |
| `LDA CARMETIBA - ZNA 19165_utm_23s.csv` | Campos `X,Y,Z,Name`; comparação numérica. |
| `ZNA19165 - ROBUSTEZ DE BT-CQTS-ATUAL.xlsx` e `ZNA19165 - ROBUSTEZ DE BT-CQTS.xlsx` | OpenXML somente leitura; macros não executadas; PONTO, montante e coordenadas. |
| `ROBUSTEZ_BT_ZNA1916_PROJETO.dwg` | Scanner somente leitura em cópia temporária da Fase 30D; original não editado. |
| `cad2kmz.lsp`, `cad_to_kmz_exporter.py` | Regras candidatas de exportação CAD→KMZ; nenhum log/proveniência os liga a este KMZ. |
| `kmz_pontos_importer.py/.lsp` | Caminho KMZ→AutoCAD; a conversão foi usada como teste numérico, não como prova de origem. |
| `sisRUA.bundle` local | Inspecionados `Resources/app.js` e scripts Python; não foi encontrada serialização `.srua` nem origem deste snapshot. |
| SISQDT_LIGHT | `DomainTypes.cs`, `PhysicalGeometryJsonImporter.cs`, `ExcelEvidenceImporter.cs`; sem importador `.srua`/KMZ para `NetworkModel`. |

## 5. Artefatos e Cronologia Observada

O `.srua` tem versão `0.9.0`, timestamp interno `2026-04-20T11:10:12.225Z`, 36 postes BT, zero transformadores BT e zero arestas. O CSV tem 36 linhas; o KMZ contém 36 placemarks de ponto. O registro de exportação salvo no `.srua`, porém, é de `2026-04-13T15:09:32.204Z` e declara `totalPoles=5`, `totalEdges=0`, `totalTransformers=0`, `parityStatus=partial` e cinco falhas. Seu `btContextUrl` aponta para download em `localhost:3001`; o arquivo com aquele identificador não foi localizado nos diretórios consultados.

Os metadados locais de criação/modificação do OneDrive não formam cronologia causal consistente entre CSV, KMZ, `.srua`, CQTS e DWG. Foram tratados somente como contexto, nunca como prova de que um artefato originou outro.

## 6. Identificadores Presentes

- Cada poste no `.srua` contém `id`, `lat`, `lng`, `title` e `ramais`. O `id` é sequencial (`P1`…`P36`) dentro deste snapshot; `title` é o rótulo exibido. Não são intercambiáveis: `id=P1` tem `title=P2`; `id=P31` tem `title=P24 - TRAFO`; `id=P32` tem `title=P25`.
- CSV usa `Name`; KMZ usa `Placemark/name`. Ambos conservam o rótulo visível, mas não identificador persistente de entidade CAD.
- CQTS usa `PONTO` e `PONTO MONTANTE` no contexto de aba/circuito/cenário; algumas abas também têm coordenadas.
- DWG tem handles locais à revisão e atributos/textos de planta. Nenhuma fonte de intercâmbio analisada guarda esses handles nem revisão/hash do DWG.

## 7. Teste `.srua` ↔ KMZ ↔ CSV

| Verificação | Resultado | Interpretação |
|---|---:|---|
| Postes BT no `.srua` | 36 | Snapshot consultado. |
| Placemarks KML com `<Point>` | 36 | Mesma cardinalidade. |
| `.srua.title` + lon/lat idênticos ao nome + lon/lat KML | 36/36 | Igualdade de conteúdo no recorte. |
| `latlon_to_utm(lat,lng,23,False)` + arredondamento a 0,001 m comparado ao CSV | 36/36 | Correspondência sob parâmetros explícitos do importer local. |
| Residual máximo antes do arredondamento | X: `0,000000034 m`; Y: `0,000005087 m` | Precisão observada, não tolerância aprovada para associação CAD. |
| Nomes CSV repetidos | `.`: 5; `P15`: 2 | Nome isolado não é chave injetiva; os P15 estão em posições distintas. |

O KMZ não possui `ExtendedData` nem IDs XML em placemarks; a coordenada KML é longitude, latitude (e altitude opcional). O conversor usa fuso 23 e arredonda a milímetros, mas esse teste não declara datum/EPSG do conjunto nem CRS/unidade do DWG.

## 8. Três Rastros Individuais até o CQTS

| Rótulo | `.srua` (`id` → `title`) | CSV/KMZ | CQTS (aba/cenário, X/Y) | Resultado |
|---|---|---|---|---|
| `P2` | `P1` → `P2` | CSV `Name=P2`; placemark `P2`; lon/lat idênticos | `LADO 1 PROJ2`, `PONTO=2`, `MONTANTE=0`, `632359 / 7459714` | Join por coordenadas inteiras no CQTS; o ID interno do `.srua` não é o rótulo. |
| `P24 - TRAFO` | `P31` → `P24 - TRAFO` | CSV `Name=P24 - TRAFO`; placemark homônimo e lon/lat idênticos | `LADO 1`, `PONTO=24`, `MONTANTE=0`, `632486 / 7459655` | Join local por coordenada e nome/sufixo; não prova handle/transformador. |
| `P25` | `P32` → `P25` | CSV `Name=P25`; placemark `P25`; lon/lat idênticos | `LADO 1`, `PONTO=25`, `MONTANTE=24`, `632519 / 7459633` | Join local por coordenada e PONTO no contexto da aba. |

As coordenadas CQTS são inteiras; CSV é decimal. A comparação confirma esses registros, não introduz tolerância geral. A Fase 30D documentou 39 ocorrências CQTS com join único no CSV/KMZ pela precisão inteira gravada; este rastro acrescenta a igualdade direta `.srua`↔KML e os exemplos acima.

## 9. O Que o Join Não Prova

1. Não prova que o `.srua` gerou CSV/KMZ ou que CSV/KMZ gerou o `.srua`; não há direção de produção registrada.
2. Não prova que os arquivos correspondam à mesma revisão de desenho/cenário.
3. Não prova que `id=P31` ou `title=P24 - TRAFO` seja chave estável entre exportações.
4. Não prova que `PONTO=24` seja a entidade transformadora ou um handle específico.
5. Não autoriza associação por proximidade, ordem de seleção ou igualdade de rótulo repetido.

## 10. Origem e Direção dos Dados

O `.srua` contém estado de seleção do mapa, metadados, topologias BT/MT e histórico resumido. O snapshot atual tem coordenadas geográficas e rótulos em `title`, mas seu único registro de exportação declara cinco postes, enquanto a topologia atual contém 36. O arquivo de contexto apontado não foi localizado. Esse histórico não pode ser tratado como registro de origem dos 36 pontos atuais.

Não foi encontrado serializador `.srua`, log de importação, manifesto de origem, hash da fonte ou procedimento do operador nos caminhos consultados. O pacote local `sisRUA.bundle` exporta GeoJSON/JSON e não preenche a lacuna. A existência do `.srua` prova um snapshot de estado, não autoria dos demais artefatos.

## 11. Ferramentas CAD/KMZ e Candidatos de Fluxo

- `cad2kmz.lsp` seleciona INSERT/POINT e LINE/LWPOLYLINE/POLYLINE. Procura tags `NUM`, `PONTO`, `N`, `Nº` ou `POSTE`; sem valor correspondente sintetiza `P1…` pela ordem de seleção. O JSON conserva ID/bloco/X/Y de ponto e camada/vértices de linha, não handle nem extremos lógicos.
- `cad_to_kmz_exporter.py` consome JSON temporário desse fluxo. Nenhum payload, log de seleção ou referência de origem identificou esse processo como produtor do KMZ ZNA19165.
- `kmz_pontos_importer.py/.lsp` faz o caminho inverso: lê placemarks, usa nome/descrição, converte lon/lat para UTM e gera comandos para inserir blocos/rótulos no AutoCAD. É importador, não evidência de como o KMZ foi criado.
- O DWG tem valores em `NUM_PLAN.XX`, mas a tag `XX` não está no filtro de atributos do `cad2kmz.lsp`. Não atribuir esse KMZ ao LISP apenas pela semelhança dos nomes.

## 12. Tentativa de Vínculo ao DWG

O DWG analisado na Fase 30D continha 13.219 entidades nas classes varridas, incluindo 1.129 INSERT, 42 POINT, 3.060 LINE e 2.602 LWPOLYLINE. Tags `NUM_PLAN.XX` P#/TR se repetem em múltiplos blocos/posições; `NUM=01…08` foi observado em blocos `N_PONTO` de MT existente. A varredura de INSERT, POINT e vértices/extremos de linha não encontrou coincidência exata nem após arredondamento ao metro com pontos CSV; 103 rótulos P#/TR consultados também não tiveram join por coordenada.

CSV, KMZ, `.srua` e CQTS não fornecem handle, GUID de entidade, hash/revisão DWG ou tabela de associação. Não há caminho reversível dos três exemplos até entidade/handle. Rótulos visuais não suprem a ausência de proveniência por serem repetidos.

## 13. CRS e Unidade

- CSV chama-se `_utm_23s`; KMZ guarda lon/lat; `.srua` guarda lat/lng. A função usada no teste assume parâmetros WGS84 e fuso 23; os metadados consultados não estabelecem EPSG oficial.
- DWG ZNA19165 tem `INSUNITS=0`, `MAPCSASSIGN=nil` e não tem `ACAD_GEOGRAPHICDATA`. Isso não declara unidade numérica nem CRS das coordenadas CAD.
- Não propagar EPSG:31983, SIRGAS 2000, WGS84, metro ou fuso 23 ao DWG sem validação de engenharia.

## 14. Comparação com Fases Anteriores e Outros Projetos

A Fase 30D encontrou crosswalk CQTS↔CSV/KMZ local no ZNA19165 e variação de convenções em ZNA855820, ZNA17846, ZNA23479 e ZNA402202. A Fase 30E fecha mais um elo de equivalência para os 36 pontos ZNA19165, mas não muda a conclusão multi-projeto: o método não se generaliza como contrato. No ZNA855820, coordenadas CQTS seguem ausentes; nos demais casos amostrados, convenções e resultados de coordenadas diferem.

## 15. Hipóteses e Resultados

| Hipótese | Resultado | Evidência |
|---|---|---|
| `.srua.title`/lat/lon representa conteúdo dos placemarks KMZ | `CONFIRMADA` para 36/36 | Igualdade literal de nome e coordenadas. |
| CSV é geometricamente coerente com `.srua`/KMZ | `CONFIRMADA` sob conversão explícita | 36/36 no fuso 23, hemisfério sul e arredondamento a 0,001 m. |
| CQTS contém crosswalk local para estes rótulos | `CONFIRMADA` no recorte | Três rastros nesta fase; 39 joins anteriores na Fase 30D. |
| `.srua` foi fonte produtora do CSV/KMZ | `INDETERMINADA` | Nenhum serializador/log/manifesto; timestamps não demonstram causalidade. |
| Histórico `.srua` representa os 36 pontos atuais | `REFUTADA` | Histórico declara cinco postes e zero arestas; contexto não localizado. |
| Nomes CSV/KMZ são chaves únicas | `REFUTADA` | `P15` em duas posições; `.` aparece cinco vezes. |
| Há handle DWG identificável para algum dos exemplos | `NÃO DEMONSTRADA` | Nenhum handle propagado; rótulos repetidos; zero joins geométricos na varredura. |
| `P24 - TRAFO` prova identidade de `Transformer.ExternalKey` | `INDETERMINADA` | Nome/coord. coincidem localmente; falta mapa para símbolo/handle. |
| Associação automática pode ser implementada | `NO-GO` | Origem, chave CAD, revisão, CRS/unidade e tolerância ausentes. |

## 16. Relações Confirmadas

1. `.srua.title` + lon/lat ↔ KML `name` + lon/lat: 36/36 neste snapshot.
2. `.srua`/KML ↔ CSV: 36/36 sob a conversão e arredondamento explícitos do importer examinado.
3. CQTS ↔ CSV/KMZ: crosswalk local corroborada na precisão inteira do workbook.
4. CQTS `PONTO MONTANTE` preserva relação lógica no contexto da aba; `PONTO=25` tem montante 24 no exemplo.

## 17. Relações Refutadas

- Usar `id` interno `.srua` como sinônimo do rótulo (`P1`→`P2` no exemplo).
- Usar `Name/title` sozinho como chave global (`P15` e `.` repetidos).
- Tratar o histórico `.srua` como exportação dos atuais 36 pontos.
- Tratar `cad2kmz.lsp` como produtor confirmado do KMZ ZNA19165.
- Tratar a conversão UTM testada como prova do CRS/unidade do DWG.
- Tratar `P24 - TRAFO` como handle CAD ou `Transformer.ExternalKey` sem mapeamento.

## 18. Relações Ainda Indeterminadas

- Aplicação que criou/gravou o `.srua` e fonte que populou sua topologia BT.
- Direção de cópia entre `.srua`, KMZ e CSV; registro/processo de exportação correspondente.
- Revisão DWG que, se alguma, originou CSV/KMZ.
- Handle/entidade DWG de cada ponto e multiplicidade poste↔bloco.
- Associação entre segmento físico CAD e `Edge`/`FromNodeId`/`ToNodeId`.
- Vínculo de `P24 - TRAFO` a símbolo físico e `Transformer.ExternalKey`.
- CRS/datum/EPSG oficial, unidade CAD, precisão, tolerância e procedimento de aprovação humana.

## 19. Contrato Mínimo para Futura Associação

Não alterar o domínio ou criar associação automática nesta fase. Qualquer futura tabela auditável de vínculo precisa registrar:

`ProjectKey + Scenario + Circuit/Side + LogicalType/Id + SourceArtifactHash + ArtifactRevision + DWGHash/Revision + Handle/EntityType + AttributeTag/Value + Name + Coordinates + CRS/Unit/Precision + MatchMethod + OperatorApproval + Timestamp`.

Rejeitar nome duplicado sem desambiguação; exigir revisão humana diante de múltiplas entidades candidatas; preservar origem e regra de transformação; revisar vínculos quando fonte ou DWG mudar. Coordenada só corrobora identidade com CRS, unidade, precisão e regra aprovados.

## 20. Gate de Engenharia

**`CROSSWALK DE DADOS CONFIRMADO ATÉ CQTS; ORIGEM E VÍNCULO AO HANDLE DWG NÃO DETERMINADOS`.**

O rastro observável é `title/name + coordenada` entre `.srua`/KMZ/CSV e `PONTO + coordenada + aba/cenário` no CQTS. O caminho termina antes do DWG: não existe handle ou revisão compartilhados. Fase 31 não aprovada; sem mudança de código, CAD/LISP, geometria física associada ou `VERSION_MANIFEST.json`.

## 21. Validação e Limitações

- Comparação somente leitura: 36/36 `.srua`↔KML por nome e lon/lat; 36/36 `.srua`/KML↔CSV após arredondamento UTM a 0,001 m.
- Três linhas CQTS conferidas por OpenXML somente leitura; macros não executadas. Os 39 joins anteriores continuam limitados ao método/precisão descritos na Fase 30D.
- Análise DWG reutiliza varredura da cópia feita na Fase 30D. Custom objects e relações proprietárias não foram decodificados; original não alterado.
- Busca de serializadores limitada aos diretórios locais de ferramentas/acervo consultados; não prova inexistência de serviço remoto, cópia externa ou processo manual.
- Nenhum dado foi adicionado a fixtures. `dotnet test --nologo`: 212 aprovados, 0 falhas, 0 ignorados.