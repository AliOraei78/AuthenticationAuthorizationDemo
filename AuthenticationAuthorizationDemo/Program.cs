using AuthenticationAuthorizationDemo.Authorization;
using AuthenticationAuthorizationDemo.Configuration;
using AuthenticationAuthorizationDemo.Data;
using AuthenticationAuthorizationDemo.Models;
using AuthenticationAuthorizationDemo.Seeders;
using AuthenticationAuthorizationDemo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookStore API",
        Version = "v1",
        Description = "Simple Api",
        Contact = new OpenApiContact
        {
            Name = "Ali Jenabi",
            Email = "a.jenabi78@example.com"
        }
    });
    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=app.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
/*
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);       // session timeout
    options.SlidingExpiration = true;                        // renew on activity
    options.LoginPath = "/api/account/login";                // not used in API, but good practice
    options.LogoutPath = "/api/account/logout";
});
*/

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<RefreshTokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
        ?? throw new InvalidOperationException("JWT settings missing");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Mandatory validations
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(5),      // Small tolerance only (default is 5 min → reduced)

        // Prevent common attacks
        RequireSignedTokens = true,
        RequireExpirationTime = true,
        ValidateActor = false,                    // Usually not needed for API
        NameClaimType = JwtRegisteredClaimNames.UniqueName,
        RoleClaimType = ClaimTypes.Role
    };

    // Optional: Reject "none" algorithm explicitly
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Additional custom checks if needed (e.g., check jti against blacklist later)
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            // Log failures in production
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrOver21", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(21)));

    // Other policies remain unchanged
    options.AddPolicy("AtLeast18", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));

    options.AddPolicy("RequireClaimEmailConfirmed", policy =>
        policy.RequireClaim("email_verified", "true"));

    options.AddPolicy("CanManageUsers", policy =>
    policy.RequireClaim("Permission", Permissions.CanManageUsers));

    options.AddPolicy("CanViewUsers", policy =>
        policy.RequireClaim("Permission", Permissions.CanViewUsers));
});

builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, AdminAsAgeBypassHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authentication & Authorization Demo v1");
        // Optional: nicer UI settings
        c.RoutePrefix = string.Empty;  // optional: open at root URL /
    });
    using var scope = app.Services.CreateScope();
    await IdentityDataSeeder.SeedRolesAndPermissions(scope.ServiceProvider);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
