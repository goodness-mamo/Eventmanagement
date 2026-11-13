
using System;
using System.Text;
using Azure.Storage.Blobs;
using EventManagementApi.Data;
using EventManagementApi.Models.Entities;
using EventManagementApi.Services;
using EventManagementApi.Services.Interfaces;
using EventManagementApi.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Register controllers
builder.Services.AddControllers();

// 2. Register Swagger with JWT Bearer authorization
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

// 3. Register SQL Server database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Register application services
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 5. Register password hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// 6. Read JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is missing. Configure Jwt:Key in User Secrets.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

// 7. Configure JWT Bearer authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// 8. Register authorization
builder.Services.AddAuthorization();

// 9. Configure Azure Blob Storage (Azurite for local development)
var blobConnectionString =
    builder.Configuration["AzureBlobStorage:ConnectionString"];

if (string.IsNullOrWhiteSpace(blobConnectionString))
{
    throw new InvalidOperationException(
        "Azure Blob Storage connection string is missing. " +
        "Configure AzureBlobStorage:ConnectionString in User Secrets.");
}

builder.Services.AddSingleton(
    new BlobServiceClient(blobConnectionString));

builder.Services.AddScoped<BlobStorageService>();

// 10. Build application
var app = builder.Build();

// 11. Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "EventManagementApi v1");

        options.RoutePrefix = "swagger";
    });
}

// 12. Configure middleware
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// 13. Map controllers
app.MapControllers();

// 14. Start application
app.Run();
