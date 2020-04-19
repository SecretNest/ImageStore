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
        List<Tuple<Guid, List<ImageStoreSimilarFile>>> ungroupedFiles; //fileId, recordId; ordered

        void ProcessInUngroup()
        {
            WriteVerbose("Calculating similar files into groups... It may take several minutes to complete.");
            while (true)
            {
                CalculateIntoUngroup();

                var fileCount = ungroupedFiles.Count;
                if (fileCount == 0)
                {
                    WriteVerbose("No record is found.");
                    WriteOutput();
                    break;
                }

                WriteVerbose("Count of files: " + fileCount.ToString());

                if (FileId != Guid.Empty)
                {
                    var file = ungroupedFiles.First(i => i.Item1 == FileId);
                    using (SimilarFileOneManager window = new SimilarFileOneManager(selectedFiles, allFileInfo, file.Item1, file.Item2, IgnoreSimilarFileHelper.MarkIgnore))
                    {
                        WriteVerbose("Please check all files you want to returned from the popped up window.");
                        var result = window.ShowDialog();
                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            WriteOutput();
                        }
                        else
                        {
                            WriteVerbose("Canceled.");
                            WriteObject(null);
                        }
                        break;
                    }
                }
                else
                {
                    using (SimilarFilesManager window = new SimilarFilesManager(selectedFiles, allFileInfo, GetFileThumbprint, ungroupedFiles, IgnoreSimilarFileHelper.MarkIgnore))
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
                    WriteVerbose("Refreshing files...");
                }
            }
        }

        void CalculateIntoUngroup()
        {
            if (IncludesDisconnected)
            {
                ungroupedFiles = fileToRecords.Select(i => new Tuple<Guid, List<ImageStoreSimilarFile>>(
                    i.Key,
                    i.Value.Select(j => allRecords[j]).ToList()))
                    .OrderByDescending(i => i.Item2.Count).ToList();
            }
            else
            {
                ungroupedFiles = fileToRecords.Select(i => new Tuple<Guid, List<ImageStoreSimilarFile>>(
                    i.Key,
                    i.Value.Select(j => allRecords[j]).Where(j => j.IgnoredMode != IgnoredMode.HiddenAndDisconnected).ToList()))
                    .Where(i => i.Item2.Count > 0)
                    .OrderByDescending(i => i.Item2.Count).ToList();
            }

            if (LoadImageHelper.cachePath != null)
            {
                ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
                Parallel.ForEach(ungroupedFiles.Select(i=>i.Item1), parallelOptions, i => LoadImageHelper.BuildCache(i, allFileInfo[i].FilePath));
            }
        }
    }
}
