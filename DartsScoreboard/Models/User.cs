namespace DartsScoreboard;

public class User
{
    [System.ComponentModel.DataAnnotations.Key]
    public int Id { get; set; }
    public string Name { get; set; }
}
public class CricketGame
{
    [System.ComponentModel.DataAnnotations.Key]
    public int Id { get; set; }
    public List<CricketPlayer> Players { get; set; }
}
public class CricketPlayer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CricketThrow> Throws { get; set; }
    public List<CricketNumberScore> Scores { get; set; }
    public int Points { get; set; }
}
public class CricketThrow
{
    public CricketDartThrow FirstDart { get; set; } = new();
    public CricketDartThrow SecondDart { get; set; } = new();
    public CricketDartThrow ThirdDart { get; set; } = new();
}
public class CricketDartThrow
{
    public bool IsMiss => Number == null;
    public int? Number { get; set; }
    public int? Count { get; set; }
}
public class CricketNumberScore
{
    public int Number { get; set; }
    public int Count { get; set; }
}