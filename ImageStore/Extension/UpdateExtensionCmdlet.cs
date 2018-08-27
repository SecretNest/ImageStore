using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

            using (var command = new SqlCommand("Update [Extension] Set Extension=@Extension, IsImage=@IsImage, [Ignored]=@Ignored where [Id]=@Id"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Extension.Id });
                command.Parameters.Add(new SqlParameter("@Extension", System.Data.SqlDbType.NVarChar, 256) { Value = Extension.Extension });
                command.Parameters.Add(new SqlParameter("@IsImage", System.Data.SqlDbType.Bit) { Value = Extension.IsImage });
                command.Parameters.Add(new SqlParameter("@Ignored", System.Data.SqlDbType.Bit) { Value = Extension.Ignored });

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
