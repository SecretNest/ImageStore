using SecretNest.ImageStore.DatabaseShared;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.IgnoredDirectory
{
    [Cmdlet(VerbsCommon.Search, "ImageStoreIgnoredDirectory")]
    [Alias("SearchIgnoredDirectory")]
    [OutputType(typeof(IEnumerable<ImageStoreIgnoredDirectory>))]
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

            using (var command = new SqlCommand("Select [Id],[FolderId],[Directory],[IsSubDirectoryIncluded] from [IgnoredDirectory]"))
            {
                command.Connection = connection;
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
                        ImageStoreIgnoredDirectory line = new ImageStoreIgnoredDirectory((Guid)reader[0])
                        {
                            FolderId = (Guid)reader[1],
                            Directory = (string)reader[2],
                            IsSubDirectoryIncluded = (bool)reader[3]
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