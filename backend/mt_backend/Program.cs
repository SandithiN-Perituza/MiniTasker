using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using mt_backend.Data;
using mt_backend.Services;
using mt_backend.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var configuration = builder.Configuration;
var azureAdSection = configuration.GetSection("AzureAd");

bool hasValidAzureAd = azureAdSection.Exists() &&
                       !string.IsNullOrEmpty(azureAdSection["Instance"]) &&
                       !string.IsNullOrEmpty(azureAdSection["TenantId"]) &&
                       !string.IsNullOrEmpty(azureAdSection["ClientId"]);

if (hasValidAzureAd)
{
    Console.WriteLine("✅ Configuring Azure AD authentication...");

    try
    {
        // Add Microsoft Identity Web services with token acquisition
        builder.Services.AddMicrosoftIdentityWebApiAuthentication(configuration, "AzureAd")
                       .EnableTokenAcquisitionToCallDownstreamApi()
                       .AddInMemoryTokenCaches();

        builder.Services.AddScoped<IGraphTokenService, GraphTokenService>();
        Console.WriteLine("✅ GraphTokenService registered successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Failed to configure Microsoft Identity Web: {ex.Message}");
        Console.WriteLine("🔄 Falling back to basic JWT authentication...");
        
        // Fallback to basic JWT authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = new[]
                    {
                        $"https://login.microsoftonline.com/{azureAdSection["TenantId"]}/v2.0",
                        $"https://sts.windows.net/{azureAdSection["TenantId"]}/"
                    },
                    ValidateAudience = true,
                    ValidAudiences = new[]
                    {
                        azureAdSection["ClientId"],
                        "00000003-0000-0000-c000-000000000000", // Microsoft Graph
                        $"api://{azureAdSection["ClientId"]}"
                    },
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });
        
        hasValidAzureAd = false; // Disable Graph API functionality
    }
}
else
{
    Console.WriteLine("⚠️ Azure AD not configured, using fallback authentication...");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false,
                RequireExpirationTime = false
            };
        });
}

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ISubtaskService, SubtaskService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IErrorLogger, ErrorLogger>();

builder.Services.AddScoped<INotificationService>(serviceProvider =>
{
    var errorLogger = serviceProvider.GetRequiredService<IErrorLogger>();
    
    IGraphTokenService? graphTokenService = null;
    if (hasValidAzureAd)
    {
        try
        {
            graphTokenService = serviceProvider.GetService<IGraphTokenService>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to resolve GraphTokenService: {ex.Message}");
            graphTokenService = null;
        }
    }

    return new NotificationService(errorLogger, graphTokenService);
});

// Configure MySQL
var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Missing connection string 'DefaultConnection'");

builder.Services.AddDbContext<MiniTaskerDbContext>(options =>
    options.UseMySQL(connectionString));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net",
            "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net",
            "https://teams.microsoft.com",
            "http://localhost:3000",
            "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// Add global exception handler middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Log the exception using the error logger service
        try
        {
            using var scope = app.Services.CreateScope();
            var errorLogger = scope.ServiceProvider.GetRequiredService<IErrorLogger>();
            await errorLogger.LogAsync(
                $"Unhandled exception: {ex.Message}",
                ex.StackTrace ?? "No stack trace available",
                "GlobalExceptionHandler"
            );
        }
        catch (Exception loggingEx)
        {
            Console.WriteLine($"❌ LOGGING ERROR: {loggingEx.Message}");
        }

        // Also log to console for immediate visibility
        Console.WriteLine($"❌ UNHANDLED EXCEPTION: {ex.Message}");
        Console.WriteLine($"   Stack Trace: {ex.StackTrace}");

        // Return a generic error response
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            success = false,
            error = "Internal server error",
            message = "An unexpected error occurred",
            timestamp = DateTime.UtcNow
        }));
    }
});

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication(); // Always use authentication middleware
app.UseAuthorization();

app.MapControllers();

// Diagnostics
app.MapGet("/", () => new
{
    message = "MiniTasker API is running!",
    version = "Clean notification system with Graph API - button triggered",
    timestamp = DateTime.UtcNow,
    azureAdConfigured = hasValidAzureAd,
    features = new
    {
        buttonTriggeredNotifications = true,
        microsoftGraphAPI = hasValidAzureAd,
        teamsNotifications = hasValidAzureAd,
        authentication = hasValidAzureAd ? "Azure AD" : "Basic"
    }
});


app.MapGet("/health", () => new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    notificationSystem = "Graph API Enabled",
    graphApiAvailable = hasValidAzureAd
});

Console.WriteLine($"🚀 MiniTasker API starting with Graph API: {(hasValidAzureAd ? "Enabled" : "Disabled")}");
app.Run();
