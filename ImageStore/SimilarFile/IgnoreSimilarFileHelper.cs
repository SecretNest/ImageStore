using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    static class IgnoreSimilarFileHelper
    {
        internal static bool MarkIgnore(Guid similarFileId, IgnoredMode ignoredMode)
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqliteCommand("Update [SimilarFile] set [IgnoredMode]=@IgnoredMode where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddInt("@IgnoredMode", (int)ignoredMode);
                command.Parameters.AddGuid("@Id", similarFileId);

                return (command.ExecuteNonQuery() == 1);
            }

        }
    }
}
