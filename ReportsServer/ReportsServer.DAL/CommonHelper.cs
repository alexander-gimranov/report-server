using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ReportsServer.DAL
{
    public class CommonHelper
    {
        private readonly string _connString;
        private readonly DatabaseType _databaseType;

        public CommonHelper(DatabaseType databaseType, string connString)
        {
            _databaseType = databaseType;
            _connString = connString;
        }

        public IDbConnection GetConnection()
        {
            switch (_databaseType)
            {
                case DatabaseType.MSSQL: return new SqlConnection(_connString);
                case DatabaseType.PostgreSQL: throw new NotSupportedException();
                default: throw new NotSupportedException();
            }
        }

        public IReadOnlyCollection<dynamic> GetData(IDbConnection connection, string cmdText,
            IReadOnlyDictionary<string, object> reportParams)
        {
            return TryGetData(connection, cmdText, reportParams, out var collection) ? collection : null;
        }

        public bool TryGetData(IDbConnection connection, string cmdText,
            IReadOnlyDictionary<string, object> reportParams,
            out IReadOnlyCollection<IReadOnlyDictionary<string, object>> collection)
        {
            if (connection is SqlConnection sqlConnection)
            {
                return TryGetDataFromSql(sqlConnection, cmdText, reportParams, out collection);
            }
            collection = null;
            return false;
        }

        private bool TryGetDataFromSql(SqlConnection connection, string cmdText,
            IReadOnlyDictionary<string, object> reportParams,
            out IReadOnlyCollection<IReadOnlyDictionary<string, object>> collection)
        {
            List<IReadOnlyDictionary<string, object>> _collection = null;
            using (var command = new SqlCommand(cmdText + " " + string.Join(",", reportParams.Keys.Select(k => "@" + k)), connection))
            {
                foreach (var param in reportParams)
                {
                    command.Parameters.AddWithValue("@" + param.Key, param.Value);
                }

                connection.Open();
                command.CommandTimeout = 180;
                using (var dataReader = command.ExecuteReader())
                    if (dataReader.HasRows)
                    {
                        var fields = new List<string>();
                        for (var i = 0; i < dataReader.FieldCount; i++)
                        {
                            fields.Add(dataReader.GetName(i));
                        }
                        _collection = new List<IReadOnlyDictionary<string, object>>();
                        while (dataReader.Read())
                        {
                            _collection.Add(fields.ToDictionary(t => t, t => dataReader[t]));
                        }
                    }
            }
            collection = _collection?.AsReadOnly();
            return collection != null;
        }
    }
}