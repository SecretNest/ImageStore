using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
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
            using (var command = new SqliteCommand("Select [Id],[Directory] from [IgnoredDirectory] Where [FolderId]=@FolderId and [Directory]=@Directory and [IsSubDirectoryIncluded]=@IsSubDirectoryIncluded"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@FolderId", FolderId);
                command.Parameters.AddText("@Directory", Directory);
                command.Parameters.AddBool("@IsSubDirectoryIncluded", IsSubDirectoryIncluded);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreIgnoredDirectory line = new ImageStoreIgnoredDirectory(reader.GetGuid(0))
                        {
                            FolderId = FolderId,
                            Directory = reader.GetString(1),
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
