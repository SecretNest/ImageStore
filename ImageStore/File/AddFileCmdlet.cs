using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsCommon.Add, "ImageStoreFile")]
    [Alias("AddFile")]
    [OutputType(typeof(ImageStoreFile))]
    public class AddFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
        public Folder.ImageStoreFolder Folder { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, Mandatory = true)]
        [AllowEmptyString()] public string Path { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, Mandatory = true, ValueFromPipeline = true)]
        [AllowEmptyString()] public string FileName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3, Mandatory = true)]
        public Extension.ImageStoreExtension Extension { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 8)]
        public SwitchParameter OverrideSealedFolder { get; set; }

        protected override void ProcessRecord()
        {
            if (!OverrideSealedFolder.IsPresent && Folder.IsSealed)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Folder containing this file is set to sealed and -" + nameof(OverrideSealedFolder) + " is not present."), "ImageStore Add File", ErrorCategory.SecurityError, Folder.Id));
            }

            if (Path == null)
                throw new ArgumentNullException(nameof(Path));

            if (FileName == null)
                throw new ArgumentNullException(nameof(FileName));

            var connection = DatabaseConnection.Current;
            var id = Guid.NewGuid();

            using (var command = new SqlCommand("Insert into [File] values(@Id, @FolderId, @Path, @FileName, @ExtensionId, null, null, -1, 0, 0)"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = id });
                command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = Folder.Id });
                command.Parameters.Add(new SqlParameter("@Path", System.Data.SqlDbType.NVarChar, 256) { Value = Path });
                command.Parameters.Add(new SqlParameter("@FileName", System.Data.SqlDbType.NVarChar, 256) { Value = FileName });
                command.Parameters.Add(new SqlParameter("@ExtensionId", System.Data.SqlDbType.UniqueIdentifier) { Value = Extension.Id });

                if (command.ExecuteNonQuery() > 0)
                {
                    WriteObject(new ImageStoreFile(id, Folder.Id, Path, FileName, Extension.Id)
                    {
                        FileSize = -1,
                        FileState = FileState.New
                    });
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot insert this file."),
                        "ImageStore Add File", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
