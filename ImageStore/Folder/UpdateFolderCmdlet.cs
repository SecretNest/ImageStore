using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
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

            using (var command = new SqliteCommand("Update [Folder] Set [Name]=@Name, [Path]=@Path, [CompareImageWith]=@CompareImageWith, [IsSealed]=@IsSealed where [Id]=@Id"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.AddGuid("@Id", Folder.Id);
                command.Parameters.AddText("@Name", Folder.Name);
                command.Parameters.AddText("@Path", Folder.Path);
                command.Parameters.AddInt("@CompareImageWith", Folder.CompareImageWithCode);
                command.Parameters.AddBool("@IsSealed", Folder.IsSealed);


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
