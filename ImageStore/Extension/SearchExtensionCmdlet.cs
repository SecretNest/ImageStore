using SecretNest.ImageStore.DatabaseShared;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Extension
{
    [Cmdlet(VerbsCommon.Search, "ImageStoreExtension")]
    [Alias("SearchExtension")]
    [OutputType(typeof(List<ImageStoreExtension>))]
    public class SearchExtensionCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true)]
        [AllowEmptyString()][AllowNull] public string Extension { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public StringPropertyComparingModes ExtensionPropertyComparingModes { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public bool? IsImage { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public bool? Ignored { get; set; }

        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand("Select [Id],[Extension],[IsImage],[Ignored] from [Extension]"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                WhereCauseBuilder whereCauseBuilder = new WhereCauseBuilder(command.Parameters);

                whereCauseBuilder.AddStringComparingCause("Extension", Extension, ExtensionPropertyComparingModes);
                whereCauseBuilder.AddBitComparingCause("IsImage", IsImage);
                whereCauseBuilder.AddBitComparingCause("Ignored", Ignored);

                command.CommandText += whereCauseBuilder.ToFullWhereCommand();

                List<ImageStoreExtension> result = new List<ImageStoreExtension>();

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while(reader.Read())
                    {
                        ImageStoreExtension line = new ImageStoreExtension((Guid)reader[0])
                        {
                            Extension = (string)reader[1],
                            IsImage = (bool)reader[2],
                            Ignored = (bool)reader[3]
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