using Microsoft.AspNetCore.Components;

namespace DartsScoreboard
{
    public partial class GamesStandard
    {
        public int SelectedScore { get; set; } = 501;

        public List<int> StartingScoreOptions { get; set; } = new() { 301, 501, 701 };

        private void SelectScore(int score)
        {
            SelectedScore = score;
        }
        [Inject] NavigationManager NavManager { get; set; } = default!;
        private void StartGame()
        {
            NavManager.NavigateTo($"/gameStandardPlay/{SelectedScore}");
        }
    }
}
