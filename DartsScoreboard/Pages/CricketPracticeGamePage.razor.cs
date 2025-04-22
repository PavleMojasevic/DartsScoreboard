using Microsoft.AspNetCore.Components;

namespace DartsScoreboard;

public partial class CricketPracticeGamePage
{
    [Inject] public ICricketPracticeGamePersistence _CricketPracticeGamePersistence { get; set; }
    [Inject] public IUserPersistence _UserPersistence { get; set; }
    [Parameter] public string? code { get; set; }
    public CricketPracticeGame CricketPracticeGame { get; set; } = new();
    public bool IsNew { get; set; }
    public string CurrentTarget { get; set; } = "";
    public List<CricketPracticePlayerPresenter> Players { get; set; } = new();
    public int PlayerOnTurnIndex { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        if (code == null)
        {
            IsNew = true;
            return;
        }
        var game = await _CricketPracticeGamePersistence.Get(code);
        if (game == null)
        {
            //TODO: handle game not found
            return;
        }
        CricketPracticeGame = game;
        int throwCount = CricketPracticeGame.Players.Min(x => x.Throws.Count);
        if (CricketPracticeGame.Targets.Count == throwCount)
        {
            //TODO: handle end of game
            return;
        }
        CurrentTarget = CricketPracticeGame.Targets[throwCount];
        var users = await _UserPersistence.GetAllUsers();
        Players = CricketPracticeGame.Players.Select(x => new CricketPracticePlayerPresenter
        {
            Throws = x.Throws,
            UserId = x.UserId,
            UserName = users.FirstOrDefault(u => u.Id == x.UserId)?.Name ?? "Unknown",
        }).ToList();

        ResolvePlayerOnTurn(throwCount);
    }

    private void ResolvePlayerOnTurn(int throwCount)
    {
        PlayerOnTurnIndex = -1;
        for (int i = 0; i < CricketPracticeGame.Players.Count; i++)
        {
            if (CricketPracticeGame.Players[i].Throws.Count < throwCount)
            {
                PlayerOnTurnIndex = i;
                break;
            }
        }
        if (PlayerOnTurnIndex == -1)
        {
            PlayerOnTurnIndex = 0;
        }
    }
}