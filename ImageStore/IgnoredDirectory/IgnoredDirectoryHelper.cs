using SecretNest.ImageStore.IgnoredDirectory;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.IgnoredDirectory
{
    static class IgnoredDirectoryHelper
    {
        internal static IEnumerable<ImageStoreIgnoredDirectory> GetAllIgnoredDirectories(Guid folderId)
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [Id],[Directory],[IsSubDirectoryIncluded] from [IgnoredDirectory] Where [FolderId]=@FolderId"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = folderId });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreIgnoredDirectory line = new ImageStoreIgnoredDirectory((Guid)reader[0])
                        {
                            FolderId = folderId,
                            Directory = (string)reader[1],
                            IsSubDirectoryIncluded = (bool)reader[2]
                        };
                        yield return line;
                    }
                    reader.Close();
                }
            }
        }
    }
}
