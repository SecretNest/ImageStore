using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    [Cmdlet(VerbsCommon.Clear, "ImageStoreEmptyFolders")]
    [Alias("ClearEmptyFolders")]
    public class ClearEmptyFoldersCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public ImageStoreFolder Folder { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public SwitchParameter OverrideSealedFolder { get; set; }
        protected override void ProcessRecord()
        {
            if (Folder == null)
                throw new ArgumentNullException(nameof(Folder));

            if (!OverrideSealedFolder.IsPresent && Folder.IsSealed)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("This folder is set to sealed and -" + nameof(OverrideSealedFolder) + " is not present."), "ImageStore Clear Empty Folders", ErrorCategory.SecurityError, Folder.Id));
            }

            foreach (var directory in Directory.GetDirectories(Folder.Path))
                ProcessDirectory(directory);
        }

        bool ProcessDirectory(string location)
        {
            bool left = Directory.GetFiles(location).Length > 0;

            foreach(var sub in Directory.GetDirectories(location))
            {
                left = ProcessDirectory(sub) || left;
            }
            
            if (!left)
            {
                Directory.Delete(location);
                WriteInformation("Directory " + location + " is deleted.", new string[] { "ClearEmptyFolders" });
            }
            return left;
        }
    }
}
