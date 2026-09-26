using Microsoft.Data.Sqlite;

namespace QdtCqts.Infrastructure.Sqlite;

public sealed class SqliteDatabase
{
    private readonly string connectionString;

    public SqliteDatabase(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
    }

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys=ON; PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    public void Initialize()
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = SchemaSql;
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    public void BackupTo(string destinationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        using var source = OpenConnection();
        using var destination = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = destinationPath }.ToString());
        destination.Open();
        source.BackupDatabase(destination);
    }

    public bool IsHealthy()
    {
        using var connection = OpenConnection();
        using var integrity = connection.CreateCommand();
        integrity.CommandText = "PRAGMA integrity_check;";
        var integrityResult = Convert.ToString(integrity.ExecuteScalar());
        using var foreignKeys = connection.CreateCommand();
        foreignKeys.CommandText = "PRAGMA foreign_key_check;";
        using var rows = foreignKeys.ExecuteReader();
        return string.Equals(integrityResult, "ok", StringComparison.OrdinalIgnoreCase) && !rows.Read();
    }

    private const string SchemaSql = """
        CREATE TABLE IF NOT EXISTS schema_migrations(version INTEGER PRIMARY KEY, applied_at TEXT NOT NULL);
        INSERT OR IGNORE INTO schema_migrations(version, applied_at) VALUES (1, datetime('now'));
        CREATE TABLE IF NOT EXISTS projects(id TEXT PRIMARY KEY, code TEXT NOT NULL UNIQUE, name TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS project_versions(id TEXT PRIMARY KEY, project_id TEXT NOT NULL REFERENCES projects(id), label TEXT NOT NULL, model_kind TEXT NOT NULL, source_hash TEXT NOT NULL, state TEXT NOT NULL, UNIQUE(project_id, label));
        CREATE TABLE IF NOT EXISTS network_models(id TEXT PRIMARY KEY, version_id TEXT NOT NULL UNIQUE REFERENCES project_versions(id), kind TEXT NOT NULL, schema_version TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS transformers(id TEXT PRIMARY KEY, network_model_id TEXT NOT NULL REFERENCES network_models(id), external_key TEXT NOT NULL, power_mva REAL, impedance_percent REAL, line_voltage_kv REAL, demand_kva REAL);
        CREATE TABLE IF NOT EXISTS circuits(id TEXT PRIMARY KEY, network_model_id TEXT NOT NULL REFERENCES network_models(id), transformer_id TEXT NOT NULL REFERENCES transformers(id), external_key TEXT NOT NULL, side_index INTEGER, mode TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS topology_nodes(id TEXT PRIMARY KEY, network_model_id TEXT NOT NULL REFERENCES network_models(id), circuit_id TEXT NOT NULL REFERENCES circuits(id), external_key TEXT NOT NULL, parent_node_id TEXT REFERENCES topology_nodes(id), x REAL, y REAL, crs TEXT, is_source INTEGER NOT NULL DEFAULT 0);
        CREATE TABLE IF NOT EXISTS topology_edges(id TEXT PRIMARY KEY, network_model_id TEXT NOT NULL REFERENCES network_models(id), circuit_id TEXT NOT NULL REFERENCES circuits(id), from_node_id TEXT NOT NULL REFERENCES topology_nodes(id), to_node_id TEXT NOT NULL REFERENCES topology_nodes(id), length_m REAL NOT NULL CHECK(length_m >= 0), conductor_id TEXT, phase TEXT, installation TEXT, CHECK(from_node_id <> to_node_id));
        CREATE TABLE IF NOT EXISTS branches(id TEXT PRIMARY KEY, network_model_id TEXT NOT NULL REFERENCES network_models(id), circuit_id TEXT NOT NULL REFERENCES circuits(id), root_node_id TEXT NOT NULL REFERENCES topology_nodes(id), external_key TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS loads(id TEXT PRIMARY KEY, network_model_id TEXT NOT NULL REFERENCES network_models(id), node_id TEXT REFERENCES topology_nodes(id), branch_id TEXT REFERENCES branches(id), kind TEXT NOT NULL, external_key TEXT, clients REAL, kva REAL, factor REAL);
        CREATE TABLE IF NOT EXISTS conductors(id TEXT PRIMARY KEY, version_id TEXT NOT NULL REFERENCES project_versions(id), catalog_key TEXT NOT NULL, name TEXT NOT NULL, ampacity_a REAL, resistance REAL, reactance REAL, units_json TEXT);
        CREATE TABLE IF NOT EXISTS electrical_parameters(id TEXT PRIMARY KEY, version_id TEXT NOT NULL REFERENCES project_versions(id), key TEXT NOT NULL, value_numeric REAL, value_text TEXT, unit TEXT NOT NULL, certainty TEXT NOT NULL, source_ref TEXT);
        CREATE TABLE IF NOT EXISTS calculation_runs(id TEXT PRIMARY KEY, version_id TEXT NOT NULL REFERENCES project_versions(id), network_model_id TEXT NOT NULL REFERENCES network_models(id), calculation_mode TEXT NOT NULL, algorithm_version TEXT NOT NULL, input_hash TEXT NOT NULL, status TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS calculation_inputs(id TEXT PRIMARY KEY, run_id TEXT NOT NULL REFERENCES calculation_runs(id), entity_type TEXT NOT NULL, entity_id TEXT NOT NULL, path TEXT NOT NULL, value_text TEXT, value_numeric REAL, unit TEXT NOT NULL, source_ref TEXT);
        CREATE TABLE IF NOT EXISTS calculation_results(id TEXT PRIMARY KEY, run_id TEXT NOT NULL REFERENCES calculation_runs(id), scope TEXT NOT NULL, entity_id TEXT NOT NULL, path TEXT NOT NULL, value_text TEXT, value_numeric REAL, unit TEXT NOT NULL, status TEXT NOT NULL, rule_id TEXT, formula_id TEXT, evidence_id TEXT);
        CREATE TABLE IF NOT EXISTS validations(id TEXT PRIMARY KEY, run_id TEXT REFERENCES calculation_runs(id), version_id TEXT NOT NULL REFERENCES project_versions(id), code TEXT NOT NULL, scope_type TEXT, scope_id TEXT, severity TEXT NOT NULL, status TEXT NOT NULL, message TEXT NOT NULL, source_ref TEXT);
        CREATE TABLE IF NOT EXISTS trace_steps(id TEXT PRIMARY KEY, run_id TEXT NOT NULL REFERENCES calculation_runs(id), step_id TEXT NOT NULL, rule_id TEXT, formula_id TEXT, evidence_id TEXT, input_paths TEXT NOT NULL, output_path TEXT NOT NULL, operands_json TEXT, value_numeric REAL, unit TEXT NOT NULL, status TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS source_artifacts(id TEXT PRIMARY KEY, path TEXT NOT NULL, file_name TEXT NOT NULL, file_type TEXT NOT NULL, sha256 TEXT NOT NULL, captured_at TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS source_cells(id TEXT PRIMARY KEY, artifact_id TEXT NOT NULL REFERENCES source_artifacts(id), sheet_name TEXT NOT NULL, address TEXT NOT NULL, formula TEXT, cached_value_text TEXT, classification TEXT NOT NULL, UNIQUE(artifact_id, sheet_name, address));
        CREATE TABLE IF NOT EXISTS formula_references(id TEXT PRIMARY KEY, source_cell_id TEXT NOT NULL REFERENCES source_cells(id), precedent_ref TEXT NOT NULL, ref_kind TEXT NOT NULL, resolution_status TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS golden_cases(id TEXT PRIMARY KEY, case_id TEXT NOT NULL UNIQUE, model_kind TEXT NOT NULL, source_artifact_id TEXT REFERENCES source_artifacts(id), source_hash TEXT NOT NULL, input_snapshot_hash TEXT NOT NULL, status TEXT NOT NULL, tolerance_policy TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS golden_observations(id TEXT PRIMARY KEY, case_id TEXT NOT NULL REFERENCES golden_cases(id), path TEXT NOT NULL, source_cell_id TEXT REFERENCES source_cells(id), expected_text TEXT, expected_numeric REAL, unit TEXT NOT NULL, formula_id TEXT, comparison_mode TEXT NOT NULL);
        CREATE INDEX IF NOT EXISTS ix_nodes_circuit ON topology_nodes(circuit_id);
        CREATE INDEX IF NOT EXISTS ix_edges_from ON topology_edges(from_node_id);
        CREATE INDEX IF NOT EXISTS ix_edges_to ON topology_edges(to_node_id);
        CREATE INDEX IF NOT EXISTS ix_run_results ON calculation_results(run_id);
        CREATE INDEX IF NOT EXISTS ix_source_cells_sheet ON source_cells(artifact_id, sheet_name);
        """;
}
