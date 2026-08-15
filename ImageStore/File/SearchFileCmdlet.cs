using SecretNest.ImageStore.DatabaseShared;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    [Cmdlet(VerbsCommon.Search, "ImageStoreFile")]
    [Alias("SearchFile")]
    [OutputType(typeof(List<ImageStoreFile>))]
    public class SearchFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        public Guid? FolderId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, ValueFromPipeline = true)]
        [AllowEmptyString()] [AllowNull] public string Path { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public StringPropertyComparingModes PathPropertyComparingModes { get; set; } = StringPropertyComparingModes.Contains;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        [AllowEmptyString()] public string FileName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 4)]
        public StringPropertyComparingModes FileNamePropertyComparingModes { get; set; } = StringPropertyComparingModes.Contains;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 5)]
        public Guid? ExtensionId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 6)]
        public SwitchParameter ImageHashIsNull { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 7)]
        public byte[] ImageHash { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 8)]
        public SwitchParameter Sha1HashIsNull { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 9)]
        public byte[] Sha1Hash { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 10)]
        public int? FileSize { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 11)]
        public int? FileSizeGreaterOrEqual { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 12)]
        public int? FileSizeLessOrEqual { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 13)]
        public SwitchParameter IncludesNewFile { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 14)]
        public SwitchParameter IncludesNotImage { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 15)]
        public SwitchParameter IncludesNotReadable { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 16)]
        public SwitchParameter IncludesSizeZero { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 17)]
        public SwitchParameter IncludesComputed { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 18)]
        public float? ImageComparedThreshold { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 19)]
        public float? ImageComparedThresholdGreaterOrEqual { get; set; }


        [Parameter(ValueFromPipelineByPropertyName = true, Position = 20)]
        public float? ImageComparedThresholdLessOrEqual { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 21)]
        public int? Top { get; set; }

        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqliteCommand(" [Id],[FolderId],[Path],[FileName],[ExtensionId],[ImageHash],[Sha1Hash],[FileSize],[FileState],[ImageComparedThreshold] from [File]"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                //SQLite has no TOP; the row limit goes on the end as LIMIT, after
                //the where and order by clauses appended below.
                command.CommandText = "SELECT" + command.CommandText;

                WhereCauseBuilder whereCauseBuilder = new WhereCauseBuilder(command.Parameters);
                whereCauseBuilder.AddUniqueIdentifierComparingCause("FolderId", FolderId);
                whereCauseBuilder.AddStringComparingCause("Path", Path, PathPropertyComparingModes);
                whereCauseBuilder.AddStringComparingCause("FileName", FileName, FileNamePropertyComparingModes);
                whereCauseBuilder.AddUniqueIdentifierComparingCause("ExtensionId", ExtensionId);
                whereCauseBuilder.AddBinaryComparingCause("ImageHash", ImageHash, ImageHashIsNull.IsPresent, 40);
                whereCauseBuilder.AddBinaryComparingCause("Sha1Hash", Sha1Hash, Sha1HashIsNull.IsPresent, 20);
                whereCauseBuilder.AddIntComparingCause("FileSize", FileSize, FileSizeGreaterOrEqual, FileSizeLessOrEqual);

                List<int> fileStates = new List<int>();
                if (IncludesNewFile.IsPresent)
                    fileStates.Add(0);
                if (IncludesNotImage.IsPresent)
                    fileStates.Add(1); 
                if (IncludesNotReadable.IsPresent)
                    fileStates.Add(2); 
                if (IncludesSizeZero.IsPresent)
                    fileStates.Add(254);
                if (IncludesComputed.IsPresent)
                    fileStates.Add(255);
                if (fileStates.Count != 5)
                    whereCauseBuilder.AddIntInRangeCause("FileState", fileStates);

                whereCauseBuilder.AddRealComparingCause("ImageComparedThreshold", ImageComparedThreshold, ImageComparedThresholdGreaterOrEqual, ImageComparedThresholdLessOrEqual);

                command.CommandText += whereCauseBuilder.ToFullWhereCommand();

                command.CommandText += " order by [FolderId], [Path], [FileName], [ExtensionId], [FileState]";

                if (Top.HasValue)
                    command.CommandText += " limit " + Top.Value.ToString();

                List<ImageStoreFile> result = new List<ImageStoreFile>();

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreFile line = new ImageStoreFile(reader.GetGuid(0), reader.GetGuid(1), reader.GetString(2), reader.GetString(3), reader.GetGuid(4))
                        {
                            ImageHash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[5]),
                            Sha1Hash = DBNullableReader.ConvertFromReferenceType<byte[]>(reader[6]),
                            FileSize = reader.GetInt32(7),
                            FileStateCode = reader.GetInt32(8),
                            ImageComparedThreshold = reader.GetFloat(9)
                        };
                        result.Add(line);
                    }
                    reader.Close();
                }

                WriteObject(result);
            }
        }
    }
}
