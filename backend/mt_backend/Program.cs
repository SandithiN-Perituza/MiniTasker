using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using mt_backend.Data;
using mt_backend.Services;
using mt_backend.Services.Interfaces;
using MySql.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var configuration = builder.Configuration;

builder.Services.AddAuthorization();

// Add controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.Authority = "https://login.microsoftonline.com/YOURTENANTID";
//        options.Audience = "api://59aef810-e681-4b84-bc17-2561fe854c0e";
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true
//        };
//    });

//builder.Services.AddAuthorization();

//builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(o =>
    {
        o.Authority = "https://login.microsoftonline.com/7b967b11-c0b9-402b-b483-d694f50dfb82/v2.0";
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = "api://59aef810-e681-4b84-bc17-2561fe854c0e",
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

// Register your services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ISubtaskService, SubtaskService>();
builder.Services.AddScoped<ICommentService, CommentService>();

// Configure MySQL database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

builder.Services.AddDbContext<MiniTaskerDbContext>(options =>
    options.UseMySQL(connectionString)
);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/", () => "MiniTasker API is running! --- Sandithi's version - feature-microsoft-login ---");

app.Run();




//builder.Services.AddDbContext<MiniTaskerDbContext>(options =>
//    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection"))
//);

// Configure MSSQL database context
//builder.Services.AddDbContext<MiniTaskerDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
//);

// Register Teams notification service
//builder.Services.AddSingleton<INotificationService>(provider =>
//    new NotificationService("https://outlook.office.com/webhook/your-webhook-url"));

//using Microsoft.AspNetCore.Authentication.OpenIdConnect;

//Add Azure AD authentication
//builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));


//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.Authority = "https://login.microsoftonline.com/7b967b11-c0b9-402b-b483-d694f50dfb82/v2.0";
//        options.Audience = "api://086fdd43-c0b7-4997-a181-dbf938026ae5"; // must match your Expose an API Application ID URI
//    });


//Add JWT Bearer authentication (for API token validation)
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

// Configure CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowFrontend",
//        policy => policy.WithOrigins("https://teams.microsoft.com", "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net")
//                        .AllowAnyHeader()
//                        .AllowAnyMethod()
//                        .AllowCredentials());
//});

//app.UseCors("AllowTeams");

// Required for both OpenID and JWT
//app.UseAuthentication();
//app.UseAuthorization();