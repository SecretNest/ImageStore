using SecretNest.ImageStore.Extension;
using SecretNest.ImageStore.Folder;
using Shipwreck.Phash;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsDiagnostic.Measure, "ImageStoreFile")]
    [Alias("MeasureFile")]
    [OutputType(typeof(ImageStoreFile))]
    public class MeasureFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public ImageStoreFile File { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public SwitchParameter Recompute { get; set; }


        protected override void ProcessRecord()
        {
            if (File == null)
                throw new ArgumentNullException(nameof(File));
            
            if (File.FileState != FileState.New && !Recompute.IsPresent)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("State of this file is not set to New and -" + nameof(Recompute) + " is not set."), "ImageStore Measure File", ErrorCategory.InvalidOperation, File.Id));
            }

            var extension = ExtensionHelper.GetExtensionName(File.ExtensionId, out var extensionIsImage, out var extensionIgnored);
            if (extension == null)
                throw new ArgumentException("Extension cannot be found.", nameof(File));

            if (extensionIgnored)
            {
                throw new ArgumentException("Extension of this file is set to ignored.", nameof(File));
            }

            var folder = FolderHelper.GetFolderPath(File.FolderId, out _);
            if (folder == null)
                throw new ArgumentException("Folder cannot be found.", nameof(File));
            if (!folder.EndsWith(DirectorySeparatorString.Value))
                folder += DirectorySeparatorString.Value;

            Digest imageHash;
            byte[] sha1Hash;
            int fileSize;
            FileState fileState;

            using (var computer = new MeasureFileHelper())
            {
                var fullFilePath = MeasureFileHelper.GetFullFilePath(folder, File.Path, File.FileName, extension);
                var ex = computer.ProcessFile(fullFilePath, extensionIsImage, out imageHash, out sha1Hash, out fileSize, out fileState);
                if (ex != null)
                {
                    WriteError(ex);
                }
            }

            var newFile = new ImageStoreFile(File.Id, File.FolderId, File.Path, File.FileName, File.ExtensionId)
            {
                ImageHashDigest = imageHash,
                Sha1Hash = sha1Hash,
                FileSize = fileSize,
                FileState = fileState,
                ImageComparedThreshold = 0
            };

            if (UpdateFileCmdlet.UpdateRecord(newFile) <= 0)
            {
                WriteError(new ErrorRecord(new InvalidOperationException("Cannot update this file."), "ImageStore - Measuring", ErrorCategory.WriteError, null));
            }

            var connection = DatabaseConnection.Current;
            using (var commandToDelete = new SqlCommand("Delete from [SimilarFile] Where [File1Id]=@Id or [File2Id]=@Id"))
            {
                commandToDelete.Connection = connection;
                commandToDelete.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = File.Id });
                commandToDelete.ExecuteNonQuery();
            }
            
            WriteObject(newFile);
        }
    }
}
