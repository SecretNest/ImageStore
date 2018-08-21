using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsCommon.Clear, "ImageStoreThumbprintCacheFolder")]
    [Alias("ClearThumbprintCacheFolder")]
    public class ClearImageStoreThumbprintCacheFolderCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            LoadImageHelper.cachePath = null;
        }
    }
}
