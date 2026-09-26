using Microsoft.Data.Sqlite;
using QdtCqts.Domain;

namespace QdtCqts.Infrastructure.Sqlite;

public sealed class EvidenceStore
{
    private readonly SqliteDatabase database;

    public EvidenceStore(SqliteDatabase database) => this.database = database;

    public void Save(ImportedEvidence evidence)
    {
        using var connection = database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        using var artifactCommand = connection.CreateCommand();
        artifactCommand.Transaction = transaction;
        artifactCommand.CommandText = "INSERT INTO source_artifacts(id,path,file_name,file_type,sha256,captured_at) VALUES($id,$path,$file_name,$file_type,$sha256,$captured_at);";
        artifactCommand.Parameters.AddWithValue("$id", evidence.Artifact.Id);
        artifactCommand.Parameters.AddWithValue("$path", evidence.Artifact.Path);
        artifactCommand.Parameters.AddWithValue("$file_name", evidence.Artifact.FileName);
        artifactCommand.Parameters.AddWithValue("$file_type", evidence.Artifact.FileType);
        artifactCommand.Parameters.AddWithValue("$sha256", evidence.Artifact.Sha256);
        artifactCommand.Parameters.AddWithValue("$captured_at", DateTimeOffset.UtcNow.ToString("O"));
        artifactCommand.ExecuteNonQuery();

        foreach (var cell in evidence.Cells)
        {
            using var cellCommand = connection.CreateCommand();
            cellCommand.Transaction = transaction;
            cellCommand.CommandText = "INSERT INTO source_cells(id,artifact_id,sheet_name,address,formula,cached_value_text,classification) VALUES($id,$artifact_id,$sheet_name,$address,$formula,$cached,$classification);";
            cellCommand.Parameters.AddWithValue("$id", cell.Id);
            cellCommand.Parameters.AddWithValue("$artifact_id", cell.ArtifactId);
            cellCommand.Parameters.AddWithValue("$sheet_name", cell.SheetName);
            cellCommand.Parameters.AddWithValue("$address", cell.Address);
            cellCommand.Parameters.AddWithValue("$formula", (object?)cell.Formula ?? DBNull.Value);
            cellCommand.Parameters.AddWithValue("$cached", (object?)cell.CachedValue ?? DBNull.Value);
            cellCommand.Parameters.AddWithValue("$classification", cell.Classification);
            cellCommand.ExecuteNonQuery();
        }

        foreach (var reference in evidence.References)
        {
            using var referenceCommand = connection.CreateCommand();
            referenceCommand.Transaction = transaction;
            referenceCommand.CommandText = "INSERT INTO formula_references(id,source_cell_id,precedent_ref,ref_kind,resolution_status) VALUES($id,$source,$precedent,$kind,$status);";
            referenceCommand.Parameters.AddWithValue("$id", reference.Id);
            referenceCommand.Parameters.AddWithValue("$source", reference.SourceCellId);
            referenceCommand.Parameters.AddWithValue("$precedent", reference.PrecedentRef);
            referenceCommand.Parameters.AddWithValue("$kind", reference.Kind);
            referenceCommand.Parameters.AddWithValue("$status", reference.ResolutionStatus);
            referenceCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}

public sealed record ImportedEvidence(SourceArtifact Artifact, IReadOnlyList<SourceCell> Cells, IReadOnlyList<FormulaReference> References);
