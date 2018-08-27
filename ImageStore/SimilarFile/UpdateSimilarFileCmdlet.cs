using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsData.Update, "ImageStoreSimilarFile")]
    [Alias("UpdateSimilarFile")]
    public class UpdateSimilarFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public ImageStoreSimilarFile SimilarFile { get; set; }

        protected override void ProcessRecord()
        {
            if (SimilarFile == null)
                throw new ArgumentNullException(nameof(SimilarFile));

            var result = IgnoreSimilarFileHelper.MarkIgnore(SimilarFile.Id, SimilarFile.IgnoredMode);

            if (!result)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot update this similar file."),
                    "ImageStore Update Similar File", ErrorCategory.WriteError, null));
            }
        }
    }
}
