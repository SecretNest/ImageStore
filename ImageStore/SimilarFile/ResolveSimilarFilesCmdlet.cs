using SecretNest.ImageStore.Extension;
using SecretNest.ImageStore.File;
using SecretNest.ImageStore.Folder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Cmdlet(VerbsDiagnostic.Resolve, "ImageStoreSimilarFiles")]
    [Alias("ResolveSimilarFiles")]
    [OutputType(typeof(List<ImageStoreFile>))]
    public class ResolveSimilarFilesCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        public float? DifferenceDegree { get; set; }

        protected override void BeginProcessing()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
        }

        protected override void ProcessRecord()
        {
            CheckDifferenceDegree();

            WriteVerbose("Loading records into memory...");
            LoadToMemory();
            var allRecordsCount = allRecords.Count;
            if (allRecordsCount == 0)
            {
                WriteVerbose("No record is found.");
                WriteObject(new List<ImageStoreSimilarFile>());
                return;
            }
            else if (allRecordsCount == 1)
            {
                WriteVerbose("Only one record is loaded.");
            }
            else
            {
                WriteVerbose(allRecordsCount.ToString() + " records are loaded.");
            }

            WriteVerbose("Calculating similar files into groups... It may take several minutes to complete.");
            while (true)
            {
                Calculate();

                WriteVerbose("Count of groups: " + groupedRecords.Count.ToString());

                using (SimilarFileManager window = new SimilarFileManager(selectedFiles, allFileInfo, GetFileThumbprint, groupedRecords, groupedFiles, allRecords, fileToRecords, IgnoreSimilarFileHelper.MarkIgnore))
                {
                    WriteVerbose("Please select all files you want to returned from the popped up window.");
                    var result = window.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        WriteObject(selectedFiles.Select(i => allFiles[i]).ToList());
                        break;
                    }
                    else if (result == System.Windows.Forms.DialogResult.Cancel)
                    {
                        WriteObject(null);
                        break;
                    }
                }
                WriteVerbose("Refreshing groups...");
            }

            Parallel.ForEach(loadedThumbprints.Values, i => i.Dispose());
        }

        HashSet<Guid> selectedFiles = new HashSet<Guid>();
        Dictionary<Guid, ImageStoreFile> allFiles = new Dictionary<Guid, ImageStoreFile>();
        Dictionary<Guid, FileInfo> allFileInfo = new Dictionary<Guid, FileInfo>();
        ConcurrentDictionary<Guid, Image> loadedThumbprints = new ConcurrentDictionary<Guid, Image>();
        Dictionary<Guid, ImageStoreFolder> allFolders = new Dictionary<Guid, ImageStoreFolder>();
        Dictionary<Guid, string> allExtensionNames = new Dictionary<Guid, string>();

        #region CheckDifferenceDegree
        void CheckDifferenceDegree()
        {
            var dbValue = GetMininalImageComparedThreshold();
            if (dbValue == float.NaN)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("No file is found."), "ImageStore Select Similar File", ErrorCategory.ObjectNotFound, null));
            }
            else if (dbValue == 0)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Some image file is not compared yet."), "ImageStore Select Similar File", ErrorCategory.LimitsExceeded, null));
            }

            if (DifferenceDegree.HasValue)
            {
                if (DifferenceDegree.Value > dbValue)
                {
                    WriteError(new ErrorRecord(new InvalidOperationException(nameof(DifferenceDegree) + " is set larger than the value acceptable, which should be the least of the threshold of compared files. The least value is taken in place."), "ImageStore Select Similar File", ErrorCategory.LimitsExceeded, null));
                    DifferenceDegree = dbValue;
                }
            }
            else
            {
                DifferenceDegree = dbValue;
            }

            WriteInformation("Difference Degree: " + DifferenceDegree.Value.ToString(), new string[] { "ResolveSimilarFile", "DifferenceDegree" });
        }

        float GetMininalImageComparedThreshold()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Select min([ImageComparedThreshold]) from [file] where [ImageHash] is not null and [FileState]=255"))
            {
                command.Connection = connection;
                var result = command.ExecuteScalar();

                if (result == DBNull.Value)
                    return float.NaN;
                else
                    return (float)result;
            }
        }
        #endregion

        Dictionary<Guid, ImageStoreSimilarFile> allRecords = new Dictionary<Guid, ImageStoreSimilarFile>();
        Dictionary<Guid, List<Guid>> fileToRecords = new Dictionary<Guid, List<Guid>>();

        void LoadToMemory()
        {
            foreach (var folder in FolderHelper.GetAllFolders())
                allFolders.Add(folder.Id, folder);

            foreach (var extension in ExtensionHelper.GetAllExtensions())
                allExtensionNames.Add(extension.Id, extension.Extension);

            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Create table #tempSimilarFile ([Id] uniqueidentifier, [File1Id] uniqueidentifier, [File2Id] uniqueidentifier, [DifferenceDegree] real, [IgnoredMode] int)"))
            {
                command.Connection = connection;
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("insert into #tempSimilarFile Select [Id],[File1Id],[File2Id],[DifferenceDegree],[IgnoredMode] from [SimilarFile] where [DifferenceDegree]<=@DifferenceDegree"))
            {
                command.Parameters.Add(new SqlParameter("@DifferenceDegree", System.Data.SqlDbType.Real) { Value = DifferenceDegree });
                command.Connection = connection;
                command.CommandTimeout = 180;
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("Select [Id],[File1Id],[File2Id],[DifferenceDegree],[IgnoredMode] from #tempSimilarFile"))
            {
                command.Connection = connection;
                command.CommandTimeout = 180;
                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreSimilarFile line = new ImageStoreSimilarFile((Guid)reader[0], (Guid)reader[1], (Guid)reader[2], (float)reader[3])
                        {
                            IgnoredModeCode = (int)reader[4]
                        };
                        allRecords.Add(line.Id, line);

                        if (!fileToRecords.TryGetValue(line.File1Id, out var records))
                        {
                            records = new List<Guid>();
                            fileToRecords.Add(line.File1Id, records);
                        }
                        records.Add(line.Id);
                        if (!fileToRecords.TryGetValue(line.File2Id, out records))
                        {
                            records = new List<Guid>();
                            fileToRecords.Add(line.File2Id, records);
                        }
                        records.Add(line.Id);
                    }
                }
            }

            using (var command = new SqlCommand("Create table #tempSimilarFileId ([Id] uniqueidentifier)"))
            {
                command.Connection = connection;
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("insert into #tempSimilarFileId select distinct * from (select [File1Id] from #tempSimilarFile union select [File2Id] from #tempSimilarFile) IdTable"))
            {
                command.Connection = connection;
                command.CommandTimeout = 180;
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("drop table #tempSimilarFile"))
            {
                command.Connection = connection;
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("Select [Id],[FolderId],[Path],[FileName],[ExtensionId],[ImageHash],[Sha1Hash],[FileSize],[FileState],[ImageComparedThreshold] from [File] Where [Id] in (select [id] from #tempSimilarFileId)"))
            {
                command.Connection = connection;
                command.CommandTimeout = 180;

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    ImageStoreFile result;
                    while (reader.Read())
                    {
                        result = new ImageStoreFile((Guid)reader[0], (Guid)reader[1], (string)reader[2], (string)reader[3], (Guid)reader[4])
                        {
                            ImageHash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[5]),
                            Sha1Hash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[6]),
                            FileSize = (int)reader[7],
                            FileStateCode = (int)reader[8],
                            ImageComparedThreshold = (float)reader[9]
                        };
                        allFiles.Add(result.Id, result);
                        allFileInfo.Add(result.Id, new FileInfo());
                    }
                    reader.Close();
                }
            }

            using (var command = new SqlCommand("drop table #tempSimilarFileId"))
            {
                command.Connection = connection;
                command.ExecuteNonQuery();
            }

            Parallel.ForEach(allFiles.Values, i => allFileInfo[i.Id].SetData(i, allFolders[i.FolderId], allExtensionNames[i.ExtensionId]));
        }

        Dictionary<int, List<Guid>> groupedRecords;
        Dictionary<int, Dictionary<Guid, List<Guid>>> groupedFiles; //group, fileid, recordId
        void Calculate()
        {
            int groupId = 0;
            List<Guid> leftRecords = new List<Guid>();
            HashSet<Guid> filesProcessed = new HashSet<Guid>();

            groupedRecords = new Dictionary<int, List<Guid>>();
            groupedFiles = new Dictionary<int, Dictionary<Guid, List<Guid>>>();
            var disconnectedGroup = new List<Guid>();
            var disconnectedFiles = new Dictionary<Guid, List<Guid>>();
            foreach (var record in allRecords)
            {
                if (record.Value.IgnoredMode == IgnoredMode.HiddenAndDisconnected)
                {
                    disconnectedGroup.Add(record.Key);
                    if (!disconnectedFiles.TryGetValue(record.Value.File1Id, out var list))
                    {
                        list = new List<Guid>();
                        disconnectedFiles.Add(record.Value.File1Id, list);
                    }
                    list.Add(record.Key);
                    if (!disconnectedFiles.TryGetValue(record.Value.File2Id, out list))
                    {
                        list = new List<Guid>();
                        disconnectedFiles.Add(record.Value.File2Id, list);
                    }
                    list.Add(record.Key);
                }
                else
                    leftRecords.Add(record.Key);
            }
            if (disconnectedGroup.Count > 0)
            {
                groupedRecords.Add(-1, disconnectedGroup);
                groupedFiles.Add(-1, disconnectedFiles);
            }

            if (leftRecords.Count == 0) return;

            BlockingCollection<Guid> needToPrepareThumbprints = new BlockingCollection<Guid>();
            Thread preparingFileThumbprints = new Thread(PreparingFileThumbprints);
            preparingFileThumbprints.Start(needToPrepareThumbprints);

            while (leftRecords.Count > 0)
            {
                var firstRecord = allRecords[leftRecords[0]];
                leftRecords.RemoveAt(0);

                List<Guid> currentRecordsGroup = new List<Guid>();
                Dictionary<Guid, List<Guid>> currentFilesGroup = new Dictionary<Guid, List<Guid>>();

                groupedRecords.Add(groupId, currentRecordsGroup);
                groupedFiles.Add(groupId++, currentFilesGroup);

                Queue<Guid> filesToProcess = new Queue<Guid>();
                currentRecordsGroup.Add(firstRecord.Id);
                if (!filesProcessed.Contains(firstRecord.File1Id))
                    filesToProcess.Enqueue(firstRecord.File1Id);
                if (!filesProcessed.Contains(firstRecord.File1Id))
                    filesToProcess.Enqueue(firstRecord.File2Id);
                currentFilesGroup.Add(firstRecord.File1Id, new List<Guid>() { firstRecord.Id });
                needToPrepareThumbprints.Add(firstRecord.File1Id);
                currentFilesGroup.Add(firstRecord.File2Id, new List<Guid>() { firstRecord.Id });
                needToPrepareThumbprints.Add(firstRecord.File2Id);

                while (filesToProcess.Count > 0)
                {
                    var fileToProcess = filesToProcess.Dequeue();
                    filesProcessed.Add(fileToProcess);

                    var records = fileToRecords[fileToProcess];

                    foreach (var recordId in records)
                    {
                        if (leftRecords.Remove(recordId))
                        {
                            var record = allRecords[recordId];
                            if (!filesProcessed.Contains(record.File1Id))
                                filesToProcess.Enqueue(record.File1Id);
                            if (!filesProcessed.Contains(record.File2Id))
                                filesToProcess.Enqueue(record.File2Id);
                            currentRecordsGroup.Add(recordId);
                            if (!currentFilesGroup.TryGetValue(record.File1Id, out var list))
                            {
                                list = new List<Guid>();
                                currentFilesGroup.Add(record.File1Id, list);
                                needToPrepareThumbprints.Add(record.File1Id);
                            }
                            list.Add(record.Id);
                            if (!currentFilesGroup.TryGetValue(record.File2Id, out list))
                            {
                                list = new List<Guid>();
                                currentFilesGroup.Add(record.File2Id, list);
                                needToPrepareThumbprints.Add(record.File2Id);
                            }
                            list.Add(record.Id);
                        }
                    }
                }
            }
            needToPrepareThumbprints.CompleteAdding();
            preparingFileThumbprints.Join();
        }

        void PreparingFileThumbprints(object parameter)
        {
            BlockingCollection<Guid> needToPrepareThumbprints = (BlockingCollection<Guid>)parameter;
            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };

            Parallel.ForEach(needToPrepareThumbprints.GetConsumingEnumerable(), parallelOptions, i => GetFileThumbprint(i));
        }

        Image GetFileThumbprint(Guid fileId)
        {
            return loadedThumbprints.GetOrAdd(fileId, i => LoadImageHelper.GetThumbprintImage(fileId, allFileInfo[i].FilePath));
        }
    }
}
