using System.Text.Json;
using QdtCqts.Domain;

namespace QdtCqts.Infrastructure.Geometry;

public sealed record PhysicalGeometryImportOptions(
    IReadOnlyDictionary<string, string>? NodeIdBySourceId = null,
    IReadOnlyDictionary<string, string>? EdgeIdBySourceId = null,
    CoordinateHemisphere? Hemisphere = null,
    string? Datum = null,
    int? Epsg = null);

public sealed record ImportedPhysicalPoint(string ExternalId, string Block, PhysicalPosition Position, string? AssociatedNodeId);

public sealed record ImportedPhysicalLine(PhysicalLineGeometry Geometry, string? AssociatedEdgeId);

public sealed record PhysicalGeometryImportResult(
    NetworkModel UpdatedNetworkModel,
    IReadOnlyList<ImportedPhysicalPoint> Points,
    IReadOnlyList<ImportedPhysicalLine> Lines,
    int AssociatedNodeCount,
    int AssociatedEdgeCount,
    int DuplicateCount,
    int InvalidCount,
    IReadOnlyList<SpatialReference> SpatialReferences,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors)
{
    public int ImportedElementCount => Points.Count + Lines.Count;
    public int UnassociatedElementCount =>
        Points.Count(point => point.AssociatedNodeId is null) + Lines.Count(line => line.AssociatedEdgeId is null);
}

public sealed class PhysicalGeometryJsonImporter
{
    private const string SourceFormat = "DWG_JSON";
    private const double MinimumEasting = 100_000.0;
    private const double MaximumEasting = 900_000.0;
    private const double MinimumNorthing = 0.0;
    private const double MaximumNorthing = 10_000_000.0;

    public PhysicalGeometryImportResult Import(
        string json,
        NetworkModel networkModel,
        UnitCode coordinateUnit,
        PhysicalGeometryImportOptions? options = null)
    {
        options ??= new PhysicalGeometryImportOptions();
        var warnings = new List<string>();
        var errors = new List<string>();
        int duplicateCount = 0;
        int invalidCount = 0;

        if (coordinateUnit != UnitCode.Meter)
        {
            return EmptyResult(networkModel, warnings, new[] { "INVALID_COORDINATE_UNIT: a unidade confirmada pela Fase 30A é metro." }, 1);
        }

        if (options.Epsg is <= 0)
        {
            return EmptyResult(networkModel, warnings, new[] { "INVALID_EPSG: o identificador EPSG informado precisa ser positivo." }, 1);
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return EmptyResult(networkModel, warnings, new[] { "INVALID_JSON: o conteúdo não é um documento JSON válido." }, 1);
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("postes", out var posts) || posts.ValueKind != JsonValueKind.Array)
            {
                return EmptyResult(networkModel, warnings, new[] { "MISSING_POSTS: o JSON deve conter a coleção 'postes'." }, 1);
            }

            bool unitDeclared = TryGetProperty(root, "unidade", "unit", out var unitElement);
            if (unitDeclared && !IsMeterUnit(unitElement))
            {
                return EmptyResult(networkModel, warnings, new[] { "INVALID_COORDINATE_UNIT: o JSON declara uma unidade diferente de metro." }, 1);
            }

            if (!TryGetProperty(root, "linhas", "lines", out var linesElement) || linesElement.ValueKind != JsonValueKind.Array)
            {
                warnings.Add("GEOMETRY_ABSENT: o JSON não contém geometria de linhas; posições dos postes ainda podem ser importadas.");
                linesElement = default;
            }

            var parsedPosts = new List<(string Id, string Block, double X, double Y, int? Zone)>();
            var seenPostIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var post in posts.EnumerateArray())
            {
                if (!TryGetRequiredString(post, "id", out var id) || !TryGetRequiredString(post, "block", out var block) ||
                    !TryGetCoordinate(post, "x", out var x) || !TryGetCoordinate(post, "y", out var y) ||
                    !TryGetOptionalZone(post, out var zone) ||
                    !IsValidUtmCoordinate(x, y))
                {
                    invalidCount++;
                    warnings.Add("INVALID_POST: um poste sem ID/bloco, coordenada UTM válida ou com fuso inválido foi ignorado.");
                    continue;
                }

                if (!zone.HasValue)
                {
                    warnings.Add($"MISSING_ZONE: o poste '{id}' não informa fuso; a posição foi preservada com CRS parcial.");
                }

                if (!seenPostIds.Add(id))
                {
                    duplicateCount++;
                    warnings.Add($"DUPLICATE_POST_ID: o ID externo '{id}' aparece mais de uma vez; ocorrências posteriores foram ignoradas.");
                    continue;
                }

                parsedPosts.Add((id, block, x, y, zone));
            }

            var knownNodeIds = networkModel.Nodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
            var mappedNodeIds = new HashSet<string>(StringComparer.Ordinal);
            var locationsByNodeId = new Dictionary<string, PhysicalPosition>(StringComparer.Ordinal);
            var importedPoints = new List<ImportedPhysicalPoint>();
            var spatialReferences = new List<SpatialReference>();
            int associatedNodeCount = 0;

            foreach (var post in parsedPosts)
            {
                var spatialReference = new SpatialReference(SpatialSystemType.Utm, post.Zone, options.Hemisphere, options.Datum, options.Epsg);
                AddUnique(spatialReferences, spatialReference);
                var position = new PhysicalPosition(post.X, post.Y, UnitCode.Meter, spatialReference, SourceFormat, post.Id, post.Block);
                string? associatedNodeId = null;

                if (options.NodeIdBySourceId?.TryGetValue(post.Id, out var nodeId) == true)
                {
                    if (!knownNodeIds.Contains(nodeId))
                    {
                        warnings.Add($"UNKNOWN_NODE_MAPPING: o ID externo '{post.Id}' aponta para nó lógico inexistente '{nodeId}'.");
                    }
                    else if (!mappedNodeIds.Add(nodeId) || networkModel.Nodes.First(node => node.Id == nodeId).PhysicalPosition is not null)
                    {
                        duplicateCount++;
                        warnings.Add($"DUPLICATE_NODE_MAPPING: mais de uma posição física aponta para o nó '{nodeId}'; a posição adicional não foi associada.");
                    }
                    else
                    {
                        associatedNodeId = nodeId;
                        locationsByNodeId.Add(nodeId, position);
                        associatedNodeCount++;
                    }
                }

                importedPoints.Add(new ImportedPhysicalPoint(post.Id, post.Block, position, associatedNodeId));
            }

            var updatedNodes = networkModel.Nodes
                .Select(node => locationsByNodeId.TryGetValue(node.Id, out var position) ? node with { PhysicalPosition = position } : node)
                .ToArray();

            var parsedLines = new List<ImportedPhysicalLine>();
            var seenLineIds = new HashSet<string>(StringComparer.Ordinal);
            int associatedEdgeCount = 0;
            var knownEdgeIds = networkModel.Edges.Select(edge => edge.Id).ToHashSet(StringComparer.Ordinal);
            var mappedEdgeIds = new HashSet<string>(StringComparer.Ordinal);
            var geometriesByEdgeId = new Dictionary<string, PhysicalLineGeometry>(StringComparer.Ordinal);
            int? commonZone = parsedPosts.Select(post => post.Zone).Distinct().Count() == 1
                ? parsedPosts.FirstOrDefault().Zone
                : null;

            if (linesElement.ValueKind == JsonValueKind.Array)
            {
                int lineOrdinal = 0;
                foreach (var line in linesElement.EnumerateArray())
                {
                    lineOrdinal++;
                    if (line.ValueKind != JsonValueKind.Object || !TryGetProperty(line, "coords", "coordinates", out var coordinatesElement) ||
                        coordinatesElement.ValueKind != JsonValueKind.Array || !TryParseLineCoordinates(coordinatesElement, out var coordinates) ||
                        !TryGetRequiredString(line, "layer", out var layer))
                    {
                        invalidCount++;
                        warnings.Add("INVALID_LINE: uma linha sem layer ou com menos de dois pares de coordenadas UTM válidas foi ignorada.");
                        continue;
                    }

                    string? sourceId = TryGetRequiredString(line, "id", out var lineId) ? lineId : null;
                    if (sourceId is not null && !seenLineIds.Add(sourceId))
                    {
                        duplicateCount++;
                        warnings.Add($"DUPLICATE_LINE_ID: o ID externo '{sourceId}' aparece mais de uma vez; ocorrências posteriores foram ignoradas.");
                        continue;
                    }

                    if (sourceId is null)
                    {
                        warnings.Add($"LINE_WITHOUT_ID: a linha ordinal {lineOrdinal} foi preservada por geometria, sem ID sintético.");
                    }

                    if (!TryGetOptionalZone(line, out var lineZone))
                    {
                        invalidCount++;
                        warnings.Add("INVALID_LINE_ZONE: uma linha com fuso inválido foi ignorada.");
                        continue;
                    }

                    int? zone = lineZone ?? commonZone;
                    if (!zone.HasValue)
                    {
                        warnings.Add($"MISSING_LINE_ZONE: a linha ordinal {lineOrdinal} não informa fuso e não há fuso único nos postes; CRS parcial preservado.");
                    }
                    var spatialReference = new SpatialReference(SpatialSystemType.Utm, zone, options.Hemisphere, options.Datum, options.Epsg);
                    AddUnique(spatialReferences, spatialReference);
                    var geometry = new PhysicalLineGeometry(coordinates, UnitCode.Meter, spatialReference, sourceId, layer, SourceFormat);
                    string? associatedEdgeId = null;

                    if (sourceId is not null && options.EdgeIdBySourceId?.TryGetValue(sourceId, out var edgeId) == true)
                    {
                        if (!knownEdgeIds.Contains(edgeId))
                        {
                            warnings.Add($"UNKNOWN_EDGE_MAPPING: o ID externo '{sourceId}' aponta para trecho lógico inexistente '{edgeId}'.");
                        }
                        else if (!mappedEdgeIds.Add(edgeId) || networkModel.Edges.First(edge => edge.Id == edgeId).PhysicalGeometry is not null)
                        {
                            duplicateCount++;
                            warnings.Add($"DUPLICATE_EDGE_MAPPING: mais de uma geometria aponta para o trecho '{edgeId}'; a geometria adicional não foi associada.");
                        }
                        else
                        {
                            associatedEdgeId = edgeId;
                            geometriesByEdgeId.Add(edgeId, geometry);
                            associatedEdgeCount++;
                        }
                    }

                    parsedLines.Add(new ImportedPhysicalLine(geometry, associatedEdgeId));
                }
            }

            var updatedEdges = networkModel.Edges
                .Select(edge => geometriesByEdgeId.TryGetValue(edge.Id, out var geometry) ? edge with { PhysicalGeometry = geometry } : edge)
                .ToArray();
            var updatedNetworkModel = new NetworkModel(networkModel.Id, networkModel.VersionId, networkModel.Transformers, networkModel.Circuits,
                updatedNodes, updatedEdges, networkModel.Branches, networkModel.Loads, networkModel.Conductors, networkModel.Parameters);

            if (parsedPosts.Select(post => post.Zone).Distinct().Count() > 1)
            {
                warnings.Add("MIXED_UTM_ZONES: o documento contém mais de um fuso; cada posição mantém o próprio fuso e linhas sem fuso ficam sem zona identificada.");
            }

            if (options.Datum is null || options.Epsg is null)
            {
                warnings.Add("PARTIAL_CRS: datum e EPSG não foram informados; nenhum datum/EPSG foi presumido.");
            }

            if (!unitDeclared)
            {
                warnings.Add("UNIT_FROM_PHASE30A: o contrato DWG-JSON não informa unidade; metro foi aplicado com base na Fase 30A.");
            }
            return new PhysicalGeometryImportResult(updatedNetworkModel, importedPoints, parsedLines, associatedNodeCount, associatedEdgeCount,
                duplicateCount, invalidCount, spatialReferences, warnings, errors);
        }
    }

    private static PhysicalGeometryImportResult EmptyResult(NetworkModel model, IReadOnlyList<string> warnings, IReadOnlyList<string> errors, int invalidCount) =>
        new(model, Array.Empty<ImportedPhysicalPoint>(), Array.Empty<ImportedPhysicalLine>(), 0, 0, 0, invalidCount,
            Array.Empty<SpatialReference>(), warnings, errors);

    private static bool TryGetProperty(JsonElement element, string primaryName, string alternateName, out JsonElement value)
    {
        value = default;
        return element.ValueKind == JsonValueKind.Object &&
            (element.TryGetProperty(primaryName, out value) || element.TryGetProperty(alternateName, out value));
    }

    private static bool TryGetRequiredString(JsonElement element, string name, out string value)
    {
        value = string.Empty;
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var property) &&
            property.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value = property.GetString()!.Trim());
    }

    private static bool TryGetCoordinate(JsonElement element, string name, out double value)
    {
        value = 0.0;
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var property) &&
            property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out value) && double.IsFinite(value);
    }

    private static bool TryGetOptionalZone(JsonElement element, out int? zone)
    {
        zone = null;
        if (!TryGetProperty(element, "fuso", "zone", out var zoneElement)) return true;
        if (zoneElement.ValueKind != JsonValueKind.Number || !zoneElement.TryGetInt32(out var parsedZone) || parsedZone is < 1 or > 60)
        {
            return false;
        }

        zone = parsedZone;
        return true;
    }

    private static bool IsValidUtmCoordinate(double easting, double northing) =>
        easting is >= MinimumEasting and <= MaximumEasting && northing is >= MinimumNorthing and <= MaximumNorthing;

    private static bool IsMeterUnit(JsonElement unitElement)
    {
        if (unitElement.ValueKind != JsonValueKind.String) return false;
        string unit = unitElement.GetString()!.Trim().ToLowerInvariant();
        return unit is "m" or "meter" or "meters" or "metro" or "metros";
    }

    private static bool TryParseLineCoordinates(JsonElement element, out IReadOnlyList<PhysicalCoordinate> coordinates)
    {
        var parsed = new List<PhysicalCoordinate>();
        coordinates = parsed;
        foreach (var pair in element.EnumerateArray())
        {
            if (pair.ValueKind != JsonValueKind.Array || pair.GetArrayLength() != 2) return false;
            var values = pair.EnumerateArray().ToArray();
            if (values[0].ValueKind != JsonValueKind.Number || !values[0].TryGetDouble(out var x) ||
                values[1].ValueKind != JsonValueKind.Number || !values[1].TryGetDouble(out var y) ||
                !double.IsFinite(x) || !double.IsFinite(y) || !IsValidUtmCoordinate(x, y)) return false;
            parsed.Add(new PhysicalCoordinate(x, y));
        }

        return parsed.Count >= 2;
    }

    private static void AddUnique(ICollection<SpatialReference> references, SpatialReference reference)
    {
        if (!references.Contains(reference)) references.Add(reference);
    }
}
