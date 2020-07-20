using UnityEngine;
using UnityEngine.UI;

public sealed class ScorePresenterReactive : OnMessage<GameStateChanged>
{
    [SerializeField] private Text scoreLabel;

    protected override void Execute(GameStateChanged msg) => scoreLabel.text = $"Score: {msg.State.Score}";
}
