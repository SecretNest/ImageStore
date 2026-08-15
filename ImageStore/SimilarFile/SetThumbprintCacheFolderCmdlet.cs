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
            //Relative to the module, which is where the old Assembly.CodeBase
            //resolved to. Not AppContext.BaseDirectory: under Import-Module that is
            //the host's directory, so the cache would land next to pwsh.exe.
            LoadImageHelper.cachePath = System.IO.Path.Combine(ModuleLifetime.Directory, Path);
            Directory.CreateDirectory(LoadImageHelper.cachePath);
        }
    }
}
