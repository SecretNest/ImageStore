using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore
{
    static class SqlServerLikeValueBuilder
    {
        internal static string EscapeAndInclude(string value)
        {
            return "%" + Escape(value) + "%";
        }

        internal static string Escape(string value)
        {
            return value.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        internal static string EscapeForEquals(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
