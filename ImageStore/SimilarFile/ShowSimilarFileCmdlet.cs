using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsCommon.Show, "ImageStoreSimilarFile", DefaultParameterSetName = "Id")]
    [Alias("ShowSimilarFile")]
    public class ShowSimilarFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "Entity")]
        public ImageStoreSimilarFile SimilarFile { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "Id")]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            if (SimilarFile != null)
                Id = SimilarFile.Id;

            var result = IgnoreSimilarFileHelper.MarkIgnore(Id, IgnoredMode.Normal);

            if (!result)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot update this similar file."),
                    "ImageStore Update Similar File", ErrorCategory.WriteError, null));
            }

            if (SimilarFile != null)
                SimilarFile.IgnoredMode = IgnoredMode.Normal;
        }
    }
}
