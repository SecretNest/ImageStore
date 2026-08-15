using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Database
{
    [Cmdlet(VerbsCommon.New, "ImageStoreDatabase")]
    [Alias("NewDatabase")]
    public class NewDatabaseCmdlet : PSCmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public string Path { get; set; }

        /// <summary>
        /// Replaces an existing file. Without this, an existing path is an error
        /// rather than something to overwrite - the file is somebody's library.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            //Resolved against the PowerShell location rather than the process working
            //directory; see the same note in Open-ImageStoreDatabase.
            var fullPath = GetUnresolvedProviderPathFromPSPath(Path);

            if (System.IO.File.Exists(fullPath))
            {
                if (!Force.IsPresent)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new IOException("File exists already. Use -Force to replace it."),
                        "ImageStore New Database", ErrorCategory.ResourceExists, fullPath));
                    return;
                }

                //Close first: the file being replaced may be the one currently open,
                //and on Windows it cannot be deleted while a handle is held.
                DatabaseConnection.Close();

                try
                {
                    DeleteDatabaseFiles(fullPath);
                }
                catch (Exception ex)
                {
                    ThrowTerminatingError(new ErrorRecord(ex,
                        "ImageStore New Database", ErrorCategory.WriteError, fullPath));
                    return;
                }
            }

            var directory = System.IO.Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new DirectoryNotFoundException("Directory of the database file is not found."),
                    "ImageStore New Database", ErrorCategory.ObjectNotFound, directory));
                return;
            }

            //ReadWriteCreate is the default, and here creating is the point.
            var connectionString = DatabaseConnection.BuildConnectionString(fullPath);

            try
            {
                DatabaseConnection.Connect(connectionString);
                CreateSchema();
            }
            catch (SqliteException ex)
            {
                DatabaseConnection.Close();
                ThrowTerminatingError(new ErrorRecord(ex,
                    "ImageStore New Database", ErrorCategory.WriteError, fullPath));
                return;
            }

            //Left open on purpose: creating a database is followed by using it, so
            //this stands in for a subsequent Open-ImageStoreDatabase.
            WriteInformation("Database created and opened: " + DatabaseConnection.CurrentPath,
                new string[] { "Database", "New", "Open" });
        }

        void CreateSchema()
        {
            var connection = DatabaseConnection.Current;

            using (var transaction = connection.BeginTransaction())
            {
                foreach (var statement in SqliteSchema.CreationStatements)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = statement;
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// Removes the database and the journal files that live beside it. Leaving a
        /// stale -wal behind would let SQLite recover content from the database this
        /// one replaced.
        /// </summary>
        static void DeleteDatabaseFiles(string fullPath)
        {
            System.IO.File.Delete(fullPath);

            foreach (var suffix in new[] { "-wal", "-shm", "-journal" })
            {
                var companion = fullPath + suffix;
                if (System.IO.File.Exists(companion))
                    System.IO.File.Delete(companion);
            }
        }
    }
}
