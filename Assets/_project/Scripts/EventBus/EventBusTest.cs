using System.Collections.Generic;
using UnityEngine;

public class EventBusTest : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to the event
        EventBus.Instance.Subscribe<HighscoreListChangedEvent>(OnHighscoreListChanged);

        // Simulate raising the event
        var testHighScores = new List<HighScoreElement>
        {
            new HighScoreElement("Player1", 100),
            new HighScoreElement("Player2", 200),
            new HighScoreElement("Player3", 300)
        };

        Debug.Log("[EventBusTest] Raising HighscoreListChangedEvent with test data.");
        EventBus.Instance.Raise(new HighscoreListChangedEvent(testHighScores));
    }

    private void OnHighscoreListChanged(HighscoreListChangedEvent eventArgs)
    {
        Debug.Log($"[EventBusTest] Received HighscoreListChangedEvent with {eventArgs.HighscoreList.Count} elements.");
        foreach (var highScore in eventArgs.HighscoreList)
        {
            Debug.Log($"Player: {highScore.playerName}, Points: {highScore.points}");
        }
    }
}
