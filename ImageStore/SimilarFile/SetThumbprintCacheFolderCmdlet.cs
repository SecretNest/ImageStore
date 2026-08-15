using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsCommon.Set, "ImageStoreThumbprintCacheFolder")]
    [Alias("SetThumbprintCacheFolder")]
    public class SetThumbprintCacheFolderCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public string Path { get; set; }

        protected override void ProcessRecord()
        {
            //Assembly.CodeBase is obsolete on .NET 5+ and throws for single-file
            //publishes. AppContext.BaseDirectory gives the module folder directly,
            //without the Uri round-trip.
            LoadImageHelper.cachePath = System.IO.Path.Combine(AppContext.BaseDirectory, Path);
            Directory.CreateDirectory(LoadImageHelper.cachePath);
        }
    }
}
