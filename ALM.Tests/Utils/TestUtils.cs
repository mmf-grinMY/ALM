using System.Text;
using System.IO;

using Oracle.ManagedDataAccess.Client;

namespace ALM.Tests
{
    internal static class TestUtils
    {
        public static string ConnectionString
        {
            get
            {
                var builder = new StringBuilder().Append("Data Source=");

                using (var reader = new StreamReader("../db.ini"))
                {
                    builder
                        .Append(reader.ReadLine()).Append(";User Id=")
                        .Append(reader.ReadLine()).Append(";Password=")
                        .Append(reader.ReadLine()).Append(";Connection Timeout=360");
                }

                return builder.ToString();
            }
        }
        private static readonly OracleRepository _repository;
        public static OracleRepository Repository => _repository ?? new OracleRepository(new OracleConnection(ConnectionString));
    }
}
