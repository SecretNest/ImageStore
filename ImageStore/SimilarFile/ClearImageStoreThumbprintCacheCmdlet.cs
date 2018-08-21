using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsCommon.Clear, "ImageStoreThumbprintCache")]
    [Alias("ClearThumbprintCache")]
    public class ClearImageStoreThumbprintCacheCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            var folder = LoadImageHelper.cachePath;
            if (folder == null)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Cache folder is not set."), "No Cache Folder", ErrorCategory.InvalidOperation, null));
            }

            LoadImageHelper.ClearCache();
        }
    }
}
