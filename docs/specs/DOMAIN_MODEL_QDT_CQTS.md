# Modelo de domínio unificado QDT + CQTS

## Agregados

### Project
Identidade persistente da rede e metadados do cliente.

### ProjectVersion
Snapshot imutável ou draft de um projeto. `ATUAL`, `PROJ` e versões futuras são apenas labels. Contém um `NetworkModel` e pode ser calculado em modos QDT ou CQTS quando compatível.

### NetworkModel
Raiz comum que agrupa transformadores, circuitos, nós, edges, branches, cargas, condutores e parâmetros. Não representa abas Excel.

### CalculationRun
Registro imutável de uma execução: versão, modo, algoritmo, input hash, estado e resultados.

## Entidades e value objects

| Conceito | Campos/valor | Invariantes |
|---|---|---|
| `Transformer` | potência, impedância, tensão, demanda | valores ausentes críticos bloqueiam cálculo |
| `Circuit` | código, lado, transformador, `CalculationMode` | pertence a uma versão; fonte identificável |
| `Node` | chave externa, coordenada, carga direta, source flag | chave única por circuito |
| `Edge` | from/to, comprimento, condutor, fase, instalação | mesma versão/circuito; sem self-loop |
| `Branch` | raiz, circuito, label | raiz pertence ao circuito |
| `Load` | tipo, node/branch, clientes, kVA, fator | carga direta não é acumulada |
| `Conductor` | catálogo, ampacidade, R/X, metadata | unidade de cada parâmetro explícita |
| `ElectricalParameter` | chave, valor, unidade, certeza, origem | `UNKNOWN` não é executável |
| `UnitValue` | magnitude + `UnitCode` | conversão explícita e registrada |
| `SourceRef` | workbook, sheet, cell, formula/evidence id | origem rastreável |
| `CalculationStatus` | `READY`, `UNKNOWN`, `BLOCKED`, `PASS`, `FAIL` | status crítico não pode ser mascarado |

## Relações

```text
Project 1 -> N ProjectVersion
ProjectVersion 1 -> 1 NetworkModel
NetworkModel 1 -> N Transformer/Circuit/Conductor/Parameter
Circuit 1 -> N Node/Edge/Branch
Node 1 -> N Load
Edge N -> 1 from Node e N -> 1 to Node
CalculationRun N -> 1 ProjectVersion e 1 CalculationMode
CalculationRun 1 -> N Result/Validation/Trace
```

## QDT no domínio

QDT `LADO 1` e `LADO 2` são circuitos/ramificações estruturais do transformador. Isso é equivalência estrutural: um modelo linear é um caso degenerado do grafo dirigido. Não implica igualdade de fórmulas CQTS.

## CQTS no domínio

CQTS usa o mesmo `Node`/`Edge`/`Load`, permitindo um nó com vários filhos. A radialidade é validada por circuito. Loops e múltiplos pais permanecem inválidos até evidência contrária.

## Regras desconhecidas

```text
UnknownRule -> CalculationResult(status=BLOCKED, reason=UNKNOWN_RULE)
MissingInput -> BLOCKED, nunca 0
MissingUnit -> BLOCKED, nunca default
BrokenReference -> BLOCKED/UNKNOWN, nunca fallback silencioso
```

O domínio não conhece WPF, Excel, SQLite ou detalhes de células.