using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using WinAuthMVC.Models;
using System.DirectoryServices.Protocols;
using System.Net;
using WinAuthMVC.Services;

namespace WinAuthMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly LoginSessionDAO _loginSessionDAO;
    private readonly AppDAO _appDao;
    
    public HomeController(ILogger<HomeController> logger,LoginSessionDAO loginSessionDAO,AppDAO appDao)
    {
        _logger = logger;
        _loginSessionDAO = loginSessionDAO;
        _appDao = appDao;
    }
    public string GetSubstringAfterBackslash(string input)
    {
        int backslashIndex = input.IndexOf('\\');

        if (backslashIndex != -1 && backslashIndex + 1 < input.Length)
        {
            return input.Substring(backslashIndex + 1);
        }
        else
        {
            throw new ArgumentException("No substring found after '\\'.");
        }
    }
    public IActionResult Index(string app = "default", string login = "default")
    {
        var identityName = @User.Identity.Name;
        var extractedLogin =  GetSubstringAfterBackslash(identityName);
        // Check if login already exists
        bool loginExists = _loginSessionDAO.DoesLoginExist(extractedLogin);
        var fullName = GetFullName(GetSubstringAfterBackslash(identityName));
        
        if (!loginExists)
        {
            var loginSession = new LoginSession
            {
                id = Guid.NewGuid().ToString(), // Generate a unique identifier
                login = extractedLogin,
                _DateTime = DateTime.Now,
                username= fullName,
                IsActive = 1,
                app = app
            };
            // Call the CreateLoginSession method to insert the login session into the database
           _loginSessionDAO.CreateLoginSession(loginSession);
        }
        
        // Store the full name in ViewBag
        ViewBag.FullName = fullName;
        ViewBag.App = app;

       var url = _appDao.GetUrl(app);
       if (!String.IsNullOrEmpty(url))
       {
           // Redirect to the URL in a new tab using JavaScript
           ViewBag.RedirectUrl = url;  
       }
        // Session is active, proceed to MainMenu view
        return View("MainMenu");
    }
    public bool Authenticate(string login, string pwd)
    {
        var identifier = new LdapDirectoryIdentifier("MGPADDC", 389);
        var credential = new NetworkCredential("iflowadmin", "wfpassword","toamasina");
        try
        {
            using (var connection = new LdapConnection(identifier,credential,AuthType.Basic))
            {
                connection.SessionOptions.ProtocolVersion = 3; // Set LDAP protocol version
                _logger.LogInformation(" --- START BINDING ----");
                connection.Bind(new NetworkCredential(login,pwd,"toamasina"));
                _logger.LogInformation(" --- BINDING FINISH ----");
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError("LDAP authentication error: " + e.Message);
            return false;
        }
    }
  public string GetFullName(string username)
{
    try
    {
        var identifier = new LdapDirectoryIdentifier("MGPADDC", 389);
        var credential = new NetworkCredential("iflowadmin", "wfpassword", "toamasina");
        using (var connection = new LdapConnection(identifier, credential, AuthType.Basic))
        {
            connection.SessionOptions.ProtocolVersion = 3; // Set LDAP protocol version
            try
            {
                connection.Bind(); // Attempt to bind to LDAP server
            }
            catch (LdapException lex)
            {
                _logger.LogError(lex, "LDAP binding failed.");
                return username; // Or throw an exception if necessary
            }

            var searchFilter = $"(&(objectclass=person)(sAMAccountName={username}))";
            var searchRequest = new SearchRequest(
                "dc=toamasina,dc=local",
                searchFilter,
                SearchScope.Subtree
            );

            var response = (SearchResponse)connection.SendRequest(searchRequest); // Send LDAP search request

            if (response.Entries.Count > 0)
            {
                var entry = response.Entries[0];
                var fullName = entry.Attributes["displayName"]?.GetValues(typeof(string))[0]?.ToString();
                if (string.IsNullOrEmpty(fullName))
                {
                    fullName = entry.Attributes["cn"]?.GetValues(typeof(string))[0]?.ToString();
                }
                return fullName ?? "";
            }
            else
            {
                _logger.LogWarning("No LDAP entry found for username: {Username}", username);
                return ""; // Consider throwing an exception here instead of returning an empty string
            }
        }
    }
    catch (LdapException lex)
    {
        // Handle LDAP-specific exceptions
        _logger.LogError(lex, "LDAP error occurred while fetching full name for username: {Username}", username);
        throw; // Rethrow the exception for higher-level handling if needed
    }
    catch (Exception ex)
    {
        // Handle other exceptions
        _logger.LogError(ex, "An error occurred while fetching full name for username: {Username}", username);
        throw; // Rethrow the exception for higher-level handling if needed
    }
}


    
    [HttpPost]
    public IActionResult Index(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        if (Authenticate(model.Username, model.Password))
        {
            // Get the full name of the authenticated user
            var fullName = GetFullName(model.Username);
            // Store the full name in ViewBag
            ViewBag.FullName = fullName;
            // Store the full name in session or ViewBag for further use
            HttpContext.Session.SetString("FullName", fullName);
            // Authentication successful, redirect to main page
            ViewBag.App = model.App; // Pass the 'app' parameter to the view
            
            bool loginExists = _loginSessionDAO.DoesLoginExist(model.Username);
            if (!loginExists)
            {
                var loginSession = new LoginSession
                {
                    id = Guid.NewGuid().ToString(), // Generate a unique identifier
                    login = model.Username,
                    _DateTime = DateTime.Now,
                    username= fullName,
                    IsActive = 1,
                    app = model.App
                };
                // Call the CreateLoginSession method to insert the login session into the database
                _loginSessionDAO.CreateLoginSession(loginSession);
            }
            
            var url = _appDao.GetUrl(model.App);
            if (!String.IsNullOrEmpty(url))
            {
                // Redirect to the URL in a new tab using JavaScript
                ViewBag.RedirectUrl = url;  
            }
            
            return View("MainMenu");
        }
        else
        {
            // Authentication failed, set ViewBag.LoginStatus to indicate invalid login
            ViewBag.LoginStatus = 0;
            // Login failed
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }
    }
    public IActionResult Login(string app = "default", string login = "default")
    {   
        ViewBag.App = app;
        ViewBag.Login = login;
        return View("index");
    }
    
    // Logout action method
    public IActionResult Logout()
    {
        // Clear session keys
        HttpContext.Session.Clear();

        // Log the logout event
        _logger.LogInformation("Session cleared.");

        HttpContext.SignOutAsync();
        
        return RedirectToAction("Login", "Home"); 
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}