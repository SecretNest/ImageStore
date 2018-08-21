using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            using (var command = new SqlCommand("Select [FolderId],[Path],[FileName],[ExtensionId],[ImageHash],[Sha1Hash],[FileSize],[FileState],[ImageComparedThreshold] from [File] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = id });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    ImageStoreFile result;
                    if (reader.Read())
                    {
                        result = new ImageStoreFile(id, (Guid)reader[0], (string)reader[1], (string)reader[2], (Guid)reader[3])
                        {
                            ImageHash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[4]),
                            Sha1Hash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[5]),
                            FileSize = (int)reader[6],
                            FileStateCode = (int)reader[7],
                            ImageComparedThreshold = (float)reader[8]
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
