using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Extension
{
    [Cmdlet(VerbsData.Update, "ImageStoreExtension")]
    [Alias("UpdateExtension")]
    public class UpdateExtensionCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public ImageStoreExtension Extension { get; set; }

        protected override void ProcessRecord()
        {
            if (Extension == null)
                throw new ArgumentNullException(nameof(Extension));

            var connection = DatabaseConnection.Current;

            using (var command = new SqliteCommand("Update [Extension] Set Extension=@Extension, IsImage=@IsImage, [Ignored]=@Ignored where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", Extension.Id);
                command.Parameters.AddText("@Extension", Extension.Extension);
                command.Parameters.AddBool("@IsImage", Extension.IsImage);
                command.Parameters.AddBool("@Ignored", Extension.Ignored);

                if (command.ExecuteNonQuery() == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot update this extension."),
                        "ImageStore Update Extension", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
