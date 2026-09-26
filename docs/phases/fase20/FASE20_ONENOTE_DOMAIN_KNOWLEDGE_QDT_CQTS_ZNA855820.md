# Fase 20 — Conhecimento de Domínio Extraído do OneNote

## 1. Origem da Fonte
- **Arquivo:** `C:\Users\jonat\OneDrive\Área de Trabalho\anotações GERAIS.one`
- **Classificação:** `USER_DOMAIN_KNOWLEDGE` (não promovido a baseline oficial)
- **Seção consultada:** `LIGHT` / Anotações Gerais do Usuário

## 2. Conteúdo e Critérios de Engenharia Relevantes Extraídos
A consulta à base de anotações técnicas do usuário forneceu subsídios operacionais e normativos adotados nos projetos da Light / concessionárias:
1. **Regra de Demanda e Carga:**
   - Levantamento de carga e determinação de demandas conforme padrões normativos (ET 285 / 283).
   - Utilização de demanda estimada por consumidor / classe de consumo.
   - Demanda diversificada aplicada a transformadores e ramais.
   - Cargas de consumidores especiais computadas com base em sua carga nominal.
2. **Critérios de Queda de Tensão:**
   - Queda de tensão máxima admissível de 6% com cálculo CQT anexado.
   - Carregamento de transformadores (existentes e novos até 120%).
3. **Condutores e Ramais:**
   - Padronização de condutores multiplexados para centros e pontas de carga: `70 mm² + 53 mm²`, `185 mm² + 120 mm²`.
   - Nomenclaturas operacionais: `ZNA` (Zona de Transformação), `TR` (Transformador), `LID` (Lâmina / Chave Seccionadora), `P` (Poste / Ponto).

## 3. Aplicação na Investigação de M13
O OneNote confirmou que as potências de trecho em projetos de distribuição da Light utilizam a combinação de:
- Contagem de consumidores atendidos a jusante;
- Demanda unitária estimada / diversificada;
- Carga nominal dos trechos.
Essa diretriz guiou a análise direta das colunas `D` (Nº Consumidores), `E` (Carga Acumulada), `G` (FDIV) e `M` (Carga no Fim do Trecho Escolhida) nas planilhas reais.
