using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    [Cmdlet(VerbsCommon.Remove, "ImageStoreFolder", DefaultParameterSetName = "Id")]
    [Alias("RemoveFolder")]
    public class RemoveFolderCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Entity", Mandatory = true)]
        public ImageStoreFolder Folder { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Id", Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            if (Folder != null)
                Id = Folder.Id;

            var connection = DatabaseConnection.Current;

            using (var commandCreateTable = new SqliteCommand("Create temp table tempFileId ([Id] BLOB)"))
            using (var commandSelect = new SqliteCommand("insert into tempFileId select [Id] from [File] where [FolderId]=@FolderId"))
            using (var commandDeleteSimilar = new SqliteCommand("Delete from [SimilarFile] where [File1Id] in (select [Id] from tempFileId) or [File2Id] in (select [Id] from tempFileId)"))
            using (var commandDropTable = new SqliteCommand("Drop Table tempFileId"))
            using (var commandDeleteFolder= new SqliteCommand("Delete from [Folder] where [Id]=@Id"))
            using (var transation = connection.BeginTransaction())
            {
                commandCreateTable.Connection = connection;
                commandCreateTable.CommandTimeout = 0;
                commandCreateTable.Transaction = transation;
                commandCreateTable.ExecuteNonQuery();

                commandSelect.Connection = connection;
                commandSelect.CommandTimeout = 0;
                commandSelect.Transaction = transation;
                commandSelect.Parameters.AddGuid("@FolderId", Id);

                if (commandSelect.ExecuteNonQuery() != 0)
                {
                    commandDeleteSimilar.Connection = connection;
                    commandDeleteSimilar.CommandTimeout = 0;
                    commandDeleteSimilar.Transaction = transation;
                    commandDeleteSimilar.ExecuteNonQuery();

                    if (SimilarFile.LoadImageHelper.cachePath != null)
                    {
                        using (var commandReadId = new SqliteCommand("Select [Id] from tempFileId"))
                        {
                            commandReadId.Connection = connection;
                            commandReadId.CommandTimeout = 0;
                            commandReadId.Transaction = transation;
                            using (var reader = commandReadId.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                            {
                                while (reader.Read())
                                    SimilarFile.LoadImageHelper.RemoveCache(reader.GetGuid(0));
                                reader.Close();
                            }
                        }
                    }
                }
                commandDropTable.Connection = connection;
                commandDropTable.CommandTimeout = 0;
                commandDropTable.Transaction = transation;
                commandDropTable.ExecuteNonQuery();

                commandDeleteFolder.Connection = connection;
                commandDeleteFolder.CommandTimeout = 0;
                commandDeleteFolder.Transaction = transation;
                commandDeleteFolder.Parameters.AddGuid("@Id", Id);
                int result = commandDeleteFolder.ExecuteNonQuery();

                if (result == 0)
                {
                    transation.Rollback();
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot delete this folder."),
                        "ImageStore Remove Folder", ErrorCategory.WriteError, null));
                }
                else
                {
                    transation.Commit();
                }
            }

        }
    }
}
