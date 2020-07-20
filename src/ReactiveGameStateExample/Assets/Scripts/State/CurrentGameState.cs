using System;
using UnityEngine;

[CreateAssetMenu(menuName = "OnlyOnce/CurrentGameState")]
public sealed class CurrentGameState : ScriptableObject
{
    [SerializeField] private GameState state = new GameState();

    public GameState Current => state;
    public void Init() => Init(new GameState());
    public void Init(GameState initialState) => state = initialState;

    public void AddScore(int amount) => UpdateState(g => g.Score += amount);

    public void UpdateState(Action<GameState> apply)
    {
        UpdateState(_ =>
        {
            apply(state);
            return state;
        });
    }

    public void UpdateState(Func<GameState, GameState> apply)
    {
        state = apply(state);
        Message.Publish(new GameStateChanged(state));
    }
}
