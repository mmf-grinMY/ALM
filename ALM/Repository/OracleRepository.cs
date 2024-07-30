using ALM.Entities;

using System.Collections.Generic;
using System.Text;
using System;

using Oracle.ManagedDataAccess.Client;

namespace ALM
{
    public class OracleRepository : IRepository
    {
        private readonly OracleConnection _connection;
        public OracleRepository(OracleConnection connection)
        {
            _connection = connection;
            _connection.Open();
        }
        public Primitive GetByGuid(string guid, string horizon = "200F")
        {
            using (var command = new OracleCommand($"SELECT * FROM \"K{horizon}_TRANS_CLONE\" WHERE \"OBJECTGUID\" = '{guid}'", _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Primitive(
                            reader.GetString(reader.GetOrdinal("GEOWKT")),
                            reader.GetString(reader.GetOrdinal("DRAWJSON")),
                            reader.GetString(reader.GetOrdinal("PARAMJSON")),
                            "",
                            10,
                            null,
                            null,
                            1,
                            Guid.Parse(guid)
                        );
                    }
                }
            }

            throw new InvalidOperationException($"Не удалось найти элемент с guid \"{guid.ToString().ToUpper()}\"");
        }
        private bool TryGetLongGeometry(string geometry, string guid, string numbColumnName, string horizon, out string longGeometry)
        {
            var builder = new StringBuilder(10_000).Append(geometry);
            var query = $"SELECT \"PAGE\" FROM \"K{horizon}_TRANS_CLONE_GEOWKT\" WHERE OBJECTGUID = '{guid}' ORDER BY \"";
            try
            {
                using (var command = new OracleCommand(query + numbColumnName + "\"", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            builder.Append(reader.GetString(0));
                        }
                    }
                }

                longGeometry = builder.ToString();
                return true;
            }
            catch (OracleException)
            {
                longGeometry = string.Empty;
                return false;
            }
        }
        public string GetLongGeometry(Primitive primitive, string horizon = "200F")
        {
            if (TryGetLongGeometry(primitive.Geometry, primitive.Guid, "NUMB", horizon, out string longGeometry))
            {
                return longGeometry;
            }
            else if (TryGetLongGeometry(primitive.Geometry, primitive.Guid, "NUMBER", horizon, out longGeometry))
            {
                return longGeometry;
            }
            else
            {
                throw new InvalidOperationException($"Не удалось прочитать длинную геометрию объекта \"{primitive.Guid}\"");
            }
        }
        public IEnumerable<Primitive> GetAllFullPolylines(int limit = int.MaxValue)
        {
            var query = $"SELECT * FROM \"K200F_TRANS_CLONE\" WHERE \"GEOWKT\" NOT LIKE 'POINT((%' AND \"GEOWKT\" LIKE '%))' AND ROWNUM < {limit}";
            using (var command = new OracleCommand(query, _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new Primitive(
                            reader.GetString(reader.GetOrdinal("GEOWKT")),
                            reader.GetString(reader.GetOrdinal("DRAWJSON")),
                            reader.GetString(reader.GetOrdinal("PARAMJSON")),
                            "",
                            10,
                            null,
                            null,
                            1,
                            Guid.Parse(reader.GetString(reader.GetOrdinal("OBJECTGUID")))
                        );
                    }
                }
            }
        }
    }
}
