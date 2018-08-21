using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsCommon.Find, "ImageStoreFile")]
    [Alias("FindFile")]
    [OutputType(typeof(ImageStoreFile))]
    public class FindFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
        public Guid FolderId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, Mandatory = true)]
        [AllowEmptyString()] public string Path { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, Mandatory = true)]
        [AllowEmptyString()] public string FileName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3, Mandatory = true)]
        public Guid ExtensionId { get; set; }

        protected override void ProcessRecord()
        {
            if (Path == null)
                throw new ArgumentNullException(nameof(Path));

            if (FileName == null)
                throw new ArgumentNullException(nameof(FileName));

            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [Id],[Path],[FileName],[ImageHash],[Sha1Hash],[FileSize],[FileState],[ImageComparedThreshold] from [File] Where [FolderId]=@FolderId and [Path]=@Path and [FileName]=@FileName and [ExtensionId]=@ExtensionId"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = FolderId });
                command.Parameters.Add(new SqlParameter("@Path", System.Data.SqlDbType.NVarChar, 256) { Value = Path });
                command.Parameters.Add(new SqlParameter("@FileName", System.Data.SqlDbType.NVarChar, 256) { Value = FileName });
                command.Parameters.Add(new SqlParameter("@ExtensionId", System.Data.SqlDbType.UniqueIdentifier) { Value = ExtensionId });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreFile line = new ImageStoreFile((Guid)reader[0], FolderId, (string)reader[1], (string)reader[2], ExtensionId)
                        {
                            ImageHash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[3]),
                            Sha1Hash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[4]),
                            FileSize = (int)reader[5],
                            FileStateCode = (int)reader[6],
                            ImageComparedThreshold = (float)reader[7]
                        };
                        WriteObject(line);
                    }
                    else
                    {
                        WriteObject(null);
                    }
                    reader.Close();
                }
            }
        }
    }
}
