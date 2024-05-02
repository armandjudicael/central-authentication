using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WinAuthMVC.Models;

namespace WinAuthMVC.Services
{
    public class AppDAO
    {
        private readonly MSSQLManager _sqlManager;

        public AppDAO(MSSQLManager sqlManager)
        {
            _sqlManager = sqlManager;
        }

        public void CreateApp(App app)
        {
            var columnValues = new Dictionary<string, object>
            {
                { "name", app.name },
                { "url", app.url },
                { "C_DATE", DateTime.Now },
                { "C_ACTIVE", true }
            };

            _sqlManager.CreateRecord("T_APP", columnValues);
        }

        public void UpdateApp(App app)
        {
            var columnValues = new Dictionary<string, object>
            {
                { "name", app.name },
                { "url", app.url },
                { "C_DATE", DateTime.Now },
                { "C_ACTIVE", true }
            };

            _sqlManager.UpdateRecord("T_APP", app.id, columnValues);
        }

        public void DeleteApp(string id)
        {
            _sqlManager.DeleteRecord("T_APP", id);
        }

        public string GetUrl(string appName)
        {
            // Query to get the URL for the given app name
            string query = "SELECT url FROM T_APP WHERE name = @AppName";

            try
            {
                // Execute the query and get the URL
                using (var command = new SqlCommand(query, _sqlManager.GetConnection()))
                {
                    command.Parameters.AddWithValue("@AppName", appName);
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