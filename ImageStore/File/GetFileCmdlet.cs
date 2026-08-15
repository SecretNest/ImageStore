using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsCommon.Get, "ImageStoreFile")]
    [Alias("GetFile")]
    [OutputType(typeof(ImageStoreFile))]
    public class GetFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(GetFile(Id));
        }


        internal static ImageStoreFile GetFile(Guid id)
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqliteCommand("Select [FolderId],[Path],[FileName],[ExtensionId],[ImageHash],[Sha1Hash],[FileSize],[FileState],[ImageComparedThreshold] from [File] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", id);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    ImageStoreFile result;
                    if (reader.Read())
                    {
                        result = new ImageStoreFile(id, reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetGuid(3))
                        {
                            ImageHash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[4]),
                            Sha1Hash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[5]),
                            FileSize = reader.GetInt32(6),
                            FileStateCode = reader.GetInt32(7),
                            ImageComparedThreshold = reader.GetFloat(8)
                        };
                    }
                    else
                    {
                        result = null;
                    }
                    reader.Close();
                    return result;
                }
            }
        }
    }
}
