using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    [Cmdlet(VerbsCommon.Remove, "ImageStoreSameFileGroup")]
    [Alias("RemoveSameFile")]
    public class RemoveSameFileGroupCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public Byte[] Sha1Hash { get; set; }

        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Delete from [SameFile] where [Sha1Hash]=@Sha1Hash"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Sha1Hash", System.Data.SqlDbType.Binary, 20) { Value = Sha1Hash });

                if (command.ExecuteNonQuery() == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot delete these same files."),
                        "ImageStore Remove Same File Group", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
