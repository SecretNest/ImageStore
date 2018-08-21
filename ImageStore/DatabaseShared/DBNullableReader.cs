using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore
{
    static class DBNullableReader
    {
        internal static T? Convert<T>(object value) where T : struct
        {
            if (value == DBNull.Value)
                return null;
            else
                unchecked
                {
                    return (T?)value;
                }
        }

        internal static T ConvertFromReferenceType<T>(object value) where T : class
        {
            if (value == DBNull.Value)
                return null;
            else
                return (T)value;
        }

        internal static object NullCheck(object value)
        {
            if (value == null)
                return DBNull.Value;
            else
                return value;
        }
    }
}
