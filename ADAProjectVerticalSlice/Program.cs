using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Extensions;
using ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment;
using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Infrastructure.Messaging;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;


// Logging konfiguration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error) // Ignorer Microsoft-logs, medmindre de er fejl
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning) // Ignorer EF Core SQL-kommandoer
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog();

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
        Description = "JWT-token til autentificering."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Database")));


builder.Services
    .AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();

var assembly = typeof(Program).Assembly;


builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddScoped<EmailTimingCalculator>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<ITicketStore, PostgresTicketStore>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));

    options.AddPolicy("AllRoles", policy =>
        policy.RequireRole("Admin", "User", "Viewer"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevelopment", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5026",
                "https://localhost:7237"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Vi registrerer både JWT og Cookie Authentication.
builder.Services
    .AddAuthentication(options =>
    {
        // Policy Scheme bliver vores fælles indgang til authentication.
        options.DefaultScheme = "Smart";
        options.DefaultAuthenticateScheme = "Smart";
        options.DefaultChallengeScheme = "Smart";
        options.DefaultForbidScheme = "Smart";
    })

    // Policy Scheme vælger authentication-mekanisme ud fra requesten.
    .AddPolicyScheme("Smart", "JWT eller Cookie", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authorization = context.Request.Headers.Authorization.ToString();

            // Hvis requesten indeholder et Bearer-token,
            // bruger vi JWT Authentication.
            if (authorization.StartsWith(
                "Bearer ",
                StringComparison.OrdinalIgnoreCase))
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }

            // Ellers forsøger vi Cookie Authentication.
            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    })

    // Eksisterende JWT-konfiguration.
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Convert.FromBase64String(
                        builder.Configuration["Jwt:SigningKey"]!))
            };
    })

    // Ny Cookie Authentication-konfiguration.
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "ADA.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // API'et skal returnere 401 i stedet for at
        // omdirigere brugeren til en login-side.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        };

        // API'et skal returnere 403 ved manglende rettigheder.
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return Task.CompletedTask;
        };
    });

// Forbinder Cookie Authentication med vores ticket store.
builder.Services
    .AddOptions<CookieAuthenticationOptions>(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<ITicketStore>((options, ticketStore) =>
    {
        options.SessionStore = ticketStore;
    });


// CSRF-beskyttelse for Cookie Authentication.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";

    options.Cookie.Name = "ADA.AntiForgery";
    options.Cookie.HttpOnly = true; // Forhindrer JavaScript i at tilgå cookien.
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Sørger for, at cookien kun sendes over HTTPS.
    options.Cookie.SameSite = SameSiteMode.None; // Tillader cross-site requests, når browserens øvrige sikkerhedsindstillinger tillader det. Dette er fordi frontend og backend kan ligge på forskellige origins. 
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine(
        $"INCOMING REQUEST: {context.Request.Method} {context.Request.Path}");

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
}


using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowDevelopment");

app.UseAuthentication();

// CSRF-beskyttelse af cookie-autentificerede requests.
app.Use(async (context, next) =>
{
    var method = context.Request.Method;

    // GET, HEAD og OPTIONS ændrer ikke data og skal ikke CSRF-valideres.
    var isSafeMethod =
        HttpMethods.IsGet(method) ||
        HttpMethods.IsHead(method) ||
        HttpMethods.IsOptions(method) ||
        HttpMethods.IsTrace(method);

    // Android bruger JWT i Authorization-headeren.
    var hasBearerToken = context.Request.Headers.Authorization
        .ToString()
        .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

    // CSRF-valider kun autentificerede requests, der ikke bruger JWT.
    var isWebLoginEndpoint =
     context.Request.Path.Equals(
         "/api/auth/web-login",
         StringComparison.OrdinalIgnoreCase);

    var isAuthenticated =
        context.User.Identity?.IsAuthenticated == true;

    if (!isSafeMethod &&
        !hasBearerToken &&
        (isAuthenticated || isWebLoginEndpoint))
    {
        var antiforgery = context.RequestServices
            .GetRequiredService<IAntiforgery>();

        var isValid = await antiforgery.IsRequestValidAsync(context);

        if (!isValid)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid or missing anti-forgery token."
            });

            return;
        }
    }

    await next();
});

app.UseAuthorization();

app.MapCarter();

app.Run();
