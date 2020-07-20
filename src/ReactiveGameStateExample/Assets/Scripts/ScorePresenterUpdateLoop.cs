using UnityEngine;
using UnityEngine.UI;

public sealed class ScorePresenterUpdateLoop : MonoBehaviour
{
    [SerializeField] private Text scoreLabel;
    [SerializeField] private CurrentGameState gameState;

    void Update() => scoreLabel.text = $"Score: {gameState.Current.Score}";
}
