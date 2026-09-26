# Fase 6 — Architecture Gate

**Projeto:** ZNA855820  
**Data:** 2026-09-26  
**Aplicativo:** único desktop QDT + CQTS  
**Arquivos originais preservados:** Sim  
**Decisão global:** `NO-GO` para engines matemáticos de produção; arquitetura autorizada.

## Estados por subsistema

| Subsistema | Decision | Evidência | Limite |
|---|---|---|---|
| Aplicativo desktop único | GO | arquitetura define uma WPF app com dois modes | não implica engine pronta |
| Architecture | GO | camadas e dependências documentadas | sem implementação ainda |
| Domain | GO | entidades comuns e invariantes | campos/unidades desconhecidos continuam bloqueados |
| Topology | GO | Node/Edge/Branch/Validator e testes definidos | sem fórmulas elétricas |
| SQLite | GO | um `application.db`, migrations, WAL, FKs, hashes | DDL final depende de implementação |
| Import | GO RESTRICTED | fluxo read-only OOXML -> evidence -> domain | sem recalcular Excel |
| Evidence | GO | SourceArtifact/Cell/FormulaReference/Trace | grafo completo ainda pendente |
| Parity Harness | GO RESTRICTED | P0/P1 autorizados; contrato definido | P3/P4 bloqueados |
| QDT Engine | NO-GO | fórmula crítica, precedentes, DecInv, unidades e golden incompletos | não implementar produção |
| CQTS Engine | NO-GO | regras próprias parcialmente fechadas | implementar depois do QDT/topologia |
| UI WPF | DESIGN ONLY | UX Fase 3 e arquitetura de projeções | UI definitiva posterior |

## Critérios de aceitação Fase 6

- [x] existe um único aplicativo desktop;
- [x] QDT e CQTS usam o mesmo banco lógico;
- [x] existe modelo de domínio comum;
- [x] modelo topológico representa linha QDT e árvore CQTS;
- [x] QDT possui contrato próprio;
- [x] CQTS possui contrato próprio;
- [x] parity é primeira classe com `PASS/FAIL/UNKNOWN/BLOCKED`;
- [x] Excel está fora do runtime final;
- [x] CQTS amplia topologia e não substitui QDT como oráculo;
- [x] regras desconhecidas não executam silenciosamente.

## Contradições verificadas

1. A Fase 2 usa `nodes/edges` diretamente no schema; a Fase 6 introduz `network_models` e nomes topológicos. Resolução: manter compatibilidade física por migration/repository, mas adotar `NetworkModel` como agregado lógico.
2. QDT aparece como dois lados lineares e CQTS como árvore. Resolução: mesma estrutura topológica, engines e regras separados.
3. `TOLERANCE` aparece como proposta diagnóstica antiga e como indefinida na Fase 5. Resolução: `UNDEFINED` é a regra vigente até calibração.
4. O workspace não tem código .NET. Resolução: esta fase produz arquitetura; scaffold fica para fase autorizada posterior.

## Gate de implementação

Pode iniciar:

- solução .NET vazia e projetos de contratos/domínio;
- SQLite/migrations/repositories;
- topology validator;
- evidence store/import abstractions;
- parity P0/P1;
- testes de contratos e invariantes.

Não pode iniciar:

- QDT engine completo;
- CQTS engine de produção;
- fórmulas inferidas;
- Solver substituto;
- UI funcional declarada como cálculo;
- defaults para `#REF!`, DecInv, unidades ou entradas ausentes.

## Próxima fase

1. Criar a solução .NET somente após autorização explícita de implementação.
2. Implementar SQLite unificado e migrations.
3. Implementar Domain/Topology e testes independentes.
4. Implementar evidence importer e Precedence Graph.
5. Implementar parity harness P0/P1.
6. Reabrir gate matemático somente após os bloqueios Fase 5.
