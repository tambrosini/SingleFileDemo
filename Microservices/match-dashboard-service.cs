#:sdk Microsoft.NET.Sdk.Web
#:package Microsoft.AspNetCore.OpenApi@10.*-*

var builder = WebApplication.CreateBuilder();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddLogging();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable CORS
app.UseCors();

app.MapGet("/matches/{matchId:int}", async (int matchId, HttpClient httpClient, IConfiguration configuration, ILogger<Program> logger) => 
{
    try
    {
        logger.LogWarning("Match dashboard request initiated for matchId: {MatchId}", matchId);
        
        // Get fixture information from the Fixture Service
        var fixture = await GetFixtureAsync(matchId, httpClient, configuration, logger);
        
        // Get team sheet information from the Teamsheet Service
        var teamSheet = await GetTeamSheetAsync(matchId, httpClient, configuration, logger);

        // Create complete match dashboard data
        var matchDashboard = new MatchDashboard
        {
            MatchId = matchId,
            Fixture = fixture,
            TeamSheet = teamSheet,
            MatchStatus = GetMatchStatus(matchId),
            LastUpdated = DateTime.Now
        };
        
        logger.LogWarning("Match dashboard request completed successfully for matchId: {MatchId}", matchId);
        return Results.Ok(matchDashboard);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving match dashboard for matchId: {MatchId}", matchId);
        return Results.Problem($"Error retrieving match dashboard: {ex.Message}");
    }
});

// Get URL from configuration
var baseUrl = app.Configuration["MatchService:BaseUrl"];
app.Run(baseUrl);

static async Task<Fixture?> GetFixtureAsync(int matchId, HttpClient httpClient, IConfiguration configuration, ILogger logger)
{
    var fixtureBaseUrl = configuration["FixtureService:BaseUrl"];
    var url = $"{fixtureBaseUrl}/fixture/{matchId}";
    
    logger.LogWarning("Making API call to Fixture Service: {Url}", url);
    var fixtureResponse = await httpClient.GetAsync(url);
    
    Fixture? fixture = null;
    if (fixtureResponse.IsSuccessStatusCode)
    {
        logger.LogWarning("Fixture Service call successful for matchId: {MatchId}", matchId);
        var fixtureJson = await fixtureResponse.Content.ReadAsStringAsync();
        fixture = System.Text.Json.JsonSerializer.Deserialize<Fixture>(fixtureJson, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    else
    {
        logger.LogWarning("Fixture Service call failed for matchId: {MatchId}, StatusCode: {StatusCode}", matchId, fixtureResponse.StatusCode);
    }
    
    return fixture;
}

static async Task<TeamSheet?> GetTeamSheetAsync(int matchId, HttpClient httpClient, IConfiguration configuration, ILogger logger)
{
    var teamsheetBaseUrl = configuration["TeemsheetService:BaseUrl"];
    var url = $"{teamsheetBaseUrl}/teamsheets/{matchId}";
    
    logger.LogWarning("Making API call to Teamsheet Service: {Url}", url);
    var teamsheetResponse = await httpClient.GetAsync(url);
    
    TeamSheet? teamSheet = null;
    if (teamsheetResponse.IsSuccessStatusCode)
    {
        logger.LogWarning("Teamsheet Service call successful for matchId: {MatchId}", matchId);
        var teamsheetJson = await teamsheetResponse.Content.ReadAsStringAsync();
        teamSheet = System.Text.Json.JsonSerializer.Deserialize<TeamSheet>(teamsheetJson, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    else
    {
        logger.LogWarning("Teamsheet Service call failed for matchId: {MatchId}, StatusCode: {StatusCode}", matchId, teamsheetResponse.StatusCode);
    }
    
    return teamSheet;
}

static string GetMatchStatus(int matchId)
{
    // Simple logic to determine match status based on match ID
    var statuses = new[] { "Scheduled", "Pre-Match", "First Half", "Half Time", "Second Half", "Full Time", "Postponed" };
    return statuses[matchId % statuses.Length];
}

public class MatchDashboard
{
    public int MatchId { get; set; }
    public Fixture? Fixture { get; set; }
    public TeamSheet? TeamSheet { get; set; }
    public string MatchStatus { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class Fixture
{
    public int Id { get; set; }
    public string Venue { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
}

public class TeamSheet
{
    public int MatchId { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public List<Player> HomeStartingLineup { get; set; } = new();
    public List<Player> AwayStartingLineup { get; set; } = new();
    public List<Player> HomeSubstitutes { get; set; } = new();
    public List<Player> AwaySubstitutes { get; set; } = new();
    public Officials? Officials { get; set; }
    public DateTime MatchDate { get; set; }
}

public class Player
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class Officials
{
    public int MatchId { get; set; }
    public string Referee { get; set; } = string.Empty;
    public string AssistantReferee1 { get; set; } = string.Empty;
    public string AssistantReferee2 { get; set; } = string.Empty;
    public string FourthOfficial { get; set; } = string.Empty;
    public DateTime AssignmentDate { get; set; }
}
