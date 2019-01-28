using SecretNest.ImageStore.Extension;
using SecretNest.ImageStore.File;
using SecretNest.ImageStore.Folder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    public enum UserInteraction : int
    {
        Auto = 0,
        Enforced = 1,
        Suppressed = 2
    }

    [Cmdlet(VerbsCommon.Select, "ImageStoreSameFile", DefaultParameterSetName = "SingleFolder")]
    [Alias("SelectSameFile")]
    [OutputType(typeof(List<ImageStoreSameFile>))]
    public class SelectSameFileCmdlet : PSCmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "SingleFolder")]
        public ImageStoreFolder Folder { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "SingleFolderId")]
        public Guid FolderId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "Folders")]
        [Alias("Folders")]
        public ImageStoreFolder[] OrderedFolders { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "FolderIds")]
        [Alias("FolderIds")]
        public Guid[] OrderedFolderIds { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public byte[] Sha1Hash { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public Color OddGroupLinesBackColor { get; set; } = Color.FromArgb(192, 255, 255);

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public Color EvenGroupLinesBackColor { get; set; } = Color.FromArgb(192, 192, 255);

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 4)]
        public Color NormalBackColor { get; set; } = Color.FromKnownColor(KnownColor.Window);

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 5)]
        public Color NormalForeColor { get; set; } = Color.Black;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 6)]
        public Color SelectedForeColor { get; set; } = Color.Red;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 7)]
        public Color IgnoredForeColor { get; set; } = Color.FromArgb(224, 224, 224);

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 8)]
        public SwitchParameter UseSystemColor { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 9)]
        public UserInteraction UserInteraction { get; set; } = UserInteraction.Auto;

        Dictionary<Guid, string> extensionNames = null;
        Dictionary<Guid, string> folderPaths = null;
        Dictionary<Guid, SameFileMatchingRecord> records = new Dictionary<Guid, SameFileMatchingRecord>();
        protected override void BeginProcessing()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
        }

        protected override void ProcessRecord()
        {
            if (UseSystemColor.IsPresent)
            {
                OddGroupLinesBackColor = Color.FromKnownColor(KnownColor.Window);
                EvenGroupLinesBackColor = Color.FromKnownColor(KnownColor.Window);
                NormalBackColor = Color.FromKnownColor(KnownColor.Window);
                NormalForeColor = Color.FromKnownColor(KnownColor.ControlText);
                SelectedForeColor = Color.FromKnownColor(KnownColor.ControlText);
                IgnoredForeColor = Color.FromKnownColor(KnownColor.ControlText);
            }

            if (UserInteraction != UserInteraction.Suppressed)
            {
                extensionNames = new Dictionary<Guid, string>();
                foreach (var ext in ExtensionHelper.GetAllExtensions())
                {
                    extensionNames.Add(ext.Id, ext.Extension);
                }
                folderPaths = new Dictionary<Guid, string>();
                foreach (var folder in FolderHelper.GetAllFolders())
                {
                    if (folder.Path.EndsWith(DirectorySeparatorString.Value))
                        folderPaths.Add(folder.Id, folder.Path);
                    else
                        folderPaths.Add(folder.Id, folder.Path + DirectorySeparatorString.Value);
                }
            }

            List<Guid> folderIds = new List<Guid>();

            if (ParameterSetName == "SingleFolder")
            {
                folderIds.Add(Folder.Id);
            }
            else if (ParameterSetName == "SingleFolderId")
            {
                folderIds.Add(FolderId);
            }
            else if (ParameterSetName == "Folders")
            {
                foreach (var folder in OrderedFolders)
                    folderIds.Add(folder.Id);
            }
            else if (ParameterSetName == "FolderIds")
            {
                folderIds.AddRange(OrderedFolderIds);
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (Sha1Hash == null)
            {
                var hashes = GetHashes();
                foreach (var hash in hashes)
                {
                    ProcessOneHash(hash, folderIds);
                }
            }
            else
            {
                ProcessOneHash(Sha1Hash, folderIds);
            }

            bool needInteraction;
            bool containsNotDealt = records.Any(i => !i.Value.IsAutoDealt);
            if (UserInteraction == UserInteraction.Suppressed)
            {
                if (containsNotDealt) WriteWarning("Some files cannot be dealt automatically.");
                needInteraction = false;
            }
            else if (UserInteraction == UserInteraction.Enforced)
                needInteraction = true;
            else
                needInteraction = containsNotDealt;

            if (needInteraction)
            {
                if (!ShowUserInteraction(folderIds))
                {
                    WriteObject(null);
                    return;
                }
            }

            List<ImageStoreSameFile> results = new List<ImageStoreSameFile>();
            foreach(var record in records)
            {
                if (!record.Value.IsSelectedToReturn) continue;
                results.Add(new ImageStoreSameFile(record.Key, record.Value.Sha1Hash, record.Value.FileId) { IsIgnored = record.Value.IsIgnored });
            }

            WriteObject(results);
        }

        List<byte[]> GetHashes()
        {
            List<byte[]> result = new List<byte[]>();

            var connection = DatabaseConnection.Current;
            using (var command = new SqlCommand("select [Sha1Hash] from [SameFile] group by [Sha1Hash] having Count([Id]) > 1"))
            {
                command.Connection = connection;

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        result.Add((byte[])reader[0]);
                    }
                    reader.Close();
                }
            }

            return result;
        }

        string GetFileName(Guid folderId, string path, string fileName, Guid extensionId)
        {
            if (folderPaths == null) return null;
            var folder = folderPaths[folderId];
            var extension = extensionNames[extensionId];
            if (path == "")
                return folder + fileName + "." + extension;
            else
                return folder + path + DirectorySeparatorString.Value + fileName + "." + extension;
        }

        int currentGroupNumber = 1;
        void ProcessOneHash(byte[] sha1Hash, List<Guid> folderIds)
        {
            var connection = DatabaseConnection.Current;

            List<SameFileMatchingRecord> recordsInGroup = new List<SameFileMatchingRecord>();
            
            using (var command = new SqlCommand("select [SameFile].[Id],[File].[FolderId],[SameFile].[FileId],[File].[Path],[File].[FileName],[File].[ExtensionId],[SameFile].[IsIgnored] from [SameFile] inner join [File] on SameFile.FileId = [File].[Id] where [SameFile].[Sha1Hash]=@Sha1Hash"))
            {
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Sha1Hash", System.Data.SqlDbType.Binary, 20) { Value = sha1Hash });

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        var sameFileId = (Guid)reader[0];
                        var folderId = (Guid)reader[1];
                        SameFileMatchingRecord record = new SameFileMatchingRecord()
                        {
                            IsInTargetFolder = folderIds.Contains(folderId),
                            FolderId = folderId,
                            FileId = (Guid)reader[2],
                            FileName = GetFileName(folderId, (string)reader[3], (string)reader[4], (Guid)reader[5]),
                            IsIgnored = (bool)reader[6],
                            GroupNumber = currentGroupNumber,
                            Sha1Hash = sha1Hash
                        };
                        recordsInGroup.Add(record);
                        records.Add(sameFileId, record);
                    }
                    reader.Close();
                }
            }
            currentGroupNumber++;

            if (recordsInGroup.Count < 2)
            {
                // less than 2 records found. Nothing need to be dealt.
                foreach (var item in recordsInGroup)
                {
                    item.IsAutoDealt = true;
                }
            }
            else
            {
                var folderMatchedAndNotIgnored = recordsInGroup.Where(i => !i.IsIgnored && i.IsInTargetFolder);
                var folderNotMatchedAndNotIgnored = recordsInGroup.Where(i => !i.IsIgnored && !i.IsInTargetFolder);
                
                if (folderNotMatchedAndNotIgnored.Count() > 0)
                {
                    //Some files is not located in this folder.
                    foreach (var item in recordsInGroup)
                    {
                        item.IsAutoDealt = true;
                    }
                    foreach (var item in folderMatchedAndNotIgnored)
                    {
                        item.IsSelectedToReturn = true;
                    }
                }
            }
        }

        bool ShowUserInteraction(List<Guid> folderIds)
        {
            using (SameFileManager window = new SameFileManager(records, OddGroupLinesBackColor, EvenGroupLinesBackColor, NormalBackColor, NormalForeColor, SelectedForeColor, IgnoredForeColor, IgnoreSameFileHelper.MarkIgnore, folderIds))
            {
                WriteVerbose("Please select all files you want to returned from the popped up window.");
                return window.ShowDialog() == System.Windows.Forms.DialogResult.OK;
            }
        }
    }
}
