using Microsoft.AspNetCore.Components;

namespace DartsScoreboard
{
    public partial class GameStandardPlay
    {
        [Inject] public PlayerSelectionService PlayerService { get; set; } = default!;
        // [Inject] public IStandardGamePersistence _StandardGamePersistence { get; set; }
        [Inject] public IUserPersistence _UserPersistence { get; set; }
        public List<User> Players { get; set; } = new();
        public Dictionary<int, int> PlayerScores { get; set; } = new(); // <user.Id, PlayerScore>
        public int CurrentPlayerIndex { get; set; } = 0;
        public string InputScore { get; set; } = "";
        public bool WinnerPopup { get; set; } = false;

        public int StartingScore = 501;

        protected override void OnInitialized()
        {
            Players = PlayerService.SelectedPlayers;

            foreach (var player in Players)
            {
                PlayerScores[player.Id] = StartingScore;
            }
        }
        private void HandleKey(KeyboardKey key)
        {
            InputScore += key.Value;
        }
        private void SubmitScore()
        {
            if (int.TryParse(InputScore, out int score))
            {
                var currentPlayer = Players[CurrentPlayerIndex];
                PlayerScores[currentPlayer.Id] -= score;

                if (PlayerScores[currentPlayer.Id] <= 0)
                {
                    WinnerPopup = true;
                    return;
                }
                else
                {
                    CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
                }
            }
            InputScore = "";
        }
        private void ResetGame()
        {   
            foreach (var player in Players)
            {
                PlayerScores[player.Id] = StartingScore;
            }
            CurrentPlayerIndex = 0;
            InputScore = "";
        }

        public KeyboardParameters KeyboardParams = new()
        {
            KeyboardKeys = new List<List<KeyboardKey>>
            {
                new List<KeyboardKey> 
                {
                    new() { Text = "1", Value = "1" },
                    new() { Text = "2", Value = "2" },
                    new() { Text = "3", Value = "3" }
                },
                new List<KeyboardKey> 
                {
                    new() { Text = "4", Value = "4" },
                    new() { Text = "5", Value = "5" },
                    new() { Text = "6", Value = "6" }
                },
                new List<KeyboardKey> 
                {
                    new() { Text = "7", Value = "7" },
                    new() { Text = "8", Value = "8" },
                    new() { Text = "9", Value = "9" }
                },
                new List<KeyboardKey> 
                {
                    new() { Text = "0", Value = "0" }
                }
            }
        };
    }
}
