using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    [Cmdlet(VerbsCommon.Get, "ImageStoreSameFile")]
    [Alias("GetSameFile")]
    [OutputType(typeof(ImageStoreSameFile))]
    public class GetSameFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [Sha1Hash],[FileId],[IsIgnored] from [SameFile] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreSameFile line = new ImageStoreSameFile(Id, (byte[])reader[0], (Guid)reader[1])
                        {
                            IsIgnored = (bool)reader[2]
                        };
                        WriteObject(line);
                    }
                    else
                    {
                        WriteObject(null);
                    }
                    reader.Close();
                }
            }
        }
    }
}
