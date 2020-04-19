using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    partial class ResolveSimilarFilesCmdlet
    {
        Dictionary<int, Dictionary<Guid, List<Guid>>> groupedFiles; //index (-1: disconnected), fileId, recordId

        void ProcessInGroup()
        {
            WriteVerbose("Calculating similar files into groups... It may take several minutes to complete.");
            while (true)
            {
                CalculateIntoGroup();

                var groupCount = groupedFiles.Count;
                if (groupCount > 0)
                {
                    WriteVerbose("Count of groups: " + groupCount.ToString());
                    using (SimilarFileInGroupManager window = new SimilarFileInGroupManager(selectedFiles, allFileInfo, GetFileThumbprint, groupedFiles, allRecords, IgnoreSimilarFileHelper.MarkIgnore))
                    {
                        WriteVerbose("Please check all files you want to returned from the popped up window.");
                        var result = window.ShowDialog();
                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            WriteOutput();
                            break;
                        }
                        else if (result == System.Windows.Forms.DialogResult.Cancel)
                        {
                            WriteVerbose("Canceled.");
                            WriteObject(null);
                            break;
                        }
                    }
                    WriteVerbose("Refreshing groups...");
                }
                else
                {
                    WriteVerbose("No record is found.");
                    WriteOutput();
                    break;
                }
            }
        }

        void CalculateIntoGroup()
        {
            List<Guid> leftRecords = new List<Guid>();
            HashSet<Guid> filesProcessed = new HashSet<Guid>();

            groupedFiles = new Dictionary<int, Dictionary<Guid, List<Guid>>>();

            Dictionary<Guid, List<Guid>> disconnectedFiles = null;
            if (IncludesDisconnected)
            {
                disconnectedFiles = new Dictionary<Guid, List<Guid>>();
                foreach (var record in allRecords)
                {
                    if (record.Value.IgnoredMode == IgnoredMode.HiddenAndDisconnected)
                    {
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
            }
            else
            {
                foreach (var record in allRecords)
                {
                    if (record.Value.IgnoredMode != IgnoredMode.HiddenAndDisconnected)
                        leftRecords.Add(record.Key);
                }
            }

            if (leftRecords.Count != 0)
            {
                //FileId, SimilarRecordID
                var unsortedGroup = new List<Dictionary<Guid, List<Guid>>>();

                BlockingCollection<Guid> needToPrepareThumbprints = null;
                Thread preparingFileThumbprints = null;
                if (LoadImageHelper.cachePath != null)
                {
                    needToPrepareThumbprints = new BlockingCollection<Guid>();
                    preparingFileThumbprints = new Thread(PreparingFileThumbprints);
                    preparingFileThumbprints.Start(needToPrepareThumbprints);
                }

                while (leftRecords.Count > 0)
                {
                    var firstRecord = allRecords[leftRecords[0]];
                    leftRecords.RemoveAt(0);

                    Dictionary<Guid, List<Guid>> currentFilesGroup = new Dictionary<Guid, List<Guid>>();
                    unsortedGroup.Add(currentFilesGroup);

                    Queue<Guid> filesToProcess = new Queue<Guid>();
                    if (!filesProcessed.Contains(firstRecord.File1Id))
                        filesToProcess.Enqueue(firstRecord.File1Id);
                    if (!filesProcessed.Contains(firstRecord.File1Id))
                        filesToProcess.Enqueue(firstRecord.File2Id);
                    currentFilesGroup.Add(firstRecord.File1Id, new List<Guid>() { firstRecord.Id });
                    needToPrepareThumbprints?.Add(firstRecord.File1Id);
                    currentFilesGroup.Add(firstRecord.File2Id, new List<Guid>() { firstRecord.Id });
                    needToPrepareThumbprints?.Add(firstRecord.File2Id);

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
                                if (!currentFilesGroup.TryGetValue(record.File1Id, out var list))
                                {
                                    list = new List<Guid>();
                                    currentFilesGroup.Add(record.File1Id, list);
                                    needToPrepareThumbprints?.Add(record.File1Id);
                                }
                                list.Add(record.Id);
                                if (!currentFilesGroup.TryGetValue(record.File2Id, out list))
                                {
                                    list = new List<Guid>();
                                    currentFilesGroup.Add(record.File2Id, list);
                                    needToPrepareThumbprints?.Add(record.File2Id);
                                }
                                list.Add(record.Id);
                            }
                        }
                    }
                    
                    var sortedGroup = unsortedGroup.OrderByDescending(i => i.Values.Sum(j => j.Count(k => allRecords[k].IgnoredMode == IgnoredMode.Effective))); //order by descending: total effective records in group.
                    var groupIndex = 0;
                    foreach (var item in sortedGroup)
                    {
                        groupedFiles.Add(groupIndex++, item);
                    }
                }

                if (LoadImageHelper.cachePath != null)
                {
                    needToPrepareThumbprints.CompleteAdding();

                    preparingFileThumbprints.Join();
                    needToPrepareThumbprints.Dispose();
                }
            }

            if (disconnectedFiles?.Count > 0)
            {
                groupedFiles.Add(-1, disconnectedFiles);
            }
        }

        void PreparingFileThumbprints(object parameter)
        {
            BlockingCollection<Guid> needToPrepareThumbprints = (BlockingCollection<Guid>)parameter;
            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };

            Parallel.ForEach(needToPrepareThumbprints.GetConsumingEnumerable(), parallelOptions, i => LoadImageHelper.BuildCache(i, allFileInfo[i].FilePath));
        }

    }
}
