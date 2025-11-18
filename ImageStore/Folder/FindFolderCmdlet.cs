using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            using (var command = new SqlCommand("Select [Id],[Path],[Name],[CompareImageWith],[IsSealed] from [Folder] Where [Name]=@Name"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Name", System.Data.SqlDbType.NVarChar, 256) { Value = Name });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreFolder line = new ImageStoreFolder((Guid)reader[0],(string)reader[1])
                        {
                            Name = (string)reader[2],
                            CompareImageWithCode = (int)reader[3],
                            IsSealed = (bool)reader[4]
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
