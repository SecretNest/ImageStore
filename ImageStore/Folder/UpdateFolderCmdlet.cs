using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    [Cmdlet(VerbsData.Update, "ImageStoreFolder")]
    [Alias("UpdateFolder")]
    public class UpdateFolderCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public ImageStoreFolder Folder { get; set; }

        protected override void ProcessRecord()
        {
            if (Folder == null)
                throw new ArgumentNullException(nameof(Folder));

            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Update [Folder] Set [Name]=@Name, [Path]=@Path, [CompareImageWith]=@CompareImageWith, [IsSealed]=@IsSealed where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier) { Value = Folder.Id });
                command.Parameters.Add(new SqlParameter("@Name", System.Data.SqlDbType.NVarChar, 256) { Value = Folder.Name });
                command.Parameters.Add(new SqlParameter("@Path", System.Data.SqlDbType.NVarChar, 256) { Value = Folder.Path });
                command.Parameters.Add(new SqlParameter("@CompareImageWith", System.Data.SqlDbType.Int) { Value = Folder.CompareImageWithCode });
                command.Parameters.Add(new SqlParameter("@IsSealed", System.Data.SqlDbType.Bit) { Value = Folder.IsSealed });


                if (command.ExecuteNonQuery() == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot update this folder."),
                        "ImageStore Update Folder", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}
