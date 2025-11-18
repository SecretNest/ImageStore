using System;
using System.Collections;
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
        Dictionary<int, Dictionary<Guid, HashSet<Guid>>> groupedFiles; //index (-1: disconnected), fileId, recordId

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
                    using (SimilarFileInGroupManager window = new SimilarFileInGroupManager(selectedFiles, allFileInfo, LoadFileThumb, groupedFiles, allRecords, IgnoreSimilarFileHelper.MarkIgnore))
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
            HashSet<Guid> leftRecords = new HashSet<Guid>();
            HashSet<Guid> filesProcessed = new HashSet<Guid>();

            groupedFiles = new Dictionary<int, Dictionary<Guid, HashSet<Guid>>>();

            Dictionary<Guid, HashSet<Guid>> disconnectedFiles = null;
            if (IncludesDisconnected)
            {
                disconnectedFiles = new Dictionary<Guid, HashSet<Guid>>();
                foreach (var record in allRecords)
                {
                    if (record.Value.IgnoredMode == IgnoredMode.HiddenAndDisconnected)
                    {
                        if (!disconnectedFiles.TryGetValue(record.Value.File1Id, out var list))
                        {
                            list = new HashSet<Guid>();
                            disconnectedFiles.Add(record.Value.File1Id, list);
                        }
                        list.Add(record.Key);
                        if (!disconnectedFiles.TryGetValue(record.Value.File2Id, out list))
                        {
                            list = new HashSet<Guid>();
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
                //FileId, (Count of Effective records, SimilarRecordID)
                var unsortedGroup = new List<Tuple<int, Dictionary<Guid, HashSet<Guid>>>>();

                var needToPrepareThumbprints = new BlockingCollection<Guid>();
                var preparingFileThumbprints = new Thread(PreparingFileThumbprints);
                preparingFileThumbprints.Start(needToPrepareThumbprints);

                while (true)
                {
                    var processingRecordId = leftRecords.FirstOrDefault();
                    if (processingRecordId == Guid.Empty) break;

                    var processingRecord = allRecords[processingRecordId];
                    leftRecords.Remove(processingRecordId);

                    //FileId, SimilarRecordID
                    Dictionary<Guid, HashSet<Guid>> currentFilesGroup = new Dictionary<Guid, HashSet<Guid>>();
                    var effectiveRecordCount = 0;
                    Queue<Guid> filesToProcess = new Queue<Guid>();

                    if (!filesProcessed.Contains(processingRecord.File1Id))
                        filesToProcess.Enqueue(processingRecord.File1Id);
                    if (!filesProcessed.Contains(processingRecord.File2Id))
                        filesToProcess.Enqueue(processingRecord.File2Id);
                    currentFilesGroup.Add(processingRecord.File1Id, new HashSet<Guid>() { processingRecord.Id });
                    if (processingRecord.IgnoredMode == IgnoredMode.Effective) effectiveRecordCount++;
                    needToPrepareThumbprints?.Add(processingRecord.File1Id);
                    currentFilesGroup.Add(processingRecord.File2Id, new HashSet<Guid>() { processingRecord.Id });
                    if (processingRecord.IgnoredMode == IgnoredMode.Effective) effectiveRecordCount++;
                    needToPrepareThumbprints?.Add(processingRecord.File2Id);

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
                                    list = new HashSet<Guid>();
                                    currentFilesGroup.Add(record.File1Id, list);
                                    needToPrepareThumbprints?.Add(record.File1Id);
                                }
                                if (list.Add(record.Id))
                                    if (record.IgnoredMode == IgnoredMode.Effective) effectiveRecordCount++;
                                if (!currentFilesGroup.TryGetValue(record.File2Id, out list))
                                {
                                    list = new HashSet<Guid>();
                                    currentFilesGroup.Add(record.File2Id, list);
                                    needToPrepareThumbprints?.Add(record.File2Id);
                                }
                                if (list.Add(record.Id))
                                    if (record.IgnoredMode == IgnoredMode.Effective) effectiveRecordCount++;
                            }
                        }
                    }

                    unsortedGroup.Add(new Tuple<int, Dictionary<Guid, HashSet<Guid>>>(effectiveRecordCount, currentFilesGroup));
                }

                var sortedGroup = unsortedGroup.OrderByDescending(i => i.Item1); //order by descending: total effective records in group.
                var groupIndex = 0;
                foreach (var item in sortedGroup)
                {
                    groupedFiles.Add(groupIndex++, item.Item2);
                }

                needToPrepareThumbprints.CompleteAdding();

                preparingFileThumbprints.Join();
                needToPrepareThumbprints.Dispose();
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

            Parallel.ForEach(needToPrepareThumbprints.GetConsumingEnumerable(), parallelOptions, i => PrepareFileThumb(i));
        }

    }
}
