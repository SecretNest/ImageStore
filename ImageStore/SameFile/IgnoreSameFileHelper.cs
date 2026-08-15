using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    static class IgnoreSameFileHelper
    {
        internal static bool MarkIgnore(Guid sameFileId, bool state)
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqliteCommand("Update [SameFile] set [IsIgnored]=@IsIgnored where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddBool("@IsIgnored", state);
                command.Parameters.AddGuid("@Id", sameFileId);

                return (command.ExecuteNonQuery() == 1);
            }

        }
    }
}
