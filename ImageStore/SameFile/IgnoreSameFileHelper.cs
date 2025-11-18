using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            using (var command = new SqlCommand("Update [SameFile] set [IsIgnored]=@IsIgnored where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@IsIgnored", System.Data.SqlDbType.Bit) { Value = state });
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = sameFileId });

                return (command.ExecuteNonQuery() == 1);
            }

        }
    }
}
