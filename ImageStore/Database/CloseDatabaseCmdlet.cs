using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Database
{
    [Cmdlet(VerbsCommon.Close, "ImageStoreDatabase")]
    [Alias("CloseDatabase")]
    public class CloseDatabaseCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            DatabaseConnection.Close();
        }
    }
}
