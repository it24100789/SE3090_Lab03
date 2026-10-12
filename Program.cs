using System.Text;
using CampusEats.Api.Data;
using CampusEats.Api.Models;
using CampusEats.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add CORS Policy for Deployed Frontend Origin (Task 03)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Allows local and any Vercel preview/production URLs
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste ONLY the token - no 'Bearer ' prefix."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IMenuService, MenuService>();

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? "super_secret_default_key_at_least_32_bytes_long_12345");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => opt.TokenValidationParameters = new()
    {
        ValidateIssuer = true, ValidIssuer = jwt["Issuer"] ?? "CampusEats",
        ValidateAudience = true, ValidAudience = jwt["Audience"] ?? "CampusEatsUsers",
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Enable Swagger in all environments for testing API endpoints
app.UseSwagger();
app.UseSwaggerUI();

// Enable CORS policy (Task 03)
app.UseCors("Frontend");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Only seed if database is available
        if (db.Database.CanConnect() && !db.MenuItems.Any())
        {
            db.MenuItems.AddRange(
                new MenuItem { Name = "Kottu Roti", Price = 750m, Category = "Mains" },
                new MenuItem { Name = "Fried Rice", Price = 850m, Category = "Mains" },
                new MenuItem { Name = "Watalappan", Price = 350m, Category = "Dessert" });
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB connection notice: {ex.Message}");
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();