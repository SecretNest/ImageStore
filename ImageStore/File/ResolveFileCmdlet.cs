using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsDiagnostic.Resolve, "ImageStoreFile", DefaultParameterSetName = "Id")]
    [Alias("ResolveFile")]
    [OutputType(typeof(string))]
    public class ResolveFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "Entity")]
        public ImageStoreFile File { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "Id")]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            if (File != null)
                Id = File.Id;

            var result = FileHelper.GetFileName(Id, out _, out _, out _, out bool isFolderSealed, out _);

            if (result == null)
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("File cannot be found.", nameof(Id)), "ImageStore Resolve File", ErrorCategory.InvalidArgument, null));

            WriteObject(result);
        }
    }
}
