using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration, "AzureAd")
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
                        // Frontend app (the one actually sending tokens)
                        "59aef810-e681-4b84-bc17-2561fe854c0e",
                        $"api://59aef810-e681-4b84-bc17-2561fe854c0e",
                        // Legacy frontend app
                        frontendClientId ?? "f6c2a5e9-3bd5-4223-ad2c-618846a668c5",
                        $"api://{frontendClientId ?? "f6c2a5e9-3bd5-4223-ad2c-618846a668c5"}",
                        // Microsoft Graph
                        "00000003-0000-0000-c000-000000000000",
                         //The client ID that's actually issuing tokens
                        "5e3ce6c0-2b1f-4285-8d4b-75ee78787346",
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

// CORS - Updated for Teams SSO
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net",
            "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net",
            "https://teams.microsoft.com",
            "https://*.teams.microsoft.com",
            "https://*.office.com",
            "https://*.sharepoint.com",
            "http://localhost:3000",
            "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
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

// Configure headers for iframe embedding in Microsoft Teams
app.Use(async (context, next) =>
{
    // Remove X-Frame-Options header to allow iframe embedding
    context.Response.Headers.Remove("X-Frame-Options");

    // Set Content Security Policy to allow embedding in Teams
    context.Response.Headers.Add("Content-Security-Policy",
        "frame-ancestors 'self' https://teams.microsoft.com https://*.teams.microsoft.com https://*.office.com https://*.sharepoint.com");

    await next();
});

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication(); // Always use authentication middleware
app.UseAuthorization();

// Add debugging middleware for API routes
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        Console.WriteLine($"🔍 API Request: {context.Request.Method} {context.Request.Path}");

        // Log to database as well for debugging
        try
        {
            using var scope = app.Services.CreateScope();
            var errorLogger = scope.ServiceProvider.GetRequiredService<IErrorLogger>();
            await errorLogger.LogAsync(
                $"API Request: {context.Request.Method} {context.Request.Path}",
                $"Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}",
                "ApiRequestDebugger"
            );
        }
        catch
        {
            // Ignore logging errors in debug middleware
        }
    }

    await next();
});

app.MapControllers();

Console.WriteLine("🔧 Registering API endpoints...");

// Platform detection helper endpoint
app.MapGet("/api/auth/detect-platform", (HttpRequest request, [FromServices] IErrorLogger errorLogger) =>
{
    try
    {
        var userAgent = request.Headers["User-Agent"].ToString();
        var isTeamsContext = userAgent.Contains("Teams") ||
                            request.Headers.ContainsKey("X-MS-Teams-Context") ||
                            request.Query.ContainsKey("context");

        var result = new
        {
            isTeams = isTeamsContext,
            userAgent = userAgent,
            recommendedAuthFlow = isTeamsContext ? "teams-sso" : "msal-redirect",
            timestamp = DateTime.UtcNow
        };

        // Log successful platform detection
        _ = Task.Run(async () =>
        {
            try
            {
                await errorLogger.LogAsync(
                    "Platform detection successful",
                    $"Is Teams: {isTeamsContext}, User Agent: {userAgent}",
                    "PlatformDetection"
                );
            }
            catch { /* Ignore logging errors */ }
        });

        return Results.Json(result);
    }
    catch (Exception ex)
    {
        // Log the error
        _ = Task.Run(async () =>
        {
            try
            {
                await errorLogger.LogAsync(
                    "Platform detection failed",
                    $"Error: {ex.Message}\nStack: {ex.StackTrace}",
                    "PlatformDetection"
                );
            }
            catch { /* Ignore logging errors */ }
        });

        return Results.Problem("Platform detection failed", statusCode: 500);
    }
});

// Universal auth configuration - works for both web and Teams
app.MapGet("/api/auth/config", (HttpRequest request, [FromServices] IErrorLogger errorLogger) =>
{
    try
    {
        var userAgent = request.Headers["User-Agent"].ToString();
        var isTeamsContext = userAgent.Contains("Teams") ||
                            request.Headers.ContainsKey("X-MS-Teams-Context") ||
                            request.Query.ContainsKey("context");

        var authConfig = new
        {
            // Common configuration for both platforms
            tenantId = tenantId ?? "missing",
            frontendClientId = frontendClientId ?? "f6c2a5e9-3bd5-4223-ad2c-618846a668c5",
            backendClientId = clientId ?? "missing",
            authority = $"https://login.microsoftonline.com/{tenantId ?? "missing"}",

            // Platform-specific configuration
            platform = isTeamsContext ? "teams" : "web",

            // Scopes for different scenarios
            scopes = new
            {
                backend = $"api://{clientId ?? "missing"}/access_as_user",
                graph = "https://graph.microsoft.com/User.Read",
                combined = new[] { $"api://{clientId ?? "missing"}/access_as_user", "https://graph.microsoft.com/User.Read" }
            },

            // Redirect URIs based on platform
            redirectUri = isTeamsContext
                ? "https://teams.microsoft.com/l/auth-callback"
                : "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net",

            // Authentication method recommendation
            recommendedFlow = isTeamsContext ? "teams-sso" : "msal-popup-or-redirect",

            // Backend support
            supportsTokenExchange = hasValidAzureAd,
            tokenExchangeEndpoint = "/api/auth/token-exchange",

            timestamp = DateTime.UtcNow
        };

        // Log successful config load
        _ = Task.Run(async () =>
        {
            try
            {
                await errorLogger.LogAsync(
                    "Auth config loaded successfully",
                    $"Platform: {authConfig.platform}, Tenant: {authConfig.tenantId}, Frontend Client: {authConfig.frontendClientId}",
                    "AuthConfig"
                );
            }
            catch { /* Ignore logging errors */ }
        });

        return Results.Json(authConfig);
    }
    catch (Exception ex)
    {
        // Log the error
        _ = Task.Run(async () =>
        {
            try
            {
                await errorLogger.LogAsync(
                    "Auth config load failed",
                    $"Error: {ex.Message}\nStack: {ex.StackTrace}",
                    "AuthConfig"
                );
            }
            catch { /* Ignore logging errors */ }
        });

        return Results.Problem("Auth config load failed", statusCode: 500);
    }
});

// Universal token exchange endpoint - handles both web tokens and Teams SSO tokens
app.MapPost("/api/auth/token-exchange", async (
    HttpContext httpContext,
    [FromServices] ITokenAcquisition tokenAcquisition,
    [FromServices] IErrorLogger errorLogger) =>
{
    try
    {
        // Get token from Authorization header or request body
        string? userToken = null;

        // Try Authorization header first (for regular web tokens)
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            userToken = authHeader.Substring("Bearer ".Length);
        }

        // Try request body for Teams SSO tokens
        if (string.IsNullOrEmpty(userToken))
        {
            using var reader = new StreamReader(httpContext.Request.Body);
            var body = await reader.ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    var requestData = System.Text.Json.JsonSerializer.Deserialize<TeamsTokenExchangeRequest>(body);
                    userToken = requestData?.UserToken;
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }

        await errorLogger.LogAsync(
            "Token exchange requested",
            $"User token length: {userToken?.Length ?? 0}, Has auth header: {!string.IsNullOrEmpty(authHeader)}",
            "TokenExchange"
        );

        if (string.IsNullOrEmpty(userToken))
        {
            return Results.BadRequest(new { error = "User token is required" });
        }

        // Validate the incoming token and exchange for Graph API token
        var scopes = new[] { "https://graph.microsoft.com/User.Read" };

        if (hasValidAzureAd)
        {
            try
            {
                // For authenticated requests, use the existing authentication context
                // The user token should already be validated by the authentication middleware
                var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);

                return Results.Ok(new
                {
                    accessToken = accessToken,
                    scopes = scopes,
                    expiresIn = 3600,
                    tokenType = "Bearer",
                    exchangeType = !string.IsNullOrEmpty(authHeader) ? "web-token" : "teams-sso"
                });
            }
            catch (Exception ex)
            {
                await errorLogger.LogAsync(
                    "Token exchange failed",
                    $"Error: {ex.Message}\nStack: {ex.StackTrace}",
                    "TokenExchange"
                );

                // Fallback: Return the original token for direct use
                return Results.Ok(new
                {
                    accessToken = userToken,
                    scopes = scopes,
                    expiresIn = 3600,
                    tokenType = "Bearer",
                    exchangeType = "passthrough",
                    note = "Token exchange not available, returning original token"
                });
            }
        }
        else
        {
            // If Azure AD not configured, return the token as-is
            return Results.Ok(new
            {
                accessToken = userToken,
                scopes = scopes,
                expiresIn = 3600,
                tokenType = "Bearer",
                exchangeType = "passthrough",
                note = "Azure AD not configured, returning original token"
            });
        }
    }
    catch (Exception ex)
    {
        await errorLogger.LogAsync(
            "Token exchange error",
            $"Error: {ex.Message}\nStack: {ex.StackTrace}",
            "TokenExchange"
        );

        return Results.Problem(
            detail: "Internal server error during token exchange",
            statusCode: 500
        );
    }
});

// Enhanced diagnostics endpoint
app.MapGet("/", () => new
{
    message = "MiniTasker API is running!",
    version = "Universal Auth - Teams SSO + Web login support",
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
        authentication = hasValidAzureAd ? "Azure AD with OBO" : "Basic",
        iframeEmbeddingEnabled = true,
        universalAuthEnabled = true,
        supportsPlatforms = new[] { "web", "teams" }
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
    headers = new
    {
        xFrameOptions = "Removed (allows iframe embedding)",
        contentSecurityPolicy = "frame-ancestors 'self' https://teams.microsoft.com https://*.teams.microsoft.com https://*.office.com https://*.sharepoint.com"
    },
    universalAuth = new
    {
        enabled = hasValidAzureAd,
        platformDetectionEndpoint = "/api/auth/detect-platform",
        configEndpoint = "/api/auth/config",
        tokenExchangeEndpoint = "/api/auth/token-exchange",
        frontendClientId = frontendClientId ?? "f6c2a5e9-3bd5-4223-ad2c-618846a668c5",
        supportedFlows = new[] { "msal-redirect", "msal-popup", "teams-sso" }
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
        "GET /api/auth/detect-platform",
        "GET /api/auth/config",
        "POST /api/auth/token-exchange",
        "GET /api/notification/test-logging",
        "GET /api/notification/test-exception",
        "GET /api/notification/error-logs",
        "POST /api/notification/send-test"
    },
    controllersRegistered = "Yes - app.MapControllers() called",
    corsConfigured = "Yes - AllowFrontend policy active",
    iframeEmbeddingConfigured = "Yes - CSP headers set for Teams",
    universalAuthConfigured = "Yes - Platform detection and unified auth",
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
    graphApiAvailable = hasValidAzureAd,
    iframeEmbeddingEnabled = true,
    universalAuthEnabled = true
});
Console.WriteLine($"🚀 MiniTasker API starting with Graph API and Universal Auth: {(hasValidAzureAd ? "Enabled" : "Disabled")}");
Console.WriteLine("🔧 Iframe embedding configured for Microsoft Teams");
Console.WriteLine("🔧 Universal authentication configured (Web + Teams)");
app.Run();

// DTO for token exchange (supports both Teams SSO and web scenarios)
public record TeamsTokenExchangeRequest(string? UserToken);