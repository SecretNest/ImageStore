using SecretNest.ImageStore.Extension;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
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
            using (var command = new SqliteCommand("Select [Id],[Extension],[IsImage],[Ignored] from [Extension]"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreExtension line = new ImageStoreExtension(reader.GetGuid(0))
                        {
                            Extension = reader.GetString(1),
                            IsImage = reader.GetBoolean(2),
                            Ignored = reader.GetBoolean(3)
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
            using (var command = new SqliteCommand("Select [Extension],[IsImage],[Ignored] from [Extension] Where [Id]=@Id"))
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
                        isImage = reader.GetBoolean(1);
                        ignored = reader.GetBoolean(2);
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