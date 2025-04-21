namespace DartsScoreboard;

public class CricketPracticeGame
{
    [System.ComponentModel.DataAnnotations.Key]
    public string Code { get; set; }
    public List<CricketPracticeGamePlayer> Players { get; set; }
    public List<string> Targets { get; set; }
    public CricketPracticeGame()
    {
        Code = Guid.NewGuid().ToString();
        Targets = new() { "20", "19", "18", "17", "16", "15", "BULL" };
    }
}
public class CricketPracticeGamePlayer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CricketPracticeGamePlayerThrow> Throws { get; set; } = new();
    public int Points => Throws.Sum(x => x.Total);
}
public class CricketPracticeGamePlayerThrow
{
    public int Number { get; set; }
    public int FirstDart { get; set; }
    public int SecondDart { get; set; }
    public int ThirdDart { get; set; }
    public int Bonus => CalculateBonus();
    public int Total => FirstDart + SecondDart + ThirdDart + Bonus;
    public int CalculateBonus()
    {
        int bonus = 0;
        if (FirstDart == 0 && SecondDart == 0 && ThirdDart == 0)
            return -1;
        if (FirstDart * SecondDart * ThirdDart > 0)
            bonus++;

        int total = FirstDart + SecondDart + ThirdDart;
        if (total >= 3)
            bonus++;
        return bonus;
    }
}