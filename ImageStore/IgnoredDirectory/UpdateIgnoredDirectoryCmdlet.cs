using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.IgnoredDirectory
{
    [Cmdlet(VerbsData.Update, "ImageStoreIgnoredDirectory")]
    [Alias("UpdateIgnoredDirectory")]
    public class UpdateIgnoredDirectoryCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public ImageStoreIgnoredDirectory IgnoredDirectory { get; set; }

        protected override void ProcessRecord()
        {
            if (IgnoredDirectory == null)
                throw new ArgumentNullException(nameof(IgnoredDirectory));

            var connection = DatabaseConnection.Current;

            using (var command = new SqliteCommand("Update [IgnoredDirectory] Set [FolderId]=@FolderId, [Directory]=@Directory, [IsSubDirectoryIncluded]=@IsSubDirectoryIncluded where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", IgnoredDirectory.Id);
                command.Parameters.AddGuid("@FolderId", IgnoredDirectory.FolderId);
                command.Parameters.AddText("@Directory", IgnoredDirectory.Directory);
                command.Parameters.AddBool("@IsSubDirectoryIncluded", IgnoredDirectory.IsSubDirectoryIncluded);

                if (command.ExecuteNonQuery() == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot update this ignored directory."),
                        "ImageStore Update Ignored Directory", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
