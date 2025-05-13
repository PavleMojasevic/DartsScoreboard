using DartsScoreboard.Services;
using Microsoft.AspNetCore.Components;
using System.Numerics;
using System.Xml.Serialization;

namespace DartsScoreboard
{
    public class PlayerScoreInfo
    {
        public int PlayerScore { get; set; }
        public int PlayerThrows { get; set; }
        public int PlayerSets { get; set; }
        public int PlayerLegs { get; set; }
        public int PlayerCollectedScore { get; set; }
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
        // Unique code per game
        [Parameter] public string GameCode { get; set; } = "";

        [Inject] public PlayerSelectionService PlayerService { get; set; } = default!;
        [Inject] public IStandardGamePersistence _StandardGamePersistence { get; set; }
        [Inject] public IUserPersistence _UserPersistence { get; set; }
        [Inject] public GameSettingsService GameSettings { get; set; } = default!;
        [Inject] StandardGameUserService StandardGameUserService { get; set; } = default!;

        // Player info
        public List<User> Players { get; set; } = new();
        public Dictionary<int, PlayerScoreInfo> PlayerScores { get; set; } = new();
        public int CurrentPlayerIndex { get; set; } = 0;

        // Input scores
        public string InputScore { get; set; } = "";
        public string InputScoreDartOne { get; set; } = "";
        public string InputScoreDartTwo { get; set; } = "";
        public string InputScoreDartThree { get; set; } = "";
        public int DartIndex { get; set; } = 1; // 1 = first, 2 = second, 3 = third

        // Keyboard selection
        public bool UseThreeDartMode { get; set; } = false;
        public string SelectedMultiplier { get; set; } = "S"; // "S", "D", or "T"

        // Winner popup
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

        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrWhiteSpace(GameCode))
            {
                var savedGame = await _StandardGamePersistence.Get(GameCode);
                if (savedGame != null)
                {
                    // Restore game state
                    GameCode = savedGame.Code;
                    Players = savedGame.Players;
                    PlayerScores = savedGame.PlayerScores;
                    CurrentPlayerIndex = savedGame.CurrentPlayerIndex;
                    DartIndex = savedGame.DartIndex;

                    InputScoreDartOne = savedGame.InputScoreDartOne;
                    InputScoreDartTwo = savedGame.InputScoreDartTwo;
                    InputScoreDartThree = savedGame.InputScoreDartThree;
                    UseThreeDartMode = savedGame.UseThreeDartMode;
                    SelectedMultiplier = savedGame.SelectedMultiplier;
                    WinnerPopup = savedGame.WinnerPopup;

                    StartingScore = savedGame.StartingPoints;
                    StartingIn = savedGame.StartingIn;
                    StartingOut = savedGame.StartingOut;
                    NumOfLegs = savedGame.NumOfLegs;
                    NumOfSets = savedGame.NumOfSets;

                    PlayerStatsToPlayers(savedGame.PlayerStats);
                    return;
                }
            }

            // Fallback: Start new game
            PlayerService.SelectedPlayers = StandardGameUserService.Players;
            Players = PlayerService.SelectedPlayers;
            GameCode = GameCode == "" ? Guid.NewGuid().ToString() : GameCode;

            StartingScore = GameSettings.StartingScore;
            StartingIn = GameSettings.StartInOption;
            StartingOut = GameSettings.EndInOption;
            NumOfLegs = GameSettings.Legs;
            NumOfSets = GameSettings.Sets;

            foreach (var player in Players)
            {
                PlayerScores[player.Id] = new PlayerScoreInfo
                {
                    PlayerScore = StartingScore,
                    PlayerThrows = 0,
                    PlayerSets = 0,
                    PlayerLegs = 0,
                    PlayerCollectedScore = 0
                };
            }
        }
        private void PlayerStatsToPlayers(List<User> savedStats)
        {
            foreach (var stat in savedStats)
            {
                var player = Players.FirstOrDefault(p => p.Id == stat.Id);
                if (player != null)
                {
                    player.Stats = stat.Stats;
                }
            }
        }

        [Inject] NavigationManager NavManager { get; set; } = default!;
        private void GoHome()
        {
            NavManager.NavigateTo("/");
        }

        private int CombineScore()
        {
            int dartOne = int.TryParse(InputScoreDartOne, out int dartOneValue) ? dartOneValue : 0;
            int dartTwo = int.TryParse(InputScoreDartTwo, out int dartTwoValue) ? dartTwoValue : 0;
            int dartThree = int.TryParse(InputScoreDartThree, out int dartThreeValue) ? dartThreeValue : 0;
            int totalScore = dartOne + dartTwo + dartThree;

            return totalScore;
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
            else if (PlayerScores[currentPlayer.Id].PlayerScore > 40 || PlayerScores[currentPlayer.Id].PlayerScore % 2 != 0 && PlayerScores[currentPlayer.Id].PlayerScore > 1 ||
                !(PlayerScores[currentPlayer.Id].PlayerScore != 101 && PlayerScores[currentPlayer.Id].PlayerScore != 104 && PlayerScores[currentPlayer.Id].PlayerScore != 107 && PlayerScores[currentPlayer.Id].PlayerScore != 110))
            {
                if (PlayerScores[currentPlayer.Id].PlayerScore - score == 0)
                {
                    AvailableDoubleDartOptions = new List<int> { 1, 2 };
                    if (UseThreeDartMode)
                    {
                        if (InputScoreDartThree != "")
                        {
                            PlayerScores[currentPlayer.Id].PlayerThrows += 3;
                        }
                        else
                        {
                            PlayerScores[currentPlayer.Id].PlayerThrows += 2;
                        }
                    }
                    else
                    {
                        AvailableCheckoutDartOptions = new List<int> { 2, 3 };      // TODO: Add eachDart keyboard options
                    }
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
                    if (UseThreeDartMode)
                    {
                        if (InputScoreDartThree != "")
                        {
                            PlayerScores[currentPlayer.Id].PlayerThrows += 3;
                        }
                        else if (InputScoreDartTwo != "")
                        {
                            PlayerScores[currentPlayer.Id].PlayerThrows += 2;
                        }
                        else
                        {
                            PlayerScores[currentPlayer.Id].PlayerThrows += 1;
                        }
                    }
                    else
                    {
                        AvailableCheckoutDartOptions = new List<int> { 1, 2, 3 };    // TODO: Add eachDart keyboard options
                    }
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
            PlayerScores[currentPlayer.Id].PlayerCollectedScore += score;
            currentPlayer.Stats.ThreeDartAverage = (double)(PlayerScores[currentPlayer.Id].PlayerCollectedScore / ((double)PlayerScores[currentPlayer.Id].PlayerThrows / 3));
            if (currentPlayer.Stats.NumOfDoublesThrown > 0)
                currentPlayer.Stats.CheckoutPercentage = (PlayerScores[currentPlayer.Id].PlayerLegs + NumOfSets * NumOfLegs) / (double)currentPlayer.Stats.NumOfDoublesThrown * 100;
            else
                currentPlayer.Stats.CheckoutPercentage = 0;
        }
        private async Task ConfirmCheckoutData()
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

                await SubmintigScore(score);
            }
        }
        private async Task SubmitScore()
        {
            var currentPlayer = Players[CurrentPlayerIndex];

            // Three dart keyboard conditions
            SelectedMultiplier = "S";
            if (UseThreeDartMode)
            {
                if (InputScoreDartOne != "" && InputScoreDartTwo != "" && InputScoreDartThree != "" || PlayerScores[currentPlayer.Id].PlayerScore - CombineScore() == 0)
                {
                    InputScore = CombineScore().ToString();
                }
                else
                {
                    // Invalid score
                    InputScore = "";
                    return;
                }
            }

            if (int.TryParse(InputScore, out int score))
            {
                // Pushing stats and info to undo stack
                if (score < 181)
                {
                    var snapshot = new RoundSnapshot
                    {
                        CurrentPlayerIndex = CurrentPlayerIndex,
                        InputScore = InputScore,
                        PlayerStates = Players.ToDictionary(p => p.Id, p => new PlayerScoreInfo
                        {
                            PlayerScore = PlayerScores[p.Id].PlayerScore,
                            PlayerThrows = PlayerScores[p.Id].PlayerThrows,
                            PlayerLegs = PlayerScores[p.Id].PlayerLegs,
                            PlayerSets = PlayerScores[p.Id].PlayerSets,
                            PlayerCollectedScore = PlayerScores[p.Id].PlayerCollectedScore
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
                        PlayerScores[currentPlayer.Id].PlayerCollectedScore += score;
                        currentPlayer.Stats.ThreeDartAverage = (double)(PlayerScores[currentPlayer.Id].PlayerCollectedScore / ((double)PlayerScores[currentPlayer.Id].PlayerThrows / 3));
                        if (currentPlayer.Stats.NumOfDoublesThrown > 0)
                            currentPlayer.Stats.CheckoutPercentage = (PlayerScores[currentPlayer.Id].PlayerLegs + NumOfSets * NumOfLegs) / (double)currentPlayer.Stats.NumOfDoublesThrown * 100;
                        else
                            currentPlayer.Stats.CheckoutPercentage = 0;
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
                        await SubmintigScore(score);
                    }
                }
                else
                {
                    // Invalid score
                    InputScore = "";
                    return;
                }
            }
        }
        private async Task SubmintigScore(int score)
        {
            DartIndex = 1; // wrap around or submit here

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
            foreach (var player in Players)
            {
                player.Stats.ThreeDartAverage = (double)(PlayerScores[player.Id].PlayerCollectedScore / ((double)PlayerScores[player.Id].PlayerThrows / 3));

                if (player.Stats.NumOfDoublesThrown > 0)
                    player.Stats.CheckoutPercentage = (PlayerScores[player.Id].PlayerLegs + NumOfSets * NumOfLegs) / (double)player.Stats.NumOfDoublesThrown * 100;
                else
                    player.Stats.CheckoutPercentage = 0;
            }

            if (PlayerScores[currentPlayer.Id].PlayerScore == 0)
            {
                if (PlayerScores[currentPlayer.Id].PlayerLegs + 1 == NumOfLegs)
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
                    await SaveGameAsync();
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
            InputScoreDartOne = "";
            InputScoreDartTwo = "";
            InputScoreDartThree = "";

            await SaveGameAsync();
        }

        private async Task UndoMove()
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

            await SaveGameAsync();
        }
        private async Task SaveGameAsync()
        {
            var save = new StandardGame
            {
                Code = GameCode,
                Players = Players,
                StartingPoints = StartingScore,
                NumOfSets = NumOfSets,
                NumOfLegs = NumOfLegs,
                StartingIn = StartingIn,
                StartingOut = StartingOut,

                CurrentPlayerIndex = CurrentPlayerIndex,
                DartIndex = DartIndex,
                InputScoreDartOne = InputScoreDartOne,
                InputScoreDartTwo = InputScoreDartTwo,
                InputScoreDartThree = InputScoreDartThree,
                UseThreeDartMode = UseThreeDartMode,
                SelectedMultiplier = SelectedMultiplier,
                WinnerPopup = WinnerPopup,

                PlayerScores = PlayerScores,
                PlayerStats = Players.Select(p => new User
                {
                    Id = p.Id,
                    Name = p.Name,
                    Stats = p.Stats
                }).ToList()
            };

            await _StandardGamePersistence.AddOrUpdate(save);
        }

        private async Task ResetGame()
        {
            GameCode = Guid.NewGuid().ToString();
            CurrentPlayerIndex = 0;
            DartIndex = 1;
            InputScore = "";
            InputScoreDartOne = "";
            InputScoreDartTwo = "";
            InputScoreDartThree = "";
            WinnerPopup = false;
            UndoStack.Clear();

            foreach (var player in Players)
            {
                PlayerScores[player.Id] = new PlayerScoreInfo
                {
                    PlayerScore = StartingScore,
                    PlayerThrows = 0,
                    PlayerSets = 0,
                    PlayerLegs = 0,
                    PlayerCollectedScore = 0
                };

                player.Stats.HighestScore = 0;
                player.Stats.ThreeDartAverage = 0;
                player.Stats.CheckoutPercentage = 0;

                foreach (var key in player.Stats.HighScoreHits.Keys.ToList())
                {
                    player.Stats.HighScoreHits[key] = 0;
                }
            }

            await SaveGameAsync();
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


        private async Task HandleKey(KeyboardKey key)
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
                await UndoMove();
                InputScore = "";
            }
            else
            {
                InputScore += key.Value;
            }
        }

        private async Task HandleKeyEachDart(KeyboardKey key)
        {
            if (key.Value == "DEL")
            {
                switch (DartIndex)
                {
                    case 1:
                        InputScoreDartOne = "";
                        break;

                    case 2:
                        if (!string.IsNullOrEmpty(InputScoreDartTwo))
                        {
                            InputScoreDartTwo = "";
                        }
                        else
                        {
                            DartIndex = 1;
                            InputScoreDartOne = "";
                        }
                        break;

                    case 3:
                        if (!string.IsNullOrEmpty(InputScoreDartThree))
                        {
                            InputScoreDartThree = "";
                        }
                        else
                        {
                            DartIndex = 2;
                            InputScoreDartTwo = "";
                        }
                        break;
                }
            }
            else if (key.Value == "UNDO")
            {
                await UndoMove();
                InputScoreDartOne = InputScoreDartTwo = InputScoreDartThree = "";
                DartIndex = 1;
            }
            else
            {
                if (key.Value == "MISS")
                {
                    ApplyDart("0");
                }
                else
                {
                    ApplyDart(key.Value);
                }
            }
        }
        private void ApplyDart(string value)
        {
            switch (DartIndex)
            {
                case 1:
                    InputScoreDartOne = value;
                    DartIndex = 2;
                    break;
                case 2:
                    InputScoreDartTwo = value;
                    DartIndex = 3;
                    break;
                case 3:
                    InputScoreDartThree = value;
                    // DartIndex = 1; // wrap around or submit here
                    break;
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

        public KeyboardParameters KeyboardParamsEachDartSingle = new()
        {
            KeyboardKeys = new List<List<KeyboardKey>>
            {
                new List<KeyboardKey>
                {
                    new() { Text = "1", Value = "1" },
                    new() { Text = "2", Value = "2" },
                    new() { Text = "3", Value = "3" },
                    new() { Text = "4", Value = "4" },
                    new() { Text = "5", Value = "5" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "6", Value = "6" },
                    new() { Text = "7", Value = "7" },
                    new() { Text = "8", Value = "8" },
                    new() { Text = "9", Value = "9" },
                    new() { Text = "10", Value = "10" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "11", Value = "11" },
                    new() { Text = "12", Value = "12" },
                    new() { Text = "13", Value = "13" },
                    new() { Text = "14", Value = "14" },
                    new() { Text = "15", Value = "15" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "16", Value = "16" },
                    new() { Text = "17", Value = "17" },
                    new() { Text = "18", Value = "18" },
                    new() { Text = "19", Value = "19" },
                    new() { Text = "20", Value = "20" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "Undo", Value = "UNDO" },
                    new() { Text = "Bull(25)", Value = "25" },
                    new() { Text = "Miss", Value = "MISS" },
                    new() { Text = "Del", Value = "DEL" }
                }
            }
        };
        public KeyboardParameters KeyboardParamsEachDartDouble = new()
        {
            KeyboardKeys = new List<List<KeyboardKey>>
            {
                new List<KeyboardKey>
                {
                    new() { Text = "2", Value = "2" },
                    new() { Text = "4", Value = "4" },
                    new() { Text = "6", Value = "6" },
                    new() { Text = "8", Value = "8" },
                    new() { Text = "10", Value = "10" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "12", Value = "12" },
                    new() { Text = "14", Value = "14" },
                    new() { Text = "16", Value = "16" },
                    new() { Text = "18", Value = "18" },
                    new() { Text = "20", Value = "20" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "22", Value = "22" },
                    new() { Text = "24", Value = "24" },
                    new() { Text = "26", Value = "26" },
                    new() { Text = "28", Value = "28" },
                    new() { Text = "30", Value = "30" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "32", Value = "32" },
                    new() { Text = "34", Value = "34" },
                    new() { Text = "36", Value = "36" },
                    new() { Text = "38", Value = "38" },
                    new() { Text = "40", Value = "40" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "Undo", Value = "UNDO" },
                    new() { Text = "Bull(50)", Value = "50" },
                    new() { Text = "Miss", Value = "MISS" },
                    new() { Text = "Del", Value = "DEL" }
                }
            }
        };
        public KeyboardParameters KeyboardParamsEachDartTriple = new()
        {
            KeyboardKeys = new List<List<KeyboardKey>>
            {
                new List<KeyboardKey>
                {
                    new() { Text = "3", Value = "3" },
                    new() { Text = "6", Value = "6" },
                    new() { Text = "9", Value = "9" },
                    new() { Text = "12", Value = "12" },
                    new() { Text = "15", Value = "15" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "18", Value = "18" },
                    new() { Text = "21", Value = "21" },
                    new() { Text = "24", Value = "24" },
                    new() { Text = "27", Value = "27" },
                    new() { Text = "30", Value = "30" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "33", Value = "33" },
                    new() { Text = "36", Value = "36" },
                    new() { Text = "39", Value = "39" },
                    new() { Text = "42", Value = "42" },
                    new() { Text = "45", Value = "45" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "48", Value = "48" },
                    new() { Text = "51", Value = "51" },
                    new() { Text = "54", Value = "54" },
                    new() { Text = "57", Value = "57" },
                    new() { Text = "60", Value = "60" }
                },
                new List<KeyboardKey>
                {
                    new() { Text = "Undo", Value = "UNDO" },
                    new() { Text = "Miss", Value = "MISS" },
                    new() { Text = "Del", Value = "DEL" }
                }
            }
        };
    }
}
