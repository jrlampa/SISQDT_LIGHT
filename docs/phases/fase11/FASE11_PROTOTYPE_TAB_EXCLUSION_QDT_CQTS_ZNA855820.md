# Fase 11 — Exclusão da aba protótipo

## Determinação

`Analise Ponto a Ponto`, com ou sem acento e em qualquer capitalização, é classificada como:

`EXCLUDED_PROTOTYPE`

Ela não é evidência matemática, unidade, precedente, golden, regra elétrica, topologia ou resultado esperado.

## Implementação

`PrecedenceGraphExtractor` agora:

- reconhece `Analise Ponto a Ponto` e `ANÁLISE PONTO A PONTO` sem depender de acentos;
- não inclui células dessa aba no grafo de ciclos válido;
- classifica referências produtivas para a aba como `EXCLUDED_PROTOTYPE` + `EXCLUDED_PROTOTYPE_DEPENDENCY`;
- classifica referências originadas na aba como `EXCLUDED_PROTOTYPE`;
- impede que ciclos exclusivos da aba contaminem o grafo válido.

A evidência histórica da auditoria que descrevia essa aba foi mantida como registro documental, mas não será reutilizada como matemática válida.

## Testes

Foram adicionados testes para reconhecimento da aba, dependência produtiva excluída e ciclo protótipo não contaminante.
