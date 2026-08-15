using SecretNest.ImageStore.DatabaseShared;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.IgnoredDirectory
{
    [Cmdlet(VerbsCommon.Search, "ImageStoreIgnoredDirectory")]
    [Alias("SearchIgnoredDirectory")]
    [OutputType(typeof(List<ImageStoreIgnoredDirectory>))]
    public class SearchIgnoredDirectoryCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        public Guid? FolderId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, ValueFromPipeline = true)]
        [AllowEmptyString()][AllowNull] public string Directory { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public StringPropertyComparingModes DirectoryPropertyComparingModes { get; set; } = StringPropertyComparingModes.Contains;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public bool? IsSubDirectoryIncluded { get; set; }


        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqliteCommand("Select [Id],[FolderId],[Directory],[IsSubDirectoryIncluded] from [IgnoredDirectory]"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                WhereCauseBuilder whereCauseBuilder = new WhereCauseBuilder(command.Parameters);

                whereCauseBuilder.AddUniqueIdentifierComparingCause("FolderId", FolderId);
                whereCauseBuilder.AddStringComparingCause("Directory", Directory, DirectoryPropertyComparingModes);
                whereCauseBuilder.AddBitComparingCause("IsSubDirectoryIncluded", IsSubDirectoryIncluded);

                command.CommandText += whereCauseBuilder.ToFullWhereCommand();

                List<ImageStoreIgnoredDirectory> result = new List<ImageStoreIgnoredDirectory>();

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreIgnoredDirectory line = new ImageStoreIgnoredDirectory(reader.GetGuid(0))
                        {
                            FolderId = reader.GetGuid(1),
                            Directory = reader.GetString(2),
                            IsSubDirectoryIncluded = reader.GetBoolean(3)
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