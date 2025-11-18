using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

            using (var command = new SqlCommand("Insert into [IgnoredDirectory] values(@Id, @FolderId, @Directory, @IsSubDirectoryIncluded)"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = id });
                command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = FolderId });
                command.Parameters.Add(new SqlParameter("@Directory", System.Data.SqlDbType.NVarChar, 256) { Value = Directory });
                command.Parameters.Add(new SqlParameter("@IsSubDirectoryIncluded", System.Data.SqlDbType.Bit) { Value = IsSubDirectoryIncluded });

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
