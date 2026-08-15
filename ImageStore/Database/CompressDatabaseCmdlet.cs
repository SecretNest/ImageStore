using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
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

            //VACUUM rebuilds the database file, reclaiming pages freed by deletes.
            //It cannot run inside a transaction and needs free disk space roughly
            //equal to the database size while it works.
            using (SqliteCommand command = new SqliteCommand("VACUUM"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }

            //Refresh the query statistics while we are here. Compacting usually
            //follows a large removal, which is exactly when the planner's idea of
            //table sizes has gone stale - and stale statistics make it stop using
            //indexes rather than merely pick a worse one.
            using (SqliteCommand command = new SqliteCommand("ANALYZE"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }

        }
    }
}
