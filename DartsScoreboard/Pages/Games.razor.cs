using Microsoft.AspNetCore.Components;

namespace DartsScoreboard
{
    public partial class Games
    {
        [Inject] public NavigationManager navManager { get; set; } = default;
        private void Standard()
        {
            navManager.NavigateTo("/gamesStandard");
        }
    }
}
