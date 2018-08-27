using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsCommon.Get, "ImageStoreSimilarFile")]
    [Alias("GetSimilarFile")]
    [OutputType(typeof(ImageStoreSimilarFile))]
    public class GetSimilarFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [File1Id],[File2Id],[DifferenceDegree],[IgnoredMode] from [SimilarFile] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreSimilarFile line = new ImageStoreSimilarFile(Id, (Guid)reader[0], (Guid)reader[1], (float)reader[2])
                        {
                            IgnoredModeCode = (int)reader[3]
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
