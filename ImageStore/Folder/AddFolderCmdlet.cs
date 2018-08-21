using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, ValueFromPipeline = true)]
        [AllowEmptyString][AllowNull]
        public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, ValueFromPipeline = true)]
        public CompareImageWith CompareImageWith { get; set; } = CompareImageWith.All;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3, ValueFromPipeline = true)]
        public bool IsSealed { get; set; } = false;

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(Path))
                throw new ArgumentNullException(nameof(Path));

            if (string.IsNullOrEmpty(Name))
                Name = Path;

            var connection = DatabaseConnection.Current;
            var id = Guid.NewGuid();

            using (var command = new SqlCommand("Insert into [Folder] values(@Id, @Name, @Path, @CompareImageWith, @IsSealed)"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = id });
                command.Parameters.Add(new SqlParameter("@Name", System.Data.SqlDbType.NVarChar, 256) { Value = Name });
                command.Parameters.Add(new SqlParameter("@Path", System.Data.SqlDbType.NVarChar, 256) { Value = Path });
                command.Parameters.Add(new SqlParameter("@CompareImageWith", System.Data.SqlDbType.Int) { Value = (int)CompareImageWith });
                command.Parameters.Add(new SqlParameter("@IsSealed", System.Data.SqlDbType.Bit) { Value = IsSealed });

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
