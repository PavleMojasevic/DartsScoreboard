using DartsScoreboard.Models;
using DartsScoreboard.Services;
using Microsoft.AspNetCore.Components;


namespace DartsScoreboard
{
    public partial class GamesStandard
    {
        // Player settings
        [Inject] DbInitializerService DbInit { get; set; } = default!;
        [Inject] PlayerSelectionService PlayerService { get; set; } = default!;
        [Inject] IUserPersistence UserPersistence { get; set; } = default!;
        [Inject] StandardGameUserService StandardGameUserService { get; set; } = default!;
        private bool IsFull => PlayerService.SelectedPlayers.Count >= 4;
        protected override async Task OnInitializedAsync()
        {
            await DbInit.EnsureStoresCreated();

            // now it is safe to call anything that uses db.Users.ToList() etc.
            await PlayerService.LoadAllUsersAsync();
        }

        private void OnUserSelected(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int id))
            {
                var user = PlayerService.AllUsers.FirstOrDefault(x => x.Id == id);
                if (user != null)
                {
                    PlayerService.AddExistingPlayer(user);
                }
            }
        }

        // Game settings
        public int SelectedScore { get; set; } = 501;
        public string SelectedStartWith { get; set; } = "STRAIGHT IN";
        public string SelectedEndWith { get; set; } = "DOUBLE OUT";
        public int SelectedNumOfLegs { get; set; } = 1;
        public int SelectedNumOfSets { get; set; } = 1;

        public List<int> StartingScoreOptions { get; set; } = new() { 301, 501, 701 };
        public List<string> StartingOptions { get; set; } = new() { "STRAIGHT IN", "DOUBLE IN", "MASTER IN" };
        public List<string> EndingOptions { get; set; } = new() { "DOUBLE OUT", "MASTER OUT", "STRAIGHT OUT" };

        private void SelectScore(int score)
        {
            SelectedScore = score;
        }
        private void SelectIn(string startWith)
        {
            SelectedStartWith = startWith;
        }
        private void SelectOut(string endWith)
        {
            SelectedEndWith = endWith;
        }

        [Inject] NavigationManager NavManager { get; set; } = default!;
        [Inject] public GameSettingsService GameSettings { get; set; } = default!;
        private void StartGame()
        {
            if (PlayerService.SelectedPlayers == null || PlayerService.SelectedPlayers.Count == 0)
            {
                // Optionally, show a message or toast here
                return;
            }

            GameSettings.SetGameOptions(SelectedScore, SelectedStartWith, SelectedEndWith, SelectedNumOfLegs, SelectedNumOfSets);
            StandardGameUserService.Players = PlayerService.SelectedPlayers;
            NavManager.NavigateTo("/gameStandardPlay");
        }
    }
}
