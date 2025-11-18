using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            using (var command = new SqlCommand("Update [SimilarFile] set [IgnoredMode]=@IgnoredMode where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@IgnoredMode", System.Data.SqlDbType.Int) { Value = (int)ignoredMode });
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = similarFileId });

                return (command.ExecuteNonQuery() == 1);
            }

        }
    }
}
