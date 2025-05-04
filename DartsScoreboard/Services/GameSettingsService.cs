namespace DartsScoreboard.Services
{
    public class GameSettingsService
    {
        public int StartingScore { get; set; } = 501;
        public string StartInOption { get; set; } = "STRAIGHT IN";
        public string EndInOption { get; set; } = "DOUBLE OUT";
        public void SetGameOptions(int score, string startIn, string endOut)
        {
            StartingScore = score;
            StartInOption = startIn;
            EndInOption = endOut;
        }
    }
}
