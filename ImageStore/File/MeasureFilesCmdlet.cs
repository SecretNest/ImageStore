using SecretNest.ImageStore.Extension;
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

namespace SecretNest.ImageStore.File
{
    [Flags]
    public enum RemeasuringFileStates : int
    {
        None = 0,
        NotImage = 1,
        NotReadable = 2,
        Computed = 4,
        SizeZero = 8,

        AllFailed = 3,
        All = 15
    }

    [Cmdlet(VerbsDiagnostic.Measure, "ImageStoreFiles")]
    [Alias("MeasureFiles")]
    public class MeasureFilesCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true)]
        public ImageStoreFolder Folder { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public RemeasuringFileStates RemeasuringFileStates { get; set; } = RemeasuringFileStates.None;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public int? FileCountLimit { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public int ComputingThreadLimit { get; set; } = Environment.ProcessorCount;


        Dictionary<Guid, ImageStoreExtension> extensionsById = null;

        protected override void ProcessRecord()
        {
            extensionsById = new Dictionary<Guid, ImageStoreExtension>();
            foreach (var ext in ExtensionHelper.GetAllExtensions())
                extensionsById.Add(ext.Id, ext);

            bool remeasuringDisabled = RemeasuringFileStates == RemeasuringFileStates.None;
            bool remeasuringComputed, remeasuringNotImage, remeasuringNotReadable, remeasuringSizeZero;
            if (remeasuringDisabled)
            {
                remeasuringComputed = false;
                remeasuringNotImage = false;
                remeasuringNotReadable = false;
                remeasuringSizeZero = false;
            }
            else
            {
                remeasuringComputed = RemeasuringFileStates.HasFlag(RemeasuringFileStates.Computed);
                remeasuringNotImage = RemeasuringFileStates.HasFlag(RemeasuringFileStates.NotImage);
                remeasuringNotReadable = RemeasuringFileStates.HasFlag(RemeasuringFileStates.NotReadable);
                remeasuringSizeZero = RemeasuringFileStates.HasFlag(RemeasuringFileStates.SizeZero);
                if (remeasuringComputed) WriteVerbose("Remeasuring is enabled for all computed files.");
                if (remeasuringNotImage) WriteVerbose("Remeasuring is enabled for all marked as not image files.");
                if (remeasuringNotReadable) WriteVerbose("Remeasuring is enabled for all marked as not readable files.");
                if (remeasuringSizeZero) WriteVerbose("Remeasuring is enabled for all 0-size files.");
            }

            int left;
            if (ComputingThreadLimit == -1)
            {
                WriteVerbose("Hash computing thread count: No limited");
            }
            else
            {
                WriteVerbose("Hash computing thread count: " + ComputingThreadLimit.ToString());
            }
            if (FileCountLimit.HasValue)
            {
                left = FileCountLimit.Value;
                WriteInformation("File count limit applied: " + left.ToString(), new string[] { "MeasureFiles", "FileCountLimit" });
            }
            else
                left = 0;
            
            if (Folder == null)
            {
                var folders = FolderHelper.GetAllFolders().ToArray();

                foreach (var folder in folders)
                {
                    if (FileCountLimit.HasValue)
                    {
                        if (left > 0)
                        {
                            left -= Compute(folder, left, remeasuringDisabled, remeasuringComputed, remeasuringNotImage, remeasuringNotReadable, remeasuringSizeZero);
                        }
                        else
                        {
                            WriteInformation("Measuring folder: " + folder.Name + " (skipped - file count limit reached)", new string[] { "MeasureFiles", "SkipFolder" });
                            continue;
                        }
                    }
                    else
                    {
                        Compute(folder, null, remeasuringDisabled, remeasuringComputed, remeasuringNotImage, remeasuringNotReadable, remeasuringSizeZero);
                    }
                }
            }
            else
            {
                Compute(Folder, FileCountLimit, remeasuringDisabled, remeasuringComputed, remeasuringNotImage, remeasuringNotReadable, remeasuringSizeZero);
            }
            
            if (exceptions.Count == 1)
            {
                ThrowTerminatingError(exceptions.ToArray()[0]);
            }
            else if (exceptions.Count > 0)
            {
                foreach(var ex in exceptions)
                {
                    WriteError(ex);
                }
                ThrowTerminatingError(new ErrorRecord(new AggregateException(exceptions.Select(i => i.Exception)), "ImageStore Measuring - Aggregate", ErrorCategory.NotSpecified, null));
            }
        }

        List<Tuple<string, bool, Guid, bool>> files = null;            //path, isImage, id, isNew
        BlockingCollection<Tuple<Guid, byte[], byte[], int, FileState, bool>> toWrite = null; //id, imagehash, sha1hash, size, state, isnew
        ConcurrentBag<ErrorRecord> exceptions = new ConcurrentBag<ErrorRecord>();
        BlockingCollection<Tuple<int, string>> outputs = null; //0: file finished; 1: file failed; 2: other exception
        int Compute(ImageStoreFolder folder, int? limit, bool remeasuringDisabled, bool remeasuringComputed, bool remeasuringNotImage, bool remeasuringNotReadable, bool remeasuringSizeZero)
        {
            WriteInformation("Measuring folder: " + folder.Name, new string[] { "MeasureFiles", "MeasureFolder" });

            //get records from database
            //path, isImage, id, isNew
            files = new List<Tuple<string, bool, Guid, bool>>();
            string baseFolder = folder.Path;
            if (!baseFolder.EndsWith(DirectorySeparatorString.Value))
                baseFolder += DirectorySeparatorString.Value;

            WriteVerbose("Reading records from database...");
            
            foreach (var file in FileHelper.GetAllFilesWithoutData(folder.Id, limit,
                remeasuringDisabled,
                remeasuringComputed,
                remeasuringNotImage,
                remeasuringNotReadable,
                remeasuringSizeZero))
            {
                var extension = extensionsById[file.ExtensionId];
                if (extension.Ignored) continue;

                string fullPath = MeasureFileHelper.GetFullFilePath(baseFolder, file.Path, file.FileName, extension.Extension);

                files.Add(new Tuple<string, bool, Guid, bool>(fullPath, extension.IsImage, file.Id, file.FileState == FileState.New));
            }

            int filesCount = files.Count;
            if (filesCount == 0)
            {
                files = null;
                WriteVerbose("Nothing to be measuring in this folder.");
                return 0;
            }
            else if (filesCount == 1)
            {
                WriteVerbose("One record is read from database.");
            }
            else
            {
                WriteVerbose(files.Count.ToString() + " records are read from database.");
            }

            WriteVerbose("Starting measuring...");

            outputs = new BlockingCollection<Tuple<int, string>>();

            int finishedCount = 0;

            Thread writing = new Thread(WriteDatabase);

            using(var measuring = new Task(ComputeAll, TaskCreationOptions.LongRunning))
            using (toWrite = new BlockingCollection<Tuple<Guid, byte[], byte[], int, FileState, bool>>())
            {
                writing.Start();
                measuring.Start();

                while (true)
                {
                    try
                    {
                        var itemToDisplay = outputs.Take();
                        if (itemToDisplay.Item1 == 2)
                        {
                            //Other exception
                            WriteWarning(itemToDisplay.Item2);
                        }
                        else
                        {
                            var text = string.Format("({0}/{1}) {2}", ++finishedCount, filesCount, itemToDisplay.Item2);

                            if (itemToDisplay.Item1 == 0)
                            {
                                //file finished
                                WriteVerbose(text);
                            }
                            else //1
                            {
                                //file failed
                                WriteWarning(text);
                            }
                        }
                    }
                    catch (InvalidOperationException) { break; }
                }

                measuring.Wait();
                writing.Join();
            }
            toWrite = null;
            outputs = null;
            files = null;

            return filesCount;
        }

        void ComputeAll()
        {
            try
            {
                ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = ComputingThreadLimit };
                Parallel.ForEach(files, options, PrepareHelper, ComputeOneFile, DisposeHelper);
            }
            catch(Exception ex)
            {
                outputs.Add(new Tuple<int, string>(2, ex.ToString()));
                exceptions.Add(new ErrorRecord(ex, "ImageStore Measuring", ErrorCategory.NotSpecified, null));
            }
            finally
            {
                toWrite.CompleteAdding();
            }
        }

        MeasureFileHelper PrepareHelper() => new MeasureFileHelper();
        void DisposeHelper(MeasureFileHelper helper) => helper.Dispose();

        MeasureFileHelper ComputeOneFile(Tuple<string, bool, Guid, bool> record, ParallelLoopState state, MeasureFileHelper helper)
        {
            var fullPath = record.Item1;
            var isImage = record.Item2;
            var fileId = record.Item3;
            var isNew = record.Item4;

            var ex = helper.ProcessFile(fullPath, isImage, out var imageHash, out var sha1Hash, out var fileSize, out var fileState);
            if (ex != null)
            {
                outputs.Add(new Tuple<int, string>(1, "Exception in measuring " + fullPath + ": " + ex.FullyQualifiedErrorId + " - " + ex.Exception.Message));
                exceptions.Add(ex);
            }
            else
            {
                outputs.Add(new Tuple<int, string>(0, "Measuring of " + fullPath + " is completed."));
            }
            toWrite.Add(new Tuple<Guid, byte[], byte[], int, FileState, bool>(fileId, imageHash?.Coefficients, sha1Hash, fileSize, fileState, isNew));

            return helper;
        }

        void WriteDatabase()
        {

            var connection = DatabaseConnection.Current;

            using (var commandToDeleteSame = new SqlCommand("Delete from [SameFile] Where [FileId]=@Id"))
            using (var commandToDeleteSimilar = new SqlCommand("Delete from [SimilarFile] Where [File1Id]=@Id or [File2Id]=@Id"))
            using (var command = new SqlCommand("Update [File] Set [ImageHash]=@ImageHash, [Sha1Hash]=@Sha1Hash, [FileSize]=@FileSize, [FileState]=@FileState, [ImageComparedThreshold]=0 where [Id]=@Id"))
            {
                commandToDeleteSame.Connection = connection;
                commandToDeleteSame.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                commandToDeleteSame.CommandTimeout = 180;
                commandToDeleteSimilar.Connection = connection;
                commandToDeleteSimilar.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                commandToDeleteSimilar.CommandTimeout = 180;
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.UniqueIdentifier));
                command.Parameters.Add(new SqlParameter("@ImageHash", System.Data.SqlDbType.Binary, 40));
                command.Parameters.Add(new SqlParameter("@Sha1Hash", System.Data.SqlDbType.Binary, 20));
                command.Parameters.Add(new SqlParameter("@FileSize", System.Data.SqlDbType.Int));
                command.Parameters.Add(new SqlParameter("@FileState", System.Data.SqlDbType.Int));

                Tuple<Guid, byte[], byte[], int, FileState, bool> record;

                while (true)
                {
                    try
                    {
                        record = toWrite.Take();

                        command.Parameters[0].Value = record.Item1;
                        command.Parameters[1].Value = DBNullableReader.NullCheck(record.Item2);
                        command.Parameters[2].Value = DBNullableReader.NullCheck(record.Item3);
                        command.Parameters[3].Value = record.Item4;
                        command.Parameters[4].Value = (int)record.Item5;

                        if (command.ExecuteNonQuery() == 0)
                        {
                            var text = "Cannot update file record. Id: " + record.Item1.ToString();
                            outputs.Add(new Tuple<int, string>(2, text));
                            exceptions.Add(new ErrorRecord(new InvalidOperationException(text), "ImageStore Measuring - Update database", ErrorCategory.WriteError, record.Item1));
                        }

                        if (!record.Item6)
                        {
                            commandToDeleteSame.Parameters[0].Value = record.Item1;
                            commandToDeleteSame.ExecuteNonQuery();
                            commandToDeleteSimilar.Parameters[0].Value = record.Item1;
                            commandToDeleteSimilar.ExecuteNonQuery();
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        outputs.CompleteAdding();
                        return;
                    }
                    catch (Exception ex)
                    {
                        outputs.Add(new Tuple<int, string>(2, ex.ToString()));
                        exceptions.Add(new ErrorRecord(ex, "ImageStore Measuring - Update database", ErrorCategory.ResourceUnavailable, null));
                    }
                }
            }

        }
    }
}
