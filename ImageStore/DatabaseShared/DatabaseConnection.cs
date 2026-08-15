using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore
{
    public static class DatabaseConnection
    {
        static SqliteConnection _current;
        static string _currentPath;

        static readonly object _lock = new object();

        public static SqliteConnection Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("Database is not specified.");
                else
                    return _current;
            }
        }

        /// <summary>
        /// Path of the open database file, or null when none is open.
        /// </summary>
        public static string CurrentPath
        {
            get { return _currentPath; }
        }

        /// <summary>
        /// Whether a database is currently open. Unlike <see cref="Current"/> this
        /// does not throw, so cleanup paths can ask without guarding.
        /// </summary>
        public static bool IsOpen
        {
            get { return _current != null; }
        }

        internal static string BuildConnectionString(string path)
        {
            return new SqliteConnectionStringBuilder
            {
                DataSource = path
            }.ToString();
        }

        internal static void Connect(string connectionString)
        {
            Close();

            var connection = new SqliteConnection(connectionString);
            connection.Open();

            //Foreign keys are enforced per connection. Microsoft.Data.Sqlite turns
            //them on by default, but the cascades this schema relies on are not
            //optional - removing a folder deletes its files only through them - so
            //set it explicitly rather than trusting a provider default.
            Execute(connection, "PRAGMA foreign_keys = ON");

            //Write-ahead logging: readers do not block the writer, which matters
            //while Measure-ImageStoreFiles streams results in from worker threads.
            //Silently ignored on filesystems that cannot support it, which is the
            //reason not to verify the result here.
            Execute(connection, "PRAGMA journal_mode = WAL");

            _current = connection;
            _currentPath = connection.DataSource;
        }

        static void Execute(SqliteConnection connection, string commandText)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Closes the current database, if any. Safe to call repeatedly and from the
        /// process-exit handler, which may race a Close-ImageStoreDatabase already
        /// running on the pipeline thread.
        /// </summary>
        internal static void Close()
        {
            lock (_lock)
            {
                if (_current == null)
                    return;

                var connection = _current;
                _current = null;
                _currentPath = null;

                try
                {
                    //Checkpoint so the -wal and -shm files are folded back into the
                    //database and removed, rather than left beside it.
                    Execute(connection, "PRAGMA optimize");
                    Execute(connection, "PRAGMA wal_checkpoint(TRUNCATE)");
                }
                catch (SqliteException)
                {
                    //Nothing useful to do while shutting down. SQLite recovers from
                    //an unclean close on its own, and this can run during process
                    //exit where reporting is no longer possible.
                }

                connection.Close();
                connection.Dispose();

                //Without this the handle stays in the connection pool and the file
                //remains locked, so -wal/-shm linger and the database cannot be
                //moved or deleted until the process ends.
                SqliteConnection.ClearAllPools();
            }
        }
    }
}
