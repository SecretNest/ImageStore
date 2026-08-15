using SecretNest.ImageStore.DatabaseShared;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
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
        public SwitchParameter IncludesEffective { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 6)]
        public SwitchParameter IncludesHiddenButConnected { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 7)]
        public SwitchParameter IncludesHiddenAndDisconnected { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 8)]
        public int? Top { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 9)]
        public SwitchParameter OrdersByDifferenceDegree { get; set; }

        protected override void ProcessRecord()
        {
            if (AnotherFileId.HasValue && !FileId.HasValue)
            {
                FileId = AnotherFileId.Value;
                AnotherFileId = null;
            }

            var connection = DatabaseConnection.Current;
            string commandPart = " [Id],[File1Id],[File2Id],[DifferenceDegree],[IgnoredMode] from [SimilarFile]";

            using (var command = new SqliteCommand() { CommandTimeout = 0 })
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                //SQLite has no TOP; the row limit goes on the end as LIMIT, after
                //the where and order by clauses appended below.
                command.CommandText = "SELECT" + commandPart;


                WhereCauseBuilder whereCauseBuilder = new WhereCauseBuilder(command.Parameters);

                if (FileId.HasValue)
                {
                    if (AnotherFileId.HasValue)
                    {
                        whereCauseBuilder.AddPlainCause("(([File1Id] = @File1Id and [File2Id] = @File2Id) or ([File2Id] = @File1Id and [File1Id] = @File2Id))");
                        command.Parameters.AddGuid("@File1Id", FileId.Value);
                        command.Parameters.AddGuid("@File2Id", AnotherFileId.Value);
                    }
                    else
                    {
                        whereCauseBuilder.AddPlainCause("([File1Id] = @FileId or [File2Id] = @FileId)");
                        command.Parameters.AddGuid("@FileId", FileId.Value);
                    }
                }

                whereCauseBuilder.AddRealComparingCause("DifferenceDegree", DifferenceDegree, DifferenceDegreeGreaterOrEqual, DifferenceDegreeLessOrEqual);

                List<int> ignoredModes = new List<int>();
                if (IncludesEffective.IsPresent)
                    ignoredModes.Add(0);
                if (IncludesHiddenButConnected.IsPresent)
                    ignoredModes.Add(1);
                if (IncludesHiddenAndDisconnected.IsPresent)
                    ignoredModes.Add(2);
                if (ignoredModes.Count != 3)
                {
                    if (ignoredModes.Count == 0)
                        ignoredModes.Add(0);
                    whereCauseBuilder.AddIntInRangeCause("IgnoredMode", ignoredModes);
                }

                command.CommandText += whereCauseBuilder.ToFullWhereCommand();

                if (OrdersByDifferenceDegree.IsPresent)
                {
                    command.CommandText += " order by [DifferenceDegree]";
                }

                if (Top.HasValue)
                    command.CommandText += " limit " + Top.Value.ToString();

                List<ImageStoreSimilarFile> result = new List<ImageStoreSimilarFile>();

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreSimilarFile line = new ImageStoreSimilarFile(reader.GetGuid(0), reader.GetGuid(1), reader.GetGuid(2), reader.GetFloat(3))
                        {
                            IgnoredModeCode = reader.GetInt32(4)
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
