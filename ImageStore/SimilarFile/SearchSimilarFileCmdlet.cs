using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    [Flags]
    public enum IgnoredModes : int
    {
        Normal = 1,
        HiddenButConnected = 2,
        HiddenAndDisconnected = 4,
        
        All = 7,
        AllHidden = 6,
        AllConnected = 3
    }

    [Cmdlet(VerbsCommon.Search, "ImageStoreSimilarFile")]
    [Alias("SearchSimilarFile")]
    [OutputType(typeof(List<ImageStoreSimilarFile>))]
    public class SearchSimilarFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true)]
        public Guid? FileId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public Guid? AnotherFileId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public float? DifferenceDegree { get; set; }


        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public float? DifferenceDegreeGreaterOrEqual { get; set; }


        [Parameter(ValueFromPipelineByPropertyName = true, Position = 4)]
        public float? DifferenceDegreeLessOrEqual { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 5)]
        public IgnoredModes IgnoredModes { get; set; } = IgnoredModes.Normal;
        
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 6)]
        public int? Top { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 7)]
        public SwitchParameter OrderByDifferenceDegree { get; set; }

        protected override void ProcessRecord()
        {
            if (AnotherFileId.HasValue && !FileId.HasValue)
            {
                FileId = AnotherFileId.Value;
                AnotherFileId = null;
            }

            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand(" [Id],[File1Id],[File2Id],[DifferenceDegree],[IgnoredMode] from [SimilarFile]"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                if (Top.HasValue)
                {
                    command.CommandText = "SELECT TOP " + Top.Value.ToString() + command.CommandText;
                }
                else
                {
                    command.CommandText = "SELECT" + command.CommandText;
                }

                List<string> where = new List<string>();

                if (FileId.HasValue)
                {
                    if (AnotherFileId.HasValue)
                    {
                        where.Add("(([File1Id] = @File1Id and [File2Id] = @File2Id) or ([File2Id] = @File1Id and [File1Id] = @File2Id))");
                        command.Parameters.Add(new SqlParameter("@File1Id", System.Data.SqlDbType.UniqueIdentifier) { Value = FileId.Value });
                        command.Parameters.Add(new SqlParameter("@File2Id", System.Data.SqlDbType.UniqueIdentifier) { Value = AnotherFileId.Value });
                    }
                    else
                    {
                        where.Add("([File1Id] = @FileId or [File2Id] = @FileId)");
                        command.Parameters.Add(new SqlParameter("@FileId", System.Data.SqlDbType.UniqueIdentifier) { Value = FileId.Value });
                    }
                }

                if (DifferenceDegree.HasValue)
                {
                    where.Add("[DifferenceDegree] = @DifferenceDegree");
                    command.Parameters.Add(new SqlParameter("@DifferenceDegree", System.Data.SqlDbType.Real) { Value = DifferenceDegree.Value });
                }
                else
                {
                    if (DifferenceDegreeGreaterOrEqual.HasValue)
                    {
                        where.Add("[DifferenceDegree] >= @DifferenceDegreeGreaterOrEqual");
                        command.Parameters.Add(new SqlParameter("@DifferenceDegreeGreaterOrEqual", System.Data.SqlDbType.Real) { Value = DifferenceDegreeGreaterOrEqual.Value });
                    }

                    if (DifferenceDegreeLessOrEqual.HasValue)
                    {
                        where.Add("[DifferenceDegree] <= @DifferenceDegreeLessOrEqual");
                        command.Parameters.Add(new SqlParameter("@DifferenceDegreeLessOrEqual", System.Data.SqlDbType.Real) { Value = DifferenceDegreeLessOrEqual.Value });
                    }
                }

                if (IgnoredModes != IgnoredModes.All)
                {
                    List<string> ignoreCode = new List<string>();
                    if (IgnoredModes.HasFlag(IgnoredModes.Normal))
                    {
                        ignoreCode.Add("[IgnoredMode] = 0");
                    }
                    if (IgnoredModes.HasFlag(IgnoredModes.HiddenButConnected))
                    {
                        ignoreCode.Add("[IgnoredMode] = 1");
                    }
                    if (IgnoredModes.HasFlag(IgnoredModes.HiddenAndDisconnected))
                    {
                        ignoreCode.Add("[IgnoredMode] = 2");
                    }

                    if (ignoreCode.Count == 1)
                    {
                        where.Add(ignoreCode[0]);
                    }
                    else
                    {
                        where.Add("(" + string.Join(" or ", ignoreCode) + ")");
                    }
                }

                if (where.Count > 0)
                {
                    command.CommandText += " where " + string.Join(" and ", where);
                }

                if (OrderByDifferenceDegree.IsPresent)
                {
                    command.CommandText += " order by [DifferenceDegree]";
                }

                List<ImageStoreSimilarFile> result = new List<ImageStoreSimilarFile>();

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreSimilarFile line = new ImageStoreSimilarFile((Guid)reader[0], (Guid)reader[1], (Guid)reader[2], (float)reader[3])
                        {
                            IgnoredModeCode = (int)reader[4]
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
