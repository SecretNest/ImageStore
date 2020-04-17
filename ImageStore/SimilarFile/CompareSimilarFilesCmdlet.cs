using SecretNest.ImageStore.Extension;
using SecretNest.ImageStore.File;
using SecretNest.ImageStore.Folder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsData.Compare, "ImageStoreSimilarFiles")]
    [Alias("CompareSimilarFiles")]
    public class CompareSimilarFilesCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true)]
        public float ImageComparedThreshold { get; set; } = 0.05f;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public int ComparingThreadLimit { get; set; } = 0;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public SwitchParameter SuppressTimeWarning { get; set; }

        BlockingCollection<Tuple<bool, string>> outputs = new BlockingCollection<Tuple<bool, string>>();
        ConcurrentBag<ErrorRecord> exceptions = new ConcurrentBag<ErrorRecord>();

        protected override void ProcessRecord()
        {
            if (!SuppressTimeWarning.IsPresent)
                WriteInformation("This may need several hours even days to finish. Please wait patiently...", new string[] { "CompareSimilarFile", "TimeWarning" });
            WriteInformation("Image compared threshold (max difference degree): " + ImageComparedThreshold.ToString(), new string[] { "CompareSimilarFile", "MaxDifferenceDegree", "ImageComparedThreshold" });

            if (ComparingThreadLimit <= 0)
            {
                ComparingThreadLimit = Environment.ProcessorCount;
                WriteVerbose("Comparing thread count: Auto (Count of CPU Cores = " + ComparingThreadLimit.ToString() + ")");
            }
            else
            {
                WriteVerbose("Comparing thread count: " + ComparingThreadLimit.ToString());
            }

            WriteVerbose("Loading folders and files data into memory...");
            PrepareFolders();
            PrepareExtensions();
            PrepareFiles();
            var filesToBeCompareCount = filesToBeCompared.Count;
            if (filesToBeCompareCount == 0)
            {
                WriteInformation("No files need to be compared.", new string[] { "CompareSimilarFile" });
                return;
            }
            else if (filesToBeCompareCount == 1)
            {
                WriteInformation("One file is found to be compared.", new string[] { "CompareSimilarFile" });
            }
            else
            {
                WriteInformation(filesToBeCompareCount.ToString() + " files are found to be compared.", new string[] { "CompareSimilarFile" });
            }


            CompareSimilarFileHelper helper = new CompareSimilarFileHelper(ImageComparedThreshold, allFiles, filesToBeCompared, existingSimilars, ComparingThreadLimit, outputs, exceptions);

            Task job = new Task(helper.Process, TaskCreationOptions.LongRunning);
            job.Start();

            while (true)
            {
                try
                {
                    var output = outputs.Take();
                    if (output.Item1)
                    {
                        WriteWarning(output.Item2);
                    }
                    else
                    {
                        WriteVerbose(output.Item2);
                    }
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }

            job.Wait();

            if (exceptions.Count == 1)
            {
                ThrowTerminatingError(exceptions.ToArray()[0]);
            }
            else if (exceptions.Count > 0)
            {
                foreach (var ex in exceptions)
                {
                    WriteError(ex);
                }
                ThrowTerminatingError(new ErrorRecord(new AggregateException(exceptions.Select(i => i.Exception)), "ImageStore Comparing Similar File - Aggregate", ErrorCategory.NotSpecified, null));
            }
        }

        Dictionary<Guid, Tuple<CompareImageWith, string>> folders = new Dictionary<Guid, Tuple<CompareImageWith, string>>(); //string: path
        void PrepareFolders()
        {
            var records = FolderHelper.GetAllFolders();
            foreach(var record in records)
            {
                string folderPath = record.Path;
                if (!folderPath.EndsWith(DirectorySeparatorString.Value))
                    folderPath += DirectorySeparatorString.Value;
                folders.Add(record.Id, new Tuple<CompareImageWith, string>(record.CompareImageWith, folderPath));
            }
        }

        Dictionary<Guid, string> extensions = new Dictionary<Guid, string>();
        void PrepareExtensions()
        {
            extensions = new Dictionary<Guid, string>();
            foreach (var ext in ExtensionHelper.GetAllExtensions())
                extensions.Add(ext.Id, ext.Extension);
        }

        //FolderId, Path, FileId, ImageHash, sequence
        Dictionary<Guid, Dictionary<string, Dictionary<Guid, Tuple<byte[], int>>>> allFiles = new Dictionary<Guid, Dictionary<string, Dictionary<Guid, Tuple<byte[], int>>>>();
        //Key = FileId; Value = FolderId, Path, ImageHash, CompareImageWith, SimilarRecordTargetFile, sequence, fullPath
        Dictionary<Guid, Tuple<Guid, string, byte[], CompareImageWith, int, string>> filesToBeCompared = new Dictionary<Guid, Tuple<Guid, string, byte[], CompareImageWith, int, string>>();
        //FileId, FileId
        Dictionary<Guid, HashSet<Guid>> existingSimilars = new Dictionary<Guid, HashSet<Guid>>();
        void PrepareFiles()
        {
            var connection = DatabaseConnection.Current;
            var sqlCommandTextGetNewFiles = "select [FolderId],[Path],[Id],[ImageHash],[ImageComparedThreshold],[FileName],[ExtensionId] from [File] where [ImageHash] is not null order by [FolderId],[Path]";
            using (var command = new SqlCommand(sqlCommandTextGetNewFiles, connection) { CommandTimeout = 0 })
            using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
            {
                Guid lastFolderKey = Guid.Empty;
                string lastPathKey = null;
                Dictionary<string, Dictionary<Guid, Tuple<byte[], int> >> lastFolder = null;
                Dictionary<Guid, Tuple<byte[], int>> lastPath = null;

                int sequence = 0;
                int currentSequence;

                while (reader.Read())
                {
                    var folderId = (Guid)reader[0];
                    string path = (string)reader[1];
                    var fileId = (Guid)reader[2];
                    var imageHash = (byte[])reader[3];
                    var threshold = (float)reader[4];
                    var fileName = (string)reader[5];
                    var extensionId = (Guid)reader[6];

                    var folder = folders[folderId];
                    var fullPath = FileHelper.GetFullFilePath(folder.Item2, path, fileName, extensions[extensionId]);

                    if (threshold < ImageComparedThreshold)
                    {
                        currentSequence = sequence++;
                        filesToBeCompared.Add(fileId, new Tuple<Guid, string, byte[], CompareImageWith, int, string>(folderId, path, imageHash, folder.Item1, currentSequence, fullPath));
                    }
                    else
                    {
                        currentSequence = -1;
                    }

                    if (folderId != lastFolderKey)
                    {
                        lastFolderKey = folderId;
                        lastFolder = new Dictionary<string, Dictionary<Guid, Tuple<byte[], int>>>(StringComparer.OrdinalIgnoreCase);
                        allFiles.Add(lastFolderKey, lastFolder);
                    }
                    if (string.Compare(path, lastPathKey, true) != 0)
                    {
                        lastPathKey = path;
                        lastPath = new Dictionary<Guid, Tuple<byte[], int>>();
                        lastFolder.Add(lastPathKey, lastPath);
                    }
                    lastPath.Add(fileId, new Tuple<byte[], int>(imageHash, currentSequence));
                }
                reader.Close();
            }

            var sqlCommandTextCreateTable = "Create table #filesToBeCompared ([Id] uniqueidentifier)";
            using (var command = new SqlCommand(sqlCommandTextCreateTable, connection))
            {
                command.ExecuteNonQuery();
            }
            var sqlCommandTextInsertFilesToBeCompared = "Insert into #filesToBeCompared Select [Id] from [File] where [ImageComparedThreshold] < @ImageComparedThreshold";
            using (var command = new SqlCommand(sqlCommandTextInsertFilesToBeCompared, connection) { CommandTimeout = 0 })
            {
                command.Parameters.Add(new SqlParameter("@ImageComparedThreshold", System.Data.SqlDbType.Real) { Value = ImageComparedThreshold });
                command.ExecuteNonQuery();
            }

            var sqlCommandTextSelectSimilar = "Select [File1Id],[File2Id] from [SimilarFile] where [File1Id] in (Select [Id] from #filesToBeCompared) or [File2Id] in (Select [Id] from #filesToBeCompared)";
            using (var command = new SqlCommand(sqlCommandTextSelectSimilar, connection) { CommandTimeout = 0 })
            using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
            {
                while (reader.Read())
                {
                    var file1Id = (Guid)reader[0];
                    var file2Id = (Guid)reader[1];

                    if (!existingSimilars.TryGetValue(file1Id, out var set))
                    {
                        set = new HashSet<Guid>();
                        existingSimilars[file1Id] = set;
                    }
                    set.Add(file2Id);

                    if (!existingSimilars.TryGetValue(file2Id, out set))
                    {
                        set = new HashSet<Guid>();
                        existingSimilars[file2Id] = set;
                    }
                    set.Add(file1Id);
                }
                reader.Close();
            }

            var sqlCommandTextDeleteTable = "Drop table #filesToBeCompared";
            using (var command = new SqlCommand(sqlCommandTextDeleteTable, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
