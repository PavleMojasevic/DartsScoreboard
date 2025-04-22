using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace DartsScoreboard
{
    public partial class Games
    {
        [Inject] public NavigationManager navManager { get; set; } = default!;
        [Inject] public ICricketPracticeGamePersistence _CricketPracticeGamePersistence { get; set; } = default!;
        [Inject] public IUserPersistence _UserPersistence { get; set; } = default!;
        private void Standard()
        {
            navManager.NavigateTo("/gamesStandard");
        }
        private async Task CricketPractice()
        {
            var user = await _UserPersistence.GetUser(1);
            if (user == null)
            {
                user = new User { Id = 1, Name = "Test User" };
                await _UserPersistence.AddUser(user);
            }
            string code = Guid.NewGuid().ToString();
            await _CricketPracticeGamePersistence.Add(
                 new CricketPracticeGame
                 {
                     Code = code,
                     Players = new List<CricketPracticeGamePlayer>
                     {
                        new CricketPracticeGamePlayer
                        {
                            Id = 1,
                            UserId = 1,
                            Throws=new()
                        }
                     }
                 });
            navManager.NavigateTo("/cricket-practice/" + code);
        }
    }
}
