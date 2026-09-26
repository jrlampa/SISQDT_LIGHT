# Modelo topológico QDT + CQTS

## Modelo comum

```text
TopologyModel
  root/source
  circuits
  nodes
  directed edges
  branches
  traversal order
  terminal nodes
```

`TopologyModel` é independente de qualquer fórmula elétrica.

## Entidades

- `TopologyNode`: id, external key, circuito, coordenadas, source, parent, children.
- `TopologyEdge`: id, from, to, circuito, comprimento e referências de entrada; parâmetros elétricos ficam associados, mas não são calculados pela topologia.
- `TopologyBranch`: raiz, circuito, conjunto de edges e terminais.
- `TopologyTraversal`: ordem topológica, caminhos da fonte, ancestrais e acumuladores estruturais.
- `TopologyValidator`: valida invariantes e retorna diagnósticos estruturados.

## Regras

1. Exatamente uma fonte por circuito calculável.
2. Cada nó não fonte possui no máximo um parent no modo radial.
3. Todo edge pertence ao mesmo circuito de seus endpoints.
4. Não aceitar self-loop, ciclo, órfão ou endpoint inexistente.
5. Terminal é nó sem filhos; branch precisa de raiz válida.
6. Comprimento negativo é inválido; comprimento zero só com caso Excel comprovado.
7. A ordem das linhas Excel não deve ser confundida com ordem topológica sem rastreabilidade.

## QDT

```text
Transformer
  -> Circuit LADO 1 -> Edge -> ... -> terminal
  -> Circuit LADO 2 -> Edge -> ... -> terminal
```

A topologia QDT é linear por lado, modelável como árvore com grau de saída máximo 1. As fórmulas continuam no `QDTCalculationEngine`.

## CQTS

```text
Transformer/source
  -> P1
     -> P2A -> P3A
     -> P2B -> P3B
```

O CQTS demonstra `PONTO`, `PONTO MONTANTE` e trechos dirigidos. A acumulação elétrica pertence ao CQTS engine; a topologia só fornece caminhos e relações.

## Testes independentes

- circuito linear válido;
- múltiplos filhos válido;
- ciclo inválido;
- nó órfão inválido;
- múltiplos pais inválido;
- root ausente/múltiplo inválido;
- terminal inconsistente inválido;
- branch com raiz fora do circuito inválido;
- edge entre circuitos inválido;
- ordenação determinística para o mesmo snapshot.

## Saída

```text
TopologyValidationResult
  status: PASS | FAIL | UNKNOWN
  ordered_nodes[]
  ordered_edges[]
  roots[]
  terminals[]
  branches[]
  diagnostics[]
```

Nenhum resultado de corrente, tensão, queda, proteção ou carregamento pertence a este contrato.
