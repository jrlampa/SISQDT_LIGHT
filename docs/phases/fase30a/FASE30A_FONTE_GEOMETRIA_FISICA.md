# FASE 30A — FONTE DA GEOMETRIA FÍSICA E COORDENADAS

**Data:** 28 de Setembro de 2026  
**Status:** CONCLUÍDO (Investigação / Sem alteração de código de produção)  
**Autor:** Jonatas (`jonatas.lampa@im3brasil.com.br`)  

---

## 1. Fontes Investigadas

| Fonte | Inspecionada | Método |
| :--- | :--- | :--- |
| `CQTS__ZNA855820_PROJ.xlsx` — aba `COORDENADAS` | Sim | Leitura OpenXML via Python |
| `CQTS__ZNA855820_PROJ.xlsx` — abas `LADO 1`, `LADO 2`, `PROJ1`, `LADO 1 PROJ` | Sim | Leitura OpenXML via Python |
| `REDE_INVERTIDA_ZNA855820.dwg` | Sim | Inspeção binária (ASCII + UTF-16LE + scan de doubles) |
| `ZNA855820.pdf` | Auxiliar | Imagem de prancha; sem coordenadas extraíveis |
| `cqt_cad_extractor.lsp` | Sim | Leitura de código-fonte |
| `mapa_postes_exec.lsp` | Sim | Leitura de código-fonte |
| `C:\Temp\cad2kmz_input.json` | Sim | JSON de intercâmbio gerado por ferramenta do acervo |
| `C:\Temp\kmz_points_input.json` | Sim | JSON de intercâmbio gerado por ferramenta do acervo |

---

## 2. Fonte Real das Coordenadas dos Postes

### 2.1. Excel CQTS — Aba COORDENADAS
Presente como campo estruturado (`PONTO`, `X / LAT / E`, `Y / LON / N`) mas **todos os valores estão nulos** no projeto real ZNA855820. A aba existe como template mas **não é preenchida operacionalmente**.

### 2.2. Excel CQTS — Abas LADO 1 / LADO 2 / PROJ1
Colunas explícitas de `COORDENADAS UTM` com subcolunas `LONG` e `LAT` presentes no cabeçalho, mas **com valores zero em todos os nós**. Somente o comprimento do trecho (`TRECHO m`) é preenchido com valores reais.

### 2.3. DWG — Fonte Primária Confirmada

O DWG `REDE_INVERTIDA_ZNA855820.dwg` contém **22 pares de coordenadas double identificados como UTM válidas** para a região do Rio de Janeiro:

| Amostra | X (Easting) | Y (Northing) |
| :--- | :---: | :---: |
| Coord 1 | 674 068.6 | 7 482 608.1 |
| Coord 2 | 673 010.1 | 7 482 568.6 |
| Coord 3 | 673 055.7 | 7 482 619.2 |
| Coord 4 | 672 921.5 | 7 482 486.6 |
| Coord 5 | 671 047.5 | 7 482 364.7 |

Faixa: X ≈ 671 000–675 200; Y ≈ 7 482 000–7 483 200. Compatível com Fuso UTM 23S, RJ.

Os blocos identificados no DWG incluem: `POSTE PROJ`, `POSTE C/ LIMINARIA`, `POSTE PARTICULAR PROJ`, `TRAFO RET`, `DESC MT 13kV`, `240AMX`, `rede_existente`, `RM240`.

### 2.4. Mecanismo de Intercâmbio do Acervo CAD

O acervo de ferramentas do usuário (`aux cad/`) já implementa o fluxo de extração e injeção de coordenadas UTM via JSON. Evidência direta em `C:\Temp\cad2kmz_input.json`:

```json
{
  "titulo": "AN_A057152931_DESENHO",
  "fuso": 23,
  "postes": [
    {"id": "P1", "block": "COORDEN", "x": 585766.649, "y": 7454193.914},
    ...
  ],
  "linhas": [
    {"layer": "CARIMBO", "coords": [[x1,y1],[x2,y2]]},
    ...
  ]
}
```

**→ FONTE REAL: DWG é a fonte de verdade das coordenadas físicas. O Excel CQTS apenas mantém colunas de coordenadas como campo futuro, não preenchidas operacionalmente.**

---

## 3. Fonte Real da Geometria dos Trechos

O DWG armazena a geometria dos trechos como entidades `LINE` e `LWPOLYLINE` (evidenciado pelo `cqt_cad_extractor.lsp` e `cad2kmz_input.json`). As linhas são representadas por coordenadas de pontos inicial e final:

```json
{"layer": "CARIMBO", "coords": [[585913.5, 7454041.8], [585866.5, 7454041.8]]}
```

O LSP `cqt_cad_extractor.lsp` extrai comprimento de trechos selecionando:
```lisp
(ssget '((0 . "LINE,LWPOLYLINE,POLYLINE,ARC")))
```

**→ GEOMETRIA: LINE simples (dois pontos) é o caso primário. LWPOLYLINE e ARC existem mas são casos secundários. A geometria intermediária pode existir mas a prática operacional usa LINE ponto a ponto.**

---

## 4. Identidade Lógica ↔ Física

### 4.1. Mapeamento Disponível no Acervo
O `cad2kmz_input.json` usa IDs sequenciais simples (`P1`, `P2`, ...) com o nome do bloco CAD (`block_name`). O `kmz_points_input.json` usa IDs compostos como `Bloco_*U729_1`.

### 4.2. Situação no Projeto ZNA855820
- O CQTS utiliza numeração sequencial de trechos (`TRECHO 1`, `TRECHO 2`, ...) e referências de montante (`PONTO MONTANTE`).
- O DWG usa blocos nomeados (`POSTE PROJ`, `POSTE C/ LIMINARIA`) com atributos CAD.
- **Não há chave explícita mapeando `ExternalKey` do sisQDT_LIGHT com bloco específico do DWG** no projeto ZNA855820 — as ferramentas do acervo criam IDs sintéticos sequenciais durante a extração do DWG.

### 4.3. Conclusão da Identidade
```
Node lógico (sisQDT_LIGHT) → ExternalKey → [chave ausente] → Bloco CAD
```

O elo `ExternalKey ↔ Bloco CAD` **não existe hoje como dado persistido** em nenhum dos arquivos analisados. Ele seria criado no momento da extração do DWG (por convenção de nomenclatura ou por seleção interativa pelo operador).

**→ IDENTIDADE: PARCIAL. Chave física existe no DWG (coordenada de inserção do bloco). Mapeamento para ExternalKey do domínio lógico é AUSENTE e precisaria ser estabelecido.**

---

## 5. CRS — Sistema de Coordenadas

### 5.1. Evidência Direta
- `cad2kmz_input.json`: `"fuso": 23`
- `kmz_points_input.json`: `"zone": 23` para todos os pontos
- Coordenadas lidas no DWG: X ≈ 671 000–675 200, Y ≈ 7 482 000–7 483 200 — valores métricos compatíveis com UTM 23S
- `mapa_postes_exec.lsp`: insere bloco com `pBase = (vlax-3d-point (list 687394.593 7465634.128 0.0))` — valores métricos UTM

### 5.2. Datum
**Não há evidência explícita de SIRGAS 2000 vs SAD69 vs WGS84 nos arquivos**. O fuso 23 é confirmado. O datum não pôde ser determinado nos arquivos analisados.

**→ CRS: PARCIALMENTE CONFIRMADO. Fuso UTM 23 (Sul) confirmado. Datum (SIRGAS 2000 / WGS84 / SAD69) NÃO DETERMINADO por ausência de marcação explícita.**

---

## 6. Unidade Geométrica

As coordenadas do DWG são métricas (metros), confirmado pela faixa dos valores (6 dígitos inteiros para Easting, 7 para Northing) e pela inserção de postes com coordenadas na ordem de centenas de milhares de metros.

**→ UNIDADE: METROS (confirmado).**

---

## 7. Geometria dos Trechos

- **Tipo primário:** `LINE` (dois pontos: P1→P2)
- **Tipos secundários presentes no DWG:** `LWPOLYLINE`, `POLYLINE`, `ARC` (cobertos pelo LSP mas caso secundário)
- **Geometria intermediária:** Pode existir em `LWPOLYLINE` com múltiplos vértices; não confirmado para trechos de BT no ZNA855820
- A prática operacional demonstrada pelo acervo usa linhas simples com dois pontos de coordenada

**→ GEOMETRIA: LINE(P1,P2) como caso primário. Suporte a LWPOLYLINE recomendado para robustez.**

---

## 8. LSP Existente — Achados Relevantes para Reutilização

| Funcionalidade | Arquivo | Relevância |
| :--- | :--- | :--- |
| Extração de comprimento de LINE/LWPOLYLINE | `cqt_cad_extractor.lsp` | `get-curve-length` reutilizável |
| Inserção de bloco com coord UTM | `mapa_postes_exec.lsp` | Padrão de `vlax-3d-point` para inserção georreferenciada |
| JSON de intercâmbio postes + linhas | Acervo `cad2kmz` | Formato `{id, block, x, y, fuso}` como contrato de transporte |
| Extração de blocos INSERT do DWG | `kmz_points_input.json` | Bloco → (`x`, `y`, `zone`) já funciona para outros projetos |

**→ O acervo já possui um fluxo operacional de extração DWG → JSON com coordenadas UTM. O sisQDT_LIGHT pode consumir esse mesmo formato JSON como entrada de coordenadas, sem precisar ler o DWG diretamente.**

---

## 9. Limitações Identificadas

1. **Coordenadas no Excel CQTS são nulas:** O preenchimento das colunas UTM no CQTS é manual/opcional. Não há garantia de que estarão preenchidas em projetos reais.
2. **Datum não determinado:** Sem evidência de SIRGAS 2000 vs SAD69 nos metadados dos arquivos.
3. **Chave lógica ↔ física ausente:** O mapeamento `ExternalKey` do sisQDT_LIGHT para bloco específico do DWG não existe hoje.
4. **DWG AC1032 não é lido por ezdxf:** A leitura direta do DWG pelo backend C# requereria biblioteca proprietária (ODA/Teigha) ou conversão prévia para DXF.

---

## 10. Recomendação Arquitetural

**Fonte de Verdade para Geometria Física:**
> **O DWG é a única fonte que contém coordenadas UTM reais dos postes.** O Excel CQTS não é fonte confiável de coordenadas geográficas.

**Estratégia para "Copiar para o CAD" (Fase 30B+):**
1. O operador usa a ferramenta existente do acervo (ou uma futura integrada) para exportar o DWG para JSON UTM `{id, block, x, y, fuso}`.
2. O sisQDT_LIGHT recebe esse JSON como entrada opcional de coordenadas, associando-as aos nós via chave a definir (sugestão: número de ordem do poste ou seleção manual).
3. O gerador LISP usa as coordenadas UTM do JSON para posicionar os elementos no AutoCAD nas coordenadas reais.
4. **Não ler o DWG diretamente no backend C#.** O DWG permanece no domínio do AutoCAD; o sisQDT_LIGHT opera sobre o JSON de intercâmbio.

---

## 11. Critérios de Conclusão — Respostas Definitivas

| Critério | Resposta |
| :--- | :--- |
| **POSTE — Fonte da coordenada** | **DWG** (coordenadas de inserção do bloco em UTM Fuso 23) |
| **TRECHO — Fonte da geometria** | **DWG** (entidades LINE/LWPOLYLINE entre blocos de postes) |
| **IDENTIDADE** | **PARCIAL** — `ExternalKey` lógica ↔ bloco CAD não tem chave explícita; seria criada por convenção ou seleção interativa |
| **CRS** | Fuso UTM 23 **CONFIRMADO**; Datum (SIRGAS/SAD69/WGS84) **NÃO DETERMINADO** |
| **UNIDADE** | **Metros** — CONFIRMADO |
| **GEOMETRIA** | **LINE(P1,P2)** — primário; LWPOLYLINE secundário |
| **FONTE** | **DWG** (não o Excel) |
| **CONFIANÇA** | **PARCIAL** — coordenadas e unidade confirmadas; datum e chave de identidade não confirmados |

## 12. Reavaliação do Caso ZNA855820 (Fase 30C)

Esta nota preserva a conclusão histórica da investigação original e registra a qualificação obtida pela inspeção direta posterior do DWG ZNA855820.

- Os JSONs temporários usados na seção 2.4 não são extrações do ZNA855820: `cad2kmz_input.json` identifica `AN_A057152931_DESENHO` e `kmz_points_input.json` contém IDs `Bloco_*U729_n`.
- No DWG real, foram encontrados atributos E/N em blocos `CO`, mas sem vínculo com nós/trechos do CQTS. O desenho retorna `MAPCSASSIGN=nil`, não contém `ACAD_GEOGRAPHICDATA` e declara `INSUNITS=4` (unidades de inserção em milímetros); isso não determina sozinho a unidade numérica das coordenadas do modelo.
- Portanto, para o ZNA855820, metro e fuso 23 não podem ser tratados como confirmados por aqueles JSONs ou pelo default do LISP. A magnitude E/N é compatível com coordenadas projetadas, mas unidade/fuso/hemisfério/datum/EPSG permanecem sem declaração suficiente.
- A fonte física continua sendo o DWG; esta reavaliação não escolhe CRS nem cria associação lógica/física. Ver [Fase 30C](../fase30c/FASE30C_RECONCILIACAO_IDENTIDADE_FISICA_LOGICA.md) para inventário e evidências.
