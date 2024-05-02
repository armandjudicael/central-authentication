namespace WinAuthMVC.Services;

using System;
using System.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.OracleClient;
    public class OracleDBManager : IDisposable
    {
        private readonly OracleConnection _sqlConnection;
        private readonly ILogger<OracleDBManager> logger;

        public OracleDBManager(string connectionString, ILogger<OracleDBManager> logger)
        {
            _sqlConnection = new OracleConnection(connectionString);
            this.logger = logger;
        }
        
        public OracleConnection GetConnection()
        {
            if (_sqlConnection.State != ConnectionState.Open)
            {
                _sqlConnection.Open();
            }

            return _sqlConnection;
        }

        public void Dispose()
        {
            if (_sqlConnection != null)
            {
                _sqlConnection.Dispose();
            }
        }
    }
