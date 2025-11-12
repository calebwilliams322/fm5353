
// using Microsoft.AspNetCore.Builder;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
using MonteCarloAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 1️⃣  Configure Services (Dependency Injection Container)
// ------------------------------------------------------------

// Add controllers (so endpoints like /api/options work)
builder.Services.AddControllers();

// Register OptionService
// - Singleton for in-memory storage (same data across all requests)
// - Later: change to Scoped when using EF Core/Postgres
builder.Services.AddSingleton<OptionService>();

// Register PricingService
// - Singleton is fine since it's stateless
builder.Services.AddSingleton<PricingService>();

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

// Enable Swagger UI for testing and documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MonteCarloAPI v1");
        c.RoutePrefix = string.Empty; // So you can hit Swagger at the root URL
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