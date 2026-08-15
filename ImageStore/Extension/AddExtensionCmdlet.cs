using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Extension
{
    [Cmdlet(VerbsCommon.Add, "ImageStoreExtension")]
    [Alias("AddExtension")]
    [OutputType(typeof(ImageStoreExtension))]
    public class AddExtensionCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        [AllowEmptyString()] public string Extension { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public bool IsImage { get; set; } = true;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public bool Ignored { get; set; } = false;


        protected override void ProcessRecord()
        {
            if (Extension == null)
                throw new ArgumentNullException(nameof(Extension));

            var connection = DatabaseConnection.Current;
            var id = Guid.NewGuid();

            using (var command = new SqliteCommand("Insert into [Extension] values(@Id, @Extension, @IsImage, @Ignored)"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", id);
                command.Parameters.AddText("@Extension", Extension);
                command.Parameters.AddBool("@IsImage", IsImage);
                command.Parameters.AddBool("@Ignored", Ignored);

                if (command.ExecuteNonQuery() > 0)
                {
                    WriteObject(new ImageStoreExtension(id)
                    {
                        Extension = Extension,
                        Ignored = Ignored,
                        IsImage = IsImage
                    });
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot insert this extension."),
                        "ImageStore Add Extension", ErrorCategory.WriteError, null));
                }
            }

        }
    }
}
