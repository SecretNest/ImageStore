using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore
{
    /// <summary>
    /// Prepares values for LIKE comparison.
    /// </summary>
    /// <remarks>
    /// This replaces the Sql Server version, which differed in two ways that both
    /// mattered.
    ///
    /// Sql Server escapes LIKE wildcards with character classes - "[%]" for a literal
    /// percent. SQLite's LIKE has no character classes at all, so those brackets would
    /// be matched literally while the percent inside them kept its wildcard meaning:
    /// the escaping would not merely fail, it would turn an exact search into a
    /// wildcard one. SQLite instead needs an explicit ESCAPE clause, which is what
    /// <see cref="EscapeClause"/> is for.
    ///
    /// The old version also doubled single quotes. That was a real bug rather than a
    /// dialect difference: these values are bound as parameters and never parsed as
    /// SQL, so doubling corrupted them. Searching for a file named "Don't.jpg" could
    /// not match, because the stored name contains one quote and the search value had
    /// been rewritten to two. Nothing here doubles quotes.
    ///
    /// Case sensitivity needs no handling: SQLite's LIKE already ignores case for
    /// ASCII, which is what Sql Server's default collation did.
    /// </remarks>
    static class SqliteLikeValueBuilder
    {
        /// <summary>
        /// Backslash, chosen because it cannot appear in a Windows file or directory
        /// name and so is never itself the thing being searched for.
        /// </summary>
        internal const string EscapeCharacter = "\\";

        /// <summary>
        /// Append to any LIKE comparison built from these values.
        /// </summary>
        internal const string EscapeClause = " escape '\\'";

        /// <summary>
        /// Escapes the LIKE metacharacters, leaving the value otherwise untouched.
        /// </summary>
        internal static string Escape(string value)
        {
            //Backslash first, or the escapes added below would be escaped again.
            return value
                .Replace(EscapeCharacter, EscapeCharacter + EscapeCharacter)
                .Replace("%", EscapeCharacter + "%")
                .Replace("_", EscapeCharacter + "_");
        }

        /// <summary>
        /// Escapes and wraps in wildcards, for a "contains" search.
        /// </summary>
        internal static string EscapeAndInclude(string value)
        {
            return "%" + Escape(value) + "%";
        }
    }
}
