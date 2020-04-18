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
    public partial class ResolveSimilarFilesCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true)]
        public float? DifferenceDegree { get; set; }

        [Parameter(Position = 1)]
        public SwitchParameter BuildsDisconnectedGroup { get; set; }

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

            selectedFiles = new HashSet<Guid>();
            loadedThumbprints = new ConcurrentDictionary<Guid, Image>();

            WriteVerbose("Calculating similar files into groups... It may take several minutes to complete.");
            while (true)
            {
                CalculateIntoGroup();

                var groupCount = groupedRecords.Count;
                if (groupCount > 0)
                {
                    WriteVerbose("Count of groups: " + groupCount.ToString());
                    using (SimilarFileManager window = new SimilarFileManager(selectedFiles, allFileInfo, GetFileThumbprint, groupedRecords, groupedFiles, allRecords, fileToRecords, IgnoreSimilarFileHelper.MarkIgnore))
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

            Parallel.ForEach(loadedThumbprints.Values, i => i.Dispose());
        }

        void WriteOutput()
        {
            var list = selectedFiles.Select(i => allFiles[i]).ToList();
            var count = list.Count;
            if (count == 0)
            {
                WriteVerbose("No file is checked.");
            }
            else if (count == 1)
            {
                var file = list[0];
                string folder = allFolders[file.FolderId].Path;
                if (!folder.EndsWith(DirectorySeparatorString.Value))
                    folder += DirectorySeparatorString.Value;
                string fileName = FileHelper.GetFullFilePath(folder, file.Path, file.FileName, allExtensionNames[file.ExtensionId]);
                WriteVerbose("The checked file (" + fileName + ") is returned.");
            }
            else
            {
                WriteVerbose("The " + count.ToString() + " checked files are returned.");
            }
            WriteObject(list);
        }

        HashSet<Guid> selectedFiles;
        Dictionary<Guid, ImageStoreFile> allFiles = new Dictionary<Guid, ImageStoreFile>();
        Dictionary<Guid, FileInfo> allFileInfo = new Dictionary<Guid, FileInfo>();
        ConcurrentDictionary<Guid, Image> loadedThumbprints;
        Dictionary<Guid, ImageStoreFolder> allFolders;
        Dictionary<Guid, string> allExtensionNames;

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

        #region LoadToMemory
        Dictionary<Guid, ImageStoreSimilarFile> allRecords = new Dictionary<Guid, ImageStoreSimilarFile>();
        Dictionary<Guid, List<Guid>> fileToRecords = new Dictionary<Guid, List<Guid>>();
        void LoadToMemory()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Create table #tempSimilarFile ([Id] uniqueidentifier, [File1Id] uniqueidentifier, [File2Id] uniqueidentifier, [DifferenceDegree] real, [IgnoredMode] int)", connection))
            {
                command.ExecuteNonQuery();
            }

            var insertCommand = "insert into #tempSimilarFile Select [Id],[File1Id],[File2Id],[DifferenceDegree],[IgnoredMode] from [SimilarFile] where [DifferenceDegree]<=@DifferenceDegree";
            if (!BuildsDisconnectedGroup.IsPresent) //skip while loading to memory
            {
                insertCommand += " and [IgnoredMode]<>2";
            }
            using (var command = new SqlCommand(insertCommand, connection) { CommandTimeout = 180 })
            {
                command.Parameters.Add(new SqlParameter("@DifferenceDegree", System.Data.SqlDbType.Real) { Value = DifferenceDegree });
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("Select [Id],[File1Id],[File2Id],[DifferenceDegree],[IgnoredMode] from #tempSimilarFile", connection) { CommandTimeout = 180 })
            {
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

            using (var command = new SqlCommand("Create table #tempSimilarFileId ([Id] uniqueidentifier)", connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("insert into #tempSimilarFileId select distinct * from (select [File1Id] from #tempSimilarFile union select [File2Id] from #tempSimilarFile) IdTable", connection) { CommandTimeout = 180 })
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("drop table #tempSimilarFile", connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("Select [Id],[FolderId],[Path],[FileName],[ExtensionId],[ImageHash],[Sha1Hash],[FileSize],[FileState],[ImageComparedThreshold] from [File] Where [Id] in (select [id] from #tempSimilarFileId)", connection) { CommandTimeout = 180 })
            {
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


            using (var command = new SqlCommand("drop table #tempSimilarFileId", connection))
            {
                command.ExecuteNonQuery();
            }

            if (allFiles.Count > 0)
            {
                allFolders = new Dictionary<Guid, ImageStoreFolder>();
                allExtensionNames = new Dictionary<Guid, string>();

                foreach (var folder in FolderHelper.GetAllFolders())
                    allFolders.Add(folder.Id, folder);

                foreach (var extension in ExtensionHelper.GetAllExtensions())
                    allExtensionNames.Add(extension.Id, extension.Extension);

                Parallel.ForEach(allFiles.Values, i => allFileInfo[i.Id].SetData(i, allFolders[i.FolderId], allExtensionNames[i.ExtensionId]));
            }
        }
        #endregion

        #region Thumb print
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
        #endregion
    }
}
