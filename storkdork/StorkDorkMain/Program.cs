

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.DAL.Concrete;
using StorkDorkMain.Data;
using Microsoft.AspNetCore.Identity;
using StorkDork.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Azure.Identity;
using StorkDorkMain.Models;
using StorkDorkMain.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
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

        builder.Services.AddDbContext<StorkDorkContext>(options => options
            .UseLazyLoadingProxies()
            .UseSqlServer(connectionString));

        //Identity database setup
        var conStrBuilderTwo = new SqlConnectionStringBuilder(
            builder.Configuration.GetConnectionString("IdentityDB"));
        var connectionStringIdentity = conStrBuilderTwo.ConnectionString;

        builder.Services.AddDbContext<StorkDorkIdentityDbContext>(options => options
            .UseLazyLoadingProxies()
            .UseSqlServer(connectionStringIdentity)
        );

        builder.Services.AddDbContext<StorkDorkDbContext>(options => options
            .UseLazyLoadingProxies()
            .UseSqlServer(builder.Configuration.GetConnectionString("StorkDorkDB"))
        );

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<StorkDorkIdentityDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        });
<<<<<<< HEAD
=======

>>>>>>> dev
        builder.Services.AddScoped<UserManager<IdentityUser>>();
        builder.Services.AddScoped<DbContext, StorkDorkContext>();
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IBirdRepository, BirdRepository>();
        builder.Services.AddScoped<ISightingService, SightingService>();
        builder.Services.AddScoped<ISDUserRepository, SDUserRepository>();
        builder.Services.AddScoped<IMilestoneRepository, MilestoneRepository>();

        builder.Services.AddSwaggerGen();

        // Removing this breaks everything for some reason T_T, even when register.cshtml.cs doesn't use IEmailSender? Just leave it. 
        builder.Services.AddHttpClient<IEmailSender, ApiEmailSender>();
        builder.Services.AddHttpClient<IEBirdService, EBirdService>();

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

        // Needed for identity ui routing to work
        app.MapRazorPages();

        app.Run();
    }
}

// Dummy email to satisfy identity requiring email sending. WILL DELETE LATER 
// public class NoOpEmailSender : IEmailSender
// {
//     public Task SendEmailAsync(string email, string subject, string htmlMessage)
//     {
//         return Task.CompletedTask; // Does nothing, but satisfies the requirement
//     }
// }