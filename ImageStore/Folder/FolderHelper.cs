using SecretNest.ImageStore.Folder;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    static class FolderHelper
    {
        internal static IEnumerable<ImageStoreFolder> GetAllFolders()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqliteCommand("Select [Id],[Path],[Name],[CompareImageWith],[IsSealed] from [Folder]"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreFolder line = new ImageStoreFolder(reader.GetGuid(0), reader.GetString(1))
                        {
                            Name = reader.GetString(2),
                            CompareImageWithCode = reader.GetInt32(3),
                            IsSealed = reader.GetBoolean(4)
                        };
                        yield return line;
                    }
                    reader.Close();
                }
            }
        }


        internal static string GetFolderPath(Guid id, out bool isSealed)
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqliteCommand("Select [Path],[IsSealed] from [Folder] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", id);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    string result;
                    if (reader.Read())
                    {
                        result = reader.GetString(0);
                        isSealed = reader.GetBoolean(1);
                    }
                    else
                    {
                        result = null;
                        isSealed = false;
                    }
                    reader.Close();
                    return result;
                }
            }
        }
    }
}
