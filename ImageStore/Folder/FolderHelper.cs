using SecretNest.ImageStore.Folder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

            using (var command = new SqlCommand("Select [Id],[Path],[Name],[CompareImageWith],[IsSealed] from [Folder]"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreFolder line = new ImageStoreFolder((Guid)reader[0], (string)reader[1])
                        {
                            Name = (string)reader[2],
                            CompareImageWithCode = (int)reader[3],
                            IsSealed = (bool)reader[4]
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
            using (var command = new SqlCommand("Select [Path],[IsSealed] from [Folder] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = id });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    string result;
                    if (reader.Read())
                    {
                        result = (string)reader[0];
                        isSealed = (bool)reader[1];
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
