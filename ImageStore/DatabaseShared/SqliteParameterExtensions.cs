using Microsoft.Data.Sqlite;
using System;

namespace SecretNest.ImageStore
{
    /// <summary>
    /// Parameter binding helpers.
    /// </summary>
    /// <remarks>
    /// The Guid overloads exist for a reason worth stating plainly: binding a Guid
    /// directly makes Microsoft.Data.Sqlite store it as 36-character TEXT. The schema
    /// declares BLOB and every other write goes through ToByteArray, so a single
    /// direct binding would store a value that no lookup ever matches - and it fails
    /// silently, as a query returning nothing rather than an error.
    ///
    /// Routing every binding through here means that conversion is written once.
    /// </remarks>
    static class SqliteParameterExtensions
    {
        internal static void AddGuid(this SqliteParameterCollection parameters, string name, Guid value)
        {
            parameters.Add(new SqliteParameter(name, SqliteType.Blob) { Value = value.ToByteArray() });
        }

        internal static void AddGuid(this SqliteParameterCollection parameters, string name, Guid? value)
        {
            parameters.Add(new SqliteParameter(name, SqliteType.Blob)
            {
                Value = value.HasValue ? (object)value.Value.ToByteArray() : DBNull.Value
            });
        }

        internal static void AddBlob(this SqliteParameterCollection parameters, string name, byte[] value)
        {
            parameters.Add(new SqliteParameter(name, SqliteType.Blob)
            {
                Value = value ?? (object)DBNull.Value
            });
        }

        internal static void AddText(this SqliteParameterCollection parameters, string name, string value)
        {
            parameters.Add(new SqliteParameter(name, SqliteType.Text)
            {
                Value = value ?? (object)DBNull.Value
            });
        }

        internal static void AddInt(this SqliteParameterCollection parameters, string name, int value)
        {
            parameters.Add(new SqliteParameter(name, SqliteType.Integer) { Value = value });
        }

        internal static void AddBool(this SqliteParameterCollection parameters, string name, bool value)
        {
            parameters.Add(new SqliteParameter(name, SqliteType.Integer) { Value = value ? 1 : 0 });
        }

        internal static void AddReal(this SqliteParameterCollection parameters, string name, float value)
        {
            parameters.Add(new SqliteParameter(name, SqliteType.Real) { Value = value });
        }
    }
}
