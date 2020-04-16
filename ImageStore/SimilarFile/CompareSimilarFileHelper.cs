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
        SortedDictionary<Guid, Tuple<Guid, string, byte[], CompareImageWith>> filesToBeCompared;
        int finishedFileCount = 0;
        string totalFileText;

        //IsWarning, Text
        BlockingCollection<Tuple<bool, string>> outputs;
        ConcurrentBag<ErrorRecord> exceptions;
        float imageComparedThreshold;
        int comparingThreadLimit;

        internal CompareSimilarFileHelper(float imageComparedThreshold, Dictionary<Guid, Dictionary<string, Dictionary<Guid, byte[]>>> allFiles, SortedDictionary<Guid, Tuple<Guid, string, byte[], CompareImageWith>> filesToBeCompared,
            int comparingThreadLimit, BlockingCollection<Tuple<bool, string>> outputs, ConcurrentBag<ErrorRecord> exceptions)
        {
            this.imageComparedThreshold = imageComparedThreshold;
            this.allFiles = allFiles;
            this.filesToBeCompared = filesToBeCompared;
            this.comparingThreadLimit = comparingThreadLimit;
            this.outputs = outputs;
            this.exceptions = exceptions;
            totalFileText = filesToBeCompared.Count.ToString();
        }

        internal void Process()
        {
            Task dbOperator = new Task(DBOperator, TaskCreationOptions.LongRunning);
            dbOperator.Start();

            jobs = new BlockingCollection<Tuple<Guid, byte[], List<KeyValuePair<Guid, byte[]>>>>(50);

            Task pub = new Task(FillJobs, TaskCreationOptions.LongRunning);
            pub.Start();

            Task[] subs = new Task[comparingThreadLimit];
            for (int i = 0; i < comparingThreadLimit; i++)
            {
                subs[i] = new Task(DoJobs, TaskCreationOptions.LongRunning);
                subs[i].Start();
            }

            pub.Wait();
            Task.WaitAll(subs);

            dbJobs.CompleteAdding();
            dbOperator.Wait();
        }

        #region Pub/Sub
        //File1Id, File1ImageHash, File2s
        BlockingCollection<Tuple<Guid, byte[], List<KeyValuePair<Guid, byte[]>>>> jobs = null;

        void FillJobs()
        {
            Parallel.ForEach(filesToBeCompared, fileToBeCompared =>
            {
                var targets = FindTargets(fileToBeCompared.Key, fileToBeCompared.Value.Item1, fileToBeCompared.Value.Item2, fileToBeCompared.Value.Item4);
                jobs.Add(new Tuple<Guid, byte[], List<KeyValuePair<Guid, byte[]>>>(fileToBeCompared.Key, fileToBeCompared.Value.Item3, targets));
            });

            jobs.CompleteAdding();
        }

        void DoJobs()
        {
            while (true)
            {
                try
                {
                    var job = jobs.Take();
                    ProcessOneFileJobs(job.Item1, job.Item2, job.Item3);
                }
                catch (InvalidOperationException)
                {
                    outputs.CompleteAdding();
                    break;
                }
            }
        }

        #endregion

        #region Find Targets
        List<KeyValuePair<Guid, byte[]>> FindTargets(Guid fileId, Guid fileFolderId, string filePath, CompareImageWith compareImageWith)
        {
            List<KeyValuePair<Guid, byte[]>> result = new List<KeyValuePair<Guid, byte[]>>();
            HashSet<Guid> compared = GetComparedSimilarFiles(fileId);

            if (compareImageWith == CompareImageWith.All)
            {
                foreach (var folder in allFiles)
                {
                    FillTargetsInFolder(fileId, folder.Value, compared, result);
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
                            else FillTargetsInPath(fileId, path.Value, compared, result);
                        }
                    }
                    else
                    {
                        FillTargetsInFolder(fileId, folder.Value, compared, result);
                    }
                }
            }
            else if (compareImageWith == CompareImageWith.FilesInOtherFolders)
            {
                foreach (var folder in allFiles)
                {
                    if (folder.Key == fileFolderId) continue;
                    else FillTargetsInFolder(fileId, folder.Value, compared, result);
                }
            }
            
            return result;
        }

        void FillTargetsInFolder(Guid fileId, Dictionary<string, Dictionary<Guid, byte[]>> filesInFolder, HashSet<Guid> compared, List<KeyValuePair<Guid, byte[]>> result)
        {
            foreach (var path in filesInFolder)
            {
                FillTargetsInPath(fileId, path.Value, compared, result);
            }
        }

        void FillTargetsInPath(Guid fileId, Dictionary<Guid, byte[]> filesInPath, HashSet<Guid> compared, List<KeyValuePair<Guid, byte[]>> result)
        {
            foreach (var file in filesInPath)
            {
                if (compared.Contains(file.Key))
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

                result.Add(new KeyValuePair<Guid, byte[]>(file.Key, file.Value));
            }
        }

        HashSet<Guid> GetComparedSimilarFiles(Guid fileId)
        {
            var item = new ReadingExistsJob(fileId);
            dbJobs.Add(item);
            item.Finished.WaitOne();
            item.Finished.Dispose();
            return item.Result;
        }
        #endregion

        #region Compare
        void ProcessOneFileJobs(Guid file1Id, byte[] file1Hash, List<KeyValuePair<Guid, byte[]>> targets)
        {
            List<InsertSimilarFileJob> results = new List<InsertSimilarFileJob>();
            try
            {
                foreach (var target in targets)
                {
                    var oneResult = ProcessOneJob(file1Hash, target.Key, target.Value);
                    if (oneResult != null)
                    {
                        results.Add(oneResult);
                    }
                }
            }
            catch (Exception ex)
            {
                outputs.Add(new Tuple<bool, string>(true, ex.ToString()));
                exceptions.Add(new ErrorRecord(ex, "ImageStore Comparing Similar File", ErrorCategory.NotSpecified, null));
                OutputOneFileFinished();
                return;
            }

            dbJobs.Add(new InsertSimilarFileJobs(file1Id, results));
        }

        InsertSimilarFileJob ProcessOneJob(byte[] file1Hash, Guid file2Id, byte[] file2Hash)
        {
            float cross = 1 - Shipwreck.Phash.CrossCorrelation.GetCrossCorrelation(file1Hash, file2Hash);
            if (cross <= imageComparedThreshold)
            {
                return new InsertSimilarFileJob(file2Id, cross);
            }
            else
            {
                return null;
            }
        }

        void OutputOneFileFinished()
        {
            var now = Interlocked.Increment(ref finishedFileCount);
            outputs.Add(new Tuple<bool, string>(false, string.Format("The {0} of {1} is processed.", now.ToString(), totalFileText)));
        }
        #endregion

        #region DB Job and Output
        BlockingCollection<DBJob> dbJobs = new BlockingCollection<DBJob>();

        abstract class DBJob
        {
            public abstract int JobType { get; }
        }
        class InsertSimilarFileJobs : DBJob
        {
            public Guid File1Id { get; }
            public override int JobType => 1;
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
        class ReadingExistsJob : DBJob
        {
            public Guid FileId { get; }
            public HashSet<Guid> Result { get; } = new HashSet<Guid>();
            public ManualResetEvent Finished { get; } = new ManualResetEvent(false);
            public override int JobType => 2;
            public ReadingExistsJob(Guid fileId)
            { FileId = fileId; }
        }

        void DBOperator()
        {
            var connection = DatabaseConnection.Current;

            using (var commandInserting = new SqlCommand("Insert into [SimilarFile] Values(@Id,@File1Id,@File2Id,@DifferenceDegree,0)"))
            using (var commandUpdating = new SqlCommand("Update [File] Set [ImageComparedThreshold]=@ImageComparedThreshold where [Id]=@Id"))
            using (var commandReading1 = new SqlCommand("Select [File1Id] from [SimilarFile] where [File2Id]=@Id"))
            using (var commandReading2 = new SqlCommand("Select [File2Id] from [SimilarFile] where [File1Id]=@Id"))
            {
                commandInserting.Connection = connection;
                commandInserting.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                commandInserting.Parameters.Add(new SqlParameter("@File1Id", System.Data.SqlDbType.UniqueIdentifier));
                commandInserting.Parameters.Add(new SqlParameter("@File2Id", System.Data.SqlDbType.UniqueIdentifier));
                commandInserting.Parameters.Add(new SqlParameter("@DifferenceDegree", System.Data.SqlDbType.Real));
                commandUpdating.Connection = connection;
                commandUpdating.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                commandUpdating.Parameters.Add(new SqlParameter("@ImageComparedThreshold", System.Data.SqlDbType.Real) { Value = imageComparedThreshold });
                commandReading1.Connection = connection;
                commandReading1.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                commandReading2.Connection = connection;
                commandReading2.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));

                while (true)
                {
                    try
                    {
                        var job = dbJobs.Take();
                        if (job.JobType == 2)
                        {
                            var item = job as ReadingExistsJob;
                            commandReading1.Parameters[0].Value = item.FileId;
                            using (var reader = commandReading1.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                            {
                                while (reader.Read())
                                {
                                    item.Result.Add((Guid)reader[0]);
                                }
                                reader.Close();
                            }
                            commandReading2.Parameters[0].Value = item.FileId;
                            using (var reader = commandReading2.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                            {
                                while (reader.Read())
                                {
                                    item.Result.Add((Guid)reader[0]);
                                }
                                reader.Close();
                            }
                            item.Finished.Set();
                        }
                        else if (job.JobType == 1)
                        {
                            var item = job as InsertSimilarFileJobs;
                            bool failed = false;
                            var targetCount = item.Jobs.Count;
                            if (targetCount > 0)
                            {
                                commandInserting.Parameters[0].Value = Guid.NewGuid();
                                commandInserting.Parameters[1].Value = item.File1Id;
                                foreach (var oneJob in item.Jobs)
                                {
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
                            OutputOneFileFinished();
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        outputs.CompleteAdding();
                        break;
                    }
                    //catch (Exception ex)
                    //{
                    //    outputs.Add(new Tuple<bool, string>(true, ex.ToString()));
                    //    exceptions.Add(new ErrorRecord(ex, "ImageStore Comparing Similar File - Operate database", ErrorCategory.ResourceUnavailable, null));
                    //}
                }
            }
        }
        #endregion
    }
}
