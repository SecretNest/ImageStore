using SecretNest.ImageStore.Extension;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsCommon.Rename, "ImageStoreFile")]
    [Alias("RenameFile")]
    [OutputType(typeof(ImageStoreFile))]
    public class RenameFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "EntityId", Mandatory = true)]
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "EntityEntity", Mandatory = true)]
        public ImageStoreFile File { get; set; } = null;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "IdEntity", Mandatory = true)]
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "IdId", Mandatory = true)]
        public Guid Id { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, Mandatory = true)]
        [AllowEmptyString()] public string NewFileName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, Mandatory = true, ParameterSetName = "EntityId")]
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, Mandatory = true, ParameterSetName = "IdId")]
        public Guid NewExtensionId { get; set; }
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, Mandatory = true, ParameterSetName = "EntityEntity")]
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, Mandatory = true, ParameterSetName = "IdEntity")]
        public ImageStoreExtension NewExtension { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public SwitchParameter SkipFile { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 4)]
        public SwitchParameter SkipReturn { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 5)]
        public SwitchParameter OverrideSealedFolder { get; set; }

        protected override void ProcessRecord()
        {
            if (NewFileName == null)
                throw new ArgumentNullException(nameof(NewFileName));
            string extensionName;
            if (NewExtension != null)
            {
                NewExtensionId = NewExtension.Id;
                extensionName = NewExtension.Extension;
            }
            else
            {
                extensionName = ExtensionHelper.GetExtensionName(NewExtensionId, out _, out _);
                if (extensionName == null)
                    throw new ArgumentException("Extension cannot be found.", nameof(NewExtensionId));
            }

            if (File != null)
                Id = File.Id;

            var connection = DatabaseConnection.Current;

            if (!SkipFile.IsPresent)
            {
                var fileName = FileHelper.GetFileName(Id, out var folderPath, out var path, out _, out var isFolderSealed, out var folderId);
                if (fileName == null)
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException("File cannot be found.", nameof(Id)), "ImageStore Rename File", ErrorCategory.InvalidArgument, null));
                }
                if (isFolderSealed && !OverrideSealedFolder.IsPresent)
                {
                    ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Folder containing this file is set to sealed and -" + nameof(OverrideSealedFolder) + " is not present."), "ImageStore Rename File", ErrorCategory.SecurityError, folderId));
                }
                if (System.IO.File.Exists(fileName))
                {
                    string newName;
                    if (path != "") newName = folderPath + path + DirectorySeparatorString.Value + NewFileName + "." + extensionName;
                    else newName = folderPath + NewFileName + "." + extensionName;
                    System.IO.File.Move(fileName, newName);
                    WriteInformation("File is renamed from " + fileName + " to " + newName + ".", new string[] { "RenameFile" });
                }
                else
                {
                    WriteWarning("File to be renaming cannot be found. Path: " + fileName);
                }
            }

            using (var command = new SqlCommand("Update [File] Set [FileName]=@FileName, [ExtensionId]=@ExtensionId where [Id]=@Id"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });
                command.Parameters.Add(new SqlParameter("@FileName", System.Data.SqlDbType.NVarChar, 256) { Value = NewFileName });
                command.Parameters.Add(new SqlParameter("@ExtensionId", System.Data.SqlDbType.UniqueIdentifier) { Value = NewExtensionId });

                if (command.ExecuteNonQuery() > 0)
                {
                    if (SkipReturn.IsPresent)
                    {
                        WriteObject(null);
                    }
                    else
                    {
                        var newFile = GetFileCmdlet.GetFile(Id);
                        WriteObject(newFile);
                    }
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot rename this file."),
                        "ImageStore Rename File", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
