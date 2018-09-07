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
        CompareImageWith compareImageWith;
        //FolderId, Path, FileId, ImageHash
        Dictionary<Guid, Dictionary<string, Dictionary<Guid, byte[]>>> files;
        HashSet<Guid> processedFiles;
        BlockingCollection<Tuple<bool, string>> outputs;
        ConcurrentBag<ErrorRecord> exceptions;
        float imageComparedThreshold;
        int comparingThreadLimit;
        string fileIndex;

        Guid fileId, folderId;
        string path;
        byte[] imageHash;

        Thread dbOperator;

        internal CompareSimilarFileHelper(float imageComparedThreshold, Dictionary<Guid, Dictionary<string, Dictionary<Guid, byte[]>>> files, HashSet<Guid> processedFiles,
            int comparingThreadLimit, BlockingCollection<Tuple<bool, string>> outputs, ConcurrentBag<ErrorRecord> exceptions)
        {
            this.imageComparedThreshold = imageComparedThreshold;
            this.files = files;
            this.processedFiles = processedFiles;
            this.comparingThreadLimit = comparingThreadLimit;
            this.outputs = outputs;
            this.exceptions = exceptions;

            dbOperator = new Thread(DBOperator);
            dbOperator.Start();
        }

        internal void Process(string fileIndex, Guid fileId, Guid folderId, string path, byte[] imageHash, CompareImageWith compareImageWith)
        {
            this.fileIndex = fileIndex;
            this.fileId = fileId;
            this.folderId = folderId;
            this.path = path;
            this.imageHash = imageHash;
            this.compareImageWith = compareImageWith;

            processedFiles.Add(fileId);

            IEnumerable<KeyValuePair<Guid, byte[]>> getFileToBeCompared;
            if (compareImageWith == CompareImageWith.All)
                getFileToBeCompared = CompareWithAllFiles(GetComparedSimilarFiles());
            else if (compareImageWith == CompareImageWith.FilesInOtherDirectories)
                getFileToBeCompared = CompareWithFilesInOtherDirectories(GetComparedSimilarFiles());
            else
                getFileToBeCompared = CompareWithFilesInOtherFolders(GetComparedSimilarFiles());

            List<KeyValuePair<Guid, byte[]>> targets = new List<KeyValuePair<Guid, byte[]>>(getFileToBeCompared);
            var targetsCount = targets.Count;

            if (targetsCount == 0)
            {
                outputs.Add(new Tuple<bool, string>(false, string.Format("Processing file {0}: Nothing to be compared with.", fileIndex)));
            }
            else
            {
                if (targetsCount == 1)
                {
                    outputs.Add(new Tuple<bool, string>(false, string.Format("Processing file {0}: Comparing with 1 file.", fileIndex)));
                }
                else
                {
                    outputs.Add(new Tuple<bool, string>(false, string.Format("Processing file {0}: Comparing with {1} files.", fileIndex, targetsCount)));
                }
                try
                {
                    ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = comparingThreadLimit };
                    Parallel.ForEach(targets, options, Compare);
                }
                catch (Exception ex)
                {
                    outputs.Add(new Tuple<bool, string>(true, ex.ToString()));
                    exceptions.Add(new ErrorRecord(ex, "ImageStore Comparing Similar File", ErrorCategory.NotSpecified, null));
                }
            }
            
            //Update file
            dbJobs.Add(new UpdateFileJob(fileId));
        }

        internal void Stop()
        {
            dbJobs.CompleteAdding();
            dbOperator.Join();
        }

        #region FilterTargets
        HashSet<Guid> GetComparedSimilarFiles()
        {
            var item = new ReadingExistsJob(fileId);
            dbJobs.Add(item);
            item.Finished.WaitOne();
            item.Finished.Dispose();
            return item.Result;
        }

        IEnumerable<KeyValuePair<Guid, byte[]>> CompareWithAllFiles(HashSet<Guid> compared)
        {
            foreach (var folder in files)
            {
                foreach (var path in folder.Value)
                {
                    foreach (var file in path.Value)
                    {
                        if (!processedFiles.Contains(file.Key) && !compared.Contains(file.Key))
                            yield return file;
                    }
                }
            }
        }

        IEnumerable<KeyValuePair<Guid, byte[]>> CompareWithFilesInOtherDirectories(HashSet<Guid> compared)
        {
            foreach (var folder in files)
            {
                if (folder.Key == folderId)
                {
                    foreach (var path in folder.Value)
                    {
                        if (string.Compare(path.Key, this.path, true) == 0) continue;
                        foreach (var file in path.Value)
                        {
                            if (!processedFiles.Contains(file.Key) && !compared.Contains(file.Key))
                                yield return file;
                        }
                    }
                }
                else
                {
                    foreach (var path in folder.Value)
                    {
                        foreach (var file in path.Value)
                        {
                            if (!processedFiles.Contains(file.Key) && !compared.Contains(file.Key))
                                yield return file;
                        }
                    }
                }
            }
        }

        IEnumerable<KeyValuePair<Guid, byte[]>> CompareWithFilesInOtherFolders(HashSet<Guid> compared)
        {
            foreach (var folder in files)
            {
                if (folder.Key == folderId) continue;
                foreach (var path in folder.Value)
                {
                    foreach (var file in path.Value)
                    {
                        if (!processedFiles.Contains(file.Key) && !compared.Contains(file.Key))
                            yield return file;
                    }
                }
            }
        }
        #endregion

        #region Writing
        BlockingCollection<DBJob> dbJobs = new BlockingCollection<DBJob>();

        abstract class DBJob
        {
            public abstract int JobType { get; }
        }
        class UpdateFileJob : DBJob
        {
            public Guid FileId { get; }

            public override int JobType => 0;
            public UpdateFileJob(Guid fileId)
            { FileId = fileId; }
        }
        class InsertSimilarFileJob : DBJob
        {
            public Guid File1Id { get; }
            public Guid File2Id { get; }
            public float Difference { get; }
            public override int JobType => 1;
            public InsertSimilarFileJob(Guid file1Id, Guid file2Id, float difference)
            {
                File1Id = file1Id;
                File2Id = file2Id;
                Difference = difference;
            }
        }
        class ReadingExistsJob : DBJob
        {
            public Guid FileId { get; }
            public HashSet<Guid> Result { get; } = new HashSet<Guid>();
            public AutoResetEvent Finished { get; } = new AutoResetEvent(false);
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
                        if (job.JobType == 0)
                        {
                            var item = job as UpdateFileJob;
                            commandUpdating.Parameters[0].Value = item.FileId;
                            if (commandUpdating.ExecuteNonQuery() == 0)
                            {
                                var text = string.Format("Cannot update file record. Id: {0}", item.FileId);
                                outputs.Add(new Tuple<bool, string>(true, text));
                                exceptions.Add(new ErrorRecord(new InvalidOperationException(text), "ImageStore Comparing Similar File - Update database", ErrorCategory.WriteError, item.FileId));
                            }
                        }
                        else if (job.JobType == 2)
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
                            var item = job as InsertSimilarFileJob;
                            commandInserting.Parameters[0].Value = Guid.NewGuid();
                            commandInserting.Parameters[1].Value = item.File1Id;
                            commandInserting.Parameters[2].Value = item.File2Id;
                            commandInserting.Parameters[3].Value = item.Difference;

                            if (commandInserting.ExecuteNonQuery() == 0)
                            {
                                var text = string.Format("Cannot insert this record. File1: {0}; File2: {1}", item.File1Id, item.File2Id);
                                outputs.Add(new Tuple<bool, string>(true, text));
                                exceptions.Add(new ErrorRecord(new InvalidOperationException(text), "ImageStore Comparing Similar File - Insert record", ErrorCategory.WriteError, null));
                            }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        outputs.CompleteAdding();
                        break;
                    }
                    catch(Exception ex)
                    {
                        outputs.Add(new Tuple<bool, string>(true, ex.ToString()));
                        exceptions.Add(new ErrorRecord(ex, "ImageStore Comparing Similar File - Operate database", ErrorCategory.ResourceUnavailable, null));
                    }
                }
            }
        }
        #endregion

        void Compare(KeyValuePair<Guid, byte[]> target)
        {
            float cross = 1 - Shipwreck.Phash.CrossCorrelationWithLimitation.GetCrossCorrelation(imageHash, target.Value, 40);
            if (cross <= imageComparedThreshold)
            {
                dbJobs.Add(new InsertSimilarFileJob(fileId, target.Key, cross));
            }
        }
    }
}
