using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
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

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, Mandatory = true, ValueFromPipeline = true)]
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
            using (var command = new SqliteCommand("Select [Id],[Path],[FileName],[ImageHash],[Sha1Hash],[FileSize],[FileState],[ImageComparedThreshold] from [File] Where [FolderId]=@FolderId and [Path]=@Path and [FileName]=@FileName and [ExtensionId]=@ExtensionId"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@FolderId", FolderId);
                command.Parameters.AddText("@Path", Path);
                command.Parameters.AddText("@FileName", FileName);
                command.Parameters.AddGuid("@ExtensionId", ExtensionId);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        ImageStoreFile line = new ImageStoreFile(reader.GetGuid(0), FolderId, reader.GetString(1), reader.GetString(2), ExtensionId)
                        {
                            ImageHash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[3]),
                            Sha1Hash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[4]),
                            FileSize = reader.GetInt32(5),
                            FileStateCode = reader.GetInt32(6),
                            ImageComparedThreshold = reader.GetFloat(7)
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
