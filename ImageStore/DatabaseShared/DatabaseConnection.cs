using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore
{
    public static class DatabaseConnection
    {
        static SqlConnection _current;

        public static SqlConnection Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("Database is not specified.");
                else
                    return _current;
            }
        }

        internal static void Connect(string connectionString)
        {
            Close();
            _current = new SqlConnection(connectionString);
            _current.Open();
        }

        internal static void Close()
        {
            if (_current != null)
            {
                _current.Close();
                _current.Dispose();
            }
            _current = null;
        }
    }
}
