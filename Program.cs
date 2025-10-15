using MongoDB.Driver;
using PRN232_Assignment1.Data;
using PRN232_Assignment1.IRepositories;
using PRN232_Assignment1.IServices;
using PRN232_Assignment1.Repositories;
using PRN232_Assignment1.Services;
using DotNetEnv;
using Supabase;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net.Http;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// MongoDB Configuration
var mongoDbSettings = builder.Configuration.GetSection("MongoDB");
var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? mongoDbSettings["ConnectionString"];
var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") ?? mongoDbSettings["DatabaseName"];

// Register Supabase client singleton for future auth/storage usage
var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY");
if (!string.IsNullOrWhiteSpace(supabaseUrl) && !string.IsNullOrWhiteSpace(supabaseKey))
{
    builder.Services.AddSingleton(provider =>
    {
        var opts = new Supabase.SupabaseOptions
        {
            AutoConnectRealtime = true
        };
        var client = new Client(supabaseUrl!, supabaseKey!, opts);
        client.InitializeAsync().GetAwaiter().GetResult();
        return client;
    });
}

builder.Services.AddSingleton<IMongoClient>(serviceProvider => 
    new MongoClient(connectionString));

builder.Services.AddSingleton<ProductContext>(serviceProvider =>
{
    var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
    return new ProductContext(mongoClient, databaseName);
});

var jwksUri = $"{supabaseUrl!.TrimEnd('/')}/auth/v1/.well-known/jwks.json";

// Fetch JWKS 1 lần khi app start
using var http = new HttpClient();
var jwksJson = await http.GetStringAsync(jwksUri);
var jwks = new JsonWebKeySet(jwksJson);


// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"{supabaseUrl.TrimEnd('/')}/auth/v1",

            ValidateAudience = true,
            ValidAudience = "authenticated",

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),

            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = jwks.Keys,
            ValidAlgorithms = new[] { SecurityAlgorithms.EcdsaSha256 } // Supabase use ES256 (ECDSA with SHA-256)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("Auth failed: " + ctx.Exception);
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Console.WriteLine("Token OK for user: " +
                                  ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier));
                return Task.CompletedTask;
            }
        };
    });

// Register repositories and services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<SupabaseStorageService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') 
                           ?? new[] { "http://localhost:3000" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PRN232 Assignment API",
        Version = "v1"
    });

    // Add Bearer token (JWT) support in Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
