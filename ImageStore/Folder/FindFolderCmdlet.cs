using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    [Cmdlet(VerbsCommon.Find, "ImageStoreFolder")]
    [Alias("FindFolder")]
    [OutputType(typeof(ImageStoreFolder))]
    public class FindFolderCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        [AllowEmptyString]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            if (Name == null)
                throw new ArgumentNullException(nameof(Name));

            var connection = DatabaseConnection.Current;
            using (var command = new SqliteCommand("Select [Id],[Path],[Name],[CompareImageWith],[IsSealed] from [Folder] Where [Name]=@Name"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddText("@Name", Name);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreFolder line = new ImageStoreFolder(reader.GetGuid(0),reader.GetString(1))
                        {
                            Name = reader.GetString(2),
                            CompareImageWithCode = reader.GetInt32(3),
                            IsSealed = reader.GetBoolean(4)
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
