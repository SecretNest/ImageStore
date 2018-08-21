using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsCommon.Remove, "ImageStoreFile", DefaultParameterSetName = "Id")]
    [Alias("RemoveFile")]
    public class RemoveFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Entity", Mandatory = true)]
        public ImageStoreFile File { get; set; } = null;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Id", Mandatory = true)]
        public Guid Id { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public SwitchParameter SkipFile { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public SwitchParameter OverrideSealedFolder { get; set; } = false;

        protected override void ProcessRecord()
        {
            if (File != null)
                Id = File.Id;

            var connection = DatabaseConnection.Current;
            var fileName = FileHelper.GetFileName(Id, out _, out _, out _, out var isFolderSealed, out var folderId);
            if (fileName == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("File cannot be found.", nameof(Id)), "ImageStore Remove File", ErrorCategory.InvalidArgument, null));
            }
            if (!OverrideSealedFolder.IsPresent && isFolderSealed)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Folder containing this file is set to sealed and -" + nameof(OverrideSealedFolder) + " is not present."), "ImageStore Remove File", ErrorCategory.SecurityError, folderId));
            }

            if (!SkipFile.IsPresent)
            {
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                    WriteInformation("File " + fileName + " is deleted.", new string[] { "RemoveFile" });
                }
                else
                {
                    WriteWarning("File to be removing cannot be found. Path: " + fileName);
                }
            }

            if (!FileHelper.Delete(Id))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot remove this file."),
                    "ImageStore Remove File", ErrorCategory.WriteError, null));
            }
        }
    }
}
