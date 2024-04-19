using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using WinAuthMVC.Models;
using System.DirectoryServices.Protocols;
using System.Net;
namespace WinAuthMVC.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    
    
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    public IActionResult Index()
    {
        var userName = HttpContext.Session.GetString("UserName");
        var identityName = @User.Identity.Name;
        if (string.IsNullOrEmpty(userName))
        {
            _logger.LogInformation(userName+" doesn't exist or session is expired  ");
            // Session expired or user not logged in, redirect to login page
            return RedirectToAction("Login", "Home");
        }
        
        _logger.LogInformation(" User :  "+identityName);

        // Session is active, proceed to MainMenu view
        return View("MainMenu");
    }
    

    public bool Authenticate(string login, string pwd)
    {
        var identifier = new LdapDirectoryIdentifier("MGPADDC", 389);
        var credential = new NetworkCredential("iflowadmin", "wfpassword");
        try
        {
            using (var connection = new LdapConnection(identifier,credential,AuthType.Basic))
            {
                connection.Bind(new NetworkCredential(login,pwd,"toamasina"));
                
                SearchRequest searchRequest = new SearchRequest(
                    "dc=toamasina,dc=local",
                    $"(&(objectclass=person)(sAMAccountName={login}))",
                    SearchScope.Subtree
                );
                
                SearchResponse searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                if (searchResponse.Entries.Count > 0)
                {
                    SearchResultEntry entry = searchResponse.Entries[0];
                    // Retrieve the full name from the directory entry
                    var fullName = entry.Attributes["displayName"]?.GetValues(typeof(string))[0]?.ToString();
                    if (string.IsNullOrEmpty(fullName))
                    {
                        // If displayName is not available, try getting common name (cn)
                        fullName = entry.Attributes["cn"]?.GetValues(typeof(string))[0]?.ToString();
                    }
                    _logger.LogInformation(" Full name : "+fullName);
                }
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("LDAP authentication error: " + e.Message);
            return false;
        }
    }
    
    public string GetFullName(string username)
    {
        try
        {
            var identifier = new LdapDirectoryIdentifier("MGPADDC", 389);
            var credential = new NetworkCredential("iflowadmin", "wfpassword");
        
            using (var connection = new LdapConnection(identifier, credential, AuthType.Basic))
            {
                connection.Bind();
                SearchRequest searchRequest = new SearchRequest(
                    "dc=toamasina,dc=local",
                    $"(&(objectclass=person)(sAMAccountName={username}))",
                    SearchScope.Subtree
                );
                SearchResponse searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

                if (searchResponse.Entries.Count > 0)
                {
                    SearchResultEntry entry = searchResponse.Entries[0];
                    // Retrieve the full name from the directory entry
                    var fullName = entry.Attributes["displayName"]?.GetValues(typeof(string))[0]?.ToString();
                    if (string.IsNullOrEmpty(fullName))
                    {
                        // If displayName is not available, try getting common name (cn)
                        fullName = entry.Attributes["cn"]?.GetValues(typeof(string))[0]?.ToString();
                    }
                    return fullName;
                }
                return null;
            }
        }
        catch (LdapException ex)
        {
            _logger.LogError(ex, "LDAP authentication error: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during LDAP authentication");
            throw;
        }
    }

    
    public bool Authenticate1(string username, string password)
    {
        _logger.LogInformation("Checking credentials, username = {Username}, password = {Password}", username,
            password);
        try
        {
            var identifier = new LdapDirectoryIdentifier("MGPADDC", 389);
            var credential = new NetworkCredential("iflowadmin", "wfpassword");
            using (var connection = new LdapConnection(identifier,credential,AuthType.Basic))
            {
                connection.Bind();
                SearchRequest searchRequest = new SearchRequest("dc=toamasina,dc=local", $"(&(objectclass=person)(sAMAccountName = {username}))", SearchScope.Subtree);
                SearchResponse searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                if (searchResponse.Entries.Count > 0)
                {
                    SearchResultEntry entry = searchResponse.Entries[0];
                    _logger.LogInformation(entry.ToString());
                    return true;
                }
                return false;
            }
        }
        catch (LdapException ex)
        {
            _logger.LogError(ex, "LDAP authentication error: {Message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during LDAP authentication");
            throw;
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
            var fullName = "";
            
            // Store the full name in ViewBag
            ViewBag.FullName = fullName;

    
            // Store the full name in session or ViewBag for further use
            HttpContext.Session.SetString("FullName", fullName);

            // Authentication successful, redirect to main page
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
    
    public IActionResult Login()
    {       
        // If authentication successful, store user's name in session
        HttpContext.Session.SetString("UserName", @User.Identity.Name);
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
        
        return RedirectToAction("Index", "Home"); 
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