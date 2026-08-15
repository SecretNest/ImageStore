using SecretNest.ImageStore.DatabaseShared;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    [Cmdlet(VerbsCommon.Search, "ImageStoreFolder")]
    [Alias("SearchFolder")]
    [OutputType(typeof(List<ImageStoreFolder>))]
    public class SearchFolderCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true)]
        [AllowEmptyString()][AllowNull] public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public StringPropertyComparingModes NamePropertyComparingModes { get; set; } = StringPropertyComparingModes.Contains;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        [AllowEmptyString()] public string Path { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public StringPropertyComparingModes PathPropertyComparingModes { get; set; } = StringPropertyComparingModes.Contains;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 4)]
        public CompareImageWith? CompareImageWith { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 5)]
        public bool? IsSealed { get; set; }

        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqliteCommand("Select [Id],[Path],[Name],[CompareImageWith],[IsSealed] from [Folder]"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                WhereCauseBuilder whereCauseBuilder = new WhereCauseBuilder(command.Parameters);

                whereCauseBuilder.AddStringComparingCause("Name", Name, NamePropertyComparingModes);
                whereCauseBuilder.AddStringComparingCause("Path", Path, PathPropertyComparingModes);
                whereCauseBuilder.AddIntComparingCause("CompareImageWith", (int?)CompareImageWith);
                whereCauseBuilder.AddBitComparingCause("IsSealed", IsSealed);

                command.CommandText += whereCauseBuilder.ToFullWhereCommand();

                List<ImageStoreFolder> result = new List<ImageStoreFolder>();

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreFolder line = new ImageStoreFolder(reader.GetGuid(0),reader.GetString(1))
                        {
                            Name = reader.GetString(2),
                            CompareImageWithCode = reader.GetInt32(3),
                            IsSealed = reader.GetBoolean(4)
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
