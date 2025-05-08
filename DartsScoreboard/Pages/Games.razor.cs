using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace DartsScoreboard
{
    public partial class Games
    {
        [Inject] public NavigationManager navManager { get; set; } = default!;
        [Inject] public ICricketPracticeGamePersistence _CricketPracticeGamePersistence { get; set; }
        [Inject] public IUserPersistence _UserPersistence { get; set; } = default!;
        private void Standard()
        {
            navManager.NavigateTo("/gamesStandard");
        }
        private async Task Back()
        {
            navManager.NavigateTo("/");
        }
        private async Task CricketPractice()
        {
            navManager.NavigateTo("/cricket-practice-setup");
        }
    }
}
