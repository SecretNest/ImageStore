using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.IgnoredDirectory
{
    [Cmdlet(VerbsCommon.Get, "ImageStoreIgnoredDirectory")]
    [Alias("GetIgnoredDirectory")]
    [OutputType(typeof(ImageStoreIgnoredDirectory))]
    public class GetIgnoredDirectoryCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [FolderId],[Directory],[IsSubDirectoryIncluded] from [IgnoredDirectory] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreIgnoredDirectory line = new ImageStoreIgnoredDirectory(Id)
                        {
                            FolderId = (Guid)reader[0],
                            Directory = (string)reader[1],
                            IsSubDirectoryIncluded = (bool)reader[2]
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
