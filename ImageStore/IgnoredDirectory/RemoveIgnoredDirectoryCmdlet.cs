using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.IgnoredDirectory
{
    [Cmdlet(VerbsCommon.Remove, "ImageStoreIgnoredDirectory", DefaultParameterSetName = "Id")]
    [Alias("RemoveIgnoredDirectory")]
    public class RemoveIgnoredDirectoryCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Entity", Mandatory = true)]
        public ImageStoreIgnoredDirectory IgnoredDirectory { get; set; } = null;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Id", Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            if (IgnoredDirectory != null)
                Id = IgnoredDirectory.Id;

            var connection = DatabaseConnection.Current;

            using (var command = new SqliteCommand("Delete from [IgnoredDirectory] where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", Id);

                if (command.ExecuteNonQuery() == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot delete this ignored directory."),
                        "ImageStore Remove Ignored Directory", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
