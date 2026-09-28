# Fase 30B — Domínio Espacial e Importação de Geometria Física

**Fonte de evidência:** [Fase 30A](../fase30a/FASE30A_FONTE_GEOMETRIA_FISICA.md)
**Estado:** implementada na branch `dev`; validação completa registrada abaixo

## Modelo Escolhido

- `Node.LayoutX/LayoutY` são coordenadas de layout esquemático; `Node.PhysicalPosition` é a posição física independente.
- `PhysicalPosition` guarda Easting, Northing, unidade, referência espacial, formato de origem, ID externo e nome do bloco.
- `SpatialReference` representa tipo de sistema, zona, hemisfério, datum e EPSG como campos independentes; zona/hemisfério/datum/EPSG podem ser desconhecidos.
- `Edge.PhysicalGeometry` é opcional e contém uma sequência de coordenadas físicas, unidade, CRS e proveniência. A geometria elétrica `Edge.Length` não é recalculada nem substituída.

## Contrato JSON

O adaptador aceita o perfil de intercâmbio evidenciado pela Fase 30A:

- `postes`: `id`, `block`, `x`, `y`, `fuso`;
- `linhas`: `layer`, `coords` como pares `[x,y]`; `id` é opcional porque o formato de origem não o garante;
- `unidade`/`unit` é opcional no JSON. O chamador declara `UnitCode.Meter`; unidade divergente é rejeitada. A associação ao perfil métrico vem da evidência da Fase 30A, não de inferência por valores.

O JSON é interpretado por `System.Text.Json` em `QdtCqts.Infrastructure.Geometry`; nenhum código lê DWG ou conhece detalhes binários do AutoCAD.

## Estratégia de Importação

`PhysicalGeometryJsonImporter` valida IDs/blocos, números finitos, faixa métrica UTM, fusos entre 1 e 60, unidade, linhas, duplicidades e mapas de associação. O resultado inclui um `NetworkModel` imutável atualizado em memória, pontos/linhas importados, contagens associados/não associados/duplicados/inválidos, referências espaciais, avisos e erros. Persistência em SQLite não foi adicionada.

IDs de origem duplicados têm ocorrências posteriores ignoradas. Linha sem `id` preserva coordenadas e layer sem fabricar identidade. Fuso ausente preserva a posição como referência UTM parcial; fuso presente inválido rejeita o elemento.

Como o formato observado não inclui fuso em `linhas`, o adaptador usa a zona comum dos postes válidos somente quando todos declaram o mesmo fuso; com fusos ausentes/mistos, mantém a zona da linha desconhecida e emite aviso.

## Estratégia de Identidade

Não há associação automática por ordem, proximidade ou igualdade incidental com `ExternalKey`. O chamador pode fornecer mapas explícitos `sourceId → Node.Id` e `sourceId → Edge.Id`. Sem mapa, o elemento permanece no resultado de importação como não associado. Um mapeamento inexistente ou duplicado gera aviso; não gera coordenada ou vínculo substituto.

## CRS

A Fase 30A confirma coordenadas métricas e fuso 23 no caso analisado, mas não determina datum. O adaptador identifica o tipo UTM e preserva o fuso por elemento quando informado. Hemisfério, datum e EPSG permanecem nulos salvo metadado explicitamente fornecido pelo chamador. Não há EPSG default, conversão ou reprojeção. A recomendação EPSG da arquitetura preliminar da Fase 28 não é adotada como contrato.

## Limitações e Decisões

- As coordenadas WPF usam `LayoutX/LayoutY`; importar `PhysicalPosition` não muda posições de tela nem chama o layout engine.
- A geometria de linhas só se associa a um `Edge` com ID externo de linha e mapa explícito. O JSON de exemplo da Fase 30A não contém essa identidade, portanto suas linhas permanecem não associadas.
- A fixture de teste é mínima e anonimizada; os arquivos físicos do ZNA855820 não foram copiados para o repositório.
- Não há leitura DWG, LISP, exportação CAD, clipboard, WebView/mapa interativo, cenários múltiplos ou transformação de CRS.

## Testes

`PhysicalGeometryJsonImporterTests`: **14 testes** cobrindo JSON válido, X/Y, fuso 23, metro, CRS parcial/desconhecido, mapeamento válido, ausência de vínculo mesmo quando `id` coincide com `ExternalKey`, ID ausente/duplicado, coordenada/fuso/unidade inválidos, linha sem ID e ausência de geometria de linha. Um teste WPF confirma que UTM não é usado como layout. Suíte completa: **212 aprovados, 0 falhas, 0 ignorados** (`dotnet test --nologo`). Build completo sem erros.

## Próximos Passos

Uma associação operacional requer definição explícita da chave ou mapa pelo usuário de engenharia. Nenhuma fase/prioridade posterior foi aprovada: `NEXT_PHASE_NOT_DEFINED`.
