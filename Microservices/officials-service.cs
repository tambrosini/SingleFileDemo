#!/usr/bin/dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:package Microsoft.AspNetCore.OpenApi@10.*-*

var builder = WebApplication.CreateBuilder();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddOpenApi();
builder.Services.AddLogging();

var app = builder.Build();

app.MapGet("/officials/{matchId:int}", (int matchId, ILogger<Program> logger) => 
{
    logger.LogWarning("Officials request received for matchId: {MatchId}", matchId);
    
    var officials = new Officials
    {
        MatchId = matchId,
        Referee = OfficialsData.RefereeNames[matchId % OfficialsData.RefereeNames.Length],
        AssistantReferee1 = OfficialsData.Assistant1Names[matchId % OfficialsData.Assistant1Names.Length],
        AssistantReferee2 = OfficialsData.Assistant2Names[matchId % OfficialsData.Assistant2Names.Length],
        FourthOfficial = OfficialsData.FourthOfficialNames[matchId % OfficialsData.FourthOfficialNames.Length],
        AssignmentDate = DateTime.Now.AddDays(-7) // Assigned a week ago
    };
    
    logger.LogWarning("Officials data generated successfully for matchId: {MatchId}", matchId);
    return Results.Ok(officials);
});

// Get URL from configuration
var baseUrl = app.Configuration["RefereeService:BaseUrl"];
app.Run(baseUrl);

public class Officials
{
    public int MatchId { get; set; }
    public string Referee { get; set; } = string.Empty;
    public string AssistantReferee1 { get; set; } = string.Empty;
    public string AssistantReferee2 { get; set; } = string.Empty;
    public string FourthOfficial { get; set; } = string.Empty;
    public DateTime AssignmentDate { get; set; } = DateTime.Now;
}

public static class OfficialsData
{
    // Simple rotation of officials based on match ID for demo purposes
    public static readonly string[] RefereeNames = { "John Smith", "Sarah Johnson", "Michael Brown", "Emma Wilson", "David Garcia" };
    public static readonly string[] Assistant1Names = { "James Davis", "Lisa Rodriguez", "Robert Miller", "Jennifer Taylor", "Christopher Anderson" };
    public static readonly string[] Assistant2Names = { "Maria Martinez", "Thomas Wilson", "Ashley Moore", "Daniel Jackson", "Jessica White" };
    public static readonly string[] FourthOfficialNames = { "Kevin Lee", "Amanda Harris", "Steven Clark", "Rachel Lewis", "Mark Thompson" };
}
