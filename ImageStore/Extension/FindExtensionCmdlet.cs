using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            using (var command = new SqlCommand("Select [Id],[Extension],[IsImage],[Ignored] from [Extension] Where [Extension]=@Extension"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Extension", System.Data.SqlDbType.NVarChar, 256) { Value = Extension });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreExtension line = new ImageStoreExtension((Guid)reader[0])
                        {
                            Extension = (string)reader[1],
                            IsImage = (bool)reader[2],
                            Ignored = (bool)reader[3]
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
