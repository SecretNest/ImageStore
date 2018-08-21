using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    [Cmdlet(VerbsCommon.Remove, "ImageStoreSameFile", DefaultParameterSetName = "Id")]
    [Alias("RemoveSameFile")]
    public class RemoveSameFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Entity", Mandatory = true)]
        public ImageStoreSameFile SameFile { get; set; } = null;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Id", Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            if (SameFile != null)
                Id = SameFile.Id;

            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Delete from [SameFile] where [Id]=@Id"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });

                if (command.ExecuteNonQuery() == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot delete this same file."),
                        "ImageStore Remove Same File", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
