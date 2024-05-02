using Microsoft.AspNetCore.Authentication.Negotiate;
using WinAuthMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
   options.IdleTimeout = TimeSpan.FromHours(1);
   options.Cookie.HttpOnly = true;
   options.Cookie.IsEssential = true;
});

// builder.Services.AddSingleton<MSSQLManager>(provider =>
// {
//     // Retrieve the connection string from configuration
//     var connectionString = builder.Configuration.GetConnectionString("SQLServerConnection");
//     // Resolve an instance of ILogger<OracleDBManager> to pass to the constructor
//     var logger = provider.GetRequiredService<ILogger<MSSQLManager>>();
//     // Create and return an instance of OracleDBManager
//     return new MSSQLManager(connectionString, logger);
// });

builder.Services.AddSingleton<OracleDBManager>(provider =>
{
    // Retrieve the connection string from configuration
    var connectionString = builder.Configuration.GetConnectionString("OracleConnection");
    // Resolve an instance of ILogger<OracleDBManager> to pass to the constructor
    var logger = provider.GetRequiredService<ILogger<OracleDBManager>>();
    // Create and return an instance of OracleDBManager
    return new OracleDBManager(connectionString, logger);
});


builder.Services.AddSingleton<LoginSessionDAO>(); // Register LoginSessionDAO as a singleton
builder.Services.AddSingleton<AppDAO>(); // Register LoginSessionDAO as a singleton

// Add logging
builder.Services.AddLogging();


builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();