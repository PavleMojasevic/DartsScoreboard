using Microsoft.AspNetCore.Components;
using System.Xml.Serialization;

namespace DartsScoreboard
{
    public partial class GameStandardPlay
    {
        [Inject] public PlayerSelectionService PlayerService { get; set; } = default!;
        [Inject] public IStandardGamePersistence _StandardGamePersistence { get; set; }
        [Inject] public IUserPersistence _UserPersistence { get; set; }
        public List<User> Players { get; set; } = new();
        public Dictionary<int, Dictionary<int, int>> PlayerScores { get; set; } = new(); // <user.Id, <PlayerScore, PlayerThrows>>
        public int CurrentPlayerIndex { get; set; } = 0;
        public string InputScore { get; set; } = "";
        public bool WinnerPopup { get; set; } = false;

        public int StartingScore = 501;

        // Checkout list and darts used on a double
        private void OnSelectNumOfDarts(int value) => SelectedNumOfDarts = value;
        private void OnSelectDartsUsedOnCheckout(int value) => SelectedDartsUsedOnCheckout = value;
        private void CloseCheckoutPopup()
        {
            ShowCheckoutPopup = false;
            InputScore = "";
        }

        public int NumOfDoubleDartsThrownAll { get; set; } = 0;
        public int SelectedNumOfDarts { get; set; } = 1;
        public int SelectedDartsUsedOnCheckout { get; set; } = 3;

        public bool ShowCheckoutPopup { get; set; } = false;
        public List<int> AvailableDoubleDartOptions { get; set; } = new();
        public List<int> AvailableCheckoutDartOptions { get; set; } = new();

        protected override void OnInitialized()
        {
            Players = PlayerService.SelectedPlayers;

            foreach (var player in Players)
            {
                PlayerScores[player.Id] = new Dictionary<int, int>
                {
                    { 0, StartingScore }, // 0 = score
                    { 1, 0 }              // 1 = throws
                };
            }
        }
        private void HandleKey(KeyboardKey key)
        {
            if (key.Value == "DEL")
            {
                if (!string.IsNullOrEmpty(InputScore))
                {
                    InputScore = InputScore.Substring(0, InputScore.Length - 1);
                }
            }
            else
            {
                InputScore += key.Value;
            }
        }

        // Numbers that cannot be finished
        private static readonly HashSet<int> NoFinishScores = new() { 169, 168, 166, 165, 163, 162, 159 };
        private bool NoScore()
        {
            var currentPlayer = Players[CurrentPlayerIndex];
            return NoFinishScores.Contains(PlayerScores[currentPlayer.Id][0]);
        }
        private void DoubleOutCheckout(int score)
        {
            var currentPlayer = Players[CurrentPlayerIndex];

            if (score > 180 || PlayerScores[currentPlayer.Id][0] - score < 0)
            {
                // Invalid score
                InputScore = "";
                return;
            }

            if (NoScore())
            {
                PlayerScores[currentPlayer.Id][1] += 3;
            }
            else if (PlayerScores[currentPlayer.Id][0] > 100 || PlayerScores[currentPlayer.Id][0] == 99)
            {
                PlayerScores[currentPlayer.Id][1] += 3;
                NumOfDoubleDartsThrownAll += 1;
                if (PlayerScores[currentPlayer.Id][0] - score < 51)
                {
                    // Setup the popup
                    AvailableDoubleDartOptions = new List<int> { 0, 1 };
                    AvailableCheckoutDartOptions = new();
                    ShowCheckoutPopup = true;
                }
            }
            else if (PlayerScores[currentPlayer.Id][0] > 40 || (PlayerScores[currentPlayer.Id][0] % 2 != 0 && PlayerScores[currentPlayer.Id][0] > 1) || 
                !(PlayerScores[currentPlayer.Id][0] != 101 && PlayerScores[currentPlayer.Id][0] != 104 && PlayerScores[currentPlayer.Id][0] != 107 && PlayerScores[currentPlayer.Id][0] != 110))
            {
                if (PlayerScores[currentPlayer.Id][0] - score == 0)
                {
                    AvailableDoubleDartOptions = new List<int> { 1, 2 };
                    AvailableCheckoutDartOptions = new List<int> { 2, 3 };
                    ShowCheckoutPopup = true;
                }
                else if (PlayerScores[currentPlayer.Id][0] - score > 50)
                {
                    PlayerScores[currentPlayer.Id][1] += 3;
                }
                else if (PlayerScores[currentPlayer.Id][0] - score < 51)
                {
                    // Setup the popup
                    PlayerScores[currentPlayer.Id][1] += 3;
                    AvailableDoubleDartOptions = new List<int> { 0, 1, 2 };
                    AvailableCheckoutDartOptions = new();
                    ShowCheckoutPopup = true;
                }
            }
            else if (PlayerScores[currentPlayer.Id][0] > 1)
            {
                if (PlayerScores[currentPlayer.Id][0] - score == 0)
                {
                    AvailableDoubleDartOptions = new List<int> { 1, 2, 3 };
                    AvailableCheckoutDartOptions = new List<int> { 1, 2, 3 };
                    ShowCheckoutPopup = true;
                }
                else if (PlayerScores[currentPlayer.Id][0] - score > 1)
                {
                    PlayerScores[currentPlayer.Id][1] += 3;
                    AvailableDoubleDartOptions = new List<int> { 0, 1, 2, 3 };
                    AvailableCheckoutDartOptions = new();
                    ShowCheckoutPopup = true;
                }
            }
            else
            {
                return; // ERROR: No valid checkout
            }
        }
        private void ConfirmCheckoutData()
        {

            if (int.TryParse(InputScore, out int score))
            {
               var currentPlayer = Players[CurrentPlayerIndex];

                NumOfDoubleDartsThrownAll += SelectedNumOfDarts;

                if (AvailableCheckoutDartOptions?.Count > 0)
                {
                    // only when you actually offered Checkout Options
                    PlayerScores[currentPlayer.Id][1] += SelectedDartsUsedOnCheckout;
                }

                // Close popup
                ShowCheckoutPopup = false;
                SelectedNumOfDarts = 0;
                SelectedDartsUsedOnCheckout = 0;
                
                SubmintigScore(score);
            }
        }
        private void SubmintigScore(int score)
        {
            var currentPlayer = Players[CurrentPlayerIndex];

            if (score > 180 || PlayerScores[currentPlayer.Id][0] - score < 0)
            {
                // Invalid score
                InputScore = "";
                return;
            }
            
            PlayerScores[currentPlayer.Id][0] -= score;
            UpdateHighScoreHits(currentPlayer, score);

            if (PlayerScores[currentPlayer.Id][0] == 0)
            {
                currentPlayer.Stats.ThreeDartAverage = (double)(StartingScore / ((double)PlayerScores[currentPlayer.Id][1] / 3));
                currentPlayer.Stats.CheckoutPercentage = (1 / (double)NumOfDoubleDartsThrownAll) * 100;
                WinnerPopup = true;
                return;
            }
            else if (PlayerScores[currentPlayer.Id][0] < 0)
            {
                // Player has busted
                PlayerScores[currentPlayer.Id][0] += score;
                PlayerScores[currentPlayer.Id][1] += 3;
            }
            else if (PlayerScores[currentPlayer.Id][0] == 1)
            {
                // Player has busted
                PlayerScores[currentPlayer.Id][0] += score;
                PlayerScores[currentPlayer.Id][1] += 3;
            }
            else
            {
                CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            }

            InputScore = "";
        }
        private void SubmitScore()
        {
            if (int.TryParse(InputScore, out int score))
            {
                var currentPlayer = Players[CurrentPlayerIndex];

                // Calculating averages
                if (PlayerScores[currentPlayer.Id][0] > 170)
                {
                    PlayerScores[currentPlayer.Id][1] += 3;
                }
                else
                {
                    if (NoScore())
                    {
                        if (PlayerScores[currentPlayer.Id][0] - score == 0)
                        {
                            // Invalid score
                            InputScore = "";
                            return;
                        }
                    }

                    DoubleOutCheckout(score);
                }

                if (!ShowCheckoutPopup)
                {
                    SubmintigScore(score);
                }
            }
        }
        private void ResetGame()
        {
            CurrentPlayerIndex = 0;
            InputScore = "";
            WinnerPopup = false;
            foreach (var player in Players)
            {
                PlayerScores[player.Id][0] = StartingScore;
                PlayerScores[player.Id][1] = 0;
                player.Stats.HighestScore = 0;
            }
        }

        private void UpdateHighScoreHits(User currentPlayer, int roundScore)
        {
            if (roundScore == 180)
            {
                currentPlayer.Stats.HighScoreHits["180"]++;
            }
            else if (roundScore >= 160)
            {
                currentPlayer.Stats.HighScoreHits["160+"]++;
            }
            else if (roundScore >= 140)
            {
                currentPlayer.Stats.HighScoreHits["140+"]++;
            }
            else if (roundScore >= 120)
            {
                currentPlayer.Stats.HighScoreHits["120+"]++;
            }
            else if (roundScore >= 100)
            {
                currentPlayer.Stats.HighScoreHits["100+"]++;
            }
            else if (roundScore >= 80)
            {
                currentPlayer.Stats.HighScoreHits["80+"]++;
            }
            else if (roundScore >= 60)
            {
                currentPlayer.Stats.HighScoreHits["60+"]++;
            }
            else if (roundScore >= 40)
            {
                currentPlayer.Stats.HighScoreHits["40+"]++;
            }

            if (currentPlayer.Stats.HighestScore < roundScore)
            {
                currentPlayer.Stats.HighestScore = roundScore;
            }
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
                    new() { Text = "0", Value = "0" },
                    new() { Text = "Del", Value = "DEL" }  // Delete button
                }
            }
        };
    }
}
