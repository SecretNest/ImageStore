using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    [Cmdlet(VerbsCommon.Search, "ImageStoreSameFile")]
    [Alias("SearchSameFile")]
    [OutputType(typeof(IEnumerable<ImageStoreSameFile>))]
    public class SearchSameFileCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true)]
        public byte[] Sha1Hash { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public SwitchParameter IncludeIgnored { get; set; }
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public SwitchParameter OnlyIgnored { get; set; }
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
        public SwitchParameter IncludeObsoleted { get; set; }


        [Parameter(ValueFromPipelineByPropertyName = true, Position = 4)]
        public int? Top { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var connection = DatabaseConnection.Current;

            using (var command = new SqlCommand())
            {
                string text = " [Id],[Sha1Hash],[FileId],[IsIgnored] from [SameFile] ";

                if (Top.HasValue)
                {
                    text = "SELECT TOP " + Top.Value.ToString() + text;
                }
                else
                {
                    text = "SELECT" + text;
                }

                if (Sha1Hash != null)
                {
                    text += "Where [Sha1Hash]=@Sha1Hash";
                    command.Parameters.Add(new SqlParameter("@Sha1Hash", System.Data.SqlDbType.Binary, 20) { Value = Sha1Hash });
                    if (!OnlyIgnored.IsPresent)
                        text += " and [IsIgnored]=1";
                    else if (!IncludeIgnored.IsPresent)
                        text += " and [IsIgnored]=0";
                }
                else if (IncludeObsoleted.IsPresent)
                {
                    if (!OnlyIgnored.IsPresent)
                        text += "where [IsIgnored]=1";
                    else if (!IncludeIgnored.IsPresent)
                        text += "where [IsIgnored]=0";
                }
                else
                {
                    text += "Where [Sha1Hash] in (Select [Sha1Hash] From [SameFile] ";
                    if (!OnlyIgnored.IsPresent)
                        text += "where [IsIgnored]=1 ";
                    else if (!IncludeIgnored.IsPresent)
                        text += "where [IsIgnored]=0 ";
                    text += "Group by [Sha1Hash] Having Count([Id]) > 1)";
                }

                command.CommandText = text + " order by [Sha1Hash]";
                command.Connection = connection;
                command.CommandTimeout = 0;

                List<ImageStoreSameFile> result = new List<ImageStoreSameFile>();

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        ImageStoreSameFile line = new ImageStoreSameFile((Guid)reader[0], (byte[])reader[1], (Guid)reader[2])
                        {
                            IsIgnored = (bool)reader[3]
                        };
                        result.Add(line);
                    }
                    reader.Close();
                }

                if (Sha1Hash != null && IncludeObsoleted.IsPresent && result.Count == 1)
                {
                    WriteObject(new List<ImageStoreSameFile>());
                }
                else
                {
                    WriteObject(result);
                }
            }
        }
    }
}
