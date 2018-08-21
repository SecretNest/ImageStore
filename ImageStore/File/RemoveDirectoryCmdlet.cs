using SecretNest.ImageStore.Folder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsCommon.Remove, "ImageStoreDirectory")]
    [Alias("RemoveDirectory")]
    public class RemoveDirectoryCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
        public ImageStoreFolder Folder { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, Mandatory = true)]
        [AllowEmptyString]
        public string Path { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public SwitchParameter SkipFile { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public SwitchParameter OverrideSealedFolder { get; set; }

        protected override void ProcessRecord()
        {
            if (Folder == null)
                throw new ArgumentNullException(nameof(Folder));
            if (Path == null)
                throw new ArgumentNullException(nameof(Path));

            if (!OverrideSealedFolder.IsPresent && Folder.IsSealed)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Target folder is set to sealed and -" + nameof(OverrideSealedFolder) + " is not present."), "ImageStore Remove Directory", ErrorCategory.SecurityError, Folder.Id));
            }

            if (!SkipFile.IsPresent)
            {
                string folderPath;

                if (Path == "")
                    folderPath = Folder.Path;
                else
                    folderPath = Folder.Path + DirectorySeparatorString.Value + Path;
                Directory.Delete(folderPath, true);
                WriteInformation("Directory " + folderPath + " is deleted.", new string[] { "RemoveDirectory" });
            }

            int result = FileHelper.Delete(Folder.Id, Path);

            if (result == 0)
                WriteVerbose("No file is removed from database.");
            else if (result == 1)
                WriteVerbose("One file is removed from database.");
            else
                WriteVerbose(result.ToString() + " files are removed from database.");
        }
    }
}
