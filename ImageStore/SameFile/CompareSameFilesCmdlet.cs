using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    [Cmdlet(VerbsData.Compare, "ImageStoreSameFiles")]
    [Alias("CompareSameFiles")]
    public class CompareSameFilesCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        public SwitchParameter DeleteCurrentRecords { get; set; }

        //[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        //static extern int memcmp(byte[] b1, byte[] b2, long count);

        //static bool ByteArrayCompare(byte[] b1, byte[] b2) //not check the length
        //{
        //    return memcmp(b1, b2, b1.Length) == 0;
        //}

        protected override void ProcessRecord()
        {
            var connection = DatabaseConnection.Current;
            HashSet<Guid> existedSameFiles = null;

            if (!DeleteCurrentRecords.IsPresent)
            {
                existedSameFiles = new HashSet<Guid>();
                using (var command = new SqliteCommand("Select [FileId] from [SameFile]"))
                {
                    command.Connection = connection;
                    command.CommandTimeout = 0;

                    using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                    {
                        while (reader.Read())
                        {
                            var fileId = reader.GetGuid(0);
                            existedSameFiles.Add(fileId);
                        }
                        reader.Close();
                    }
                }
            }
            else
            {
                using (var command = new SqliteCommand("Delete from [SameFile]"))
                {
                    command.Connection = connection;
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();
                }
            }

            List<Tuple<Guid, byte[]>> files = new List<Tuple<Guid, byte[]>>();
            using (var command = new SqliteCommand("Select [Id],[Sha1Hash] from [File] where [Sha1Hash] in (Select [Sha1Hash] from [File] where [Sha1Hash] is not null group by [Sha1Hash] Having Count([Id]) > 1)"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        var fileId = reader.GetGuid(0);
                        var sha1Hash = (byte[])reader[1];
                        files.Add(new Tuple<Guid, byte[]>(fileId, sha1Hash));
                    }
                    reader.Close();
                }
            }

            WriteVerbose("Same file(s) count: " + files.Count.ToString());

            using (var command = new SqliteCommand("Insert into [SameFile] values(@Id, @Sha1Hash, @FileId, 0)"))
            {
                command.Connection = connection;
                command.CommandTimeout = 0;
                command.Parameters.Add(new SqliteParameter("@Id", SqliteType.Blob));
                command.Parameters.Add(new SqliteParameter("@Sha1Hash", SqliteType.Blob));
                command.Parameters.Add(new SqliteParameter("@FileId", SqliteType.Blob));

                foreach (var record in files)
                {
                    if (existedSameFiles != null && existedSameFiles.Remove(record.Item1))
                        continue;
                    command.Parameters[0].Value = Guid.NewGuid().ToByteArray();
                    command.Parameters[1].Value = record.Item2;
                    command.Parameters[2].Value = record.Item1.ToByteArray();
                    if (command.ExecuteNonQuery() == 0)
                        WriteError(new ErrorRecord(
                            new InvalidOperationException("Cannot insert this same file."),
                            "ImageStore Compute Same Files", ErrorCategory.WriteError, record.Item1));
                }
            }
        }
    }
}
