using System;
using System.Data;
using System.Data.OracleClient;
using Microsoft.Extensions.Logging;
using WinAuthMVC.Models;

namespace WinAuthMVC.Services
{
    public class LoginSessionDAO
    {
        private readonly ILogger<LoginSessionDAO> _logger;
        private readonly OracleDBManager _oracleManager;

        public LoginSessionDAO(OracleDBManager oracleManager, ILogger<LoginSessionDAO> logger)
        {
            _oracleManager = oracleManager;
            _logger = logger;
        }

        public bool DoesLoginExist(string login)
        {
            try
            {
                    string query = "SELECT COUNT(*) FROM T_LOGIN_SESSION WHERE C_LOGIN = :Login";
                    using (var command = new OracleCommand(query, _oracleManager.GetConnection()))
                    {
                        command.Parameters.Add(":Login", login);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if login exists.");
                throw new Exception("Error checking if login exists.", ex);
            }
        }

        public void CreateLoginSession(LoginSession loginSession)
        {
            try
            {
                    string query = @"
                        INSERT INTO T_LOGIN_SESSION (C_APP, C_ID, C_LOGIN, C_DATE, C_NAME, C_ACTIVE)
                        VALUES (:C_APP, :C_ID, :C_LOGIN, :C_DATE, :C_NAME, :C_ACTIVE)";
                    using (var command = new OracleCommand(query, _oracleManager.GetConnection()))
                    {
                        command.Parameters.Add(":C_APP", loginSession.app);
                        command.Parameters.Add(":C_ID", loginSession.id);
                        command.Parameters.Add(":C_LOGIN", loginSession.login);
                        command.Parameters.Add(":C_DATE", loginSession._DateTime);
                        command.Parameters.Add(":C_NAME", loginSession.username);
                        command.Parameters.Add(":C_ACTIVE", loginSession.IsActive);
                        command.ExecuteNonQuery();
                    }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating login session.");
                throw new Exception("Error creating login session.", ex);
            }
        }

        public void UpdateLoginSession(LoginSession loginSession)
        {
            try
            {
                    string query = @"
                        UPDATE T_LOGIN_SESSION 
                        SET C_DATE = :C_DATE, C_NAME = :C_NAME, C_ACTIVE = :C_ACTIVE 
                        WHERE C_LOGIN = :C_LOGIN";
                    using (var command = new OracleCommand(query, _oracleManager.GetConnection()))
                    {
                        command.Parameters.Add(":C_DATE", loginSession._DateTime);
                        command.Parameters.Add(":C_NAME", loginSession.username);
                        command.Parameters.Add(":C_ACTIVE", loginSession.IsActive);
                        command.Parameters.Add(":C_LOGIN", loginSession.login);
                        command.ExecuteNonQuery();
                    }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating login session.");
                throw new Exception("Error updating login session.", ex);
            }
        }

        public void DeleteLoginSession(string id)
        {
            try
            {
                    string query = "DELETE FROM T_LOGIN_SESSION WHERE C_ID = :Id";
                    using (var command = new OracleCommand(query, _oracleManager.GetConnection()))
                    {
                        command.Parameters.Add(":Id", id);
                        command.ExecuteNonQuery();
                    }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting login session.");
                throw new Exception("Error deleting login session.", ex);
            }
        }
    }
}
