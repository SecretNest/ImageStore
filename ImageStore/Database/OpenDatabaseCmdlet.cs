using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Database
{
    [Cmdlet(VerbsCommon.Open, "ImageStoreDatabase")]
    [Alias("OpenDatabase")]
    public class OpenDatabaseCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public string ConnectionString { get; set; }

        protected override void ProcessRecord()
        {
            DatabaseConnection.Connect(ConnectionString);
        }
    }
}
