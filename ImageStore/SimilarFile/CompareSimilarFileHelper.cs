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
    class CompareSimilarFileHelper
    {
        //FolderId, Path, FileId, ImageHash
        Dictionary<Guid, Dictionary<string, Dictionary<Guid, byte[]>>> allFiles;
        //Key = FileId; Value = FolderId, Path, ImageHash, CompareImageWith, SimilarRecordTargetFile
        Dictionary<Guid, Tuple<Guid, string, byte[], CompareImageWith>> filesToBeCompared;
        //FileId, FileId
        Dictionary<Guid, HashSet<Guid>> existingSimilars;
        int finishedFileCount = 0;
        float totalFileCount;
        string totalFileText;

        //IsWarning, Text
        BlockingCollection<Tuple<bool, string>> outputs;
        ConcurrentBag<ErrorRecord> exceptions;
        float imageComparedThreshold;
        int comparingThreadLimit;

        internal CompareSimilarFileHelper(float imageComparedThreshold, Dictionary<Guid, Dictionary<string, Dictionary<Guid, byte[]>>> allFiles, Dictionary<Guid, Tuple<Guid, string, byte[], CompareImageWith>> filesToBeCompared, Dictionary<Guid, HashSet<Guid>> existingSimilars,
            int comparingThreadLimit, BlockingCollection<Tuple<bool, string>> outputs, ConcurrentBag<ErrorRecord> exceptions)
        {
            this.imageComparedThreshold = imageComparedThreshold;
            this.allFiles = allFiles;
            this.filesToBeCompared = filesToBeCompared;
            this.existingSimilars = existingSimilars;
            this.comparingThreadLimit = comparingThreadLimit;
            this.outputs = outputs;
            this.exceptions = exceptions;
            totalFileCount = filesToBeCompared.Count;
            totalFileText = filesToBeCompared.Count.ToString();
        }

        internal void Process()
        {
            Task dbOperator = new Task(DBOperator, TaskCreationOptions.LongRunning);
            dbOperator.Start();

            try
            {
                ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = comparingThreadLimit };
                Parallel.ForEach(filesToBeCompared, options, ProcessOneFile);
            }
            catch (Exception ex)
            {
                outputs.Add(new Tuple<bool, string>(true, ex.ToString()));
                exceptions.Add(new ErrorRecord(ex, "ImageStore Comparing Similar File", ErrorCategory.NotSpecified, null));
            }

            dbJobs.CompleteAdding();
            dbOperator.Wait();

            outputs.CompleteAdding();
        }


        void ProcessOneFile(KeyValuePair<Guid , Tuple<Guid, string, byte[], CompareImageWith>> one)
        {
            ProcessOneFile(one.Key, one.Value.Item1, one.Value.Item2, one.Value.Item3, one.Value.Item4);
        }

        void ProcessOneFile(Guid fileId, Guid fileFolderId, string filePath, byte[] imageHash, CompareImageWith compareImageWith)
        {
            existingSimilars.TryGetValue(fileId, out HashSet<Guid> existingSimilar);

            List<InsertSimilarFileJob> results = new List<InsertSimilarFileJob>();

            if (compareImageWith == CompareImageWith.All)
            {
                foreach (var folder in allFiles)
                {
                    ProcessOneFileTargetsInFolder(fileId, folder.Value, existingSimilar, imageHash, results);
                }
            }
            else if (compareImageWith == CompareImageWith.FilesInOtherDirectories)
            {
                foreach (var folder in allFiles)
                {
                    if (folder.Key == fileFolderId)
                    {
                        foreach (var path in folder.Value)
                        {
                            if (string.Compare(path.Key, filePath, true) == 0) continue;
                            else ProcessOneFileTargetsInPath(fileId, path.Value, existingSimilar, imageHash, results);
                        }
                    }
                    else
                    {
                        ProcessOneFileTargetsInFolder(fileId, folder.Value, existingSimilar, imageHash, results);
                    }
                }
            }
            else if (compareImageWith == CompareImageWith.FilesInOtherFolders)
            {
                foreach (var folder in allFiles)
                {
                    if (folder.Key == fileFolderId) continue;
                    else ProcessOneFileTargetsInFolder(fileId, folder.Value, existingSimilar, imageHash, results);
                }
            }

            dbJobs.Add(new InsertSimilarFileJobs(fileId, results));
        }

        void ProcessOneFileTargetsInFolder(Guid fileId, Dictionary<string, Dictionary<Guid, byte[]>> filesInFolder, HashSet<Guid> existingSimilar, byte[] imageHash, List<InsertSimilarFileJob> result)
        {
            foreach (var path in filesInFolder)
            {
                ProcessOneFileTargetsInPath(fileId, path.Value, existingSimilar, imageHash, result);
            }
        }

        void ProcessOneFileTargetsInPath(Guid fileId, Dictionary<Guid, byte[]> filesInPath, HashSet<Guid> existingSimilar, byte[] imageHash, List<InsertSimilarFileJob> result)
        {
            foreach (var file in filesInPath)
            {
                if (existingSimilar != null && existingSimilar.Contains(file.Key))
                {
                    continue;
                }

                if (filesToBeCompared.ContainsKey(file.Key))
                {
                    if (Comparer<Guid>.Default.Compare(fileId, file.Key) > 0) //fileId is larger than file.Key => leave this job when processing file.Key one.
                    {
                        continue;
                    }
                }

                float cross = 1 - Shipwreck.Phash.CrossCorrelation.GetCrossCorrelation(imageHash, file.Value);
                if (cross <= imageComparedThreshold)
                {
                    result.Add(new InsertSimilarFileJob(file.Key, cross));
                }
            }
        }

        void OutputOneFileFinished()
        {
            var now = Interlocked.Increment(ref finishedFileCount);
            outputs.Add(new Tuple<bool, string>(false, string.Format("{2:P} ({0} of {1}) processed.", now.ToString(), totalFileText, now / totalFileCount)));
        }

        #region DB Job and Output
        BlockingCollection<InsertSimilarFileJobs> dbJobs = new BlockingCollection<InsertSimilarFileJobs>();

        class InsertSimilarFileJobs
        {
            public Guid File1Id { get; }
            public List<InsertSimilarFileJob> Jobs { get; }
            public InsertSimilarFileJobs(Guid file1Id, List<InsertSimilarFileJob> jobs)
            {
                File1Id = file1Id;
                Jobs = jobs;
            }
        }
        class InsertSimilarFileJob
        {
            public Guid File2Id { get; }
            public float Difference { get; }
            public InsertSimilarFileJob(Guid file2Id, float difference)
            {
                File2Id = file2Id;
                Difference = difference;
            }
        }

        void DBOperator()
        {
            var connection = DatabaseConnection.Current;

            using (var commandInserting = new SqlCommand("Insert into [SimilarFile] Values(@Id,@File1Id,@File2Id,@DifferenceDegree,0)"))
            using (var commandUpdating = new SqlCommand("Update [File] Set [ImageComparedThreshold]=@ImageComparedThreshold where [Id]=@Id"))
            {
                commandInserting.Connection = connection;
                commandInserting.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                commandInserting.Parameters.Add(new SqlParameter("@File1Id", System.Data.SqlDbType.UniqueIdentifier));
                commandInserting.Parameters.Add(new SqlParameter("@File2Id", System.Data.SqlDbType.UniqueIdentifier));
                commandInserting.Parameters.Add(new SqlParameter("@DifferenceDegree", System.Data.SqlDbType.Real));
                commandUpdating.Connection = connection;
                commandUpdating.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                commandUpdating.Parameters.Add(new SqlParameter("@ImageComparedThreshold", System.Data.SqlDbType.Real) { Value = imageComparedThreshold });

                while (true)
                {
                    InsertSimilarFileJobs item;
                    try
                    {
                        item = dbJobs.Take();
                    }
                    catch (InvalidOperationException)
                    {
                        outputs.CompleteAdding();
                        break;
                    }

                    try
                    {
                        bool failed = false;
                        var targetCount = item.Jobs.Count;
                        if (targetCount > 0)
                        {
                            commandInserting.Parameters[1].Value = item.File1Id;
                            foreach (var oneJob in item.Jobs)
                            {
                                commandInserting.Parameters[0].Value = Guid.NewGuid();
                                commandInserting.Parameters[2].Value = oneJob.File2Id;
                                commandInserting.Parameters[3].Value = oneJob.Difference;
                                if (commandInserting.ExecuteNonQuery() == 0)
                                {
                                    var text = string.Format("Cannot insert this record. File1: {0}; File2: {1}", item.File1Id, oneJob.File2Id);
                                    outputs.Add(new Tuple<bool, string>(true, text));
                                    exceptions.Add(new ErrorRecord(new InvalidOperationException(text), "ImageStore Comparing Similar File - Insert record", ErrorCategory.WriteError, null));
                                    failed = true;
                                }
                            }
                        }

                        if (failed)
                        {
                            var text = string.Format("Skip updating file record due to failed processes. Id: {0}", item.File1Id);
                            outputs.Add(new Tuple<bool, string>(true, text));
                        }
                        else
                        {
                            commandUpdating.Parameters[0].Value = item.File1Id;
                            if (commandUpdating.ExecuteNonQuery() == 0)
                            {
                                var text = string.Format("Cannot update file record. Id: {0}", item.File1Id);
                                outputs.Add(new Tuple<bool, string>(true, text));
                                exceptions.Add(new ErrorRecord(new InvalidOperationException(text), "ImageStore Comparing Similar File - Update database", ErrorCategory.WriteError, item.File1Id));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        outputs.Add(new Tuple<bool, string>(true, ex.ToString()));
                        exceptions.Add(new ErrorRecord(ex, "ImageStore Comparing Similar File - Operate database", ErrorCategory.ResourceUnavailable, null));
                    }
                    finally
                    {
                        OutputOneFileFinished();
                    }
                }
            }
        }
        #endregion
    }
}
