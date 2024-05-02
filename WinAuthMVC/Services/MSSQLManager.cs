using System.Data;

namespace WinAuthMVC.Services;

using System.Data.SqlClient;
public class MSSQLManager : IDisposable
{
        private SqlConnection _sqlConnection;

        private readonly ILogger<MSSQLManager> _logger;
    

        public MSSQLManager(String connectionString,ILogger<MSSQLManager> logger)
        {
            _sqlConnection = new SqlConnection(connectionString);
            _logger = logger;
            CheckConnection();
        }
        
        public SqlConnection GetConnection()
        {
            if (_sqlConnection.State != ConnectionState.Open)
            {
                _sqlConnection.Open();
            }

            return _sqlConnection;
        }
        
        public void CreateRecord(string tableName, Dictionary<string, object> columnValues)
        {
            try
            {
                _sqlConnection.Open();
                var commandText = $"INSERT INTO {tableName} ({string.Join(", ", columnValues.Keys)}) VALUES ({string.Join(", ", columnValues.Keys)})";
                var command = new SqlCommand(commandText, _sqlConnection);
                foreach (var kvp in columnValues)
                {
                    command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                }
                command.ExecuteNonQuery();
                _logger.LogInformation("Record inserted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting record.");
            }
            finally
            {
                _sqlConnection.Close();
            }
        }

        public void UpdateRecord(string tableName, string id, Dictionary<string, object> columnValues)
        {
            try
            {
                _sqlConnection.Open();
                var setClause = string.Join(", ", columnValues.Keys);
                var commandText = $"UPDATE {tableName} SET {setClause} WHERE ID = @Id";
                var command = new SqlCommand(commandText, _sqlConnection);
                foreach (var kvp in columnValues)
                {
                    command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                }
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
                _logger.LogInformation("Record updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating record.");
            }
            finally
            {
                _sqlConnection.Close();
            }
        }

        public void DeleteRecord(string tableName, string id)
        {
            try
            {
                _sqlConnection.Open();
                var commandText = $"DELETE FROM {tableName} WHERE ID = @Id";
                var command = new SqlCommand(commandText, _sqlConnection);
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
                _logger.LogInformation("Record deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting record.");
            }
            finally
            {
                _sqlConnection.Close();
            }
        }

        public bool CheckConnection()
        {
            try
            {
                _sqlConnection.Open();
                _logger.LogInformation("Connection to SQL Server established successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to SQL Server.");
                return false;
            }
            finally
            {
                _sqlConnection.Close();
            }
        }

        public string GetDatabaseVersion()
        {
            try
            {
                _sqlConnection.Open();
                var command = new SqlCommand("SELECT @@VERSION", _sqlConnection);
                string version = command.ExecuteScalar()?.ToString();
                _logger.LogInformation($"Database version: {version}");
                return version;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving database version.");
                return null;
            }
            finally
            {
                _sqlConnection.Close();
            }
        }

        public void Dispose()
        {
            _sqlConnection.Dispose();
        }
    }