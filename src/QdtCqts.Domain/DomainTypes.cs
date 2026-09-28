namespace QdtCqts.Domain;

public enum CalculationMode { Qdt, Cqts }
public enum CalculationStatus { Ready, Pass, Fail, Unknown, Blocked }
public enum ProtectionEvidenceStatus { Available, Missing, Invalid }
public enum ProtectionAssessmentStatus { Pass, Fail, EvidenceBlocked }
public enum ResultScope { Transformer, Circuit, Branch, Node, Edge, Load, Point, Global }
public enum UnitCode { Unknown, Mva, Kva, Kv, V, Ampere, Meter, Ohm, OhmPerKilometer, Celsius, Percent, Factor, UtmMeter, ConductorKey, Second, SquareMillimeter }
public enum LoadKind { Point, Ramal, Client, Distributed, Unknown }
public enum TopologyDiagnosticCode { MissingRoot, MultipleRoots, SelfLoop, Cycle, Orphan, MultipleParents, MissingEndpoint, CrossCircuitEdge, InvalidBranch, InvalidTerminal, NegativeLength }

public sealed record ComplexImpedance(double Real, double Imaginary)
{
    public static readonly ComplexImpedance Zero = new(0.0, 0.0);
    public double Magnitude => Math.Sqrt((Real * Real) + (Imaginary * Imaginary));
    public static ComplexImpedance operator +(ComplexImpedance a, ComplexImpedance b) => new(a.Real + b.Real, a.Imaginary + b.Imaginary);
}

public sealed record ProtectionDeviceEvidence(
    string EvidenceId,
    string SourceReference,
    string Sha256,
    string Manufacturer,
    string Model,
    string CurveId,
    double RatedCurrentAmperes,
    double EvaluationCurrentAmperes,
    double TotalClearingTimeSeconds,
    double InterruptingCapacityAmperes);

public sealed record ProtectionAssessment(
    string ConductorKey,
    double? ConductorSectionMm2,
    string? ConductorSectionEvidenceId,
    double MinSinglePhaseShortCircuitAmperes,
    double MaxThreePhaseShortCircuitAmperes,
    double ConductorOperatingTemperatureCelsius,
    string? ConductorTemperatureEvidenceId,
    double? MaxAdmissibleTimeSeconds,
    double ProjectCurrentAmperes,
    ProtectionEvidenceStatus EvidenceStatus,
    ProtectionAssessmentStatus AssessmentStatus,
    ProtectionDeviceEvidence? DeviceEvidence,
    bool? IsRatedCurrentAdequate,
    bool? IsThermalWithstandAdequate,
    bool? IsInterruptingCapacityAdequate,
    string StatusMessage);

public sealed record UnitValue(double Magnitude, UnitCode Unit)
{
    public static UnitValue Unknown(double magnitude = 0) => new(magnitude, UnitCode.Unknown);
}

public sealed record SourceRef(string Workbook, string Sheet, string Cell, string? Formula = null, string? EvidenceId = null);

public sealed record ElectricalParameter(string Key, double? NumericValue, string? TextValue, UnitCode Unit, bool IsKnown, SourceRef? Source = null);

public sealed record Transformer(
    string Id,
    string ExternalKey,
    UnitValue Power,
    UnitValue Impedance,
    UnitValue LineVoltage,
    UnitValue Demand);

public sealed record Circuit(string Id, string ExternalKey, string TransformerId, int? SideIndex, CalculationMode Mode);

public sealed record Node(
    string Id,
    string ExternalKey,
    string CircuitId,
    double? X,
    double? Y,
    bool IsSource,
    string? ParentNodeId = null);

public sealed record Edge(
    string Id,
    string ExternalKey,
    string CircuitId,
    string FromNodeId,
    string ToNodeId,
    UnitValue Length,
    string? ConductorId,
    string? Phase,
    string? InstallationMethod);

public sealed record Branch(string Id, string ExternalKey, string CircuitId, string RootNodeId);

public sealed record Load(
    string Id,
    string VersionId,
    string? NodeId,
    string? BranchId,
    LoadKind Kind,
    double? Clients,
    UnitValue Kva,
    double? Factor,
    SourceRef? Source = null);

public sealed record Conductor(
    string Id,
    string VersionId,
    string CatalogKey,
    string Name,
    UnitValue Ampacity,
    UnitValue Resistance,
    UnitValue Reactance);

public sealed class NetworkModel
{
    public NetworkModel(
        string id,
        string versionId,
        IEnumerable<Transformer> transformers,
        IEnumerable<Circuit> circuits,
        IEnumerable<Node> nodes,
        IEnumerable<Edge> edges,
        IEnumerable<Branch> branches,
        IEnumerable<Load> loads,
        IEnumerable<Conductor> conductors,
        IEnumerable<ElectricalParameter> parameters)
    {
        Id = Require(id, nameof(id));
        VersionId = Require(versionId, nameof(versionId));
        Transformers = transformers.ToArray();
        Circuits = circuits.ToArray();
        Nodes = nodes.ToArray();
        Edges = edges.ToArray();
        Branches = branches.ToArray();
        Loads = loads.ToArray();
        Conductors = conductors.ToArray();
        Parameters = parameters.ToArray();
    }

    public string Id { get; }
    public string VersionId { get; }
    public IReadOnlyList<Transformer> Transformers { get; }
    public IReadOnlyList<Circuit> Circuits { get; }
    public IReadOnlyList<Node> Nodes { get; }
    public IReadOnlyList<Edge> Edges { get; }
    public IReadOnlyList<Branch> Branches { get; }
    public IReadOnlyList<Load> Loads { get; }
    public IReadOnlyList<Conductor> Conductors { get; }
    public IReadOnlyList<ElectricalParameter> Parameters { get; }

    private static string Require(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", parameterName) : value;
}

public sealed record Project(string Id, string Code, string Name);

public sealed record ProjectVersion(
    string Id,
    string ProjectId,
    string Label,
    NetworkModel NetworkModel,
    string SourceHash,
    bool IsFrozen);

public sealed record CalculationRun(
    string Id,
    string ProjectVersionId,
    CalculationMode Mode,
    string AlgorithmVersion,
    string InputHash,
    CalculationStatus Status);
