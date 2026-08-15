using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
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
            using (var command = new SqliteCommand("Select [File1Id],[File2Id],[DifferenceDegree],[IgnoredMode] from [SimilarFile] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", Id);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreSimilarFile line = new ImageStoreSimilarFile(Id, reader.GetGuid(0), reader.GetGuid(1), reader.GetFloat(2))
                        {
                            IgnoredModeCode = reader.GetInt32(3)
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
