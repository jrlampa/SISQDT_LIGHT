using QdtCqts.Domain;
using QdtCqts.Infrastructure.ExcelEvidence;
using QdtCqts.Infrastructure.Sqlite;

namespace QdtCqts.Tests.Import;

public sealed class EvidenceStoreTests
{
    [Fact]
    public void ExtractorPreservesBrokenAndExternalReferences()
    {
        var artifact = new SourceArtifact("artifact", "x.xlsx", "x.xlsx", ".xlsx", "hash");
        var cell = new SourceCell("cell", artifact.Id, "Sheet1", "A1", "=SUM(#REF!,'[external.xlsx]Sheet1'!B2)", null, "unknown");
        var references = new PrecedenceGraphExtractor().Extract(new[] { cell });
        Assert.Contains(references, item => item.Kind == "BROKEN_REFERENCE");
        Assert.Contains(references, item => item.Kind == "EXTERNAL_REFERENCE");
        Assert.Contains(references, item => item.Kind == "CELL");
    }

    [Fact]
    public void EvidenceStorePersistsArtifactCellsAndReferences()
    {
        var directory = Directory.CreateTempSubdirectory("qdt-evidence-");
        try
        {
            var database = new SqliteDatabase(Path.Combine(directory.FullName, "application.db"));
            database.Initialize();
            var artifact = new SourceArtifact("artifact", "x.xlsx", "x.xlsx", ".xlsx", "hash");
            var cell = new SourceCell("cell", artifact.Id, "Sheet1", "A1", "=1", "1", "unknown");
            var reference = new FormulaReference("ref", cell.Id, "A2", "CELL", "unknown");
            new EvidenceStore(database).Save(new ImportedEvidence(artifact, new[] { cell }, new[] { reference }));
            using var connection = database.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM formula_references;";
            Assert.Equal(1L, Convert.ToInt64(command.ExecuteScalar()));
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            directory.Delete(true);
        }
    }

    [Fact]
    public void FormulaCycleIsDetectedSeparatelyFromTopology()
    {
        var artifact = new SourceArtifact("artifact", "x.xlsx", "x.xlsx", ".xlsx", "hash");
        var cells = new[]
        {
            new SourceCell("a", artifact.Id, "Sheet1", "A1", "=B1", null, "unknown"),
            new SourceCell("b", artifact.Id, "Sheet1", "B1", "=A1", null, "unknown")
        };
        Assert.True(new PrecedenceGraphExtractor().HasFormulaCycle(cells));
    }

    [Fact]
    public void DuplicateEvidenceIsRejectedInsteadOfOverwritten()
    {
        var directory = Directory.CreateTempSubdirectory("qdt-immutable-");
        try
        {
            var database = new SqliteDatabase(Path.Combine(directory.FullName, "application.db"));
            database.Initialize();
            var artifact = new SourceArtifact("artifact", "x.xlsx", "x.xlsx", ".xlsx", "hash");
            var evidence = new ImportedEvidence(artifact, Array.Empty<SourceCell>(), Array.Empty<FormulaReference>());
            var store = new EvidenceStore(database);
            store.Save(evidence);
            Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(() => store.Save(evidence));
            Assert.True(database.IsHealthy());
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            directory.Delete(true);
        }
    }

    [Fact]
    public void MigrationIsIdempotentAndIntegrityChecksPass()
    {
        var directory = Directory.CreateTempSubdirectory("qdt-migration-");
        try
        {
            var database = new SqliteDatabase(Path.Combine(directory.FullName, "application.db"));
            database.Initialize();
            database.Initialize();
            Assert.True(database.IsHealthy());
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            directory.Delete(true);
        }
    }
}
