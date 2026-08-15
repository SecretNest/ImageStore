using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    [Cmdlet(VerbsCommon.Add, "ImageStoreFolder")]
    [Alias("AddFolder")]
    [OutputType(typeof(ImageStoreFolder))]
    public class AddFolderCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public string Path { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        [AllowEmptyString][AllowNull]
        public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public CompareImageWith CompareImageWith { get; set; } = CompareImageWith.All;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public bool IsSealed { get; set; } = false;

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(Path))
                throw new ArgumentNullException(nameof(Path));

            if (string.IsNullOrEmpty(Name))
                Name = Path;

            var connection = DatabaseConnection.Current;
            var id = Guid.NewGuid();

            using (var command = new SqliteCommand("Insert into [Folder] values(@Id, @Name, @Path, @CompareImageWith, @IsSealed)"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", id);
                command.Parameters.AddText("@Name", Name);
                command.Parameters.AddText("@Path", Path);
                command.Parameters.AddInt("@CompareImageWith", (int)CompareImageWith);
                command.Parameters.AddBool("@IsSealed", IsSealed);

                if (command.ExecuteNonQuery() > 0)
                {
                    WriteObject(new ImageStoreFolder(id, Path)
                    {
                        Name = Name,
                        CompareImageWith = CompareImageWith,
                        IsSealed = IsSealed
                    });
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot insert this folder."),
                        "ImageStore Add Folder", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
