using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Extension
{
    [Cmdlet(VerbsCommon.Find, "ImageStoreExtension")]
    [Alias("FindExtension")]
    [OutputType(typeof(ImageStoreExtension))]
    public class FindExtensionCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        [AllowEmptyString()] public string Extension { get; set; }

        protected override void ProcessRecord()
        {
            if (Extension == null)
                throw new ArgumentNullException(nameof(Extension));

            var connection = DatabaseConnection.Current;
            using (var command = new SqliteCommand("Select [Id],[Extension],[IsImage],[Ignored] from [Extension] Where [Extension]=@Extension"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddText("@Extension", Extension);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreExtension line = new ImageStoreExtension(reader.GetGuid(0))
                        {
                            Extension = reader.GetString(1),
                            IsImage = reader.GetBoolean(2),
                            Ignored = reader.GetBoolean(3)
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
