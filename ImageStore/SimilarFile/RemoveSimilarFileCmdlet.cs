using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsCommon.Remove, "ImageStoreSimilarFile", DefaultParameterSetName = "Id")]
    [Alias("RemoveSimilarFile")]
    public class RemoveSimilarFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Entity", Mandatory = true)]
        public ImageStoreSimilarFile SimilarFile { get; set; } = null;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Id", Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            if (SimilarFile != null)
                Id = SimilarFile.Id;

            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Delete from [SimilarFile] where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });

                if (command.ExecuteNonQuery() == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot delete this similar file."),
                        "ImageStore Remove Similar File", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
