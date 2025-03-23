using Microsoft.EntityFrameworkCore;
using Serilog;
using VkPostsAnalyzer.Data;
using VkPostsAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, config) =>
{
    config.WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
          .WriteTo.Console();
});

Log.Information("Starting application configuration...");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<VkService>();
builder.Services.AddScoped<VkService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Log.Error("Connection string 'DefaultConnection' is missing or empty.");
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

Log.Information("Application configuration completed.");

var app = builder.Build();

Log.Information("Applying database migrations...");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VK Posts Analyzer API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var swaggerUrl = "http://localhost:5144/swagger";
Log.Information("Application is starting...");
Log.Information($"Swagger UI is available at: {swaggerUrl}");

app.Run();

Log.Information("Application has stopped.");