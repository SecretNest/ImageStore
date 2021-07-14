using SecretNest.ImageStore.Extension;
using SecretNest.ImageStore.File;
using SecretNest.ImageStore.Folder;
using SecretNest.ImageStore.IgnoredDirectory;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    [Cmdlet(VerbsData.Sync, "ImageStoreFolder")]
    [Alias("SyncFolder")]
    public class SyncFolderCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true)]
        public ImageStoreFolder Folder { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public SwitchParameter OverrideSealedFolder { get; set; }

        Dictionary<Guid, ImageStoreExtension> extensionsById = null;
        Dictionary<string, ImageStoreExtension> extensionsByName = null;

        protected override void ProcessRecord()
        {
            extensionsById = new Dictionary<Guid, ImageStoreExtension>();
            extensionsByName = new Dictionary<string, ImageStoreExtension>(StringComparer.OrdinalIgnoreCase);
            foreach(var ext in ExtensionHelper.GetAllExtensions())
            {
                extensionsById.Add(ext.Id, ext);
                extensionsByName.Add(ext.Extension, ext);
            }

            if (Folder == null)
            {
                var folders = FolderHelper.GetAllFolders().ToArray();
                foreach(var folder in folders)
                {
                    if (folder.IsSealed && !OverrideSealedFolder.IsPresent)
                    {
                        WriteInformation("Syncing folder: " + folder.Name + " (skipped - sealed folder)", new string[] { "SyncFolder", "SkipFolder" });
                        continue;
                    }
                    Sync(folder);
                }
            }
            else
            {
                if (Folder.IsSealed && !OverrideSealedFolder.IsPresent)
                {
                    ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("This folder is set to sealed and -" + nameof(OverrideSealedFolder) + " is not present."), "ImageStore Sync Folder", ErrorCategory.SecurityError, Folder.Id));
                }
                Sync(Folder);
            }
        }

        void Sync(ImageStoreFolder folder)
        {
            WriteInformation("Syncing folder: " + folder.Name, new string[] { "SyncFolder" });

            //path, filename+ext, fileId
            Dictionary<string, Dictionary<string, Guid>> dbFiles = new Dictionary<string, Dictionary<string, Guid>>(StringComparer.OrdinalIgnoreCase);

            //Read records from db
            string lastPath = null;
            Dictionary<string, Guid> filesInDirectory = null;
            foreach (var file in FileHelper.GetAllFilesWithoutData(folder.Id, null))
            {
                var fileNameWithExtension = file.FileName + "." + extensionsById[file.ExtensionId].Extension;
                if (string.Compare(lastPath, file.Path, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    lastPath = file.Path;
                    if (!dbFiles.TryGetValue(lastPath, out filesInDirectory))
                    {
                        filesInDirectory = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
                        dbFiles.Add(lastPath, filesInDirectory);
                    }
                }
                filesInDirectory.Add(fileNameWithExtension, file.Id);
            }

            HashSet<string> ignoredDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> ignoredDirectoriesWithSub = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dir in IgnoredDirectoryHelper.GetAllIgnoredDirectories(folder.Id))
            {
                ignoredDirectories.Add(dir.Directory);
                if (dir.IsSubDirectoryIncluded)
                {
                    if (dir.Directory == "") ignoredDirectoriesWithSub.Add("");
                    else ignoredDirectoriesWithSub.Add(dir.Directory + @"\");
                }
            }

            //Find all directories in file system
            var subFolders = Directory.GetDirectories(folder.Path, "*.*", SearchOption.AllDirectories);
            var subFoldersIndex = -1;
            var subFoldersLength = subFolders.Length;
            var folderPathLength = folder.Path.Length;
            if (!folder.Path.EndsWith(DirectorySeparatorString.Value)) folderPathLength++; //e.g. C:\, C:\Test or C:\Test\
            string originalDirectoryName = folder.Path;
            string directoryName = "";

            while (true)
            {
                //Skip any ignored
                if (ignoredDirectories.Remove(directoryName))
                {
                    if (++subFoldersIndex == subFoldersLength)
                    {
                        break;
                    }
                    else
                    {
                        originalDirectoryName = subFolders[subFoldersIndex];
                        directoryName = originalDirectoryName.Substring(folderPathLength);
                    }

                    continue;
                }

                bool needContinue = false;
                foreach(var item in ignoredDirectoriesWithSub)
                {
                    if (directoryName.StartsWith(item))
                    {
                        needContinue = true;
                        continue;
                    }
                }
                if (needContinue)
                {
                    if (++subFoldersIndex == subFoldersLength)
                    {
                        break;
                    }
                    else
                    {
                        originalDirectoryName = subFolders[subFoldersIndex];
                        directoryName = originalDirectoryName.Substring(folderPathLength);
                    }

                    continue;
                }

                //find match
                if (!dbFiles.TryGetValue(directoryName, out filesInDirectory))
                {
                    filesInDirectory = null; //No matched directory in database
                }
                else
                {
                    dbFiles.Remove(directoryName);
                }

                ProcessDirectory(folder, directoryName, originalDirectoryName, filesInDirectory);

                if (++subFoldersIndex == subFoldersLength)
                {
                    break;
                }
                else
                {
                    originalDirectoryName = subFolders[subFoldersIndex];
                    directoryName = originalDirectoryName.Substring(folderPathLength);
                }
            }

            //All left
            if (dbFiles.Count > 0)
            {
                var operations = dbFiles.Keys.Select(i =>
                    new Tuple<string, Action, Action>(i,
                    () => WriteInformation(string.Format("Directory of folder {0} with all files under it are removed from database: {1}", folder.Name, i), new string[] { "SyncFolder", "DirectoryRemoved" }),
                    () => WriteError(new ErrorRecord(
                            new InvalidOperationException(string.Format("Cannot remove the directory with all files under it in folder {0} from database: {1}", folder.Name, i)),
                            "ImageStore Sync Folder - Removing obsolete record", ErrorCategory.WriteError, null))));

                FileHelper.Delete(folder.Id, operations);
            }

        }

        void ProcessDirectory(ImageStoreFolder folder, string directoryName, string originalDirectoryName, Dictionary<string, Guid> dbFilesInDirectory)
        {
            WriteVerbose(string.Format("Processing files in directory of folder {0}: {1}", folder.Name, directoryName));
            var allFiles = Directory.GetFiles(originalDirectoryName);
            var connection = DatabaseConnection.Current;

            if (allFiles.Length != 0)
            {
                using (var command = new SqlCommand("Insert into [File] values(@Id, @FolderId, @Path, @FileName, @ExtensionId, NULL, NULL, -1, 0, 0)"))
                {
                    command.Connection = connection;
                    command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                    command.Parameters.Add(new SqlParameter("@FolderId", System.Data.SqlDbType.UniqueIdentifier) { Value = folder.Id });
                    command.Parameters.Add(new SqlParameter("@Path", System.Data.SqlDbType.NVarChar, 256));
                    command.Parameters.Add(new SqlParameter("@FileName", System.Data.SqlDbType.NVarChar, 256));
                    command.Parameters.Add(new SqlParameter("@ExtensionId", System.Data.SqlDbType.UniqueIdentifier));

                    foreach (var fullFilePath in allFiles)
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath);
                        var extensionName = Path.GetExtension(fullFilePath);
                        if (extensionName.StartsWith(".")) extensionName = extensionName.Substring(1);

                        if (extensionsByName.TryGetValue(extensionName, out var extension))
                        {
                            if (extension.Ignored) continue;
                        }
                        else
                        {
                            WriteWarning(string.Format("Extension {0} cannot be found in database. Ignored.", extensionName));
                            extensionsByName.Add(extensionName, new ImageStoreExtension() { Ignored = true });
                            continue;
                        }

                        var fileNameKey = fileNameWithoutExtension + "." + extensionName;

                        //find match
                        string fileNameWithDirectory;
                        if (directoryName == "")
                        {
                            fileNameWithDirectory = fileNameKey;
                        }
                        else
                        {
                            fileNameWithDirectory = directoryName + "\\" + fileNameKey;
                        }
                        if (dbFilesInDirectory == null || !dbFilesInDirectory.Remove(fileNameKey))
                        {
                            //Add file
                            command.Parameters[0].Value = Guid.NewGuid();
                            command.Parameters[2].Value = directoryName;
                            command.Parameters[3].Value = fileNameWithoutExtension;
                            command.Parameters[4].Value = extension.Id;

                            if (command.ExecuteNonQuery() == 0)
                            {
                                WriteError(new ErrorRecord(
                                    new InvalidOperationException(string.Format("Cannot add the file in folder {0} to database: {1}", folder.Name, fileNameWithDirectory)),
                                    "ImageStore Sync Folder - Adding records", ErrorCategory.WriteError, null));
                            }
                            else
                            {
                                WriteInformation(string.Format("New file is found in folder {0}: {1}", folder.Name, fileNameWithDirectory), new string[] { "SyncFolder", "FileAdded" });
                            }
                        }
                    }
                }
            }

            //all left
            if (dbFilesInDirectory != null && dbFilesInDirectory.Count > 0)
            {
                var operations = dbFilesInDirectory.Select(i =>
                    new Tuple<Guid, Action, Action>(
                        i.Value,
                        () => WriteInformation(string.Format("File of folder {0} is removed from database: {1}", folder.Name, GetFileNameWithDirectory(directoryName, i.Key)), new string[] { "SyncFolder", "FileRemoved" }),
                        () => WriteError(new ErrorRecord(
                                new InvalidOperationException(string.Format("Cannot remove the file in folder {0} from database: {1}", folder.Name, GetFileNameWithDirectory(directoryName, i.Key))),
                                "ImageStore Sync Folder - Removing obsolete record", ErrorCategory.WriteError, null))));

                FileHelper.Delete(operations);
            }
        }

        string GetFileNameWithDirectory(string directoryName, string fileName)
        {
            if (directoryName == "")
            {
                return fileName;
            }
            else
            {
                return directoryName + DirectorySeparatorString.Value + fileName;
            }
        }
    }
}
