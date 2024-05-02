namespace WinAuthMVC.Services;

using System;
using System.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.OracleClient;
    public class OracleDBManager : IDisposable
    {
        private readonly OracleConnection connection;
        private readonly ILogger<OracleDBManager> logger;

        public OracleDBManager(string connectionString, ILogger<OracleDBManager> logger)
        {
            connection = new OracleConnection(connectionString);
            this.logger = logger;
        }

        public DataTable ExecuteQuery(string query, IDictionary<string, object> parameters = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param.Key, param.Value);
                        }
                    }

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    OracleDataAdapter adapter = new OracleDataAdapter(command);
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing query: {Query}", query);
                // Log the exception
            }
            return dataTable;
        }

        public int ExecuteNonQuery(string query, IDictionary<string, object> parameters = null)
        {
            int rowsAffected = 0;

            try
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param.Key, param.Value);
                        }
                    }

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing non-query: {Query}", query);
                // Log the exception
            }

            return rowsAffected;
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
