using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsData.Update, "ImageStoreFile")]
    [Alias("UpdateFile")]
    public class UpdateFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public ImageStoreFile File { get; set; }

        protected override void ProcessRecord()
        {
            if (File == null)
                throw new ArgumentNullException(nameof(File));

            if (UpdateRecord(File) == 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot update this file."),
                    "ImageStore Update File", ErrorCategory.WriteError, null));
            }
        }

        internal static int UpdateRecord(ImageStoreFile file)
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqliteCommand("Update [File] Set [ImageHash]=@ImageHash, [Sha1Hash]=@Sha1Hash, [FileSize]=@FileSize, [FileState]=@FileState, [ImageComparedThreshold]=@ImageComparedThreshold where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", file.Id);
                command.Parameters.AddBlob("@ImageHash", file.ImageHash);
                command.Parameters.AddBlob("@Sha1Hash", file.Sha1Hash);
                command.Parameters.AddInt("@FileSize", file.FileSize);
                command.Parameters.AddInt("@FileState", file.FileStateCode);
                command.Parameters.AddReal("@ImageComparedThreshold", file.ImageComparedThreshold);

                return command.ExecuteNonQuery();
            }
        }
    }
}
