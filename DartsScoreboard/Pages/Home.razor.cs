using Microsoft.AspNetCore.Components;

namespace DartsScoreboard;

public partial class Home
{
    [Inject] public IUserPersistence _UserPersistence { get; set; } = default!;
    [Inject] public NavigationManager NavManager { get; set; } = default!;
    private async Task NewGame()
    {
        NavManager.NavigateTo("/games");
    }
    private async Task OpenStats()
    {
    }
    private async Task TestAddUser()
    {
        string name = Guid.NewGuid().ToString();
        var user = new User
        {
            Name = name
        };
        await _UserPersistence.AddUser(user);
    }
    private void Settings()
    {
        NavManager.NavigateTo("/settings");
    }
}
