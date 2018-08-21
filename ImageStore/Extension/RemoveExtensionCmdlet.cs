using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Extension
{
    [Cmdlet(VerbsCommon.Remove, "ImageStoreExtension", DefaultParameterSetName = "Id")]
    [Alias("RemoveExtension")]
    public class RemoveExtensionCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Entity", Mandatory = true)]
        public ImageStoreExtension Extension { get; set; } = null;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Id", Mandatory = true)]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            if (Extension != null)
                Id = Extension.Id;

            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Delete from [Extension] where [Id]=@Id"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Id });

                if (command.ExecuteNonQuery() == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot delete this extension."),
                        "ImageStore Remove Extension", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
