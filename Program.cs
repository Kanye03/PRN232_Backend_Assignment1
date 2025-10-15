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

// Add Authentication
var supabaseProjectRef = Environment.GetEnvironmentVariable("SUPABASE_PROJECT_REF");
if (!string.IsNullOrWhiteSpace(supabaseProjectRef))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{supabaseProjectRef}.supabase.co/auth/v1";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://{supabaseProjectRef}.supabase.co/auth/v1",
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
            options.MetadataAddress = $"https://{supabaseProjectRef}.supabase.co/auth/v1/.well-known/openid-configuration";
        });
}

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
builder.Services.AddSwaggerGen();

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
