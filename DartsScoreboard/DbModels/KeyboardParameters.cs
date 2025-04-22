namespace DartsScoreboard;

public class KeyboardParameters
{
    public List<List<KeyboardKey>> KeyboardKeys { get; set; }
}
public class KeyboardKey
{
    public required string Text { get; set; }
    public required string Value { get; set; }
}
