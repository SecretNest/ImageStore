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
        Dictionary<int, List<Guid>> groupedRecords;
        Dictionary<int, Dictionary<Guid, List<Guid>>> groupedFiles; //group, fileid, recordId


        void ProcessInGroup()
        {
            WriteVerbose("Calculating similar files into groups... It may take several minutes to complete.");
            while (true)
            {
                CalculateIntoGroup();

                var groupCount = groupedRecords.Count;
                if (groupCount > 0)
                {
                    WriteVerbose("Count of groups: " + groupCount.ToString());
                    using (SimilarFileInGroupManager window = new SimilarFileInGroupManager(selectedFiles, allFileInfo, GetFileThumbprint, groupedRecords, groupedFiles, allRecords, IgnoreSimilarFileHelper.MarkIgnore))
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
            int groupId = 0;
            List<Guid> leftRecords = new List<Guid>();
            HashSet<Guid> filesProcessed = new HashSet<Guid>();

            groupedRecords = new Dictionary<int, List<Guid>>();
            groupedFiles = new Dictionary<int, Dictionary<Guid, List<Guid>>>();

            if (IncludesDisconnected)
            {
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
            }
            else
            {
                foreach (var record in allRecords)
                {
                    if (record.Value.IgnoredMode != IgnoredMode.HiddenAndDisconnected)
                        leftRecords.Add(record.Key);
                }
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
            needToPrepareThumbprints.Dispose();
        }
    }
}
