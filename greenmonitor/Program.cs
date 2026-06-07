using greenmonitor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
//port 7081
// Register controllers, API explorer, and Swagger for development testing
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<AppDbContext>(options =>
//options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configure MySQL database connection using Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Parse MySQL URL format from Railway (mysql://user:password@host:port/database)
if (connectionString != null && connectionString.StartsWith("mysql://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Server={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};User={userInfo[0]};Password={userInfo[1]};";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var port = Environment.GetEnvironmentVariable("PORT") ?? "5071";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
var app = builder.Build();

// Enable Swagger UI in development
app.UseSwagger();
app.UseSwaggerUI();

// HTTPS redirection disabled to allow local network access from mobile app
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();