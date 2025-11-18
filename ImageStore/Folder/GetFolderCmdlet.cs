using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    [Cmdlet(VerbsCommon.Get, "ImageStoreFolder")]
    [Alias("GetFolder")]
    [OutputType(typeof(ImageStoreFolder))]
    public class GetFolderCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [Path],[Name],[CompareImageWith],[IsSealed] from [Folder] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreFolder line = new ImageStoreFolder(Id,(string)reader[0])
                        {
                            Name = (string)reader[1],
                            CompareImageWithCode = (int)reader[2],
                            IsSealed = (bool)reader[3]
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
