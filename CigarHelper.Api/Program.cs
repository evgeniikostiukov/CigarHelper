using System.Text;
using System.Reflection;
using CigarHelper.Data.Data;
using CigarHelper.Api.Services;
using CigarHelper.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options => {
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
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
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Configure DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
                Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured")))
        };
    });

// Register Services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IHumidorService, HumidorService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://localhost:3000") // Конкретный origin для frontend
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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
app.UseCors("AllowSpecificOrigin");

// Conditional HTTPS redirection only if we're not using HTTP explicitly
if (!app.Environment.IsDevelopment() || !builder.Configuration.GetValue<bool>("UseHttp", false))
{
    app.UseHttpsRedirection();
}

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply database migrations
app.ApplyMigrations<AppDbContext>(app.Services.GetRequiredService<ILogger<Program>>());

app.Run();
