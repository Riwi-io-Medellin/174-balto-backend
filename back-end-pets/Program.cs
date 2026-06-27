using BackEndPets.API.Endpoints;
using BackEndPets.API.Hubs;
using BackEndPets.Application;
using BackEndPets.Infrastructure;
using BackEndPets.Infrastructure.Identity;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "balto";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "balto.api";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-only-change-this-secret-key-32-chars";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/walk-tracking"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSignalR();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var cloudName = builder.Configuration["Cloudinary:CloudName"];
if (!string.IsNullOrEmpty(cloudName))
{
    var cloudinaryAccount = new Account(
        cloudName,
        builder.Configuration["Cloudinary:ApiKey"],
        builder.Configuration["Cloudinary:ApiSecret"]);
    builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));
}

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    await db.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await IdentitySeeder.SeedDemoUserAsync(userManager);
}

app.MapGet("/health", () => Results.Ok("OK"));

app.MapSwaggerEndpoints();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<WalkTrackingHub>("/hubs/walk-tracking");

app.MapAuthEndpoints();
app.MapUsersEndpoints();
app.MapMeEndpoints();
app.MapWalkersEndpoints();
app.MapBusinessesEndpoints();
app.MapPetsEndpoints();
app.MapWalkingHistoryEndpoints();
app.MapWalkSessionsEndpoints();
app.MapFeedbackEndpoints();
app.MapWalkerAssetsEndpoints();
app.MapBusinessAssetsEndpoints();
app.MapBusinessServicesEndpoints();
app.MapPetHistoryEndpoints();
app.MapUploadEndpoints();
app.MapNotificationsEndpoints();

app.Run();