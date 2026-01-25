using Npgsql;
using Dapper;
using System.Data;
using System.Reflection;
using NpgsqlTypes;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using JewelryMS.Domain.Enums;
using JewelryMS.Domain.Interfaces;
using JewelryMS.Infrastructure.Repositories;
using Microsoft.OpenApi.Models; 
using Microsoft.AspNetCore.Diagnostics.HealthChecks; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization; // Required for Handlers
using JewelryMS.API.Authorization;     // Required for PermissionHandler/Provider


var builder = WebApplication.CreateBuilder(args);

// --- 1. DATABASE & ENUMS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<UserRole>("user_role");
dataSourceBuilder.MapEnum<MetalPurity>("metal_purity");
dataSourceBuilder.MapEnum<JewelryCategory>("jewelry_category");
dataSourceBuilder.MapEnum<MaterialType>("material_type");

var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);

// --- HEALTH CHECKS ---
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString!);

// --- 2. DAPPER SETUP ---
DefaultTypeMap.MatchNamesWithUnderscores = true;
SqlMapper.AddTypeHandler(typeof(MetalPurity), new UniversalEnumHandler<MetalPurity>());
SqlMapper.AddTypeHandler(typeof(JewelryCategory), new UniversalEnumHandler<JewelryCategory>());
SqlMapper.AddTypeHandler(typeof(MaterialType), new UniversalEnumHandler<MaterialType>());

builder.Services.AddControllers()
    .AddApplicationPart(Assembly.GetExecutingAssembly())
    .AddControllersAsServices();

// --- SWAGGER CONFIGURATION ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jewelry Management API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token: Bearer {your_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPublicProductRepository, PublicProductRepository>();
builder.Services.AddScoped<IMetalRateRepository, MetalRateRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();

// --- 3. ROLE PERMISSION LOGIC (INTEGRATED) ---
// These are the pieces that connect the [HasPermission] attribute to the DB
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

// --- 4. JWT AUTHENTICATION ---
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("JWT Key missing!");
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            
            // This ensures System.Security.Claims.ClaimTypes.Role maps correctly
            RoleClaimType = System.Security.Claims.ClaimTypes.Role, 
            ClockSkew = TimeSpan.Zero 
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// --- 5. MIDDLEWARE PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JewelryMS API v1"));
}
app.UseStaticFiles();

app.UseRouting();
app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// --- 6. ENUM HANDLER (Remains Same) ---
public class UniversalEnumHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override void SetValue(IDbDataParameter parameter, T value) => parameter.Value = value.ToString();
    public override T Parse(object value)
    {
        if (value == null || value is DBNull) return default;
        string dbValue = value.ToString()!;
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<PgNameAttribute>();
            if (attr != null && attr.PgName == dbValue) return (T)field.GetValue(null)!;
        }
        return Enum.TryParse<T>(dbValue, true, out var result) ? result : default;
    }
}