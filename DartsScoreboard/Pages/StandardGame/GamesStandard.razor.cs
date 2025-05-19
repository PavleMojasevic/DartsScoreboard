using DartsScoreboard.Models;
using DartsScoreboard.Services;
using Microsoft.AspNetCore.Components;


namespace DartsScoreboard
{
    public partial class GamesStandard
    {
        [Parameter] public string GameCode { get; set; } = "";

        [Inject] NavigationManager NavManager { get; set; } = default!;
        [Inject] IStandardGamePersistence _StandardGamePersistence { get; set; } = default!;
        [Inject] public GameSettingsService GameSettings { get; set; } = default!;
        [Inject] DbInitializerService DbInit { get; set; } = default!;
        [Inject] PlayerSelectionService PlayerService { get; set; } = default!;

        // Error popup
        public bool ErrorPopup { get; set; } = false;
        private string? CurrentErrorMessage;

        private bool IsFull => PlayerService.SelectedPlayers.Count >= 4;
        protected override async Task OnInitializedAsync()
        {
            await DbInit.EnsureStoresCreated();

            // now it is safe to call anything that uses db.Users.ToList() etc.
            await PlayerService.LoadAllUsersAsync();
        }
        private void ShowErrorMessage(string message)
        {
            CurrentErrorMessage = message;
        }

        private void ClearErrorMessage()
        {
            CurrentErrorMessage = null;
            ErrorPopup = false;
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
        private void GoToSavedGames()
        {
            NavManager.NavigateTo("/saved-games");
        }
        private async Task StartGame()
        {
            if (PlayerService.SelectedPlayers == null || PlayerService.SelectedPlayers.Count == 0)
            {
                ErrorPopup = true;
                ShowErrorMessage("Please select at least one player.");
                return;
            }

            string gameCode = Guid.NewGuid().ToString();

            GameSettings.SetGameOptions(SelectedScore, SelectedStartWith, SelectedEndWith, SelectedNumOfLegs, SelectedNumOfSets);

            var save = new StandardGame
            {
                Code = gameCode,
                Players = PlayerService.SelectedPlayers,
                StartingPoints = SelectedScore,
                NumOfLegs = SelectedNumOfLegs,
                NumOfSets = SelectedNumOfSets,
                StartingIn = SelectedStartWith,
                StartingOut = SelectedEndWith,
                PlayerScores = PlayerService.SelectedPlayers.ToDictionary(p => p.Id, p => new PlayerScoreInfo
                {
                    PlayerScore = SelectedScore,
                    PlayerThrows = 0,
                    PlayerThrowsLeg = 0,
                    PlayerLegs = 0,
                    PlayerSets = 0,
                    PlayerCollectedScore = 0
                }),
                PlayerStats = PlayerService.SelectedPlayers.Select(p => new User
                {
                    Id = p.Id,
                    Name = p.Name,
                    Stats = new UserStats
                    {
                        HighScoreHits = new Dictionary<string, int>
                        {
                            ["180"] = 0,
                            ["160+"] = 0,
                            ["140+"] = 0,
                            ["120+"] = 0,
                            ["100+"] = 0,
                            ["80+"] = 0,
                            ["60+"] = 0,
                            ["40+"] = 0
                        }
                    },
                    GameHistory = new List<OldGamesStats>
                    {
                        new OldGamesStats
                        {
                            GameCode = gameCode,
                            OldThreeDartAverage = 0,
                            OldCheckoutPercentage = 0,
                            OldTotalDartsThrown = 0
                        }
                    }

                }).ToList()
            };

            await _StandardGamePersistence.AddOrUpdate(save);

            NavManager.NavigateTo($"/gameStandardPlay/{gameCode}");
        }
    }
}
