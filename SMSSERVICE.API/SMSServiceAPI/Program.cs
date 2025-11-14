using IntegratedImplementation.Datas;
using IntegratedImplementation.DTOS.Authentication;
using IntegratedInfrustructure.Data;
using IntegratedInfrustructure.Model.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Session;
using SMSServiceAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(policyBuilder =>
{
    policyBuilder.AddDefaultPolicy(policy =>
    {
        policy
           .WithOrigins(
                "http://localhost:4200",
                "https://smsu.daftechsocialictsolution.com"
            )
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials(); // This is okay since you've specified origins
    });
});

// Configure JSON serialization options
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});



// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<ApplicationSetting>(builder.Configuration.GetSection("ApplicationSetting"));

// Configure database
var connectionString = builder.Configuration["ConnectionStrings:SqlConnection"];
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure SignalR
builder.Services.AddSignalR();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Strengthen password policy
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 3;
    
    // Configure lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // Configure user settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
});

// Configure cookie policy for secure cookies
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

// Configure session options
builder.Services.Configure<SessionOptions>(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15); // Set to 15 minutes as requested
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = "SMSService.Session";
    options.Cookie.MaxAge = TimeSpan.FromMinutes(15);
});

// Configure form options with reasonable limits
builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = 1024 * 1024; // 1MB limit
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB limit
    o.MemoryBufferThreshold = 64 * 1024; // 64KB buffer
});

// Configure AutoMapper and other services
builder.Services.AddAutoMapper(typeof(AutoMapperConfigurations));
builder.Services.AddCoreBusiness();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization(options =>
          {
              options.AddPolicy("ValidToken", policy =>
                  policy.Requirements.Add(new TokenBlacklistRequirement()));
          });

// Configure JWT authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["ApplicationSetting:JWT_Secret"].ToString());
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // Allow HTTP for development
    x.SaveToken = false;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["ApplicationSetting:JWT_Issuer"] ?? "SMS_Service",
        ValidAudience = builder.Configuration["ApplicationSetting:JWT_Audience"] ?? "SMS_Service_Users",
        ClockSkew = TimeSpan.Zero,
        ValidateLifetime = true,
        RequireExpirationTime = true
    };
});

// Add necessary services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Seed roles if they don't exist
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "SuperAdmin", "Admin" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"Created role: {role}");
        }
    }
}

// Register global exception handling middleware (should be first)
app.UseGlobalExceptionHandling();

// Register request validation middleware
app.UseRequestValidation();

//version disclosure
app.Use(async (context, next) =>
{
            // Remove server information disclosure headers
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("X-AspNet-Version");
            context.Response.Headers.Remove("X-AspNetMvc-Version");
            context.Response.Headers.Remove("X-SourceFiles");
            context.Response.Headers.Remove("X-Frame-Options"); // We'll set this properly below
            
            // Remove additional version disclosure headers
            context.Response.Headers.Remove("X-Version");
            context.Response.Headers.Remove("X-Build");
            context.Response.Headers.Remove("X-Environment");
            context.Response.Headers.Remove("X-Runtime");
            context.Response.Headers.Remove("X-Framework");
            context.Response.Headers.Remove("X-Application");
    
    await next();
});

// Security configuration based on environment
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    // Development-specific security headers (less restrictive for debugging)
    app.Use(async (context, next) =>
    {
        context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
        context.Response.Headers.TryAdd("X-Frame-Options", "SAMEORIGIN"); // Less restrictive for dev
        context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer-when-downgrade");
        await next();
    });
}
else
{
    // Production security - more restrictive
    app.Use(async (context, next) =>
    {
        // Get HTTPS configuration from appsettings
        var enforceHttps = builder.Configuration.GetValue<bool>("Security:HTTPS:EnforceHTTPS", true);
        var redirectToHttps = builder.Configuration.GetValue<bool>("Security:HTTPS:RedirectToHTTPS", true);
        var maxRedirects = builder.Configuration.GetValue<int>("Security:HTTPS:MaxRedirects", 3);
        var trustProxyHeaders = builder.Configuration.GetValue<bool>("Security:HTTPS:TrustProxyHeaders", true);
        
        if (enforceHttps && redirectToHttps)
        {
            // Enhanced HTTPS detection for production environments
            var isHttps = context.Request.IsHttps;
            
            if (trustProxyHeaders)
            {
                isHttps = isHttps || 
                          context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() == "https" ||
                          context.Request.Headers["X-Forwarded-Ssl"].FirstOrDefault() == "on" ||
                          context.Request.Headers["X-Forwarded"].FirstOrDefault()?.Contains("https") == true ||
                          context.Request.Headers["X-Forwarded-Host"].FirstOrDefault()?.Contains("https") == true;
            }
            
            // Only redirect if we're certain it's not HTTPS and we're not already in a redirect
            if (!isHttps && !context.Request.Path.StartsWithSegments("/Error"))
            {
                var redirectCount = context.Request.Headers["X-Redirect-Count"].FirstOrDefault();
                var currentCount = string.IsNullOrEmpty(redirectCount) ? 0 : int.Parse(redirectCount);
                
                if (currentCount < maxRedirects)
                {
                    var redirectUrl = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
                    context.Response.Headers.Add("X-Redirect-Count", (currentCount + 1).ToString());
                    context.Response.Redirect(redirectUrl, permanent: false);
                    return;
                }
                else
                {
                    // If too many redirects, return an error instead
                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"Too many redirects. Please use HTTPS directly.\",\"statusCode\":400}");
                    return;
                }
            }
        }
        await next();
    });
    
    // Production error handling - don't expose stack traces
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

// Production security headers (applied in all environments but more restrictive in production)
app.Use(async (context, next) =>
{
    // Enhanced HTTPS detection for production environments
    var isHttps = context.Request.IsHttps || 
                  context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() == "https" ||
                  context.Request.Headers["X-Forwarded-Ssl"].FirstOrDefault() == "on" ||
                  context.Request.Headers["X-Forwarded"].FirstOrDefault()?.Contains("https") == true ||
                  context.Request.Headers["X-Forwarded-Host"].FirstOrDefault()?.Contains("https") == true;
    
    // X-Content-Type-Options: Prevents MIME-type sniffing
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    
    // X-Frame-Options: Prevents clickjacking attacks
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    
    // Strict-Transport-Security: Forces HTTPS and protects against SSL stripping
    // Only add HSTS if we're confident it's HTTPS
    if (isHttps)
    {
        context.Response.Headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    }
    
    // Referrer-Policy: Controls referrer information leakage
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // X-Permitted-Cross-Domain-Policies: Restricts cross-domain policies
    context.Response.Headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");
    
    // Permissions-Policy: Restricts browser features and APIs
    context.Response.Headers.TryAdd("Permissions-Policy", 
        "geolocation=(), " +
        "microphone=(), " +
        "camera=(), " +
        "payment=(), " +
        "usb=(), " +
        "magnetometer=(), " +
        "gyroscope=(), " +
        "accelerometer=(), " +
        "ambient-light-sensor=(), " +
        "autoplay=(), " +
        "encrypted-media=(), " +
        "picture-in-picture=(), " +
        "speaker-selection=(), " +
        "cross-origin-isolated=(), " +
        "display-capture=(), " +
        "document-domain=(), " +
        "execution-while-not-rendered=(), " +
        "execution-while-out-of-viewport=(), " +
        "fullscreen=(), " +
        "keyboard-map=(), " +
        "oversized-images=(), " +
        "publickey-credentials-get=(), " +
        "screen-wake-lock=(), " +
        "sync-xhr=(), " +
        "trust-token-redemption=(), " +
        "web-share=()");
    
    // Enhanced Content Security Policy (CSP) - More restrictive and secure
    context.Response.Headers.TryAdd("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-eval'; " + // Note: Consider removing 'unsafe-eval' if possible
        "style-src 'self' 'unsafe-inline'; " + // Note: Consider removing 'unsafe-inline' if possible
        "img-src 'self' data: https: blob:; " +
        "font-src 'self' data: https:; " +
        "connect-src 'self' https: wss:; " +
        "frame-src 'none'; " +
        "frame-ancestors 'none'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "upgrade-insecure-requests;");

    // Additional security headers
    context.Response.Headers.TryAdd("X-Download-Options", "noopen");
    context.Response.Headers.TryAdd("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.TryAdd("X-DNS-Prefetch-Control", "off");
    
    await next();
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SMS Service API v1");
        c.RoutePrefix = "api-docs"; // Change from default /swagger to /api-docs
    });
}

// Remove Swagger in production for security
if (app.Environment.IsProduction())
{
    // Don't expose Swagger in production
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot")),
    RequestPath = new PathString("/wwwroot")
});

app.UseRouting();

// CORS configuration must be placed before authentication and authorization middleware
app.UseCors();

// Add session and cookie policy middleware
app.UseSession();
app.UseCookiePolicy();

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<NotificationHub>("/notificationHub");
});

app.MapControllers();
app.Run();
