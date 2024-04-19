namespace WinAuthMVC.Controllers;
using System;
using System.DirectoryServices.Protocols;
using System.Net;

public class LdapConnectionService
{
    private readonly string _serverAddress;
    private readonly int _port;
    private readonly string _adminUsername;
    private readonly string _adminPassword;

    public LdapConnectionService(string serverAddress, int port, string adminUsername, string adminPassword)
    {
        _serverAddress = serverAddress;
        _port = port;
        _adminUsername = adminUsername;
        _adminPassword = adminPassword;
    }

    public bool Authenticate(string username, string password)
    {
        var identifier = new LdapDirectoryIdentifier(_serverAddress, _port);
        var credential = new NetworkCredential(_adminUsername, _adminPassword);
        
        try
        {
            using (var connection = new LdapConnection(identifier, credential, AuthType.Basic))
            {
                connection.Bind(new NetworkCredential(username, password));
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("LDAP authentication error: " + e.Message);
            return false;
        }
    }
}
