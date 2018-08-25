using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Extension
{
    [Cmdlet(VerbsCommon.Remove, "ImageStoreExtension", DefaultParameterSetName = "Id")]
    [Alias("RemoveExtension")]
    public class RemoveExtensionCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Entity", Mandatory = true)]
        public ImageStoreExtension Extension { get; set; } = null;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Id", Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            if (Extension != null)
                Id = Extension.Id;

            var connection = DatabaseConnection.Current;

            using (var commandCreateTable = new SqlCommand("Create Table #tempFileId ([Id] uniqueidentifier)"))
            using (var commandSelect = new SqlCommand("insert into #tempFileId select [Id] from [File] where [ExtensionId]=@ExtensionId"))
            using (var commandDeleteSimilar = new SqlCommand("Delete from [SimilarFile] where [File1Id] in (select [Id] from #tempFileId) or [File2Id] in (select [Id] from #tempFileId)"))
            using (var commandDropTable = new SqlCommand("Drop Table #tempFileId"))
            using (var commandDeleteExtension = new SqlCommand("Delete from [Extension] where [Id]=@Id"))
            using (var transation = connection.BeginTransaction())
            {
                commandCreateTable.Connection = connection;
                commandCreateTable.Transaction = transation;
                commandCreateTable.ExecuteNonQuery();

                commandSelect.Connection = connection;
                commandSelect.Transaction = transation;
                commandSelect.Parameters.Add(new SqlParameter("@ExtensionId", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });

                if (commandSelect.ExecuteNonQuery() != 0)
                {
                    commandDeleteSimilar.Connection = connection;
                    commandDeleteSimilar.Transaction = transation;
                    commandDeleteSimilar.ExecuteNonQuery();

                    if (SimilarFile.LoadImageHelper.cachePath != null)
                    {
                        using (var commandReadId = new SqlCommand("Select [Id] from #tempFileId"))
                        {
                            commandReadId.Connection = connection;
                            commandReadId.Transaction = transation;
                            using (var reader = commandReadId.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                            {
                                while (reader.Read())
                                    SimilarFile.LoadImageHelper.RemoveCache((Guid)reader[0]);
                                reader.Close();
                            }
                        }
                    }
                }
                commandDropTable.Connection = connection;
                commandDropTable.Transaction = transation;
                commandDropTable.ExecuteNonQuery();

                commandDeleteExtension.Connection = connection;
                commandDeleteExtension.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });
                int result = commandDeleteExtension.ExecuteNonQuery();

                if (result == 0)
                {
                    transation.Rollback();
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot delete this extension."),
                        "ImageStore Remove Extension", ErrorCategory.WriteError, null));
                }
                else
                {
                    transation.Commit();
                }
            }
        }
    }
}
