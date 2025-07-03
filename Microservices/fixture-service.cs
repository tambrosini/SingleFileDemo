#:sdk Microsoft.NET.Sdk.Web
#:package Microsoft.AspNetCore.OpenApi@10.*-*

var builder = WebApplication.CreateBuilder();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddOpenApi();
builder.Services.AddLogging();

var app = builder.Build();

app.MapGet("/fixture/{id:int}", (int id, ILogger<Program> logger) => 
{
    logger.LogWarning("Fixture request received for fixture ID: {FixtureId}", id);
    
    var fixture = new Fixture
    {
        Id = id,
        Venue = "Stadium",
        Date = DateTime.Now.AddDays(id)
    };
    
    logger.LogWarning("Fixture data generated successfully for fixture ID: {FixtureId}", id);
    return Results.Ok(fixture);
});

// Get URL from configuration
var baseUrl = app.Configuration["FixtureService:BaseUrl"];
app.Run(baseUrl);

public class Fixture
{
    public int Id { get; set; }
    public string Venue { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
}