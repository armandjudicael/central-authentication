using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Data.SqlClient;
using WinAuthMVC.Models;

namespace WinAuthMVC.Services
{
    public class AppDAO
    {
        private readonly OracleDBManager _sqlManager;

        public AppDAO(OracleDBManager sqlManager)
        {
            _sqlManager = sqlManager;
        }
        
        public string GetUrl(string appName)
        {
            string query = "SELECT C_URL FROM T_APP WHERE C_NAME = :AppName";

            try
            {
                // Execute the query and get the URL
                using (var command = new OracleCommand(query, _sqlManager.GetConnection()))
                {
                    command.Parameters.Add(":AppName", appName);
                    var result = command.ExecuteScalar();
                    return result?.ToString();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., logging)
                throw new Exception("Error getting URL for app.", ex);
            }
        }

    }
}