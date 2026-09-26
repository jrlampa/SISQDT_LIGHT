using Microsoft.Data.Sqlite;
using QdtCqts.Infrastructure.Sqlite;

namespace QdtCqts.Tests.Import;

public sealed class SqliteTests
{
    [Fact]
    public void DatabaseInitializesWithForeignKeysAndWalAndCanBackup()
    {
        var directory = Directory.CreateTempSubdirectory("qdt-cqts-");
        var databasePath = Path.Combine(directory.FullName, "application.db");
        var backupPath = Path.Combine(directory.FullName, "backup.db");
        try
        {
            var database = new SqliteDatabase(databasePath);
            database.Initialize();
            using (var connection = database.OpenConnection())
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table';";
                Assert.True(Convert.ToInt32(command.ExecuteScalar()) >= 20);
                command.CommandText = "PRAGMA foreign_keys;";
                Assert.Equal(1L, Convert.ToInt64(command.ExecuteScalar()));
                command.CommandText = "PRAGMA journal_mode;";
                Assert.Equal("wal", Convert.ToString(command.ExecuteScalar()));
            }

            database.BackupTo(backupPath);
            Assert.True(File.Exists(backupPath));
            using (var restored = new SqliteDatabase(backupPath).OpenConnection())
            {
                using var restoreCommand = restored.CreateCommand();
                restoreCommand.CommandText = "SELECT COUNT(*) FROM projects;";
                Assert.Equal(0L, Convert.ToInt64(restoreCommand.ExecuteScalar()));
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void ForeignKeyRejectsMissingParent()
    {
        var directory = Directory.CreateTempSubdirectory("qdt-fk-");
        try
        {
            var database = new SqliteDatabase(Path.Combine(directory.FullName, "application.db"));
            database.Initialize();
            using var connection = database.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO project_versions(id, project_id, label, model_kind, source_hash, state) VALUES('v1', 'missing', 'ATUAL', 'QDT', 'hash', 'frozen');";
            Assert.Throws<SqliteException>(() => command.ExecuteNonQuery());
            Assert.True(database.IsHealthy());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            directory.Delete(true);
        }
    }

    [Fact]
    public void TransactionRollbackLeavesNoPartialEvidence()
    {
        var directory = Directory.CreateTempSubdirectory("qdt-rollback-");
        try
        {
            var database = new SqliteDatabase(Path.Combine(directory.FullName, "application.db"));
            database.Initialize();
            using var connection = database.OpenConnection();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO source_artifacts(id,path,file_name,file_type,sha256,captured_at) VALUES('a','x','x.xlsx','.xlsx','hash','now');";
            command.ExecuteNonQuery();
            command.CommandText = "INSERT INTO source_cells(id,artifact_id,sheet_name,address,formula,cached_value_text,classification) VALUES('c','a','Sheet1','A1','=1','1','unknown');";
            command.ExecuteNonQuery();
            transaction.Rollback();
            command.Transaction = null;
            command.CommandText = "SELECT COUNT(*) FROM source_artifacts;";
            Assert.Equal(0L, Convert.ToInt64(command.ExecuteScalar()));
            command.CommandText = "SELECT COUNT(*) FROM source_cells;";
            Assert.Equal(0L, Convert.ToInt64(command.ExecuteScalar()));
            Assert.True(database.IsHealthy());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            directory.Delete(true);
        }
    }

    [Fact]
    public void BackupPreservesEvidenceDataAndIntegrity()
    {
        var directory = Directory.CreateTempSubdirectory("qdt-backup-data-");
        try
        {
            var databasePath = Path.Combine(directory.FullName, "application.db");
            var backupPath = Path.Combine(directory.FullName, "backup.db");
            var database = new SqliteDatabase(databasePath);
            database.Initialize();
            var artifact = new QdtCqts.Domain.SourceArtifact("a", "x.xlsx", "x.xlsx", ".xlsx", "hash");
            var cell = new QdtCqts.Domain.SourceCell("c", "a", "Sheet1", "A1", "=1", "1", "unknown");
            new EvidenceStore(database).Save(new ImportedEvidence(artifact, new[] { cell }, Array.Empty<QdtCqts.Domain.FormulaReference>()));
            database.BackupTo(backupPath);
            var restoredDatabase = new SqliteDatabase(backupPath);
            using var restored = restoredDatabase.OpenConnection();
            using var command = restored.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM source_cells;";
            Assert.Equal(1L, Convert.ToInt64(command.ExecuteScalar()));
            Assert.True(restoredDatabase.IsHealthy());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            directory.Delete(true);
        }
    }
}
