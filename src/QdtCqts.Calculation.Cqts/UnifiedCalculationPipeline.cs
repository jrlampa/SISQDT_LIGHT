using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Motor unificado de execução ponta a ponta para a cadeia de cálculo sisQDT_LIGHT.
/// Integra topologia radial, cargas terminais, acumulação a montante, dimensionamento,
/// temperatura térmica, impedância linear, queda no trecho, transformador e queda acumulada.
/// </summary>
public sealed class UnifiedCalculationPipeline
{
    private readonly ILogger? _logger;
    private readonly CandidateConsumerLoadAggregationRule _consumerLoadRule = new();
    private readonly CandidateRadialLoadAccumulationRule _radialLoadRule = new();
    private readonly CandidateEndLoadSelectionRule _endLoadRule = new();
    private readonly CandidateCableTemperatureRule _cableTempRule = new();
    private readonly CandidateThermalResistanceRule _thermalResistanceRule = new();
    private readonly CandidateSegmentVoltageDropRule _segmentDropRule = new();
    private readonly CandidateTransformerVoltageDropRule _trafoDropRule = new();
    private readonly CandidateAccumulatedVoltageDropRule _accumulatedDropRule = new();
    private readonly CandidateShortCircuitRule _shortCircuitRule = new();
    private readonly CandidateProtectionRule _protectionRule = new();

    public UnifiedCalculationPipeline(ILogger? logger = null)
    {
        _logger = logger;
    }

    public CalculationResult Execute(CalculationRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var runId = Guid.NewGuid().ToString("N");

        var networkModel = request.ProjectVersion?.NetworkModel;
        if (networkModel is null)
        {
            return CalculationResult.Blocked(request, "MISSING_NETWORK_MODEL", "O modelo de rede elétrica não foi fornecido na requisição de cálculo.");
        }

        if (networkModel.Circuits.Count == 0)
        {
            return CalculationResult.Blocked(request, "ENGINE_NOT_READY", "O modelo de rede não possui circuitos configurados para cálculo.");
        }

        var topologyValidator = new TopologyValidator();
        var segmentResults = new List<SegmentCalculationResult>();
        var nodeResults = new List<NodeCalculationResult>();
        var trafoResults = new List<TransformerCalculationResult>();

        // Parmetros globais de circuito / projeto
        double defaultVoltage = GetNumericParam(networkModel, "V", 220.0);
        string applyFloorParam = GetTextParam(networkModel, "CH5", "SIM");
        double mtDropParam = GetNumericParam(networkModel, "MtVoltageDropPercent", 0.0);

        foreach (var circuit in networkModel.Circuits)
        {
            var topoValidation = topologyValidator.Validate(networkModel, circuit.Id);
            if (!topoValidation.IsValid)
            {
                var firstDiag = topoValidation.Diagnostics.First();
                return CalculationResult.Blocked(request, $"TOPOLOGY_{firstDiag.Code}", firstDiag.Message);
            }

            var circuitNodes = networkModel.Nodes.Where(n => n.CircuitId == circuit.Id).ToList();
            var circuitEdges = networkModel.Edges.Where(e => e.CircuitId == circuit.Id).ToList();

            // Mapeamentos topolgicos
            var outgoingEdges = circuitEdges.ToLookup(e => e.FromNodeId, StringComparer.Ordinal);
            var incomingEdge = circuitEdges.ToDictionary(e => e.ToNodeId, StringComparer.Ordinal);

            // 1. Agregao de Cargas Locais por N
            var localLoadKva = new Dictionary<string, double>(StringComparer.Ordinal);
            var localConsumers = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var node in circuitNodes)
            {
                var attachedLoads = networkModel.Loads.Where(l => l.NodeId == node.Id).ToList();
                if (attachedLoads.Count > 0)
                {
                    var groups = attachedLoads.Select(l => (Count: (int)(l.Clients ?? 1.0), UnitLoadKva: l.Kva.Magnitude)).ToList();
                    var aggResult = _consumerLoadRule.Execute(new[]
                    {
                        new RuleInput("ConsumerGroups", groups, UnitCode.Kva)
                    }, new RuleExecutionContext(runId, "AGGREGATION"));

                    localLoadKva[node.Id] = aggResult.Status == CalculationStatus.Pass ? (double)aggResult.OutputValue! : 0.0;
                    localConsumers[node.Id] = groups.Sum(g => g.Count);
                }
                else
                {
                    localLoadKva[node.Id] = 0.0;
                    localConsumers[node.Id] = 0;
                }
            }

            // 2. Acumulao a Montante (Post-Order / Jusante -> Montante)
            var accumulatedLoadKva = new Dictionary<string, double>(StringComparer.Ordinal);
            var accumulatedConsumers = new Dictionary<string, double>(StringComparer.Ordinal);
            var reverseTopologicalNodes = topoValidation.OrderedNodeIds.Reverse().ToList();

            foreach (var nodeId in reverseTopologicalNodes)
            {
                if (!incomingEdge.TryGetValue(nodeId, out var edge))
                {
                    // N raiz no possui aresta de entrada
                    continue;
                }

                double myLocalLoad = localLoadKva.TryGetValue(nodeId, out var lVal) ? lVal : 0.0;
                double myLocalConsumers = localConsumers.TryGetValue(nodeId, out var cVal) ? cVal : 0.0;

                var childEdges = outgoingEdges[nodeId].ToList();
                var childLoads = childEdges.Select(ce => accumulatedLoadKva.TryGetValue(ce.Id, out var aL) ? aL : 0.0).ToList();
                double childConsumersTotal = childEdges.Sum(ce => accumulatedConsumers.TryGetValue(ce.Id, out var aC) ? aC : 0.0);

                var accResult = _radialLoadRule.Execute(new[]
                {
                    new RuleInput("LocalLoadKva", myLocalLoad, UnitCode.Kva),
                    new RuleInput("DownstreamBranchesLoadKva", childLoads, UnitCode.Kva),
                    new RuleInput("LocalConsumers", myLocalConsumers, UnitCode.Unknown),
                    new RuleInput("DownstreamConsumers", childConsumersTotal, UnitCode.Unknown)
                }, new RuleExecutionContext(runId, "ACCUMULATION"));

                accumulatedLoadKva[edge.Id] = accResult.Status == CalculationStatus.Pass ? (double)accResult.OutputValue! : myLocalLoad + childLoads.Sum();
                accumulatedConsumers[edge.Id] = myLocalConsumers + childConsumersTotal;
            }

            // 3. Seleo de Carga de Fim de Trecho (M) e Dimensionamento Fsico
            var edgeEndLoadKva = new Dictionary<string, double>(StringComparer.Ordinal);
            var edgeSegmentDropPercent = new Dictionary<string, double>(StringComparer.Ordinal);

            foreach (var edge in circuitEdges)
            {
                double dConsumers = accumulatedConsumers.TryGetValue(edge.Id, out var dc) ? dc : 1.0;
                double eLoad = accumulatedLoadKva.TryGetValue(edge.Id, out var el) ? el : 0.0;

                // Fator de diversidade FDIV (G)
                double gFactor = 1.0;
                var attachedLoad = networkModel.Loads.FirstOrDefault(l => l.NodeId == edge.ToNodeId);
                if (attachedLoad?.Factor.HasValue == true)
                {
                    gFactor = attachedLoad.Factor.Value;
                }

                var mResult = _endLoadRule.Execute(new[]
                {
                    new RuleInput("D13", dConsumers, UnitCode.Unknown),
                    new RuleInput("E13", eLoad, UnitCode.Kva),
                    new RuleInput("G13", gFactor, UnitCode.Unknown),
                    new RuleInput("CH5", applyFloorParam, UnitCode.Unknown)
                }, new RuleExecutionContext(runId, "END_LOAD"));

                double mKva = mResult.Status == CalculationStatus.Pass ? (double)mResult.OutputValue! : eLoad * gFactor;
                edgeEndLoadKva[edge.Id] = mKva;

                // Consulta do condutor no catlogo do modelo
                var conductor = networkModel.Conductors.FirstOrDefault(c =>
                    string.Equals(c.Id, edge.ConductorId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.CatalogKey, edge.ConductorId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, edge.ConductorId, StringComparison.OrdinalIgnoreCase))
                    ?? FallbackConductor(edge.ConductorId);

                double rcc20 = conductor.Resistance.Magnitude;
                double xReactance = conductor.Reactance.Magnitude;
                double izAmpacity = conductor.Ampacity.Magnitude;
                double alpha20 = conductor.CatalogKey.Contains("Al", StringComparison.OrdinalIgnoreCase) ? 0.00403 : 0.00393;
                double kStar = 1.0652244659520294; // Padro calibrado de efeito pelicular

                // Cabos em paralelo (AP) e fases (H)
                double apCables = 1.0;
                if (!string.IsNullOrWhiteSpace(edge.InstallationMethod) && double.TryParse(edge.InstallationMethod, out var apVal) && apVal > 0)
                {
                    apCables = apVal;
                }

                int phaseCount = 3;
                if (!string.IsNullOrWhiteSpace(edge.Phase) && int.TryParse(edge.Phase, out var pVal) && pVal > 0)
                {
                    phaseCount = pVal;
                }

                double physicalLength = edge.Length.Magnitude;
                double equivLength = physicalLength / apCables;

                // Corrente de projeto Ib
                double ibCurrent = phaseCount switch
                {
                    3 => mKva / (defaultVoltage * Math.Sqrt(3) / 1000.0),
                    2 => mKva / (defaultVoltage / 1000.0),
                    _ => mKva / (defaultVoltage / Math.Sqrt(3) / 1000.0)
                };

                double totalAmpacity = izAmpacity * apCables;
                bool isOverloaded = ibCurrent > totalAmpacity;

                // Temperatura de regime contnuo (T)
                double operatingTemp = 30.0;
                if (phaseCount == 3 && totalAmpacity > 0)
                {
                    var tempResult = _cableTempRule.Execute(new[]
                    {
                        new RuleInput("M13", mKva, UnitCode.Kva),
                        new RuleInput("BX6", defaultVoltage, UnitCode.V),
                        new RuleInput("H13", 3.0, UnitCode.ConductorKey),
                        new RuleInput("AN13", izAmpacity, UnitCode.Ampere),
                        new RuleInput("AP13", apCables, UnitCode.Meter) // Contrato tipado
                    }, new RuleExecutionContext(runId, "TEMPERATURE"));

                    if (tempResult.Status == CalculationStatus.Pass)
                    {
                        operatingTemp = (double)tempResult.OutputValue!;
                    }
                }
                else
                {
                    operatingTemp = 30.0 + (ibCurrent / Math.Max(1.0, totalAmpacity)) * 60.0;
                }

                // Resistncia CA corrigida na temperatura
                var rcaResult = _thermalResistanceRule.Execute(new[]
                {
                    new RuleInput("Rcc20", rcc20, UnitCode.Ohm),
                    new RuleInput("Alpha20", alpha20, UnitCode.Unknown),
                    new RuleInput("Temperature", operatingTemp, UnitCode.Celsius),
                    new RuleInput("KStar", kStar, UnitCode.Unknown)
                }, new RuleExecutionContext(runId, "RCA"));

                double rca = rcaResult.Status == CalculationStatus.Pass ? (double)rcaResult.OutputValue! : rcc20 * (1 + alpha20 * (operatingTemp - 20)) * kStar;
                double zImpedance = Math.Sqrt((rca * rca) + (xReactance * xReactance));
                double factorBj = zImpedance / ((defaultVoltage * defaultVoltage) / 100.0);

                double phaseMultiplier = phaseCount switch
                {
                    3 => 1.0,
                    2 => 2.0,
                    1 => 6.0,
                    _ => 1.0
                };

                // Queda de tenso no trecho (Delta V %)
                var dropResult = _segmentDropRule.Execute(new[]
                {
                    new RuleInput("M", mKva, UnitCode.Kva),
                    new RuleInput("R", rca, UnitCode.Ohm),
                    new RuleInput("X", xReactance, UnitCode.Ohm),
                    new RuleInput("L", physicalLength, UnitCode.Meter),
                    new RuleInput("AP", apCables, UnitCode.Unknown),
                    new RuleInput("V", defaultVoltage, UnitCode.V),
                    new RuleInput("H", (double)phaseCount, UnitCode.ConductorKey)
                }, new RuleExecutionContext(runId, "SEGMENT_DROP"));

                double deltaVPercent = dropResult.Status == CalculationStatus.Pass ? (double)dropResult.OutputValue! : mKva * factorBj * equivLength * phaseMultiplier;
                edgeSegmentDropPercent[edge.Id] = deltaVPercent;
            }

            // 4. Queda de Tensão do Transformador e Média Tensão
            var trafo = networkModel.Transformers.FirstOrDefault(t => t.Id == circuit.TransformerId)
                ?? networkModel.Transformers.FirstOrDefault()
                ?? new Transformer("TR_DEF", "TR", new UnitValue(112.5, UnitCode.Kva), new UnitValue(3.5, UnitCode.Percent), new UnitValue(220, UnitCode.V), new UnitValue(0, UnitCode.Kva));

            // Carga do transformador é a carga do trecho inicial saindo do TR
            var rootNode = circuitNodes.FirstOrDefault(n => n.IsSource) ?? circuitNodes.First();
            var firstEdge = outgoingEdges[rootNode.Id].FirstOrDefault();
            double trafoOperatingLoadKva = firstEdge != null && edgeEndLoadKva.TryGetValue(firstEdge.Id, out var fLoad) ? fLoad : accumulatedLoadKva.Values.DefaultIfEmpty(0.0).Max();

            var trafoDropResult = _trafoDropRule.Execute(new[]
            {
                new RuleInput("TrafoLoadKva", trafoOperatingLoadKva, UnitCode.Kva),
                new RuleInput("TrafoNominalKva", trafo.Power.Magnitude, UnitCode.Kva),
                new RuleInput("TrafoImpedancePercent", trafo.Impedance.Magnitude, UnitCode.Percent)
            }, new RuleExecutionContext(runId, "TRAFO_DROP"));

            double trafoDropPercent = trafoDropResult.Status == CalculationStatus.Pass ? (double)trafoDropResult.OutputValue! : (trafoOperatingLoadKva / trafo.Power.Magnitude) * trafo.Impedance.Magnitude;
            double totalOriginDrop = trafoDropPercent + mtDropParam;

            trafoResults.Add(new TransformerCalculationResult(
                trafo.Id,
                trafo.ExternalKey,
                trafo.Power.Magnitude,
                trafoOperatingLoadKva,
                trafo.Impedance.Magnitude,
                trafoDropPercent,
                mtDropParam,
                totalOriginDrop));

            // 5. Cadeia de Impedâncias a Montante (Subestação AT/MT + MT + Trafo de Distribuição)
            double stationMva = GetNumericParam(networkModel, "StationMva", 40.0);
            double stationZPercent = GetNumericParam(networkModel, "StationZPercent", 20.0);
            double mtVoltageKv = GetNumericParam(networkModel, "MtVoltageKv", 13.2);
            double mtCableLengthKm = GetNumericParam(networkModel, "MtCableLengthKm", 2.0);
            double mtResistancePerKm = GetNumericParam(networkModel, "MtResistancePerKm", 0.7171);
            double mtReactancePerKm = GetNumericParam(networkModel, "MtReactancePerKm", 0.3512);

            double ratioSq = Math.Pow(defaultVoltage / (mtVoltageKv * 1000.0), 2.0);
            double zEstX = (stationZPercent / 100.0 * Math.Pow(mtVoltageKv, 2.0) / stationMva) * ratioSq;
            double zEstR = 0.0;
            double zMtR = mtResistancePerKm * ratioSq * mtCableLengthKm;
            double zMtX = mtReactancePerKm * ratioSq * mtCableLengthKm;
            double zTrafoX = (trafo.Impedance.Magnitude / 100.0 * Math.Pow(defaultVoltage, 2.0)) / (trafo.Power.Magnitude * 1000.0);
            double zTrafoR = 0.0;

            double zUpstreamR = zEstR + zMtR + zTrafoR;
            double zUpstreamX = zEstX + zMtX + zTrafoX;

            // 6. Queda de Tensão Acumulada e Propagação de Impedância Radial de BT
            var accumulatedVoltageDropPercent = new Dictionary<string, double>(StringComparer.Ordinal);
            var accumulatedBtImpedanceR = new Dictionary<string, double>(StringComparer.Ordinal);
            var accumulatedBtImpedanceX = new Dictionary<string, double>(StringComparer.Ordinal);
            var accumulatedBtRfn = new Dictionary<string, double>(StringComparer.Ordinal);

            accumulatedVoltageDropPercent[rootNode.Id] = totalOriginDrop;
            accumulatedBtImpedanceR[rootNode.Id] = 0.0;
            accumulatedBtImpedanceX[rootNode.Id] = 0.0;
            accumulatedBtRfn[rootNode.Id] = 0.0;

            foreach (var nodeId in topoValidation.OrderedNodeIds)
            {
                if (nodeId == rootNode.Id) continue;

                if (incomingEdge.TryGetValue(nodeId, out var edge))
                {
                    double parentDrop = accumulatedVoltageDropPercent.TryGetValue(edge.FromNodeId, out var pDrop) ? pDrop : totalOriginDrop;
                    double segDrop = edgeSegmentDropPercent.TryGetValue(edge.Id, out var sDrop) ? sDrop : 0.0;

                    var caResult = _accumulatedDropRule.Execute(new[]
                    {
                        new RuleInput("BZ", segDrop, UnitCode.Percent),
                        new RuleInput("CA_Parent", parentDrop, UnitCode.Percent)
                    }, new RuleExecutionContext(runId, "CA"));

                    double myCa = caResult.Status == CalculationStatus.Pass ? (double)caResult.OutputValue! : parentDrop + segDrop;
                    accumulatedVoltageDropPercent[nodeId] = myCa;

                    // Condutor do trecho
                    var conductor = networkModel.Conductors.FirstOrDefault(c =>
                        string.Equals(c.Id, edge.ConductorId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.CatalogKey, edge.ConductorId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Name, edge.ConductorId, StringComparison.OrdinalIgnoreCase))
                        ?? FallbackConductor(edge.ConductorId);

                    double apCables = 1.0;
                    if (!string.IsNullOrWhiteSpace(edge.InstallationMethod) && double.TryParse(edge.InstallationMethod, out var apVal) && apVal > 0)
                        apCables = apVal;

                    int phaseCount = 3;
                    if (!string.IsNullOrWhiteSpace(edge.Phase) && int.TryParse(edge.Phase, out var pVal) && pVal > 0)
                        phaseCount = pVal;

                    double physicalLength = edge.Length.Magnitude;
                    double equivLength = physicalLength / apCables;

                    double mKva = edgeEndLoadKva.TryGetValue(edge.Id, out var mk) ? mk : 0.0;
                    double dConsumers = accumulatedConsumers.TryGetValue(edge.Id, out var dc) ? dc : 1.0;
                    double eLoad = accumulatedLoadKva.TryGetValue(edge.Id, out var el) ? el : 0.0;
                    double gFactor = 1.0;
                    var attachedLoad = networkModel.Loads.FirstOrDefault(l => l.NodeId == edge.ToNodeId);
                    if (attachedLoad?.Factor.HasValue == true) gFactor = attachedLoad.Factor.Value;

                    double ibCurrent = phaseCount switch
                    {
                        3 => mKva / (defaultVoltage * Math.Sqrt(3) / 1000.0),
                        2 => mKva / (defaultVoltage / 1000.0),
                        _ => mKva / (defaultVoltage / Math.Sqrt(3) / 1000.0)
                    };
                    double izAmpacity = conductor.Ampacity.Magnitude;
                    double totalAmpacity = izAmpacity * apCables;
                    bool isOverloaded = ibCurrent > totalAmpacity;

                    double operatingTemp = 30.0;
                    if (phaseCount == 3 && totalAmpacity > 0)
                    {
                        var tempResult = _cableTempRule.Execute(new[]
                        {
                            new RuleInput("M13", mKva, UnitCode.Kva),
                            new RuleInput("BX6", defaultVoltage, UnitCode.V),
                            new RuleInput("H13", 3.0, UnitCode.ConductorKey),
                            new RuleInput("AN13", izAmpacity, UnitCode.Ampere),
                            new RuleInput("AP13", apCables, UnitCode.Meter)
                        }, new RuleExecutionContext(runId, "TEMPERATURE"));
                        if (tempResult.Status == CalculationStatus.Pass) operatingTemp = (double)tempResult.OutputValue!;
                    }
                    else
                    {
                        operatingTemp = 30.0 + (ibCurrent / Math.Max(1.0, totalAmpacity)) * 60.0;
                    }

                    double rcc20 = conductor.Resistance.Magnitude;
                    double xReactance = conductor.Reactance.Magnitude;
                    double alpha20 = conductor.CatalogKey.Contains("Al", StringComparison.OrdinalIgnoreCase) ? 0.00403 : 0.00393;
                    double kStar = 1.0652244659520294;

                    var rcaResult = _thermalResistanceRule.Execute(new[]
                    {
                        new RuleInput("Rcc20", rcc20, UnitCode.Ohm),
                        new RuleInput("Alpha20", alpha20, UnitCode.Unknown),
                        new RuleInput("Temperature", operatingTemp, UnitCode.Celsius),
                        new RuleInput("KStar", kStar, UnitCode.Unknown)
                    }, new RuleExecutionContext(runId, "RCA"));

                    double rca = rcaResult.Status == CalculationStatus.Pass ? (double)rcaResult.OutputValue! : rcc20 * (1 + alpha20 * (operatingTemp - 20)) * kStar;
                    double zImpedance = Math.Sqrt((rca * rca) + (xReactance * xReactance));
                    double factorBj = zImpedance / ((defaultVoltage * defaultVoltage) / 100.0);
                    double phaseMultiplier = phaseCount == 2 ? 2.0 : (phaseCount == 1 ? 6.0 : 1.0);

                    // Resistência de neutro aproximada ou catálogo
                    double rcaNeutral = rca;
                    if (conductor.CatalogKey.Contains("240 Cu", StringComparison.OrdinalIgnoreCase))
                    {
                        rcaNeutral = rca * 2.0; // Neutro de 120mm² típico Light para cabo de 240
                    }
                    else if (conductor.CatalogKey.Contains("185 Al", StringComparison.OrdinalIgnoreCase))
                    {
                        rcaNeutral = rca * (185.0 / 120.0); // Neutro de 120mm² típico Light para cabo de 185
                    }

                    // Impedâncias acumuladas do nó pai
                    double zBtPriorR = accumulatedBtImpedanceR.TryGetValue(edge.FromNodeId, out var zbr) ? zbr : 0.0;
                    double zBtPriorX = accumulatedBtImpedanceX.TryGetValue(edge.FromNodeId, out var zbx) ? zbx : 0.0;
                    double rfnPrior = accumulatedBtRfn.TryGetValue(edge.FromNodeId, out var rfn) ? rfn : 0.0;

                    var scResult = _shortCircuitRule.Execute(new[]
                    {
                        new RuleInput("Vnom", defaultVoltage, UnitCode.V),
                        new RuleInput("Z_Upstream_Real", zUpstreamR, UnitCode.Ohm),
                        new RuleInput("Z_Upstream_Imag", zUpstreamX, UnitCode.Ohm),
                        new RuleInput("Z_BtPrior_Real", zBtPriorR, UnitCode.Ohm),
                        new RuleInput("Z_BtPrior_Imag", zBtPriorX, UnitCode.Ohm),
                        new RuleInput("Rca_Phase", rca, UnitCode.Ohm),
                        new RuleInput("Rca_Neutral", rcaNeutral, UnitCode.Ohm),
                        new RuleInput("X_Phase", xReactance, UnitCode.Ohm),
                        new RuleInput("Length_Equiv_Meters", equivLength, UnitCode.Meter),
                        new RuleInput("Rfn_Prior", rfnPrior, UnitCode.Ohm)
                    }, new RuleExecutionContext(runId, "SHORT_CIRCUIT"));

                    double icc3ph = 0.0;
                    double icc1ph = 0.0;
                    if (scResult.Status == CalculationStatus.Pass && scResult.OutputValue is ValueTuple<double, double, double, double, double, double, double> scTuple)
                    {
                        icc3ph = scTuple.Item1;
                        icc1ph = scTuple.Item2;
                        accumulatedBtImpedanceR[nodeId] = zBtPriorR + scTuple.Item3;
                        accumulatedBtImpedanceX[nodeId] = zBtPriorX + scTuple.Item4;
                        accumulatedBtRfn[nodeId] = rfnPrior + scTuple.Item5;
                    }
                    else
                    {
                        accumulatedBtImpedanceR[nodeId] = zBtPriorR + (rca * equivLength / 1000.0);
                        accumulatedBtImpedanceX[nodeId] = zBtPriorX + (xReactance * equivLength / 1000.0);
                        accumulatedBtRfn[nodeId] = rfnPrior + ((rca + rcaNeutral) * equivLength / 1000.0);
                    }

                    segmentResults.Add(new SegmentCalculationResult(
                        edge.Id,
                        edge.FromNodeId,
                        edge.ToNodeId,
                        conductor.CatalogKey,
                        phaseCount,
                        apCables,
                        physicalLength,
                        equivLength,
                        dConsumers,
                        eLoad,
                        gFactor,
                        mKva,
                        ibCurrent,
                        totalAmpacity,
                        isOverloaded,
                        operatingTemp,
                        rca,
                        xReactance,
                        zImpedance,
                        factorBj,
                        phaseMultiplier,
                        segDrop,
                        icc3ph,
                        icc1ph,
                        zUpstreamR,
                        zUpstreamX));
                }
            }

            foreach (var node in circuitNodes)
            {
                double ca = accumulatedVoltageDropPercent.TryGetValue(node.Id, out var drop) ? drop : 0.0;
                nodeResults.Add(new NodeCalculationResult(
                    node.Id,
                    node.ExternalKey,
                    localConsumers.TryGetValue(node.Id, out var lc) ? lc : 0,
                    localLoadKva.TryGetValue(node.Id, out var ll) ? ll : 0.0,
                    ca));
            }
        }

        // 7. Avaliação de Proteção do Circuito (Corrente do Fusível e Suportabilidade Térmica)
        ProtectionCalculationResult? protectionResult = null;
        if (segmentResults.Count > 0)
        {
            var endSegment = segmentResults.OrderBy(s => s.ShortCircuit1PhaseAmperes).First();
            double minIcc1ph = endSegment.ShortCircuit1PhaseAmperes;
            string criticalConductor = endSegment.ConductorKey ?? "16 Al_CONC_Tri";
            double criticalTemp = endSegment.OperatingTemperatureCelsius;
            double rootLoadKva = trafoResults.FirstOrDefault()?.OperatingLoadKva ?? 112.5;

            var protRuleResult = _protectionRule.Execute(new[]
            {
                new RuleInput("RootLoadKva", rootLoadKva, UnitCode.Kva),
                new RuleInput("Vnom", defaultVoltage, UnitCode.V),
                new RuleInput("MinIcc1Phase", minIcc1ph, UnitCode.Ampere),
                new RuleInput("CriticalTemperature", criticalTemp, UnitCode.Celsius),
                new RuleInput("CriticalConductorKey", criticalConductor, UnitCode.ConductorKey)
            }, new RuleExecutionContext(runId, "PROTECTION"));

            if (protRuleResult.Status == CalculationStatus.Pass && protRuleResult.OutputValue is ProtectionAssessment ass)
            {
                protectionResult = new ProtectionCalculationResult(
                    ass.ProjectCurrentAmperes,
                    ass.FuseRatedCurrentAmperes,
                    ass.MinSinglePhaseShortCircuitAmperes,
                    ass.ConductorKey,
                    ass.ConductorSectionMm2,
                    ass.ConductorOperatingTemperatureCelsius,
                    ass.MaxAdmissibleTimeSeconds,
                    ass.FuseMeltingTimeSeconds,
                    ass.IsRatedCurrentAdequate,
                    ass.IsThermalWithstandAdequate,
                    ass.StatusMessage);
            }
        }

        stopwatch.Stop();
        string outputHash = DeterministicHashing.ComputeReportHash(segmentResults, nodeResults);

        var report = new NetworkCalculationReport(
            runId,
            request.Mode,
            request.AlgorithmVersion,
            request.InputHash,
            outputHash,
            trafoResults,
            segmentResults,
            nodeResults,
            networkModel.Circuits.Count,
            stopwatch.Elapsed.TotalMilliseconds,
            protectionResult);

        return CalculationResult.Succeeded(request, report);
    }


    private static double GetNumericParam(NetworkModel model, string key, double defaultValue)
    {
        var param = model.Parameters.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
        return param?.NumericValue ?? defaultValue;
    }

    private static string GetTextParam(NetworkModel model, string key, string defaultValue)
    {
        var param = model.Parameters.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
        return param?.TextValue ?? defaultValue;
    }

    private static Conductor FallbackConductor(string? key)
    {
        string norm = key ?? "240 Cu";
        if (norm.Contains("70", StringComparison.OrdinalIgnoreCase))
        {
            return new Conductor("70_Al", "1", "70 Al - MX", "70 Al - MX", new UnitValue(195, UnitCode.Ampere), new UnitValue(0.472, UnitCode.Ohm), new UnitValue(0.126, UnitCode.Ohm));
        }
        if (norm.Contains("16", StringComparison.OrdinalIgnoreCase))
        {
            return new Conductor("16_Al", "1", "16 Al_CONC_Tri", "16 Al_CONC_Tri", new UnitValue(80, UnitCode.Ampere), new UnitValue(2.06, UnitCode.Ohm), new UnitValue(0.85, UnitCode.Ohm));
        }
        return new Conductor("240_Cu", "1", "240 Cu", "240 Cu", new UnitValue(430, UnitCode.Ampere), new UnitValue(0.0762, UnitCode.Ohm), new UnitValue(0.0897, UnitCode.Ohm));
    }
}
