using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using visor.Data;
using visor.Modules.Authentication.Services;
using visor.Modules.UserManagement.Services;
using visor.Modules.RoleManagement.Services;
using visor.Modules.PolicyManagement.Services;
using visor.Middleware;
using DotNetEnv;

// Load environment variables from .env file if it exists
if (File.Exists(".env"))
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Handle Elastic Beanstalk environment variables for database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Replace environment variable placeholders if they exist
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RDS_HOSTNAME")))
{
    connectionString = $"Server={Environment.GetEnvironmentVariable("RDS_HOSTNAME")};"
                    + $"Database={Environment.GetEnvironmentVariable("RDS_DB_NAME")};"
                    + $"User={Environment.GetEnvironmentVariable("RDS_USERNAME")};"
                    + $"Password={Environment.GetEnvironmentVariable("RDS_PASSWORD")};"
                    + $"Port={Environment.GetEnvironmentVariable("RDS_PORT")};"
                    + "SslMode=Required;";
}

// Use in-memory database for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("VisorDevDB"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, 
            ServerVersion.AutoDetect(connectionString)));
}

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IPrivilegeService, PrivilegeService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

// Add distributed cache (required for session support)
builder.Services.AddDistributedMemoryCache();

// Add data protection for OAuth state management
builder.Services.AddDataProtection();

// Add session support for OAuth state management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP for localhost
    options.Cookie.Name = "__visor_session";
    options.IOTimeout = TimeSpan.FromMinutes(5); // Increase timeout
    options.Cookie.Path = "/";
    options.Cookie.Domain = null; // Let it default to current domain
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/api/auth/signin-google";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/api/auth/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP for localhost
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "__visor_auth";
})
// Google OAuth temporarily disabled - uncomment and set environment variables to enable
/*
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? throw new InvalidOperationException("GOOGLE_CLIENT_ID environment variable is not set");
    options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? throw new InvalidOperationException("GOOGLE_CLIENT_SECRET environment variable is not set");
    options.CallbackPath = "/auth/google/middleware-callback"; // Different path to avoid our controller
    options.SaveTokens = true;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    
    // Enhanced configuration for state management
    options.AccessDeniedPath = "/?error=access_denied";
    options.ReturnUrlParameter = "returnUrl";
    options.UsePkce = true; // Enable PKCE for better security
    
    // Configure cookies for better compatibility and state management
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP for localhost
    options.CorrelationCookie.HttpOnly = true;
    options.CorrelationCookie.IsEssential = true;
    options.CorrelationCookie.Name = "__GoogleCorrelation";
    options.CorrelationCookie.MaxAge = TimeSpan.FromMinutes(30); // Increase expiration
    options.CorrelationCookie.Path = "/";
    options.CorrelationCookie.Domain = null; // Let it default to current domain
    options.CorrelationCookie.Expiration = TimeSpan.FromMinutes(30);
    
    // Add correlation cookie options for better state management
    options.StateDataFormat = null; // Use default data protection
    
    // Disable built-in OAuth event handlers since we're using manual token exchange
    options.Events.OnRemoteFailure = context =>
    {
        Console.WriteLine($"OAuth middleware failure (bypassed): {context.Failure?.Message}");
        // Don't handle the failure here - let our manual callback handle it
        return Task.CompletedTask;
    };
    
    // Handle successful authentication (for logging only)
    options.Events.OnTicketReceived = context =>
    {
        Console.WriteLine($"OAuth ticket received for user: {context.Principal?.Identity?.Name}");
        return Task.CompletedTask;
    };
    
    // Add redirect validation
    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        Console.WriteLine($"Redirecting to Google OAuth: {context.RedirectUri}");
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    
    // Handle OAuth timeout issues
    options.RemoteAuthenticationTimeout = TimeSpan.FromMinutes(5);
});
*/;

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "visor",
        Version = "v1",
        Description = "A comprehensive backend API for visor system"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Apply pending migrations automatically (only for non-in-memory databases)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Only run migrations if not using in-memory database
        if (!context.Database.IsInMemory())
        {
            context.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully.");
        }
        else
        {
            // For in-memory database, ensure it's created
            context.Database.EnsureCreated();
            Console.WriteLine("In-memory database created successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error setting up database: {ex.Message}");
        // Log the error but don't stop the application
        // You might want to implement proper logging here
    }
}

// Configure the HTTP request pipeline.
// Enable Swagger in all environments (including production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "visor v1");
    c.DocumentTitle = "visor Documentation";
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
});

app.UseMiddleware<ExceptionMiddleware>();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable static file serving
app.UseStaticFiles();

// Enable session middleware
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Add root route to serve the status page
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.MapControllers();

app.Run();


