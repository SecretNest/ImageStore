using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

            using (var command = new SqlCommand("Update [IgnoredDirectory] Set [FolderId]=@FolderId, [Directory]=@Directory, [IsSubDirectoryIncluded]=@IsSubDirectoryIncluded where [Id]=@Id"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = IgnoredDirectory.Id });
                command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = IgnoredDirectory.FolderId });
                command.Parameters.Add(new SqlParameter("@Directory", System.Data.SqlDbType.NVarChar, 256) { Value = IgnoredDirectory.Directory });
                command.Parameters.Add(new SqlParameter("@IsSubDirectoryIncluded", System.Data.SqlDbType.Bit) { Value = IgnoredDirectory.IsSubDirectoryIncluded });

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
