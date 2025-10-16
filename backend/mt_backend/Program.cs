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

// Enhanced configuration validation with detailed logging
var instance = azureAdSection["Instance"];
var tenantId = azureAdSection["TenantId"];
var clientId = azureAdSection["ClientId"];
var clientSecret = azureAdSection["ClientSecret"];
var frontendClientId = configuration["FrontendApp:ClientId"];


if (!string.IsNullOrEmpty(clientSecret))
{
    Console.WriteLine($"   ClientSecret Format: {(clientSecret.Contains('~') ? "✅ Valid format (contains ~)" : "⚠️ May be invalid format")}");
}

bool hasValidAzureAd = azureAdSection.Exists() &&
                       !string.IsNullOrEmpty(instance) &&
                       !string.IsNullOrEmpty(tenantId) &&
                       !string.IsNullOrEmpty(clientId) &&
                       !string.IsNullOrEmpty(clientSecret);

if (hasValidAzureAd)
{
    Console.WriteLine("✅ Configuring Azure AD authentication with Client Secret...");

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
        Console.WriteLine($"   Exception Type: {ex.GetType().Name}");
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
                        $"https://login.microsoftonline.com/{tenantId}/v2.0",
                        $"https://sts.windows.net/{tenantId}/"
                    },
                    ValidateAudience = true,
                    ValidAudiences = new[]
                    {
                        // Backend API
                        clientId,
                        $"api://{clientId}",
                        // Frontend app (for cross-app tokens)
                        frontendClientId ?? "f6c2a5e9-3bd5-4223-ad2c-618846a668c5",
                        $"api://{frontendClientId ?? "f6c2a5e9-3bd5-4223-ad2c-618846a668c5"}",
                        // Microsoft Graph
                        "00000003-0000-0000-c000-000000000000"
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
    var missingFields = new List<string>();
    if (string.IsNullOrEmpty(instance)) missingFields.Add("Instance");
    if (string.IsNullOrEmpty(tenantId)) missingFields.Add("TenantId");
    if (string.IsNullOrEmpty(clientId)) missingFields.Add("ClientId");
    if (string.IsNullOrEmpty(clientSecret)) missingFields.Add("ClientSecret");

    Console.WriteLine($"⚠️ Azure AD not fully configured. Missing: {string.Join(", ", missingFields)}");
    Console.WriteLine("🔄 Using fallback authentication...");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    $"https://login.microsoftonline.com/{tenantId}/v2.0",
                    $"https://sts.windows.net/{tenantId}/"
                },
                ValidateAudience = true,
                ValidAudiences = new[]
                {
                    // Backend API
                    clientId,
                    $"api://{clientId}",
                    // Frontend app (for cross-app tokens)
                    frontendClientId ?? "f6c2a5e9-3bd5-4223-ad2c-618846a668c5",
                    $"api://{frontendClientId ?? "f6c2a5e9-3bd5-4223-ad2c-618846a668c5"}",
                    // Microsoft Graph
                    "00000003-0000-0000-c000-000000000000"
                },
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5)
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
            Console.WriteLine($"✅ GraphTokenService resolved: {graphTokenService != null}");
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

Console.WriteLine("🔧 Registering API endpoints...");

// Enhanced diagnostics endpoint
app.MapGet("/", () => new
{
    message = "MiniTasker API is running!",
    version = "Clean notification system with Graph API - button triggered",
    timestamp = DateTime.UtcNow,
    azureAdConfigured = hasValidAzureAd,
    configuration = new
    {
        azureAdInstanceSet = !string.IsNullOrEmpty(instance),
        azureAdTenantIdSet = !string.IsNullOrEmpty(tenantId),
        azureAdClientIdSet = !string.IsNullOrEmpty(clientId),
        azureAdClientSecretSet = !string.IsNullOrEmpty(clientSecret),
        clientSecretLength = clientSecret?.Length ?? 0,
        clientSecretFormat = clientSecret?.Contains('~') == true ? "Valid" : "Invalid"
    },
    features = new
    {
        buttonTriggeredNotifications = true,
        microsoftGraphAPI = hasValidAzureAd,
        teamsNotifications = hasValidAzureAd,
        authentication = hasValidAzureAd ? "Azure AD" : "Basic"
    }
});

// Configuration debug endpoint
app.MapGet("/debug/config", () => new
{
    azureAdConfigurationStatus = new
    {
        instance = !string.IsNullOrEmpty(instance) ? "Set" : "Missing",
        tenantId = !string.IsNullOrEmpty(tenantId) ? "Set" : "Missing",
        clientId = !string.IsNullOrEmpty(clientId) ? "Set" : "Missing",
        clientSecret = !string.IsNullOrEmpty(clientSecret) ? $"Set (Length: {clientSecret.Length})" : "Missing",
        clientSecretFormat = !string.IsNullOrEmpty(clientSecret) && clientSecret.Contains('~') ? "Valid Azure AD format" : "Invalid format",
        hasValidConfiguration = hasValidAzureAd
    },
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
    timestamp = DateTime.UtcNow
});

// Routes debugging endpoint
app.MapGet("/debug/routes", () => new
{
    availableRoutes = new[]
    {
        "GET /",
        "GET /debug/config",
        "GET /debug/routes",
        "GET /health",
        "GET /api/notification/test-logging",
        "GET /api/notification/test-exception",
        "GET /api/notification/error-logs",
        "POST /api/notification/send-test"
    },
    controllersRegistered = "Yes - app.MapControllers() called",
    corsConfigured = "Yes - AllowFrontend policy active",
    timestamp = DateTime.UtcNow
});

// Simple notification test endpoint (no auth required)
app.MapGet("/api/notification/ping", () => new
{
    message = "Notification controller is accessible",
    timestamp = DateTime.UtcNow,
    availableEndpoints = new[]
    {
        "GET /api/notification/test-logging",
        "GET /api/notification/test-exception", 
        "GET /api/notification/error-logs",
        "POST /api/notification/send-test (requires auth)"
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
