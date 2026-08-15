using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    static class FileHelper
    {
        internal static string GetFullFilePath(string folder, string path, string fileName, string extension)
        {
            if (path == "") return folder + fileName + "." + extension;
            else return folder + path + DirectorySeparatorString.Value + fileName + "." + extension;
        }

        internal static string GetFileName(Guid fileId, out string folderPath, out string path, out string fileNameWithoutPath, out bool isFolderSealed, out Guid folderId)
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqliteCommand("Select [Folder].[Path],[File].[Path],[File].[FileName],[Extension].[Extension],[Folder].[IsSealed],[File].[FolderId] from [File] inner join [Folder] on [File].[FolderId]=[Folder].[Id] inner join [Extension] on [File].[ExtensionId]=[Extension].[Id] where [File].[Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", fileId);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    string result;
                    if (reader.Read())
                    {
                        folderPath = reader.GetString(0);
                        path = reader.GetString(1);
                        fileNameWithoutPath = reader.GetString(2) + "." + reader.GetString(3);
                        isFolderSealed = reader.GetBoolean(4);
                        folderId = reader.GetGuid(5);
                        if (!folderPath.EndsWith(DirectorySeparatorString.Value))
                            folderPath += DirectorySeparatorString.Value;
                        if (path == "") result = folderPath + fileNameWithoutPath;
                        else result = folderPath + path + DirectorySeparatorString.Value + fileNameWithoutPath;
                    }
                    else
                    {
                        folderPath = null;
                        path = null;
                        fileNameWithoutPath = null;
                        result = null;
                        isFolderSealed = false;
                        folderId = Guid.Empty;
                    }
                    reader.Close();
                    return result;
                }
            }
        }

        internal static IEnumerable<ImageStoreFile> GetAllFilesWithoutData(Guid folderId, int? top)
        {
            var connection = DatabaseConnection.Current;
            var text = " [Id],[Path],[FileName],[ExtensionId] from [File] where [FolderId]=@FolderId order by [Path],[FileName],[ExtensionId]";
            using (var command = new SqliteCommand())
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                //LIMIT goes last, after the order by already inside text.
                command.CommandText = "SELECT" + text;
                if (top != null)
                    command.CommandText += " limit " + top.Value.ToString();
                command.Parameters.AddGuid("@FolderId", folderId);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while(reader.Read())
                    {
                        ImageStoreFile line = new ImageStoreFile(reader.GetGuid(0), folderId, reader.GetString(1), reader.GetString(2), reader.GetGuid(3));
                        yield return line;
                    }
                    reader.Close();
                }
            }
        }

        internal static IEnumerable<ImageStoreFile> GetAllFilesWithoutData(Guid folderId, int? top, 
            bool onlyNew, bool includingComputed, bool includingNotImage, bool includingNotReadable, bool includingSizeZero)
        {
            var connection = DatabaseConnection.Current;
            string text = "SELECT";

            text += " [Id],[Path],[FileName],[ExtensionId],[FileState] from [File] where [FolderId]=@FolderId and ";

            if (onlyNew)
            {
                text += "[FileState]=0";
            }
            else
            {
                List<string> states = new List<string>();

                if (includingComputed)
                    states.Add("[FileState]=255");
                if (includingNotImage)
                    states.Add("[FileState]=1");
                if (includingNotReadable)
                    states.Add("[FileState]=2");
                if (includingSizeZero)
                    states.Add("[FileState]=254");

                if (states.Count == 1)
                    text += states[0];
                else
                    text += "(" + string.Join(" or ", states) + ")";
            }

            text += " order by [Path],[FileName],[ExtensionId]";

            //LIMIT goes last, after the where and order by clauses built above.
            if (top != null)
                text += " limit " + top.Value.ToString();

            using (var command = new SqliteCommand(text))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;

                command.Parameters.AddGuid("@FolderId", folderId);

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreFile line = new ImageStoreFile(reader.GetGuid(0), folderId, reader.GetString(1), reader.GetString(2), reader.GetGuid(3))
                        {
                            FileStateCode = reader.GetInt32(4)
                        };
                        yield return line;
                    }
                    reader.Close();
                }
            }
        }

        internal static bool Delete(Guid id)
        {
            var connection = DatabaseConnection.Current;
            using (var commandFile = new SqliteCommand("Delete from [File] where [Id]=@Id"))
            using (var commandSimilar = new SqliteCommand("Delete from [SimilarFile] where [File1Id]=@Id or [File2Id]=@Id"))
            using (var transation = connection.BeginTransaction())
            {
                commandFile.Connection = connection;
                commandFile.CommandTimeout = 0;
                commandFile.Transaction = transation;
                commandFile.Parameters.AddGuid("@Id", id);
                commandSimilar.Connection = connection;
                commandSimilar.CommandTimeout = 0;
                commandSimilar.Transaction = transation;
                commandSimilar.Parameters.AddGuid("@Id", id);

                commandSimilar.ExecuteNonQuery();

                if (commandFile.ExecuteNonQuery() == 0)
                {
                    transation.Rollback();
                    return false;
                }

                transation.Commit();
                SimilarFile.LoadImageHelper.RemoveCache(id);
                return true;
            }
        }

        internal static void Delete(IEnumerable<Tuple<Guid, Action, Action>> operations)
        {
            var connection = DatabaseConnection.Current;
            using (var commandFile = new SqliteCommand("Delete from [File] where [Id]=@Id"))
            using (var commandSimilar = new SqliteCommand("Delete from [SimilarFile] where [File1Id]=@Id or [File2Id]=@Id"))
            using (var transation = connection.BeginTransaction())
            {
                commandFile.Connection = connection;
                commandFile.CommandTimeout = 0;
                commandFile.Transaction = transation;
                commandFile.Parameters.Add(new SqliteParameter("@Id", SqliteType.Blob));
                commandSimilar.Connection = connection;
                commandSimilar.CommandTimeout = 0;
                commandSimilar.Transaction = transation;
                commandSimilar.Parameters.Add(new SqliteParameter("@Id", SqliteType.Blob));

                foreach(var operation in operations)
                {

                    commandFile.Parameters[0].Value = operation.Item1.ToByteArray();
                    commandSimilar.Parameters[0].Value = operation.Item1.ToByteArray();

                    commandSimilar.ExecuteNonQuery();
                    if (commandFile.ExecuteNonQuery() == 0)
                    {
                        operation.Item3();
                    }
                    else
                    {
                        SimilarFile.LoadImageHelper.RemoveCache(operation.Item1);

                        operation.Item2();
                    }
                }

                transation.Commit();
            }
        }

        internal static int Delete(Guid folderId, string pathStart)
        {
            int result;

            var connection = DatabaseConnection.Current;
            using (var commandCreateTable = new SqliteCommand("Create temp table tempFileId ([Id] BLOB)"))
            using (var commandSelect = new SqliteCommand("insert into tempFileId select [Id] from [File] where [FolderId]=@FolderId and "))
            using (var commandDeleteSimilar = new SqliteCommand("Delete from [SimilarFile] where [File1Id] in (select [Id] from tempFileId) or [File2Id] in (select [Id] from tempFileId)"))
            using (var commandDeleteFile = new SqliteCommand("Delete from [File] where [Id] in (select [Id] from tempFileId)"))
            using (var commandDropTable = new SqliteCommand("Drop Table tempFileId"))
            using (var transation = connection.BeginTransaction())
            {
                commandCreateTable.Connection = connection;
                commandCreateTable.CommandTimeout = 0;
                commandCreateTable.Transaction = transation;
                commandCreateTable.ExecuteNonQuery();

                commandSelect.Connection = connection;
                commandSelect.CommandTimeout = 0;
                commandSelect.Transaction = transation;
                commandSelect.Parameters.AddGuid("@FolderId", folderId);
                if (pathStart == "")
                {
                    commandSelect.CommandText += "[Path] = ''";
                }
                else
                {
                    //The escape clause belongs to the LIKE, not the equality test.
                    commandSelect.CommandText += "([Path] = @Path or [Path] like @PathStart"
                        + SqliteLikeValueBuilder.EscapeClause + ")";
                    commandSelect.Parameters.AddText("@Path", pathStart);
                    commandSelect.Parameters.AddText("@PathStart", SqliteLikeValueBuilder.Escape(pathStart + DirectorySeparatorString.Value) + "%");
                }

                if (commandSelect.ExecuteNonQuery() != 0)
                {
                    commandDeleteSimilar.Connection = connection;
                    commandDeleteSimilar.CommandTimeout = 0;
                    commandDeleteSimilar.Transaction = transation;
                    commandDeleteSimilar.ExecuteNonQuery();
                    commandDeleteFile.Connection = connection;
                    commandDeleteFile.CommandTimeout = 0;
                    commandDeleteFile.Transaction = transation;
                    result = commandDeleteFile.ExecuteNonQuery();

                    if (SimilarFile.LoadImageHelper.cachePath != null)
                    {
                        using (var commandReadId = new SqliteCommand("Select [Id] from tempFileId"))
                        {
                            commandReadId.Connection = connection;
                            commandReadId.CommandTimeout = 0;
                            commandReadId.Transaction = transation;
                            using (var reader = commandReadId.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                            {
                                while (reader.Read())
                                    SimilarFile.LoadImageHelper.RemoveCache(reader.GetGuid(0));
                                reader.Close();
                            }
                        }
                    }
                }
                else
                    result = 0;

                commandDropTable.Connection = connection;
                commandDropTable.CommandTimeout = 0;
                commandDropTable.Transaction = transation;
                commandDropTable.ExecuteNonQuery();

                transation.Commit();
            }

            return result;
        }


        internal static void Delete(Guid folderId, IEnumerable<Tuple<string, Action, Action>> operations)
        {
            var connection = DatabaseConnection.Current;
            using (var commandCreateTable = new SqliteCommand("Create temp table tempFileId ([Id] BLOB)"))
            using (var commandSelect = new SqliteCommand("insert into tempFileId select [Id] from [File] where [FolderId]=@FolderId and [Path] = @Path"))
            using (var commandDeleteSimilar = new SqliteCommand("Delete from [SimilarFile] where [File1Id] in (select [Id] from tempFileId) or [File2Id] in (select [Id] from tempFileId)"))
            using (var commandDeleteFile = new SqliteCommand("Delete from [File] where [Id] in (select [Id] from tempFileId)"))
            using (var commandDropTable = new SqliteCommand("Drop Table tempFileId"))
            using (var transation = connection.BeginTransaction())
            {
                commandCreateTable.Connection = connection;
                commandCreateTable.CommandTimeout = 0;
                commandCreateTable.Transaction = transation;
                commandSelect.Connection = connection;
                commandSelect.CommandTimeout = 0;
                commandSelect.Parameters.AddGuid("@FolderId", folderId);
                commandSelect.Parameters.Add(new SqliteParameter("@Path", SqliteType.Text));
                commandSelect.Transaction = transation;
                commandDeleteSimilar.Connection = connection;
                commandDeleteSimilar.CommandTimeout = 0;
                commandDeleteSimilar.Transaction = transation;
                commandDeleteFile.Connection = connection;
                commandDeleteFile.CommandTimeout = 0;
                commandDeleteFile.Transaction = transation;
                commandDropTable.Connection = connection;
                commandDropTable.CommandTimeout = 0;
                commandDropTable.Transaction = transation;
                foreach (var operation in operations)
                {
                    commandCreateTable.ExecuteNonQuery();

                    commandSelect.Parameters[1].Value = operation.Item1;

                    if (commandSelect.ExecuteNonQuery() != 0)
                    {
                        bool result;

                        commandDeleteSimilar.ExecuteNonQuery();

                        result = commandDeleteFile.ExecuteNonQuery() != 0;

                        if (SimilarFile.LoadImageHelper.cachePath != null)
                        {
                            using (var commandReadId = new SqliteCommand("Select [Id] from tempFileId"))
                            {
                                commandReadId.Connection = connection;
                                commandReadId.CommandTimeout = 0;
                                commandReadId.Transaction = transation;
                                using (var reader = commandReadId.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                                {
                                    while (reader.Read())
                                        SimilarFile.LoadImageHelper.RemoveCache(reader.GetGuid(0));
                                    reader.Close();
                                }
                            }
                        }
                        
                        if (result)
                            operation.Item2();
                        else
                            operation.Item3();
                    }
                    else
                        operation.Item3();

                    commandDropTable.ExecuteNonQuery();

                }

                transation.Commit();
            }
        }
    }
}
