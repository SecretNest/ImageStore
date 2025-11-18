using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

            using (var command = new SqlCommand("Update [File] Set [ImageHash]=@ImageHash, [Sha1Hash]=@Sha1Hash, [FileSize]=@FileSize, [FileState]=@FileState, [ImageComparedThreshold]=@ImageComparedThreshold where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = file.Id });
                command.Parameters.Add(new SqlParameter("@ImageHash", System.Data.SqlDbType.Binary, 40) { Value = DBNullableReader.NullCheck(file.ImageHash) });
                command.Parameters.Add(new SqlParameter("@Sha1Hash", System.Data.SqlDbType.Binary, 20) { Value = DBNullableReader.NullCheck(file.Sha1Hash) });
                command.Parameters.Add(new SqlParameter("@FileSize", System.Data.SqlDbType.Int) { Value = file.FileSize });
                command.Parameters.Add(new SqlParameter("@FileState", System.Data.SqlDbType.Int) { Value = file.FileStateCode });
                command.Parameters.Add(new SqlParameter("@ImageComparedThreshold", System.Data.SqlDbType.Real) { Value = file.ImageComparedThreshold });

                return command.ExecuteNonQuery();
            }
        }
    }
}
