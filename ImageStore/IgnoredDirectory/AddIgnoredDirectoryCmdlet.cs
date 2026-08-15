using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.IgnoredDirectory
{
    [Cmdlet(VerbsCommon.Add, "ImageStoreIgnoredDirectory")]
    [Alias("AddIgnoredDirectory")]
    [OutputType(typeof(ImageStoreIgnoredDirectory))]
    public class AddIgnoredDirectoryCmdlet : Cmdlet
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
            var id = Guid.NewGuid();

            using (var command = new SqliteCommand("Insert into [IgnoredDirectory] values(@Id, @FolderId, @Directory, @IsSubDirectoryIncluded)"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", id);
                command.Parameters.AddGuid("@FolderId", FolderId);
                command.Parameters.AddText("@Directory", Directory);
                command.Parameters.AddBool("@IsSubDirectoryIncluded", IsSubDirectoryIncluded);

                if (command.ExecuteNonQuery() > 0)
                {
                    WriteObject(new ImageStoreIgnoredDirectory(id)
                    {
                        FolderId = FolderId,
                        Directory = Directory,
                        IsSubDirectoryIncluded = IsSubDirectoryIncluded
                    });
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot insert this ignored directory."),
                        "ImageStore Add Ignored Directory", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
