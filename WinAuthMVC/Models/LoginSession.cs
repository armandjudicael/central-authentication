namespace WinAuthMVC.Models;

public class LoginSession
{
    internal String id { get; set; }
    internal String login { get; set; }
    internal String app { get; set; }
    internal String username { get; set; }
    internal int IsActive { get; set;}
    
    internal DateTime _DateTime { get; set; }
}