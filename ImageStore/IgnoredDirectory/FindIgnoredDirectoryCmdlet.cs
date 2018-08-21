using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.IgnoredDirectory
{
    [Cmdlet(VerbsCommon.Find, "ImageStoreIgnoredDirectory")]
    [Alias("FindIgnoredDirectory")]
    [OutputType(typeof(ImageStoreIgnoredDirectory))]
    public class FindIgnoredDirectoryCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
        public Guid FolderId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, ValueFromPipeline = true, Mandatory = true)]
        [AllowEmptyString()] public string Directory { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public bool IsSubDirectoryIncluded { get; set; } = true;

        protected override void ProcessRecord()
        {
            if (Directory == null)
                throw new ArgumentNullException(nameof(Directory));

            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [Id],[Directory] from [IgnoredDirectory] Where [FolderId]=@FolderId and [Directory]=@Directory and [IsSubDirectoryIncluded]=@IsSubDirectoryIncluded"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = FolderId });
                command.Parameters.Add(new SqlParameter("@Directory", System.Data.SqlDbType.NVarChar, 256) { Value = Directory });
                command.Parameters.Add(new SqlParameter("@IsSubDirectoryIncluded", System.Data.SqlDbType.Bit) { Value = IsSubDirectoryIncluded });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreIgnoredDirectory line = new ImageStoreIgnoredDirectory((Guid)reader[0])
                        {
                            FolderId = FolderId,
                            Directory = (string)reader[1],
                            IsSubDirectoryIncluded = IsSubDirectoryIncluded
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
