using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Database
{
    [Cmdlet(VerbsData.Compress, "ImageStoreDatabase")]
    [Alias("ShrinkDatabase", "CompressDatabase", "Shrink-ImageStoreDatabase")]
    public class CompressDatabaseCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;

            using (SqlCommand command = new SqlCommand("DECLARE @dbName VARCHAR(500); SELECT @dbName = DB_NAME(); DBCC SHRINKDATABASE(@dbName)"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }

        }
    }
}
