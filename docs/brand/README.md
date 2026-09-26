# Manual de Identidade Visual e Diretrizes de Marca — sisQDT_LIGHT

> **Sistema Unificado de Cálculo de Redes Elétricas (QDT + CQTS)**  
> **Versão da Marca:** 1.0.0  
> **Data:** 2026-09-26  
> **Status:** Aprovado e Integrado ao Sistema  

---

## 1. Conceito e Significado

O produto **sisQDT_LIGHT** é a plataforma profissional de engenharia elétrica dedicada ao dimensionamento, análise térmica e cálculo de queda de tensão em redes de distribuição de energia elétrica.

A identidade visual foi concebida para transmitir:
- **Precisão Geométrica e Rigor Técnico:** Estruturas ortogonais e radiais limpas, sem ornamentos supérfluos.
- **Topologia de Redes Elétricas:** Representação visual de tronco de alimentação, derivações, bifurcações e nós de carga/consumidores.
- **Integração QDT + LIGHT:** União harmoniosa entre o motor matemático histórico e as especificidades técnicas da rede de distribuição, preservando total originalidade sem reproduzir ou violar logotipos proprietários ou marcas registradas de concessionárias.

### Anatomia do Símbolo
1. **Nó Raiz Superior (TR):** Representa o transformador de distribuição ou ponto de entrega com anel de proteção e destaque em âmbar elétrico.
2. **Nó Central (LID):** Ponto focal de interconexão, barramento e derivação de circuitos.
3. **Malha Técnica Circular:** Círculos concêntricos e linhas pontilhadas de cota que remetem aos diagramas unifilares e coordenadas geográficas (2.5D Half-way BIM).
4. **Ramificações Radiais:** Linhas azuis de espessura técnica que modelam os alimentadores principais e secundários ramificados.
5. **Cota Diferencial (Delta V):** Segmento vertical âmbar adjacente ao tronco que simboliza a medição de queda de tensão e a impedância linear calculada.
6. **Nós Terminais (Folhas):** Cargas concentradas e agrupamentos de unidades consumidoras.

---

## 2. Nomenclatura e Tipografia Oficial

O nome oficial do software deve ser grafado estritamente como:
$$\textbf{sisQDT\_LIGHT}$$

- **sis:** minúsculo, peso regular/médio, cor neutra (grafite escuro em fundo claro, branco suave em fundo escuro). Simboliza "sistema" como infraestrutura sólida e discreta.
- **QDT:** maiúsculo, peso extra-bold técnico, cor Azul Elétrico (`#0A58CA` / `#3B82F6`). Destaca o núcleo de cálculo e metodologia de Queda de Tensão.
- **_LIGHT:** maiúsculo iniciado por underscore, peso bold, cor Âmbar Energia (`#F59E0B`). Identifica a parametrização de rede de distribuição e normas de concessionária.

A família tipográfica primária recomendada é a **Segoe UI** (nativa no ecossistema Windows desktop) com fallbacks técnicos para **Inter**, **Roboto** ou **Arial**.

---

## 3. Paleta de Cores Oficial

| Aplicação | Nome | Hex | RGB | Significado |
| :--- | :--- | :--- | :--- | :--- |
| **Primária** | Azul Elétrico Técnico | `#0A58CA` | `rgb(10, 88, 202)` | Confiabilidade, engenharia, circuitos e cálculo |
| **Secundária** | Azul Distribuição | `#3B82F6` | `rgb(59, 130, 246)` | Ramificações, alimentadores e caminhos de rede |
| **Destaque** | Âmbar Energia | `#F59E0B` | `rgb(245, 158, 11)` | Potência ativa ($M$), fluxo de corrente e nós ativos |
| **Escuro** | Grafite Profundo | `#0F172A` | `rgb(15, 23, 42)` | Fundo escuro (Dark Mode) e alto contraste |
| **Claro** | Branco Técnico | `#F8FAFC` | `rgb(248, 250, 252)` | Fundo claro e preenchimento de nós |
| **Metadados** | Cinza Técnico | `#64748B` | `rgb(100, 116, 139)` | Linhas de cota, eixos de malha e subtítulos |

---

## 4. Arquivos e Recursos Oficiais

Todos os assets oficiais residem no diretório imutável [`assets/brand/`](file:///c:/Users/jonat/OneDrive%20-%20IM3%20Brasil/models/SISQDT_LIGHT/assets/brand) e são espelhados no projeto WPF em [`src/QdtCqts.Desktop.Wpf/Assets/`](file:///c:/Users/jonat/OneDrive%20-%20IM3%20Brasil/models/SISQDT_LIGHT/src/QdtCqts.Desktop.Wpf/Assets):

| Arquivo | Formato | Resolução / Dimensões | Uso Principal |
| :--- | :--- | :--- | :--- |
| `sisQDT_LIGHT.ico` | ICO Multi-res | 16, 24, 32, 48, 64, 128, 256 px (32-bit RGBA) | Ícone da aplicação Windows, barra de tarefas e atalho |
| `sisQDT_LIGHT.svg` | Vetorial | Escalável (1200×340 viewBox) | Logotipo completo padrão com fundo transparente |
| `sisQDT_LIGHT.png` | Raster RGBA | 1200×340 px | Telas de carregamento, documentação e relatórios |
| `sisQDT_LIGHT_dark.svg` | Vetorial | Escalável (1200×340 viewBox) | Aplicação em interfaces e cabeçalhos em modo escuro |
| `sisQDT_LIGHT_dark.png` | Raster RGBA | 1200×340 px | Telas de splash e temas escuros |
| `sisQDT_LIGHT_light.svg`| Vetorial | Escalável (1200×340 viewBox) | Aplicação em relatórios impressos e folhas de cálculo |
| `sisQDT_LIGHT_light.png`| Raster RGBA | 1200×340 px | Documentos de impressão com fundo branco opaco |
| `sisQDT_LIGHT_mono.svg` | Vetorial | Escalável (1200×340 viewBox) | Impressão monocromática e gravação técnica |
| `sisQDT_LIGHT_mono.png` | Raster RGBA | 1200×340 px | Plantas e plotagens preto e branco |
| `sisQDT_LIGHT_icon.svg` | Vetorial | Escalável (512×512 viewBox) | Símbolo geométrico isolado para avatares e menus |
| `sisQDT_LIGHT_icon.png` | Raster RGBA | 512×512 px | Botões de aplicativo, favicon e splash |

---

## 5. Diretrizes de Uso e Restrições

### Usos Recomendados:
- **Tamanho Mínimo do Símbolo:** O símbolo preserva legibilidade completa até $16 \times 16\text{ px}$.
- **Tamanho Mínimo da Composição Completa:** Largura mínima recomendada de $200\text{ px}$.
- **Área de Não Interferência:** Manter margem mínima de respiro equivalente a $20\%$ da altura do símbolo em torno de toda a marca.

### Restrições Críticas:
1. **Não distorcer:** Nunca alterar a proporção horizontal/vertical da marca.
2. **Não alterar a caixa alta/baixa:** O nome deve sempre manter a grafia exata `sisQDT_LIGHT`.
3. **Não aproximar de marcas de terceiros:** Não tentar associar a marca a logotipos corporativos proprietários ou designs de concessionárias de energia.
4. **Não aplicar efeitos desnecessários:** Evitar sombras pesadas, chanfros 3D ou gradientes saturados em excesso.
