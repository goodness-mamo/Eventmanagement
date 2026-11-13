using EventManagement.Mvc.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURATION DIAGNOSTICS
// ==========================================

Console.WriteLine("========== CONFIGURATION DEBUG ==========");

Console.WriteLine(
    $"Content Root: {builder.Environment.ContentRootPath}");

Console.WriteLine(
    $"Environment: {builder.Environment.EnvironmentName}");

var settingsFile = Path.Combine(
    builder.Environment.ContentRootPath,
    "appsettings.json");

Console.WriteLine(
    $"appsettings.json exists: {File.Exists(settingsFile)}");

Console.WriteLine(
    $"API Base URL: {builder.Configuration["ApiSettings:BaseUrl"] ?? "NULL"}");

Console.WriteLine("=========================================");

// ==========================================
// 2. REGISTER MVC
// ==========================================

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<OrganiserApiService>();

// ==========================================
// 3. CONFIGURE API CONNECTION
// ==========================================

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException(
        "API BaseUrl is missing. Check appsettings.json.");
}

builder.Services.AddHttpClient("RaceDayApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ==========================================
// 4. REGISTER API SERVICES
// ==========================================

builder.Services.AddScoped<ApiConnectionService>();
builder.Services.AddScoped<AuthenticationApiService>();
builder.Services.AddScoped<ParticipantApiService>();
builder.Services.AddScoped<ResultsApiService>();

// ==========================================
// 5. CONFIGURE COOKIE AUTHENTICATION
// ==========================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "RaceDay.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

// ==========================================
// 6. REGISTER AUTHORIZATION
// ==========================================

builder.Services.AddAuthorization();

// ==========================================
// 7. BUILD APPLICATION
// ==========================================

var app = builder.Build();

// ==========================================
// 8. CONFIGURE MIDDLEWARE
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Authentication must run before authorization
app.UseAuthentication();

app.UseAuthorization();

// ==========================================
// 9. CONFIGURE DEFAULT MVC ROUTE
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==========================================
// 10. START APPLICATION
// ==========================================

app.Run();