using DartsScoreboard.Models;
using Microsoft.AspNetCore.Components;


namespace DartsScoreboard
{
    public partial class GamesStandard
    {
        // Player settings
        [Inject] DbInitializerService DbInit { get; set; } = default!;
        [Inject] PlayerSelectionService PlayerService { get; set; } = default!;
        [Inject] IUserPersistence UserPersistence { get; set; } = default!;
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
        public string GameOptionsOutput { get; set; } = "211";

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

        private void GameOptionsOutputFunction()
        {
            char[] output = new char[3];
            for (int i = 0; i < StartingScoreOptions.Count; i++)
            {
                if (StartingScoreOptions[i] == SelectedScore)
                {
                    output[0] = (char)('0' + i + 1);
                }
            }
            for (int i = 0; i < StartingOptions.Count; i++)
            {
                if (StartingOptions[i] == SelectedStartWith)
                {
                    output[1] = (char)('0' + i + 1);
                }
            }
            for (int i = 0; i < EndingOptions.Count; i++)
            {
                if (EndingOptions[i] == SelectedEndWith)
                {
                    output[2] = (char)('0' + i + 1);
                }
            }

            GameOptionsOutput = new string(output);
        }

        [Inject] NavigationManager NavManager { get; set; } = default!;
        private void StartGame()
        {
            GameOptionsOutputFunction();
            NavManager.NavigateTo($"/gameStandardPlay/{Uri.EscapeDataString(GameOptionsOutput)}");
        }
    }
}
