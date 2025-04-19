using Microsoft.AspNetCore.Components;

namespace DartsScoreboard;

public partial class Home
{
    [Inject] public IUserPersistence _UserPersistence { get; set; } = default!;
    private async Task NewGame()
    {
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
}
