using SecretNest.ImageStore.IgnoredDirectory;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
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
            using (var command = new SqliteCommand("Select [Id],[Directory],[IsSubDirectoryIncluded] from [IgnoredDirectory] Where [FolderId]=@FolderId"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@FolderId", folderId);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreIgnoredDirectory line = new ImageStoreIgnoredDirectory(reader.GetGuid(0))
                        {
                            FolderId = folderId,
                            Directory = reader.GetString(1),
                            IsSubDirectoryIncluded = reader.GetBoolean(2)
                        };
                        yield return line;
                    }
                    reader.Close();
                }
            }
        }
    }
}
