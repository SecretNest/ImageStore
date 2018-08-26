using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    [Cmdlet(VerbsCommon.Hide, "ImageStoreSameFile", DefaultParameterSetName = "Id")]
    [Alias("HideSameFile")]
    public class HideSameFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "Entity")]
        public ImageStoreSameFile SameFile { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "Id")]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            if (SameFile != null)
                Id = SameFile.Id;

            var result = IgnoreSameFileHelper.MarkIgnore(Id, true);

            if (!result)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot update this same file."),
                    "ImageStore Update Same File", ErrorCategory.WriteError, null));
            }

            if (SameFile != null)
                SameFile.IsIgnored = true;
        }
    }
}
