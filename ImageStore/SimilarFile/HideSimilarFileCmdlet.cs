using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsCommon.Hide, "ImageStoreSimilarFile", DefaultParameterSetName = "Id")]
    [Alias("HideSimilarFile")]
    public class HideSimilarFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "Entity")]
        public ImageStoreSimilarFile SimilarFile { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "Id")]
        public Guid Id { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, Mandatory = true)]
        public bool MakeDisconnected { get; set; }

        protected override void ProcessRecord()
        {
            if (SimilarFile != null)
                Id = SimilarFile.Id;

            IgnoredMode ignoredMode;
            if (MakeDisconnected)
                ignoredMode = IgnoredMode.HiddenAndDisconnected;
            else
                ignoredMode = IgnoredMode.HiddenButConnected;

            var result = IgnoreSimilarFileHelper.MarkIgnore(Id, ignoredMode);

            if (!result)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot update this similar file."),
                    "ImageStore Update Similar File", ErrorCategory.WriteError, null));
            }

            if (SimilarFile != null)
                SimilarFile.IgnoredMode = ignoredMode;
        }
    }
}
