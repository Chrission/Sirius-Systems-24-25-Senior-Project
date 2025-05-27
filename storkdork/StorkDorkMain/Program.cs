using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.DAL.Concrete;
using StorkDorkMain.Data;
using StorkDorkMain.Services;
using Microsoft.AspNetCore.Identity;
using StorkDork.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Azure.Identity;
using StorkDorkMain.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();

    // Access the SendGrid API Key from User Secrets in Development
    var sendGridApiKey = builder.Configuration["SendGrid:ApiKey"];

    // Add SendGrid to Dependency Injection
    builder.Services.AddSingleton(new SendGridService(sendGridApiKey));
}

if (builder.Environment.IsProduction()) // Use the Azure key vault during development
{
    var keyVaultUrl = builder.Configuration["KeyVault:KeyVaultURL"];

    var credential = new DefaultAzureCredential();  // Uses Managed Identity
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);

    // Development SendGrid setup
    builder.Services.AddSingleton<SendGridService>(serviceProvider =>
    {
        var apiKey = builder.Configuration["SendGridApiKey"];  // This will get the key from Azure Key Vault now
        return new SendGridService(apiKey);
    });
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Needed for identity ui to route properly
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


//StorkDork database setup
var conStrBuilder = new SqlConnectionStringBuilder(
    builder.Configuration.GetConnectionString("StorkDorkDB"));
var connectionString = conStrBuilder.ConnectionString;

//Identity database setup
var conStrBuilderTwo = new SqlConnectionStringBuilder(
    builder.Configuration.GetConnectionString("IdentityDB"));
var connectionStringIdentity = conStrBuilderTwo.ConnectionString;

builder.Services.AddDbContext<StorkDorkIdentityDbContext>(options => options
    .UseLazyLoadingProxies()
    .UseSqlServer(connectionStringIdentity, options =>
        {
            options.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }
    )
);

builder.Services.AddDbContext<StorkDorkDbContext>(options => options
    .UseLazyLoadingProxies()
    .UseSqlServer(connectionString, options =>
        {
            options.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }
    )
);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
.AddEntityFrameworkStores<StorkDorkIdentityDbContext>()
.AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        });
        builder.Services.AddScoped<UserManager<IdentityUser>>();
        builder.Services.AddScoped<DbContext, StorkDorkDbContext>();
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IBirdRepository, BirdRepository>();
        builder.Services.AddScoped<ISightingService, SightingService>();
        builder.Services.AddScoped<ISDUserRepository, SDUserRepository>();
        builder.Services.AddScoped<IMilestoneRepository, MilestoneRepository>();
        builder.Services.AddScoped<IModerationService, ModerationService>();
        builder.Services.AddScoped<IModeratedContentRepository, ModeratedContentRepository>();
        builder.Services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddSwaggerGen();

// Removing this breaks everything for some reason T_T, even when register.cshtml.cs doesn't use IEmailSender? Just leave it. 
builder.Services.AddHttpClient<IEmailSender, ApiEmailSender>();
builder.Services.AddHttpClient<IEBirdService, EBirdService>();

// Add after existing Identity configuration
builder.Services.AddScoped<RoleInitializerService>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Leaflet",
    pattern: "Leaflet/{action=Index}/{id?}",
    defaults: new { controller = "Leaflet" });

app.MapControllerRoute(
    name: "search",
    pattern: "Search/{action=Index}/{id?}",
    defaults: new { controller = "Search" });

app.MapControllerRoute(
    name: "BirdLog",
    pattern: "BirdLog/{action=Index}/{id?}",
    defaults: new { controller = "BirdLog" });

app.MapControllerRoute(
    name: "Bird",
    pattern: "Bird/{action=Index}/{id?}",
    defaults: new { controller = "Bird" });

app.MapControllerRoute(
    name: "Email",
    pattern: "Email/{action=Send}/{id?}",
    defaults: new { controller = "Email" });

app.MapControllerRoute(
    name: "Moderation",
    pattern: "Moderation/{action=Index}/{id?}",
    defaults: new { controller = "Moderation" });

app.MapControllerRoute(
    name: "Admin",
    pattern: "Admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "nearbysightings",
    pattern: "NearbySightings/{action=Index}/{id?}");

// Needed for identity ui routing to work
app.MapRazorPages();

// Ensure roles and admin user exist
using (var scope = app.Services.CreateScope())
{
    var roleInitializer = scope.ServiceProvider.GetRequiredService<RoleInitializerService>();
    await roleInitializer.InitializeRoles();

    // if (app.Environment.IsProduction())
    // {
    //     var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    //     var adminEmail = configuration["AdminUser:Email"];

    //     if (string.IsNullOrEmpty(adminEmail))
    //     {
    //         throw new InvalidOperationException(
    //             "Admin email not found in configuration. Ensure AdminUser:Email is set in Azure Key Vault.");
    //     }

    //     // This will only assign the role if the user exists
    //     await roleInitializer.AssignAdminRole(adminEmail);
    // }
}

app.Run();

// Dummy email to satisfy identity requiring email sending. WILL DELETE LATER 
// public class NoOpEmailSender : IEmailSender
// {
//     public Task SendEmailAsync(string email, string subject, string htmlMessage)
//     {
//         return Task.CompletedTask; // Does nothing, but satisfies the requirement
//     }
// }