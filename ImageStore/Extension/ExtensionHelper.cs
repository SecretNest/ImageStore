using SecretNest.ImageStore.Extension;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Extension
{
    static class ExtensionHelper
    {
        internal static IEnumerable<ImageStoreExtension> GetAllExtensions()
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [Id],[Extension],[IsImage],[Ignored] from [Extension]"))
            {
                command.Connection = connection;
                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreExtension line = new ImageStoreExtension((Guid)reader[0])
                        {
                            Extension = (string)reader[1],
                            IsImage = (bool)reader[2],
                            Ignored = (bool)reader[3]
                        };
                        yield return line;
                    }
                    reader.Close();
                }
            }
        }

        internal static string GetExtensionName(Guid id, out bool isImage, out bool ignored)
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [Extension],[IsImage],[Ignored] from [Extension] Where [Id]=@Id"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = id });
                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    string result;
                    if (reader.Read())
                    {
                        result = (string)reader[0];
                        isImage = (bool)reader[1];
                        ignored = (bool)reader[2];
                    }
                    else
                    {
                        isImage = false;
                        ignored = true;
                        result = null;
                    }
                    return result;
                }
            }
        }
    }
}