using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.Services;
using mt_backend.Services.Interfaces;
using MySql.EntityFrameworkCore.Extensions;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var configuration = builder.Configuration;

//Add JWT Bearer authentication (for API token validation)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// Add controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();

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

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// Required for both OpenID and JWT
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "MiniTasker API is running!");

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