using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Database
{
    [Cmdlet(VerbsCommon.Open, "ImageStoreDatabase", DefaultParameterSetName = PathParameterSet)]
    [Alias("OpenDatabase")]
    public class OpenDatabaseCmdlet : PSCmdlet
    {
        internal const string PathParameterSet = "Path";
        internal const string ConnectionStringParameterSet = "ConnectionString";

        [Parameter(ParameterSetName = PathParameterSet, ValueFromPipelineByPropertyName = true,
            Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public string Path { get; set; }

        /// <summary>
        /// For settings the path alone cannot express, such as Mode=ReadOnly.
        /// </summary>
        [Parameter(ParameterSetName = ConnectionStringParameterSet, ValueFromPipelineByPropertyName = true,
            Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public string ConnectionString { get; set; }

        protected override void ProcessRecord()
        {
            string connectionString;

            if (ParameterSetName == ConnectionStringParameterSet)
            {
                connectionString = ConnectionString;
            }
            else
            {
                //Resolved against the PowerShell location, not the process working
                //directory. The two are routinely different, and using the wrong one
                //would silently open or create a file somewhere unexpected.
                var fullPath = GetUnresolvedProviderPathFromPSPath(Path);

                if (!System.IO.File.Exists(fullPath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new System.IO.FileNotFoundException("Database file is not found.", fullPath),
                        "ImageStore Open Database", ErrorCategory.ObjectNotFound, fullPath));
                    return;
                }

                //Mode=ReadWrite rather than the default ReadWriteCreate: opening a
                //path that does not exist should fail, not quietly produce an empty
                //database with no tables that then fails on the first real query.
                connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = fullPath,
                    Mode = SqliteOpenMode.ReadWrite
                }.ToString();
            }

            DatabaseConnection.Connect(connectionString);

            WriteInformation("Database opened: " + DatabaseConnection.CurrentPath,
                new string[] { "Database", "Open" });
        }
    }
}
