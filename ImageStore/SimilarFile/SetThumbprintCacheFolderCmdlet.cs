using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
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
            Assembly assembly = typeof(SetThumbprintCacheFolderCmdlet).Assembly;
            var path = new Uri(assembly.CodeBase).LocalPath;
            var assemblyFolder = System.IO.Path.GetDirectoryName(path);
            LoadImageHelper.cachePath = System.IO.Path.Combine(assemblyFolder, Path);
        }
    }
}
