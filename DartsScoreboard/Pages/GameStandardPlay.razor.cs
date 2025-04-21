using Microsoft.AspNetCore.Components;

namespace DartsScoreboard
{
    public partial class GameStandardPlay
    {
        [Parameter]
        public int StartingScore { get; set; }
    }
}
