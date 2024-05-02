using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WinAuthMVC.Models;

namespace WinAuthMVC.Services 
{
    public class LoginSessionDAO 
    {
        private readonly MSSQLManager _sqlManager;
        private readonly ILogger<LoginSessionDAO> _logger;

        public LoginSessionDAO(MSSQLManager sqlManager, ILogger<LoginSessionDAO> logger)
        {
            _sqlManager = sqlManager;
            _logger = logger;
        }
        public bool DoesLoginExist(string login)
        {
            try
            {
                // Query to check if the login exists
                string query = "SELECT COUNT(*) FROM T_LOGIN_SESSION WHERE C_LOGIN = @Login";
                    using (var command = new SqlCommand(query, _sqlManager.GetConnection()))
                    {
                        command.Parameters.AddWithValue("@Login", login);
                        // Execute the command and get the result
                        int count = (int)command.ExecuteScalar();
                        // If count > 0, the login exists; otherwise, it doesn't
                        return count > 0;
                    }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., logging)
                throw new Exception("Error checking if login exists.", ex);
            }
        }

        public void CreateLoginSession(LoginSession loginSession)
        {
            try
            {
                // Query to insert login session into the table
                string query = @"
                    INSERT INTO T_LOGIN_SESSION (C_APP,C_ID,C_LOGIN, C_DATE, C_NAME, C_ACTIVE)
                    VALUES (@C_APP,@C_ID,@C_LOGIN,@C_DATE,@C_NAME,@C_ACTIVE)";
                
                    // Create a command object with parameters
                    using (var command = new SqlCommand(query, _sqlManager.GetConnection()))
                    {
                        command.Parameters.AddWithValue("@C_APP", loginSession.app);
                        command.Parameters.AddWithValue("@C_ID", loginSession.id);
                        command.Parameters.AddWithValue("@C_LOGIN", loginSession.login);
                        command.Parameters.AddWithValue("@C_DATE", loginSession._DateTime);
                        command.Parameters.AddWithValue("@C_NAME", loginSession.username);
                        command.Parameters.AddWithValue("@C_ACTIVE", loginSession.IsActive);

                        // Execute the command
                        command.ExecuteNonQuery();
                    }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., logging)
                throw new Exception("Error creating login session.", ex);
            }
        }

        public void UpdateLoginSession(LoginSession loginSession)
        {
            try
            {
                // Query to update login session in the table
                string query = @"
                    UPDATE T_LOGIN_SESSION 
                    SET C_DATE = @Date, C_NAME = @Name, C_ACTIVE = @Active 
                    WHERE C_LOGIN = @Login";
                
                using (var command = new SqlCommand(query, _sqlManager.GetConnection()))
                {
                    command.Parameters.AddWithValue("@Date", loginSession._DateTime);
                    command.Parameters.AddWithValue("@Name", loginSession.username);
                    command.Parameters.AddWithValue("@Active", loginSession.IsActive);
                    command.Parameters.AddWithValue("@Login", loginSession.login);

                    // Execute the command
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., logging)
                throw new Exception("Error updating login session.", ex);
            }
        }

        public void DeleteLoginSession(string id)
        {
            try
            {
                // Query to delete login session from the table
                string query = "DELETE FROM T_LOGIN_SESSION WHERE C_ID = @Id";
                
                using (var command = new SqlCommand(query,_sqlManager.GetConnection()))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    // Execute the command
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., logging)
                throw new Exception("Error deleting login session.", ex);
            }
        }
    }
}
