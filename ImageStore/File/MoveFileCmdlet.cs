using SecretNest.ImageStore.Folder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsCommon.Move, "ImageStoreFile")]
    [Alias("MoveFile")]
    [OutputType(typeof(ImageStoreFile))]
    public class MoveFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "EntityEntity", Mandatory = true)]
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "EntityId", Mandatory = true)]
        public ImageStoreFile File { get; set; } = null;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "IdId", Mandatory = true)]
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "IdEntity", Mandatory = true)]
        public Guid Id { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, ParameterSetName = "EntityId", Mandatory = true)]
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, ParameterSetName = "IdId", Mandatory = true)]
        public Guid NewFolderId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, ParameterSetName = "EntityEntity", Mandatory = true)]
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, ParameterSetName = "IdEntity", Mandatory = true)]
        public ImageStoreFolder NewFolder { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        [AllowEmptyString()][AllowNull] public string NewPath { get; set; }


        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public SwitchParameter SkipFile { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 4)]
        public SwitchParameter SkipReturn { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 5)]
        public SwitchParameter OverrideSealedFolder { get; set; }

        protected override void ProcessRecord()
        {

            if (NewFolder != null)
                NewFolderId = NewFolder.Id;
            var newFolderPath = FolderHelper.GetFolderPath(NewFolderId, out bool isFolderSealed);
            if (newFolderPath == null)
                throw new ArgumentException("Folder cannot be found.", nameof(newFolderPath));

            if (File != null)
                Id = File.Id;

            if (!OverrideSealedFolder.IsPresent && isFolderSealed)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Target folder is set to sealed and -" + nameof(OverrideSealedFolder) + " is not present."), "ImageStore Move File", ErrorCategory.SecurityError, NewFolderId));
            }



            var connection = DatabaseConnection.Current;

            var fileName = FileHelper.GetFileName(Id, out _, out string oldPath, out var fileNameWithoutPath, out isFolderSealed, out _);
            if (!SkipFile.IsPresent)
            {
                if (NewPath == null)
                    NewPath = oldPath;
                if (fileName == null)
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException("File cannot be found.", nameof(Id)), "ImageStore Move File", ErrorCategory.InvalidArgument, null));
                }
                if (!OverrideSealedFolder.IsPresent && isFolderSealed)
                {
                    ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Folder containing this file is set to sealed and -" + nameof(OverrideSealedFolder) + " is not present."), "ImageStore Move File", ErrorCategory.SecurityError, File.FolderId));
                }
                if (System.IO.File.Exists(fileName))
                {
                    string newName;
                    if (NewPath == "") newName = newFolderPath + DirectorySeparatorString.Value + fileNameWithoutPath;
                    else
                    {
                        var newDirectory = newFolderPath + DirectorySeparatorString.Value + NewPath;
                        newName = newDirectory + DirectorySeparatorString.Value + fileNameWithoutPath;
                        System.IO.Directory.CreateDirectory(newDirectory);
                    }

                    System.IO.File.Move(fileName, newName);
                    WriteInformation("File is moved from " + fileName + " to " + newName + ".", new string[] { "MoveFile" });
                }
                else
                {
                    WriteWarning("File to be renaming cannot be found. Path: " + fileName);
                }
            }

            using (var command = new SqlCommand("Update [File] Set [FolderId]=@FolderId, [Path]=@Path where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });
                command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = NewFolderId });
                command.Parameters.Add(new SqlParameter("@Path", System.Data.SqlDbType.NVarChar, 256) { Value = NewPath });

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
                        new InvalidOperationException("Cannot move this file."),
                        "ImageStore Move File", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
