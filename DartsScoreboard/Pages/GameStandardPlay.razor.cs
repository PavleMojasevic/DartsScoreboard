using Microsoft.AspNetCore.Components;

namespace DartsScoreboard
{
    public partial class GameStandardPlay
    {
        [Parameter]
        public string GameOptionsOutput { get; set; }
        public string DecodedOptions => Uri.UnescapeDataString(GameOptionsOutput);
    }
}
