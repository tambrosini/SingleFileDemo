#!/usr/bin/dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:package Microsoft.AspNetCore.OpenApi@10.*-*

var builder = WebApplication.CreateBuilder();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddLogging();

var app = builder.Build();

app.MapGet("/teamsheets/{matchId:int}", async (int matchId, HttpClient httpClient, IConfiguration configuration, ILogger<Program> logger) => 
{
    try
    {
        logger.LogWarning("Teamsheet request initiated for matchId: {MatchId}", matchId);
        
        // Get officials information from the Officials Service
        var officials = await GetOfficialsAsync(matchId, httpClient, configuration, logger);

        // Create team sheet data
        var teamSheet = new TeamSheet
        {
            MatchId = matchId,
            HomeTeam = TeamSheetData.Teams[matchId % TeamSheetData.Teams.Length],
            AwayTeam = TeamSheetData.Teams[(matchId + 1) % TeamSheetData.Teams.Length],
            HomeStartingLineup = GetStartingLineup(matchId, true),
            AwayStartingLineup = GetStartingLineup(matchId, false),
            HomeSubstitutes = GetSubstitutes(matchId, true),
            AwaySubstitutes = GetSubstitutes(matchId, false),
            Officials = officials,
            MatchDate = DateTime.Now.AddDays(matchId % 30) // Spread matches across 30 days
        };
        
        logger.LogWarning("Teamsheet request completed successfully for matchId: {MatchId}", matchId);
        return Results.Ok(teamSheet);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving team sheet for matchId: {MatchId}", matchId);
        return Results.Problem($"Error retrieving team sheet: {ex.Message}");
    }
});

// Get URL from configuration
var baseUrl = app.Configuration["TeemsheetService:BaseUrl"];
app.Run(baseUrl);

static async Task<Officials?> GetOfficialsAsync(int matchId, HttpClient httpClient, IConfiguration configuration, ILogger logger)
{
    var officialsBaseUrl = configuration["RefereeService:BaseUrl"];
    var url = $"{officialsBaseUrl}/officials/{matchId}";
    
    logger.LogWarning("Making API call to Officials Service: {Url}", url);
    var officialsResponse = await httpClient.GetAsync(url);
    
    Officials? officials = null;
    if (officialsResponse.IsSuccessStatusCode)
    {
        logger.LogWarning("Officials Service call successful for matchId: {MatchId}", matchId);
        var officialsJson = await officialsResponse.Content.ReadAsStringAsync();
        officials = System.Text.Json.JsonSerializer.Deserialize<Officials>(officialsJson, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    else
    {
        logger.LogWarning("Officials Service call failed for matchId: {MatchId}, StatusCode: {StatusCode}", matchId, officialsResponse.StatusCode);
    }
    
    return officials;
}

static List<Player> GetStartingLineup(int matchId, bool isHome)
{
    var seed = matchId * (isHome ? 1 : 2);
    var random = new Random(seed);
    var allPlayers = isHome ? TeamSheetData.HomePlayers : TeamSheetData.AwayPlayers;

    return allPlayers.OrderBy(x => random.Next()).Take(11).ToList();
}

static List<Player> GetSubstitutes(int matchId, bool isHome)
{
    var seed = matchId * (isHome ? 3 : 4);
    var random = new Random(seed);
    var allPlayers = isHome ? TeamSheetData.HomePlayers : TeamSheetData.AwayPlayers;
    var startingLineup = GetStartingLineup(matchId, isHome);
    
    return allPlayers.Except(startingLineup).OrderBy(x => random.Next()).Take(7).ToList();
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

public static class TeamSheetData
{
    public static readonly string[] Teams = { 
        "Manchester United", "Liverpool", "Arsenal", "Chelsea", "Manchester City",
        "Tottenham", "Newcastle", "Brighton", "Aston Villa", "West Ham"
    };

    public static readonly List<Player> HomePlayers = new()
    {
        new Player { Number = 1, Name = "David Henderson", Position = "GK", Age = 28 },
        new Player { Number = 2, Name = "Kyle Walker", Position = "RB", Age = 31 },
        new Player { Number = 3, Name = "Andrew Robertson", Position = "LB", Age = 29 },
        new Player { Number = 4, Name = "Virgil van Dijk", Position = "CB", Age = 32 },
        new Player { Number = 5, Name = "John Stones", Position = "CB", Age = 29 },
        new Player { Number = 6, Name = "Declan Rice", Position = "CDM", Age = 25 },
        new Player { Number = 7, Name = "Kevin De Bruyne", Position = "CM", Age = 32 },
        new Player { Number = 8, Name = "Jordan Henderson", Position = "CM", Age = 33 },
        new Player { Number = 9, Name = "Harry Kane", Position = "ST", Age = 30 },
        new Player { Number = 10, Name = "Raheem Sterling", Position = "RW", Age = 29 },
        new Player { Number = 11, Name = "Marcus Rashford", Position = "LW", Age = 27 },
        new Player { Number = 12, Name = "Nick Pope", Position = "GK", Age = 31 },
        new Player { Number = 13, Name = "Conor Gallagher", Position = "CM", Age = 24 },
        new Player { Number = 14, Name = "Kalvin Phillips", Position = "CDM", Age = 28 },
        new Player { Number = 15, Name = "Eric Dier", Position = "CB", Age = 30 },
        new Player { Number = 16, Name = "Trent Alexander-Arnold", Position = "RB", Age = 25 },
        new Player { Number = 17, Name = "Jack Grealish", Position = "LW", Age = 28 },
        new Player { Number = 18, Name = "Gareth Southgate", Position = "CB", Age = 53 }
    };

    public static readonly List<Player> AwayPlayers = new()
    {
        new Player { Number = 1, Name = "Jordan Pickford", Position = "GK", Age = 30 },
        new Player { Number = 2, Name = "Reece James", Position = "RB", Age = 24 },
        new Player { Number = 3, Name = "Luke Shaw", Position = "LB", Age = 28 },
        new Player { Number = 4, Name = "Harry Maguire", Position = "CB", Age = 31 },
        new Player { Number = 5, Name = "Marc Guehi", Position = "CB", Age = 24 },
        new Player { Number = 6, Name = "Kobbie Mainoo", Position = "CDM", Age = 19 },
        new Player { Number = 7, Name = "Bukayo Saka", Position = "RW", Age = 23 },
        new Player { Number = 8, Name = "Jude Bellingham", Position = "CM", Age = 21 },
        new Player { Number = 9, Name = "Ollie Watkins", Position = "ST", Age = 28 },
        new Player { Number = 10, Name = "Phil Foden", Position = "AM", Age = 24 },
        new Player { Number = 11, Name = "Anthony Gordon", Position = "LW", Age = 23 },
        new Player { Number = 12, Name = "Aaron Ramsdale", Position = "GK", Age = 26 },
        new Player { Number = 13, Name = "Ezri Konsa", Position = "CB", Age = 27 },
        new Player { Number = 14, Name = "Adam Wharton", Position = "CM", Age = 20 },
        new Player { Number = 15, Name = "Lewis Dunk", Position = "CB", Age = 33 },
        new Player { Number = 16, Name = "Jarrad Branthwaite", Position = "CB", Age = 22 },
        new Player { Number = 17, Name = "Ivan Toney", Position = "ST", Age = 28 },
        new Player { Number = 18, Name = "Jarrod Bowen", Position = "RW", Age = 27 }
    };
}
