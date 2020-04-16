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
        public int ComparingThreadLimit { get; set; } = Environment.ProcessorCount;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public SwitchParameter SuppressTimeWarning { get; set; }

        BlockingCollection<Tuple<bool, string>> outputs = new BlockingCollection<Tuple<bool, string>>();
        ConcurrentBag<ErrorRecord> exceptions = new ConcurrentBag<ErrorRecord>();

        protected override void ProcessRecord()
        {
            if (!SuppressTimeWarning.IsPresent)
                WriteInformation("This may need several hours even days to finish. Please wait patiently...", new string[] { "CompareSimilarFile", "TimeWarning" });
            WriteInformation("Image compared threshold (max difference degree): " + ImageComparedThreshold.ToString(), new string[] { "CompareSimilarFile", "MaxDifferenceDegree", "ImageComparedThreshold" });

            if (ComparingThreadLimit < 1)
            {
                ComparingThreadLimit = 1;
            }
            WriteVerbose("Comparing thread count: " + ComparingThreadLimit.ToString());

            WriteVerbose("Loading folders and files data into memory...");
            PrepareFolders();
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


            CompareSimilarFileHelper helper = new CompareSimilarFileHelper(ImageComparedThreshold, allFiles, filesToBeCompared, ComparingThreadLimit, outputs, exceptions);

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

        Dictionary<Guid, CompareImageWith> folders = new Dictionary<Guid, CompareImageWith>();
        void PrepareFolders()
        {
            var records = FolderHelper.GetAllFolders();
            foreach(var record in records)
            {
                folders.Add(record.Id, record.CompareImageWith);
            }
        }

        //FolderId, Path, FileId, ImageHash
        Dictionary<Guid, Dictionary<string, Dictionary<Guid, byte[]>>> allFiles = new Dictionary<Guid, Dictionary<string, Dictionary<Guid, byte[]>>>();
        //FileId, FolderId, Path, ImageHash, CompareImageWith
        List<Tuple<Guid, Guid, string, byte[], CompareImageWith>> filesToBeCompared = new List<Tuple<Guid, Guid, string, byte[], CompareImageWith>>();
        void PrepareFiles()
        {
            var connection = DatabaseConnection.Current;
            var sqlCommandTextGetNewFiles = "select [FolderId],[Path],[Id],[ImageHash],[ImageComparedThreshold] from [File] where [ImageHash] is not null order by [FolderId],[Path]";
            using (var command = new SqlCommand(sqlCommandTextGetNewFiles, connection) { CommandTimeout = 0 })
            using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
            {
                Guid lastFolderKey = Guid.Empty;
                string lastPathKey = null;
                Dictionary<string, Dictionary<Guid, byte[]>> lastFolder = null;
                Dictionary<Guid, byte[]> lastPath = null;

                while (reader.Read())
                {
                    var folderId = (Guid)reader[0];
                    string path = (string)reader[1];
                    var fileId = (Guid)reader[2];
                    var imageHash = (byte[])reader[3];
                    var threshold = (float)reader[4];

                    if (folderId != lastFolderKey)
                    {
                        lastFolderKey = folderId;
                        lastFolder = new Dictionary<string, Dictionary<Guid, byte[]>>(StringComparer.OrdinalIgnoreCase);
                        allFiles.Add(lastFolderKey, lastFolder);
                    }
                    if (string.Compare(path, lastPathKey, true) != 0)
                    {
                        lastPathKey = path;
                        lastPath = new Dictionary<Guid, byte[]>();
                        lastFolder.Add(lastPathKey, lastPath);
                    }
                    lastPath.Add(fileId, imageHash);
                    if (threshold < ImageComparedThreshold)
                        filesToBeCompared.Add(new Tuple<Guid, Guid, string, byte[], CompareImageWith>(fileId, folderId, path, imageHash, folders[folderId]));
                }
                reader.Close();
            }
        }
    }
}
