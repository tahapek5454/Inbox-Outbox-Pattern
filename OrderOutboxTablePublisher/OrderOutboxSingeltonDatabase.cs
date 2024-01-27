using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OrderOutboxTablePublisher
{
    public static class OrderOutboxSingeltonDatabase
    {
        private static IDbConnection _dbConnection;
        private static bool _dataReaderState = true;

        public static void InitilazeDb(string connectionString)
            => _dbConnection = new SqlConnection(connectionString);

        public static IDbConnection Connection
        {
            get
            {
                if (_dbConnection.State == ConnectionState.Closed)
                    _dbConnection.Open();

                return _dbConnection;
            }
        }

        public static bool DataReaderState => _dataReaderState;

        public static async Task<IEnumerable<T>> QueryAsync<T>(string sql)
            => await _dbConnection.QueryAsync<T>(sql);

        public static async Task<int> ExecuteAsync(string sql)
            => await _dbConnection.ExecuteAsync(sql);

        public static void DataReaderReady()
            => _dataReaderState = true;
        public static void DataReaderBusy()
            => _dataReaderState = false;
    }
}
