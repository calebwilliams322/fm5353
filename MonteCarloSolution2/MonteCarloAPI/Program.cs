
using Microsoft.EntityFrameworkCore;
using MonteCarloAPI.Configuration;
using MonteCarloAPI.Data;
using MonteCarloAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 1️⃣  Configure Services (Dependency Injection Container)
// ------------------------------------------------------------

// Add controllers (so endpoints like /api/options work)
builder.Services.AddControllers();

// Register PostgreSQL DbContext
builder.Services.AddDbContext<MonteCarloDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register OptionService
// - Scoped for database access (new DbContext per request)
builder.Services.AddScoped<OptionService>();

// Register PricingService
// - Singleton is fine since it's stateless
builder.Services.AddSingleton<PricingService>();

// Register PortfolioService
// - Scoped for database access (new DbContext per request)
builder.Services.AddScoped<PortfolioService>();

// Register StockService
// - Scoped for database access (new DbContext per request)
builder.Services.AddScoped<StockService>();

// Register ExchangeService
// - Scoped for database access (new DbContext per request)
builder.Services.AddScoped<ExchangeService>();

// Configure Alpaca API settings
builder.Services.Configure<AlpacaConfiguration>(
    builder.Configuration.GetSection("Alpaca"));

// Configure Rate Curve settings
builder.Services.Configure<RateCurveConfiguration>(
    builder.Configuration.GetSection("RateCurve"));

// Register RateCurveService
// - Singleton since it only reads from configuration
builder.Services.AddSingleton<RateCurveService>();

// Register AlpacaService
// - Scoped for API access
builder.Services.AddScoped<AlpacaService>();

// Register StockPriceUpdateService as a hosted background service
// - Runs in the background to periodically update stock prices
builder.Services.AddHostedService<StockPriceUpdateService>();

// Add API documentation via Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Optional: configure CORS policy (useful if you build a frontend later)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ------------------------------------------------------------
// 2️⃣  Build the Application
// ------------------------------------------------------------

var app = builder.Build();

// ------------------------------------------------------------
// 3️⃣  Configure the HTTP Request Pipeline
// ------------------------------------------------------------

// Enable serving static files from wwwroot folder
app.UseDefaultFiles();  // Makes index.html the default file
app.UseStaticFiles();   // Enables serving files from wwwroot

// Enable Swagger UI for testing and documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MonteCarloAPI v1");
        c.RoutePrefix = "swagger"; // Move Swagger to /swagger instead of root
    });
}

// Enforce HTTPS redirection for security
app.UseHttpsRedirection();

// Enable CORS policy
app.UseCors("AllowAll");

// Enable authorization middleware (kept for future JWT/auth expansion)
app.UseAuthorization();

// Map all controller endpoints (automatically detects routes like /api/options)
app.MapControllers();

// ------------------------------------------------------------
// 4️⃣  Run the App
// ------------------------------------------------------------

app.Run();