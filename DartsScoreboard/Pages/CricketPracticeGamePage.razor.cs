using Microsoft.AspNetCore.Components;

namespace DartsScoreboard;

public partial class CricketPracticeGamePage
{
    [Inject] public ICricketPracticeGamePersistence _CricketPracticeGamePersistence { get; set; }
    [Parameter] public string? code { get; set; }
    public CricketPracticeGame CricketPracticeGame { get; set; } = new();
    public bool IsNew { get; set; }
    public string CurrentTarget { get; set; } = "";
    public CricketPracticeGamePlayer PlayerOnTurn { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        if (code == null)
        {
            IsNew = true;
            return;
        }
        var game = await _CricketPracticeGamePersistence.Get(code);
        IsNew = game == null;
        CricketPracticeGame = game ?? new();
        if (IsNew)
        {
            CurrentTarget = CricketPracticeGame.Targets[0];
            PlayerOnTurn = CricketPracticeGame.Players[0];
        }
        else
        {
            int throwCount = CricketPracticeGame.Players.Min(x => x.Throws.Count);
            if (CricketPracticeGame.Targets.Count == throwCount)
            {
                //TODO: handle end of game
                return;
            }
            CurrentTarget = CricketPracticeGame.Targets[throwCount];
            PlayerOnTurn = CricketPracticeGame.Players.FirstOrDefault(x => x.Throws.Count < throwCount) ??
                           CricketPracticeGame.Players[0];
        }
    }

}