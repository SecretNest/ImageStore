using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
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
            using (var command = new SqliteCommand("Select [Path],[Name],[CompareImageWith],[IsSealed] from [Folder] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", Id);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreFolder line = new ImageStoreFolder(Id,reader.GetString(0))
                        {
                            Name = reader.GetString(1),
                            CompareImageWithCode = reader.GetInt32(2),
                            IsSealed = reader.GetBoolean(3)
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
