using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using SecretNest.ImageStore.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SecretNest.ImageStore.Migrator
{
    /// <summary>
    /// Copies an ImageStore library from Sql Server into a SQLite file.
    /// </summary>
    /// <remarks>
    /// Shipped separately from the module because it is needed exactly once per
    /// library, and because it is the only piece that still has to talk to Sql
    /// Server.
    /// </remarks>
    static class Program
    {
        static int Main(string[] args)
        {
            string source = null;
            string target = null;

            for (var i = 0; i < args.Length - 1; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--source":
                    case "-s":
                        source = args[++i];
                        break;
                    case "--target":
                    case "-t":
                        target = args[++i];
                        break;
                }
            }

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                Console.Error.WriteLine("ImageStore migration tool - Sql Server to SQLite");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  ImageStore.Migrator --source <connection string> --target <file>");
                Console.Error.WriteLine();
                Console.Error.WriteLine("Example:");
                Console.Error.WriteLine("  ImageStore.Migrator \\");
                Console.Error.WriteLine("    --source \"server=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=D:\\DataStore.mdf;Integrated Security=True\" \\");
                Console.Error.WriteLine("    --target D:\\library.db");
                return 2;
            }

            try
            {
                Migrate(source, target);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Migration failed: " + ex.Message);
                return 1;
            }
        }

        static void Migrate(string source, string targetPath)
        {
            var fullPath = Path.GetFullPath(targetPath);

            if (File.Exists(fullPath))
                throw new IOException("Target file exists already: " + fullPath);

            Console.WriteLine("Source: " + source);
            Console.WriteLine("Target: " + fullPath);
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            using (var sqlServer = new SqlConnection(source))
            using (var sqlite = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = fullPath }.ToString()))
            {
                sqlServer.Open();
                sqlite.Open();

                CreateSchema(sqlite);

                var total = 0L;
                foreach (var table in SqliteSchema.TablesInDependencyOrder)
                    total += CopyTable(sqlServer, sqlite, table);

                stopwatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"Done. {total:N0} rows in {stopwatch.Elapsed:hh\\:mm\\:ss}.");
                Console.WriteLine();
                Console.WriteLine("Open it with:  Open-ImageStoreDatabase \"" + fullPath + "\"");
            }
        }

        static void CreateSchema(SqliteConnection sqlite)
        {
            using (var transaction = sqlite.BeginTransaction())
            {
                foreach (var statement in SqliteSchema.CreationStatements)
                {
                    using (var command = sqlite.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = statement;
                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            Console.WriteLine("Schema created.");
        }

        /// <summary>
        /// Columns per table, matching both schemas. Written out rather than
        /// discovered so that a column added on one side without the other shows up
        /// as an error here instead of as silently missing data.
        /// </summary>
        static readonly Dictionary<string, string[]> Columns = new Dictionary<string, string[]>
        {
            ["Folder"] = new[] { "Id", "Name", "Path", "CompareImageWith", "IsSealed" },
            ["Extension"] = new[] { "Id", "Extension", "IsImage", "Ignored" },
            ["File"] = new[] { "Id", "FolderId", "Path", "FileName", "ExtensionId", "ImageHash", "Sha1Hash", "FileSize", "FileState", "ImageComparedThreshold" },
            ["IgnoredDirectory"] = new[] { "Id", "FolderId", "Directory", "IsSubDirectoryIncluded" },
            ["SameFile"] = new[] { "Id", "Sha1Hash", "FileId", "IsIgnored" },
            ["SimilarFile"] = new[] { "Id", "File1Id", "File2Id", "DifferenceDegree", "IgnoredMode" },
        };

        static long CopyTable(SqlConnection sqlServer, SqliteConnection sqlite, string table)
        {
            var columns = Columns[table];
            var columnList = string.Join(",", Array.ConvertAll(columns, c => "[" + c + "]"));
            var valueList = string.Join(",", Array.ConvertAll(columns, c => "@" + c));

            Console.Write($"{table,-18} ");

            var rows = 0L;

            //One transaction for the whole table. SQLite commits per statement
            //otherwise, which turns a million inserts into a million fsyncs.
            using (var transaction = sqlite.BeginTransaction())
            using (var insert = sqlite.CreateCommand())
            {
                insert.Transaction = transaction;
                insert.CommandText = $"INSERT INTO [{table}] ({columnList}) VALUES ({valueList})";

                foreach (var column in columns)
                    insert.Parameters.Add(new SqliteParameter("@" + column, DBNull.Value));

                using (var select = new SqlCommand($"SELECT {columnList} FROM [{table}]", sqlServer) { CommandTimeout = 0 })
                using (var reader = select.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        for (var i = 0; i < columns.Length; i++)
                            insert.Parameters[i].Value = Convert(reader[i]);

                        insert.ExecuteNonQuery();
                        rows++;

                        if (rows % 20000 == 0)
                            Console.Write(".");
                    }
                }

                transaction.Commit();
            }

            Console.WriteLine($" {rows:N0} rows");
            return rows;
        }

        /// <summary>
        /// Maps a Sql Server value onto its SQLite representation.
        /// </summary>
        static object Convert(object value)
        {
            if (value == null || value == DBNull.Value)
                return DBNull.Value;

            //uniqueidentifier becomes the same 16 bytes the module writes. Storing
            //the string form instead would produce a database that opens fine and
            //then matches nothing.
            if (value is Guid guid)
                return guid.ToByteArray();

            //bit becomes 0 or 1.
            if (value is bool flag)
                return flag ? 1 : 0;

            //real, int, binary and nvarchar map across as they are.
            return value;
        }
    }
}
