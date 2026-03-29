using System.Security.Claims;
using System.Text;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CigarHelper.Data.Data;
using CigarHelper.Api.Services;
using CigarHelper.Api.Extensions;
using CigarHelper.Api.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Microsoft.OpenApi;

static string GetRateLimitPartitionKey(HttpContext httpContext)
{
    var ip = httpContext.Connection.RemoteIpAddress?.ToString();
    return string.IsNullOrEmpty(ip) ? "unknown" : ip;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options => {
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
})
.ConfigureApiBehaviorOptions(options => {
    options.SuppressModelStateInvalidFilter = true; // Отключаем автоматическую валидацию модели
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Cigar Helper API", 
        Version = "v1",
        Description = "API для управления хьюмидорами и сигарами",
        Contact = new OpenApiContact
        {
            Name = "Cigar Helper Team"
        }
    });
    
    // Включаем XML-документацию
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    
    // Определяем JWT Bearer авторизацию
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer"
    });
    
    options.AddSecurityRequirement(document => new() { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });

    // options.AddSecurityRequirement(document =>

        // new OpenApiSecurityRequirement()
    // {
    //     [new OpenApiSecuritySchemeReference("oauth2", document)] = []
    // }
    // );
    //     new OpenApiSecurityRequirement
    // {
    //     {
    //         new OpenApiSecurityScheme
    //         {
    //             Reference = new OpenApiSecuritySchemeReference("Bearer"),
    //             // new OpenApiReference
    //             // {
    //             //     Type = ReferenceType.SecurityScheme,
    //             //     Id = "Bearer"
    //             // },
    //             Scheme = "oauth2",
    //             Name = "Bearer",
    //             In = ParameterLocation.Header
    //         },
    //         new List<string>()
    //     }
    // });
});

// Configure DbContext: один провайдер на процесс (интеграционные тесты — InMemory)
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase($"Integration_{Guid.NewGuid():N}"));
}
else
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
    var csb = new NpgsqlConnectionStringBuilder(conn)
    {
        // Подробности ошибок БД — только в Development (меньше утечек в prod).
        IncludeErrorDetail = builder.Environment.IsDevelopment(),
    };
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(csb.ConnectionString));
}

// Configure JWT Authentication
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
                Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured"))),
            // После чтения JWT inbound-маппинг превращает claim "role" в ClaimTypes.Role; иначе [Authorize(Roles)] не видит роль → 403
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth-login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetRateLimitPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("auth-register", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetRateLimitPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddMemoryCache();

builder.Services.Configure<ImageUploadOptions>(builder.Configuration.GetSection(ImageUploadOptions.SectionName));

// Register Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IHumidorService, HumidorService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:3000"];
if (corsOrigins.Length == 0)
    corsOrigins = ["http://localhost:3000"];

var corsPolicyName = builder.Configuration["Cors:PolicyName"] ?? "DefaultCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName,
        policy => policy
            .WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddHsts(options =>
{
    options.Preload = false;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.Services.AddCigarForwardedHeaders(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
// Первым: иначе схема/Host/клиентский IP из запроса к Kestrel без учёта X-Forwarded-*.
app.UseForwardedHeaders();

app.UseSecurityHeaders();

if (app.Environment.IsProduction())
    app.UseHsts();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cigar Helper API v1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1); // Скрываем секцию схем по умолчанию
    });
}

// Use CORS middleware before routing and endpoint execution
app.UseCors(corsPolicyName);

// Conditional HTTPS redirection only if we're not using HTTP explicitly
if (!app.Environment.IsDevelopment() || !builder.Configuration.GetValue("UseHttp", false))
{
    app.UseHttpsRedirection();
}

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

// В интеграционных тестах БД — InMemory, миграции не применяем
if (!app.Environment.IsEnvironment("Testing"))
    app.ApplyMigrations<AppDbContext>(app.Services.GetRequiredService<ILogger<Program>>());

app.Run();
