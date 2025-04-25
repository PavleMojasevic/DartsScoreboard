namespace DartsScoreboard;

public class User
{
    [System.ComponentModel.DataAnnotations.Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public List<UserStats> Stats { get; set; } = new ();
}

public class  UserStats
{
    public int ThreeDartAverage { get; set; }
    public int FirstNineAverage { get; set; }
    public int CheckoutPercentage { get; set; }
    public int CheckoutCount { get; set; }
    public int HighestFinish { get; set; }
    public int HighestScore { get; set; }
    public Dictionary<string, int> HighScoreHits { get; set; } = new()
    {
        { "180", 0 },
        { "160+", 0 },
        { "140+", 0 },
        { "120+", 0 },
        { "100+", 0 },
        { "80+", 0 },
        { "60+", 0 },
        { "40+", 0 }
    };
}
