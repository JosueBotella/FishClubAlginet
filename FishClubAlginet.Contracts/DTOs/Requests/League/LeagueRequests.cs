namespace FishClubAlginet.Contracts.Dtos.Requests.League;

public record CreateLeagueRequest(
    string Name,
    int Year,
    int MinPoints,
    int WorstResultsToDiscard);

public record UpdateLeagueRequest(
    string Name,
    int MinPoints,
    int WorstResultsToDiscard);
