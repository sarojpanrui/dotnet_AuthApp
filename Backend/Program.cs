using System.Text;
using AuthApp.Models;
using AuthApp.Config;
using AuthApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Configure MongoDB settings
builder.Services.Configure<AuthDataBaseSettings>(
    builder.Configuration.GetSection("AuthDataBaseSettings"));


builder.Services.AddSingleton<AuthServices>();

// 2️⃣ Configure JWT settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));


// 3️⃣ Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
               .AllowCredentials();
    });
});

// 4️⃣ Add controllers, Swagger, Authorization
builder.Services.AddControllers()
.AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

var app = builder.Build();

// 5️⃣ Middleware
app.UseCors("AllowReactApp");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();   // optional but recommended
app.UseAuthentication();     // MUST be before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();
