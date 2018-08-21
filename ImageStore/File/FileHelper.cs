using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    static class FileHelper
    {
        internal static string GetFileName(Guid fileId, out string folderPath, out string path, out string fileNameWithoutPath, out bool isFolderSealed, out Guid folderId)
        {
            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("Select [Folder].[Path],[File].[Path],[File].[FileName],[Extension].[Extension],[Folder].[IsSealed],[File].[FolderId] from [File] inner join [Folder] on [File].[FolderId]=[Folder].[Id] inner join [Extension] on [File].[ExtensionId]=[Extension].[Id] where [File].[Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = fileId });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    string result;
                    if (reader.Read())
                    {
                        folderPath = (string)reader[0];
                        path = (string)reader[1];
                        fileNameWithoutPath = (string)reader[2] + "." + (string)reader[3];
                        isFolderSealed = (bool)reader[4];
                        folderId = (Guid)reader[5];
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
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                if (top != null)
                    command.CommandText = "SELECT TOP " + top.Value.ToString() + text;
                else
                    command.CommandText = "SELECT" + text;
                command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = folderId });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while(reader.Read())
                    {
                        ImageStoreFile line = new ImageStoreFile((Guid)reader[0], folderId, (string)reader[1], (string)reader[2], (Guid)reader[3]);
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
            string text;
            if (top != null)
                text = "SELECT TOP " + top.Value.ToString();
            else
                text = "SELECT";

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

            using (var command = new SqlCommand(text))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;

                command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = folderId });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreFile line = new ImageStoreFile((Guid)reader[0], folderId, (string)reader[1], (string)reader[2], (Guid)reader[3])
                        {
                            FileStateCode = (int)reader[4]
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
            using (var commandFile = new SqlCommand("Delete from [File] where [Id]=@Id"))
            using (var commandSimilar = new SqlCommand("Delete from [SimilarFile] where [File1Id]=@Id or [File2Id]=@Id"))
            using (var transation = connection.BeginTransaction())
            {
                commandFile.Connection = connection;
                commandFile.Transaction = transation;
                commandFile.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = id });
                commandSimilar.Connection = connection;
                commandSimilar.Transaction = transation;
                commandSimilar.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = id });

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
            using (var commandFile = new SqlCommand("Delete from [File] where [Id]=@Id"))
            using (var commandSimilar = new SqlCommand("Delete from [SimilarFile] where [File1Id]=@Id or [File2Id]=@Id"))
            using (var transation = connection.BeginTransaction())
            {
                commandFile.Connection = connection;
                commandFile.Transaction = transation;
                commandFile.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                commandSimilar.Connection = connection;
                commandSimilar.Transaction = transation;
                commandSimilar.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));

                foreach(var operation in operations)
                {

                    commandFile.Parameters[0].Value = operation.Item1;
                    commandSimilar.Parameters[0].Value = operation.Item1;

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
            using (var commandCreateTable = new SqlCommand("Create Table #tempFileId ([Id] uniqueidentifier)"))
            using (var commandSelect = new SqlCommand("insert into #tempFileId select [Id] from [File] where [FolderId]=@FolderId and "))
            using (var commandDeleteSimilar = new SqlCommand("Delete from [SimilarFile] where [File1Id] in (select [Id] from #tempFileId) or [File2Id] in (select [Id] from #tempFileId)"))
            using (var commandDeleteFile = new SqlCommand("Delete from [File] where [Id] in (select [Id] from #tempFileId)"))
            using (var commandDropTable = new SqlCommand("Drop Table #tempFileId"))
            using (var transation = connection.BeginTransaction())
            {
                commandCreateTable.Connection = connection;
                commandCreateTable.Transaction = transation;
                commandCreateTable.ExecuteNonQuery();

                commandSelect.Connection = connection;
                commandSelect.Transaction = transation;
                commandSelect.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = folderId });
                if (pathStart == "")
                {
                    commandSelect.CommandText += "[Path] = ''";
                }
                else
                {
                    commandSelect.CommandText += "([Path] = @Path or [Path] like @PathStart)";
                    commandSelect.Parameters.Add(new SqlParameter("@Path", System.Data.SqlDbType.NVarChar, 256) { Value = pathStart });
                    commandSelect.Parameters.Add(new SqlParameter("@PathStart", System.Data.SqlDbType.NVarChar, 769) { Value = SqlServerLikeValueBuilder.Escape(pathStart + DirectorySeparatorString.Value) + "%" });
                }

                if (commandSelect.ExecuteNonQuery() != 0)
                {
                    commandDeleteSimilar.Connection = connection;
                    commandDeleteSimilar.Transaction = transation;
                    commandDeleteSimilar.ExecuteNonQuery();
                    commandDeleteFile.Connection = connection;
                    commandDeleteFile.Transaction = transation;
                    result = commandDeleteFile.ExecuteNonQuery();

                    if (SimilarFile.LoadImageHelper.cachePath != null)
                    {
                        using (var commandReadId = new SqlCommand("Select [Id] from #tempFileId"))
                        {
                            commandReadId.Connection = connection;
                            commandReadId.Transaction = transation;
                            using (var reader = commandReadId.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                            {
                                while (reader.Read())
                                    SimilarFile.LoadImageHelper.RemoveCache((Guid)reader[0]);
                                reader.Close();
                            }
                        }
                    }

                    commandDropTable.Connection = connection;
                    commandDropTable.Transaction = transation;
                    commandDropTable.ExecuteNonQuery();
                }
                else
                    result = 0;

                transation.Commit();
            }

            return result;
        }


        internal static void Delete(Guid folderId, IEnumerable<Tuple<string, Action, Action>> operations)
        {
            var connection = DatabaseConnection.Current;
            using (var commandCreateTable = new SqlCommand("Create Table #tempFileId ([Id] uniqueidentifier)"))
            using (var commandSelect = new SqlCommand("insert into #tempFileId select [Id] from [File] where [FolderId]=@FolderId and [Path] = @Path"))
            using (var commandDeleteSimilar = new SqlCommand("Delete from [SimilarFile] where [File1Id] in (select [Id] from #tempFileId) or [File2Id] in (select [Id] from #tempFileId)"))
            using (var commandDeleteFile = new SqlCommand("Delete from [File] where [Id] in (select [Id] from #tempFileId)"))
            using (var commandDropTable = new SqlCommand("Drop Table #tempFileId"))
            using (var transation = connection.BeginTransaction())
            {
                commandCreateTable.Connection = connection;
                commandCreateTable.Transaction = transation;
                commandSelect.Connection = connection;
                commandSelect.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = folderId });
                commandSelect.Parameters.Add(new SqlParameter("@Path", System.Data.SqlDbType.NVarChar, 256));
                commandSelect.Transaction = transation;
                commandDeleteSimilar.Connection = connection;
                commandDeleteSimilar.Transaction = transation;
                commandDeleteFile.Connection = connection;
                commandDeleteFile.Transaction = transation;
                commandDropTable.Connection = connection;
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
                            using (var commandReadId = new SqlCommand("Select [Id] from #tempFileId"))
                            {
                                commandReadId.Connection = connection;
                                commandReadId.Transaction = transation;
                                using (var reader = commandReadId.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                                {
                                    while (reader.Read())
                                        SimilarFile.LoadImageHelper.RemoveCache((Guid)reader[0]);
                                    reader.Close();
                                }
                            }
                        }

                        commandDropTable.ExecuteNonQuery();

                        if (result)
                            operation.Item2();
                        else
                            operation.Item3();
                    }
                    else
                        operation.Item3();
                }

                transation.Commit();
            }
        }
    }
}
