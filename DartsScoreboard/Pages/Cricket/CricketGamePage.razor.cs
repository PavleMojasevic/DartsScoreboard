
using Microsoft.AspNetCore.Components;

namespace DartsScoreboard;

public partial class CricketGamePage
{
    [Inject] public ICricketPersistence _CricketPersistence { get; set; }
    [Inject] public IUserPersistence _UserPersistence { get; set; }
    [Inject] public NavigationManager _NavigationManager { get; set; }
    [Parameter] public string? gameCode { get; set; }
    public CricketGame Game { get; set; } = new();
    public List<CricketPlayerPresenter> Players { get; set; } = new();
    protected override async Task OnParametersSetAsync()
    {
        if (gameCode == null)
        {
            return;
        }
        var game = await _CricketPersistence.Get(gameCode);
        if (game == null)
        {
            _NavigationManager.NavigateTo($"/cricket-setup");
            return;
        }
        Game = game;
        if (IsEndOfGame())
        {
            await EndOfGame();
            return;
        }
        var users = await _UserPersistence.GetAllUsers();
        Players = Game.Players.Select(x => new CricketPlayerPresenter
        {
            Throws = x.Throws,
            Points = x.Points,
            Scores = x.Scores,
            UserId = x.UserId,
            Name = users.FirstOrDefault(u => u.Id == x.UserId)?.Name ?? x.GuestName ?? "Guest",
        }).ToList();

        ResolvePlayerOnTurn();
    }

    private void ResolvePlayerOnTurn()
    {
        return;
        throw new NotImplementedException();
    }

    private async Task EndOfGame()
    {
        return;
        throw new NotImplementedException();
    }

    private bool IsEndOfGame()
    {
        return false;
        throw new NotImplementedException();
    }
}
