using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    [Cmdlet(VerbsCommon.Clear, "ImageStoreSameFileObsoletedGroups")]
    [Alias("ClearSameFileObsoletedGroups")]
    public class ClearSameFileObsoletedGroupsCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Delete from [SameFile] where [Sha1Hash] in (Select [Sha1Hash] From [SameFile] Group by [Sha1Hash] Having count([id]) = 1)"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;

                var result = command.ExecuteNonQuery();
                if (result == 0)
                    WriteVerbose("No record is removed.");
                else if (result == 1)
                    WriteVerbose("One record is removed.");
                else
                    WriteVerbose(result.ToString() + " records are removed.");
            }
        }

    }
}
