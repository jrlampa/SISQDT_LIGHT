using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;
using QdtCqts.Infrastructure.Geometry;

namespace QdtCqts.Tests.Import;

public sealed class PhysicalGeometryJsonImporterTests
{
    private const string ValidJson = """
        {
          "titulo": "REDE_TESTE_ANONIMIZADA",
          "postes": [
            { "id": "CAD-P-001", "block": "POSTE PROJ", "x": 500000.0, "y": 7400000.0, "fuso": 23 },
            { "id": "CAD-P-002", "block": "POSTE PROJ", "x": 500050.0, "y": 7400030.0, "fuso": 23 }
          ],
          "linhas": [
            { "layer": "BT", "coords": [[500000.0, 7400000.0], [500050.0, 7400030.0]] }
          ]
        }
        """;

    private static NetworkModel CreateModel() => new(
        "NET-1", "V1", Array.Empty<Transformer>(), Array.Empty<Circuit>(),
        new[]
        {
            new Node("N-1", "LOG-1", "C-1", 0, 0, false),
            new Node("N-2", "LOG-2", "C-1", 1, 0, false)
        },
        new[]
        {
            new Edge("E-1", "EDGE-1", "C-1", "N-1", "N-2", new UnitValue(50, UnitCode.Meter), null, null, null)
        },
        Array.Empty<Branch>(), Array.Empty<Load>(), Array.Empty<Conductor>(), Array.Empty<ElectricalParameter>());

    [Fact]
    public void ValidJson_PreservesPointsLinesZoneAndMetricUnits()
    {
        var result = new PhysicalGeometryJsonImporter().Import(ValidJson, CreateModel(), UnitCode.Meter);

        Assert.Empty(result.Errors);
        Assert.Equal(3, result.ImportedElementCount);
        Assert.Equal(0, result.AssociatedNodeCount);
        Assert.Equal(0, result.AssociatedEdgeCount);
        Assert.Equal(3, result.UnassociatedElementCount);
        Assert.Equal(2, result.Points.Count);
        Assert.Single(result.Lines);
        Assert.Equal(UnitCode.Meter, result.Points[0].Position.Unit);
        Assert.Equal(500000.0, result.Points[0].Position.EastingX);
        Assert.Equal(7400000.0, result.Points[0].Position.NorthingY);
        Assert.Equal(23, result.Points[0].Position.SpatialReference.Zone);
        Assert.Equal(SpatialSystemType.Utm, result.Points[0].Position.SpatialReference.SystemType);
        Assert.Null(result.Points[0].Position.SpatialReference.Hemisphere);
        Assert.Null(result.Points[0].Position.SpatialReference.Datum);
        Assert.Null(result.Points[0].Position.SpatialReference.Epsg);
        Assert.Equal(2, result.Lines[0].Geometry.Coordinates.Count);
        Assert.Null(result.Lines[0].AssociatedEdgeId);
    }

    [Fact]
    public void ExplicitMapping_AssociatesNodeAndEdgeWithoutExternalKeyHeuristics()
    {
        var options = new PhysicalGeometryImportOptions(
            NodeIdBySourceId: new Dictionary<string, string> { ["CAD-P-001"] = "N-1" },
            EdgeIdBySourceId: new Dictionary<string, string> { ["LINE-1"] = "E-1" });
        const string json = """
            {
              "postes": [{ "id": "CAD-P-001", "block": "POSTE PROJ", "x": 500000.0, "y": 7400000.0, "fuso": 23 }],
              "linhas": [{ "id": "LINE-1", "layer": "BT", "coords": [[500000.0, 7400000.0], [500050.0, 7400030.0]] }]
            }
            """;

        var result = new PhysicalGeometryJsonImporter().Import(json, CreateModel(), UnitCode.Meter, options);

        Assert.Equal(1, result.AssociatedNodeCount);
        Assert.Equal(1, result.AssociatedEdgeCount);
        Assert.Equal("CAD-P-001", result.UpdatedNetworkModel.Nodes[0].PhysicalPosition!.SourceId);
        Assert.Equal("POSTE PROJ", result.UpdatedNetworkModel.Nodes[0].PhysicalPosition!.SourceBlock);
        Assert.NotNull(result.UpdatedNetworkModel.Edges[0].PhysicalGeometry);
        Assert.Equal("LINE-1", result.UpdatedNetworkModel.Edges[0].PhysicalGeometry!.SourceId);
    }

    [Fact]
    public void MissingIdentityMapping_LeavesLogicalNodesUnchanged()
    {
        var result = new PhysicalGeometryJsonImporter().Import(ValidJson, CreateModel(), UnitCode.Meter);

        Assert.Equal(3, result.UnassociatedElementCount);
        Assert.Equal(0, result.AssociatedNodeCount);
        Assert.All(result.UpdatedNetworkModel.Nodes, node => Assert.Null(node.PhysicalPosition));
        Assert.Contains(result.Points, point => point.AssociatedNodeId is null && point.Position.SourceId == "CAD-P-001");
        Assert.Contains(result.Warnings, warning => warning.Contains("PARTIAL_CRS", StringComparison.Ordinal));
    }

    [Fact]
    public void MatchingExternalKey_DoesNotAutoAssociateWithoutExplicitMap()
    {
        const string json = """
            { "postes": [{ "id": "LOG-1", "block": "POSTE PROJ", "x": 500000.0, "y": 7400000.0, "fuso": 23 }] }
            """;

        var result = new PhysicalGeometryJsonImporter().Import(json, CreateModel(), UnitCode.Meter);

        Assert.Equal("LOG-1", result.UpdatedNetworkModel.Nodes[0].ExternalKey);
        Assert.Null(result.UpdatedNetworkModel.Nodes[0].PhysicalPosition);
        Assert.Null(Assert.Single(result.Points).AssociatedNodeId);
    }

    [Fact]
    public void InvalidCoordinatesAndFuso_AreRejected()
    {
        const string json = """
            { "postes": [
              { "id": "BAD-X", "block": "POSTE", "x": "NaN", "y": 7400000.0, "fuso": 23 },
              { "id": "BAD-ZONE", "block": "POSTE", "x": 500000.0, "y": 7400000.0, "fuso": 61 }
            ] }
            """;

        var result = new PhysicalGeometryJsonImporter().Import(json, CreateModel(), UnitCode.Meter);

        Assert.Empty(result.Points);
        Assert.Equal(2, result.InvalidCount);
        Assert.Contains(result.Warnings, warning => warning.StartsWith("INVALID_POST", StringComparison.Ordinal));
    }

    [Fact]
    public void DuplicatePointIds_AreReportedAndLaterOccurrenceIsIgnored()
    {
        const string json = """
            { "postes": [
              { "id": "DUP", "block": "POSTE A", "x": 500000.0, "y": 7400000.0, "fuso": 23 },
              { "id": "DUP", "block": "POSTE B", "x": 500050.0, "y": 7400030.0, "fuso": 23 }
            ] }
            """;

        var result = new PhysicalGeometryJsonImporter().Import(json, CreateModel(), UnitCode.Meter);

        Assert.Single(result.Points);
        Assert.Equal(1, result.DuplicateCount);
        Assert.Contains(result.Warnings, warning => warning.StartsWith("DUPLICATE_POST_ID", StringComparison.Ordinal));
    }

    [Fact]
    public void ExplicitNonMetricUnit_IsRejected()
    {
        var result = new PhysicalGeometryJsonImporter().Import(ValidJson, CreateModel(), UnitCode.Unknown);

        Assert.Empty(result.Points);
        Assert.Equal(1, result.InvalidCount);
        Assert.Contains(result.Errors, error => error.StartsWith("INVALID_COORDINATE_UNIT", StringComparison.Ordinal));
    }

    [Fact]
    public void JsonDeclaringNonMetricUnit_IsRejected()
    {
        const string json = """
            { "unidade": "ft", "postes": [{ "id": "CAD-P-001", "block": "POSTE PROJ", "x": 500000.0, "y": 7400000.0, "fuso": 23 }] }
            """;

        var result = new PhysicalGeometryJsonImporter().Import(json, CreateModel(), UnitCode.Meter);

        Assert.Empty(result.Points);
        Assert.Contains(result.Errors, error => error.StartsWith("INVALID_COORDINATE_UNIT", StringComparison.Ordinal));
    }

    [Fact]
    public void JsonDeclaringMeters_IsAcceptedWithoutProfileWarning()
    {
        const string json = """
            { "unidade": "m", "postes": [{ "id": "CAD-P-001", "block": "POSTE PROJ", "x": 500000.0, "y": 7400000.0, "fuso": 23 }] }
            """;

        var result = new PhysicalGeometryJsonImporter().Import(json, CreateModel(), UnitCode.Meter);

        Assert.Single(result.Points);
        Assert.DoesNotContain(result.Warnings, warning => warning.StartsWith("UNIT_FROM_PHASE30A", StringComparison.Ordinal));
    }

    [Fact]
    public void NoLineGeometry_IsValidButWarns()
    {
        const string json = """
            { "postes": [{ "id": "CAD-P-001", "block": "POSTE PROJ", "x": 500000.0, "y": 7400000.0, "fuso": 23 }] }
            """;

        var result = new PhysicalGeometryJsonImporter().Import(json, CreateModel(), UnitCode.Meter);

        Assert.Single(result.Points);
        Assert.Empty(result.Lines);
        Assert.Contains(result.Warnings, warning => warning.StartsWith("GEOMETRY_ABSENT", StringComparison.Ordinal));
    }

    [Fact]
    public void MissingLineId_PreservesGeometryWithoutSyntheticId()
    {
        var result = new PhysicalGeometryJsonImporter().Import(ValidJson, CreateModel(), UnitCode.Meter);

        Assert.Single(result.Lines);
        Assert.Null(result.Lines[0].Geometry.SourceId);
        Assert.Contains(result.Warnings, warning => warning.StartsWith("LINE_WITHOUT_ID", StringComparison.Ordinal));
    }

    [Fact]
    public void MissingPostIdentity_IsRejectedWithoutGuessing()
    {
        const string json = """
            { "postes": [{ "block": "POSTE PROJ", "x": 500000.0, "y": 7400000.0, "fuso": 23 }] }
            """;

        var result = new PhysicalGeometryJsonImporter().Import(json, CreateModel(), UnitCode.Meter);

        Assert.Empty(result.Points);
        Assert.Equal(1, result.InvalidCount);
        Assert.All(result.UpdatedNetworkModel.Nodes, node => Assert.Null(node.PhysicalPosition));
    }

    [Fact]
    public void MissingFuso_PreservesPositionWithPartialCrs()
    {
        const string json = """
            { "postes": [{ "id": "CAD-P-001", "block": "POSTE PROJ", "x": 500000.0, "y": 7400000.0 }] }
            """;

        var result = new PhysicalGeometryJsonImporter().Import(json, CreateModel(), UnitCode.Meter);
        var reference = Assert.Single(result.Points).Position.SpatialReference;

        Assert.Equal(SpatialSystemType.Utm, reference.SystemType);
        Assert.Null(reference.Zone);
        Assert.Null(reference.Hemisphere);
        Assert.Null(reference.Datum);
        Assert.Null(reference.Epsg);
        Assert.Contains(result.Warnings, warning => warning.StartsWith("MISSING_ZONE", StringComparison.Ordinal));
    }

    [Fact]
    public void DomainModel_AllowsCompletelyUnknownSpatialReference()
    {
        var reference = new SpatialReference(SpatialSystemType.Unknown, null, null, null, null);
        var position = new PhysicalPosition(500000, 7400000, UnitCode.Meter, reference, "DWG_JSON", "CAD-1", "POSTE");

        Assert.Equal(SpatialSystemType.Unknown, position.SpatialReference.SystemType);
        Assert.Null(position.SpatialReference.Zone);
        Assert.Null(position.SpatialReference.Hemisphere);
        Assert.Null(position.SpatialReference.Datum);
        Assert.Null(position.SpatialReference.Epsg);
    }
}
