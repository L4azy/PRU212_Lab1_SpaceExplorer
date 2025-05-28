using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static HighscoreListChangedEvent;

public class LeaderboardUI : MonoBehaviour
{
	[SerializeField] private GameObject _panel;
	[SerializeField] private GameObject _leaderboardUIElementPrefab;
	[SerializeField] private Transform _elementWrapper;

	private readonly List<GameObject> _uiElements = new();

	private void OnEnable()
	{
		Debug.Log("[LeaderboardUI] Subscribing to HighscoreListChangedEvent via EventBus.");
		EventBus.Instance.Subscribe<HighscoreListChangedEvent>(OnHighscoreListChanged);
		EventBus.Instance.Subscribe<LeaderboardUpdatedEvent>(OnLeaderboardUpdated);
	}

	private void OnDisable()
	{
		if (EventBus.Instance == null)
		{
			Debug.LogWarning("[LeaderboardUI] EventBus.Instance is null in OnDisable. Skipping unsubscription.");
			return;
		}

		Debug.Log("[LeaderboardUI] Unsubscribing from HighscoreListChangedEvent via EventBus.");
		EventBus.Instance.Unsubscribe<HighscoreListChangedEvent>(OnHighscoreListChanged);
		EventBus.Instance.Unsubscribe<LeaderboardUpdatedEvent>(OnLeaderboardUpdated);
	}

	private void OnHighscoreListChanged(HighscoreListChangedEvent eventArgs)
	{
		Debug.Log($"[LeaderboardUI] Received HighscoreListChangedEvent with {eventArgs.HighscoreList.Count} elements.");
		UpdateUI(eventArgs.HighscoreList);
	}

	private void OnLeaderboardUpdated(LeaderboardUpdatedEvent _)
	{
		Debug.Log("[LeaderboardUI] Received LeaderboardUpdatedEvent. Refreshing leaderboard.");
		// Re-read the leaderboard data and update the UI
		List<HighScoreElement> highscoreList = FileHandler.ReadListFromJSON<HighScoreElement>("leaderboard.json");
		UpdateUI(highscoreList);
	}

	public void ShowPanel()
	{
		_panel.SetActive(true);
	}

	public void ClosePanel()
	{
		_panel.SetActive(false);
	}

	private void UpdateUI(List<HighScoreElement> highscoreList)
	{
		Debug.Log($"[LeaderboardUI] UpdateUI called with {highscoreList.Count} elements.");

		// Clear existing UI elements
		foreach (var element in _uiElements)
		{
			Destroy(element);
		}
		_uiElements.Clear();

		// Populate the leaderboard UI with the provided highscore list
		for (int i = 0; i < highscoreList.Count; i++)
		{
			HighScoreElement highScore = highscoreList[i];
			Debug.Log($"[LeaderboardUI] Adding element {i}: {highScore.playerName}, {highScore.points}");

			// Instantiate a new leaderboard entry
			var instance = Instantiate(_leaderboardUIElementPrefab, _elementWrapper);
			var texts = instance.GetComponentsInChildren<Text>();

			if (texts.Length >= 2)
			{
				texts[0].text = highScore.playerName; // Player name
				texts[1].text = highScore.points.ToString(); // Player points
			}
			else
			{
				Debug.LogError("[LeaderboardUI] Prefab does not have enough Text components to display player name and points.");
			}

			_uiElements.Add(instance);
		}
	}
}
