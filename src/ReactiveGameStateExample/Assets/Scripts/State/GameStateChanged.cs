public sealed class GameStateChanged
{
    public GameState State { get; }

    public GameStateChanged(GameState s) => State = s;
}
