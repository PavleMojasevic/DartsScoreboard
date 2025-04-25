namespace DartsScoreboard;

public class StandardGame
{
    [System.ComponentModel.DataAnnotations.Key]
    public string Code { get; set; } = Guid.NewGuid().ToString();
    public List<User> Players { get; set; } = new();
    public int StartingPoints { get; set; }

    public StandardGame() { }

    public StandardGame(List<User> players, int startingPoints)
    {
        Players = players;
        StartingPoints = startingPoints;
    }
}
