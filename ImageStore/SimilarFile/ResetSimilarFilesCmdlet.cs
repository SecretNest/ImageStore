using SecretNest.ImageStore.Folder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsCommon.Reset, "ImageStoreSimilarFiles")]
    [Alias("ResetSimilarFiles")]
    public class ResetSimilarFilesCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("DELETE FROM [SimilarFile]"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;

                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("Update [File] Set [ImageComparedThreshold]=0"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;

                command.ExecuteNonQuery();
            }
        }

    }
}
