using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    [Cmdlet(VerbsData.Update, "ImageStoreSameFile")]
    [Alias("UpdateSameFile")]
    public class UpdateSameFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public ImageStoreSameFile SameFile { get; set; }

        protected override void ProcessRecord()
        {
            if (SameFile == null)
                throw new ArgumentNullException(nameof(SameFile));

            var result = IgnoreSameFileHelper.MarkIgnore(SameFile.Id, SameFile.IsIgnored);

            if (!result)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot update this same file."),
                    "ImageStore Update Same File", ErrorCategory.WriteError, null));
            }
        }
    }
}
