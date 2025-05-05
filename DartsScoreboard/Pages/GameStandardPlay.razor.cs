using DartsScoreboard.Services;
using Microsoft.AspNetCore.Components;
using System.Xml.Serialization;

namespace DartsScoreboard
{
    public class PlayerScoreInfo
    {
        public int PlayerScore { get; set; }
        public int PlayerThrows { get; set; }
        public int PlayerSets { get; set; }
        public int PlayerLegs { get; set; }
    }
    public class RoundSnapshot
    {
        public int CurrentPlayerIndex { get; set; }
        public string InputScore { get; set; } = "";
        public Dictionary<int, PlayerScoreInfo> PlayerStates { get; set; } = new();
        public List<User> PlayerStatsSnapshot { get; set; } = new();
    }
    public partial class GameStandardPlay
    {
        [Inject] public PlayerSelectionService PlayerService { get; set; } = default!;
        [Inject] public IStandardGamePersistence _StandardGamePersistence { get; set; }
        [Inject] public IUserPersistence _UserPersistence { get; set; }
        [Inject] public GameSettingsService GameSettings { get; set; } = default!;
        public List<User> Players { get; set; } = new();
        public Dictionary<int, PlayerScoreInfo> PlayerScores { get; set; } = new();
        public int CurrentPlayerIndex { get; set; } = 0;
        public string InputScore { get; set; } = "";
        public bool WinnerPopup { get; set; } = false;

        // Player rotation
        private int StartingPlayerIndexSets = 0;
        private int StartingPlayerIndexLegs = 0;

        // Default game settings
        public int StartingScore = 501;
        public string StartingIn = "STRAIGHT IN";
        public string StartingOut = "DOUBLE OUT";
        public int NumOfSets = 1; // Number of sets per leg
        public int NumOfLegs = 1; // Number of legs to play

        // Creating undo stack
        public Stack<RoundSnapshot> UndoStack = new();

        // Checkout list and darts used on a double
        private void OnSelectDartsUsedOnDouble(int value) => SelectedDartsUsedOnDouble = value;
        private void OnSelectDartsUsedOnCheckout(int value) => SelectedDartsUsedOnCheckout = value;
        private void CloseCheckoutPopup()
        {
            ShowCheckoutPopup = false;
            InputScore = "";
        }

        public int SelectedDartsUsedOnDouble { get; set; } = 1;
        public int SelectedDartsUsedOnCheckout { get; set; } = 3;

        public bool ShowCheckoutPopup { get; set; } = false;
        public List<int> AvailableDoubleDartOptions { get; set; } = new();
        public List<int> AvailableCheckoutDartOptions { get; set; } = new();

        protected override void OnInitialized()
        {
            StartingPlayerIndexSets = 0;
            StartingPlayerIndexLegs = 0;
            CurrentPlayerIndex = StartingPlayerIndexLegs;

            StartingScore = GameSettings.StartingScore;
            StartingIn = GameSettings.StartInOption;
            StartingOut = GameSettings.EndInOption;

            Players = PlayerService.SelectedPlayers;

            foreach (var player in Players)
            {
                PlayerScores[player.Id] = new PlayerScoreInfo
                {
                    PlayerScore = StartingScore,
                    PlayerThrows = 0,
                    PlayerSets = 0,
                    PlayerLegs = 0
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
            else if (key.Value == "UNDO")
            {
                UndoMove();
                InputScore = "";
            }
            else
            {
                InputScore += key.Value;
            }
        }

        [Inject] NavigationManager NavManager { get; set; } = default!;
        private void GoHome()
        {
            NavManager.NavigateTo("/");
        }

        // Numbers that cannot be finished
        private static readonly HashSet<int> NoFinishScores = new() { 169, 168, 166, 165, 163, 162, 159 };
        private bool NoScore()
        {
            var currentPlayer = Players[CurrentPlayerIndex];
            return NoFinishScores.Contains(PlayerScores[currentPlayer.Id].PlayerScore);
        }
        private void DoubleOutCheckout(int score)
        {
            var currentPlayer = Players[CurrentPlayerIndex];

            if (score > 180 || PlayerScores[currentPlayer.Id].PlayerScore - score < 0)
            {
                // Invalid score
                InputScore = "";
                return;
            }

            if (NoScore())
            {
                PlayerScores[currentPlayer.Id].PlayerThrows += 3;
            }
            else if (PlayerScores[currentPlayer.Id].PlayerScore > 100 || PlayerScores[currentPlayer.Id].PlayerScore == 99)
            {
                PlayerScores[currentPlayer.Id].PlayerThrows += 3;
                currentPlayer.Stats.NumOfDoublesThrown += 1;
                if (PlayerScores[currentPlayer.Id].PlayerScore - score < 51 && PlayerScores[currentPlayer.Id].PlayerScore - score > 1)
                {
                    // Setup the popup
                    AvailableDoubleDartOptions = new List<int> { 0, 1 };
                    AvailableCheckoutDartOptions = new();
                    ShowCheckoutPopup = true;
                }
            }
            else if (PlayerScores[currentPlayer.Id].PlayerScore > 40 || (PlayerScores[currentPlayer.Id].PlayerScore % 2 != 0 && PlayerScores[currentPlayer.Id].PlayerScore > 1) || 
                !(PlayerScores[currentPlayer.Id].PlayerScore != 101 && PlayerScores[currentPlayer.Id].PlayerScore != 104 && PlayerScores[currentPlayer.Id].PlayerScore != 107 && PlayerScores[currentPlayer.Id].PlayerScore != 110))
            {
                if (PlayerScores[currentPlayer.Id].PlayerScore - score == 0)
                {
                    AvailableDoubleDartOptions = new List<int> { 1, 2 };
                    AvailableCheckoutDartOptions = new List<int> { 2, 3 };
                    ShowCheckoutPopup = true;
                }
                else if (PlayerScores[currentPlayer.Id].PlayerScore - score > 50)
                {
                    PlayerScores[currentPlayer.Id].PlayerThrows += 3;
                }
                else if (PlayerScores[currentPlayer.Id].PlayerScore - score < 51)
                {
                    // Setup the popup
                    PlayerScores[currentPlayer.Id].PlayerThrows += 3;
                    AvailableDoubleDartOptions = new List<int> { 0, 1, 2 };
                    AvailableCheckoutDartOptions = new();
                    ShowCheckoutPopup = true;
                }
            }
            else if (PlayerScores[currentPlayer.Id].PlayerScore > 1)
            {
                if (PlayerScores[currentPlayer.Id].PlayerScore - score == 0)
                {
                    AvailableDoubleDartOptions = new List<int> { 1, 2, 3 };
                    AvailableCheckoutDartOptions = new List<int> { 1, 2, 3 };
                    ShowCheckoutPopup = true;
                }
                else if (PlayerScores[currentPlayer.Id].PlayerScore - score > 1)
                {
                    PlayerScores[currentPlayer.Id].PlayerThrows += 3;
                    AvailableDoubleDartOptions = new List<int> { 0, 1, 2, 3 };
                    AvailableCheckoutDartOptions = new();
                    ShowCheckoutPopup = true;
                }
            }
            else
            {
                // Invalid score
                InputScore = "";
                return; // ERROR: No valid checkout
            }

            // Stats
            if (StartingScore - PlayerScores[currentPlayer.Id].PlayerScore != 0)
            {
                currentPlayer.Stats.ThreeDartAverage = (double)((StartingScore - PlayerScores[currentPlayer.Id].PlayerScore) / ((double)PlayerScores[currentPlayer.Id].PlayerThrows / 3));
                if (currentPlayer.Stats.NumOfDoublesThrown > 0)
                    currentPlayer.Stats.CheckoutPercentage = (1 / (double)currentPlayer.Stats.NumOfDoublesThrown) * 100;
                else
                    currentPlayer.Stats.CheckoutPercentage = 0;
            }

        }
        private void ConfirmCheckoutData()
        {

            if (int.TryParse(InputScore, out int score))
            {
               var currentPlayer = Players[CurrentPlayerIndex];

                currentPlayer.Stats.NumOfDoublesThrown += SelectedDartsUsedOnDouble;

                if (AvailableCheckoutDartOptions?.Count > 0)
                {
                    // Only when you actually offered Checkout Options
                    PlayerScores[currentPlayer.Id].PlayerThrows += SelectedDartsUsedOnCheckout;
                }

                // Close popup
                ShowCheckoutPopup = false;
                SelectedDartsUsedOnDouble = 0;
                SelectedDartsUsedOnCheckout = 0;
                
                SubmintigScore(score);
            }
        }
        private void SubmitScore()
        {
            if (int.TryParse(InputScore, out int score))
            {
                var currentPlayer = Players[CurrentPlayerIndex];

                // Pushing stats and info to undo stack
                var snapshot = new RoundSnapshot
                {
                    CurrentPlayerIndex = CurrentPlayerIndex,
                    InputScore = InputScore,
                    PlayerStates = Players.ToDictionary(p => p.Id, p => new PlayerScoreInfo
                    {
                        PlayerScore = PlayerScores[p.Id].PlayerScore,
                        PlayerThrows = PlayerScores[p.Id].PlayerThrows,
                        PlayerLegs = PlayerScores[p.Id].PlayerLegs,
                        PlayerSets = PlayerScores[p.Id].PlayerSets
                    }),
                    PlayerStatsSnapshot = Players.Select(p => new User
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Stats = new UserStats
                        {
                            ThreeDartAverage = p.Stats.ThreeDartAverage,
                            CheckoutPercentage = p.Stats.CheckoutPercentage,
                            HighScoreHits = new Dictionary<string, int>(p.Stats.HighScoreHits)
                        }
                    }).ToList()
                };

                UndoStack.Push(snapshot);

                // Calculating averages
                if (PlayerScores[currentPlayer.Id].PlayerScore > 170 || StartingOut != "DOUBLE OUT")
                {
                    PlayerScores[currentPlayer.Id].PlayerThrows += 3;
                    // Stats
                    if (StartingScore - PlayerScores[currentPlayer.Id].PlayerScore != 0)
                    {
                        currentPlayer.Stats.ThreeDartAverage = (double)((StartingScore - PlayerScores[currentPlayer.Id].PlayerScore) / ((double)PlayerScores[currentPlayer.Id].PlayerThrows / 3));
                        if (currentPlayer.Stats.NumOfDoublesThrown > 0)
                            currentPlayer.Stats.CheckoutPercentage = (1 / (double)currentPlayer.Stats.NumOfDoublesThrown) * 100;
                        else
                            currentPlayer.Stats.CheckoutPercentage = 0;
                    }

                }
                else
                {
                    if (NoScore())
                    {
                        if (PlayerScores[currentPlayer.Id].PlayerScore - score == 0)
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
        private void SubmintigScore(int score)
        {
            var currentPlayer = Players[CurrentPlayerIndex];

            if (score > 180 || PlayerScores[currentPlayer.Id].PlayerScore - score < 0)
            {
                // Invalid score
                InputScore = "";
                return;
            }
            
            PlayerScores[currentPlayer.Id].PlayerScore -= score;
            UpdateHighScoreHits(currentPlayer, score);

            // Loading other player scores
            if (StartingScore - PlayerScores[currentPlayer.Id].PlayerScore != 0)
            {
                foreach (var player in Players)
                {
                    player.Stats.ThreeDartAverage = (double)((StartingScore - PlayerScores[player.Id].PlayerScore) / ((double)PlayerScores[player.Id].PlayerThrows / 3));

                    if (player.Stats.NumOfDoublesThrown > 0)
                        player.Stats.CheckoutPercentage = (1 / (double)player.Stats.NumOfDoublesThrown) * 100;
                    else
                        player.Stats.CheckoutPercentage = 0;
                }
            }

            if (PlayerScores[currentPlayer.Id].PlayerScore == 0)
            {
                // Winner player stats
                currentPlayer.Stats.ThreeDartAverage = (double)((StartingScore - PlayerScores[currentPlayer.Id].PlayerScore) / ((double)PlayerScores[currentPlayer.Id].PlayerThrows / 3));
                if (currentPlayer.Stats.NumOfDoublesThrown > 0)
                    currentPlayer.Stats.CheckoutPercentage = (1 / (double)currentPlayer.Stats.NumOfDoublesThrown) * 100;
                else
                    currentPlayer.Stats.CheckoutPercentage = 0;
                
                if (PlayerScores[currentPlayer.Id].PlayerLegs == NumOfLegs)
                {
                    PlayerScores[currentPlayer.Id].PlayerSets++;
                    foreach (var players in Players)
                    {
                        PlayerScores[players.Id].PlayerLegs = 0;
                        PlayerScores[players.Id].PlayerScore = GameSettings.StartingScore;
                    }

                    // Rotate starting player for next leg/set
                    StartingPlayerIndexSets = (StartingPlayerIndexSets + 1) % Players.Count;
                    CurrentPlayerIndex = StartingPlayerIndexSets;
                }
                else
                {
                    PlayerScores[currentPlayer.Id].PlayerLegs++;
                    foreach (var players in Players)
                    {
                        PlayerScores[players.Id].PlayerScore = GameSettings.StartingScore;
                    }

                    // Rotate starting player for next leg/set
                    StartingPlayerIndexLegs = (StartingPlayerIndexLegs + 1) % Players.Count;
                    CurrentPlayerIndex = StartingPlayerIndexLegs;
                }

                if (PlayerScores[currentPlayer.Id].PlayerSets == NumOfSets)
                {
                    WinnerPopup = true;
                    return;
                }
            }
            else if (PlayerScores[currentPlayer.Id].PlayerScore < 0)
            {
                // Player has busted
                PlayerScores[currentPlayer.Id].PlayerScore += score;
                PlayerScores[currentPlayer.Id].PlayerThrows += 3;
            }
            else if (PlayerScores[currentPlayer.Id].PlayerScore == 1)
            {
                // Player has busted
                PlayerScores[currentPlayer.Id].PlayerScore += score;
                PlayerScores[currentPlayer.Id].PlayerThrows += 3;
            }
            else
            {
                CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            }

            InputScore = "";
        }

        private void UndoMove()
        {
            if (UndoStack.TryPop(out var snapshot))
            {
                CurrentPlayerIndex = snapshot.CurrentPlayerIndex;
                InputScore = snapshot.InputScore;

                // Restore player scores
                foreach (var kvp in snapshot.PlayerStates)
                {
                    if (PlayerScores.ContainsKey(kvp.Key))
                    {
                        PlayerScores[kvp.Key] = kvp.Value;
                    }
                }

                // Restore player stats
                foreach (var statSnapshot in snapshot.PlayerStatsSnapshot)
                {
                    var player = Players.FirstOrDefault(p => p.Id == statSnapshot.Id);
                    if (player != null)
                    {
                        player.Stats = statSnapshot.Stats;
                    }
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
                PlayerScores[player.Id].PlayerScore = StartingScore;
                PlayerScores[player.Id].PlayerThrows = 0;
                PlayerScores[player.Id].PlayerSets = 0;
                PlayerScores[player.Id].PlayerLegs = 0;
                player.Stats.HighestScore = 0;

                player.Stats.ThreeDartAverage = 0;
                player.Stats.CheckoutPercentage = 0;
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
                    new() { Text = "Undo", Value = "UNDO" },
                    new() { Text = "0", Value = "0" },
                    new() { Text = "Del", Value = "DEL" }  // Delete button
                }
            }
        };
    }
}
