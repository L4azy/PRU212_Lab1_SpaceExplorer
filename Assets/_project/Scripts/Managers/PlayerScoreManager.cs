using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScoreManager : SingletonMonoBehaviour<PlayerScoreManager>
{
	[SerializeField] TMP_InputField _scoreInput;
	[SerializeField] string filename;
	[SerializeField] GameObject _scoreInputPanel;
	[SerializeField] int maxCount;

	List<PlayerScoreInput> entries = new();

	private void Start()
	{
		Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");
		Debug.Log($"High score filename: {filename}");
		entries = FileHandler.ReadListFromJSON<PlayerScoreInput>(filename);

		Debug.Log($"[PlayerScoreManager] Loaded {entries.Count} entries from JSON.");
		foreach (var entry in entries)
		{
			Debug.Log($"[PlayerScoreManager] Player: {entry.playerName}, Points: {entry.points}");
		}

		// Convert PlayerScoreInput to HighScoreElement and raise the event
		var highScoreElements = new List<HighScoreElement>();
		foreach (var entry in entries)
		{
			highScoreElements.Add(new HighScoreElement(entry.playerName, entry.points));
		}

		Debug.Log("[PlayerScoreManager] Raising HighscoreListChangedEvent via EventBus.");
		EventBus.Instance.Raise(new HighscoreListChangedEvent(highScoreElements));

		_scoreInputPanel.SetActive(false);
	}


	public void AddNameToList()
	{
		// Ensure the input field is not empty
		if (string.IsNullOrWhiteSpace(_scoreInput.text))
		{
			//Debug.LogWarning("[PlayerScoreManager] Score input is empty. Cannot add to the list.");
			return;
		}

		// Add the new entry to the list
		var newEntry = new PlayerScoreInput(_scoreInput.text, GameManager.Instance.Score);
		entries.Add(newEntry);

		// Sort the list by points in descending order
		entries.Sort((a, b) => b.points.CompareTo(a.points));
		
		// Trim the list to the maximum number of entries
		if (entries.Count > maxCount)
		{
			int excessCount = entries.Count - maxCount;
			entries.RemoveRange(maxCount, excessCount);
		}

		// Clear the input field
		_scoreInput.text = "";

		// Save the updated list to the JSON file
		FileHandler.SaveToJSON<PlayerScoreInput>(entries, filename);

		// Debug to confirm the save operation
		Debug.Log($"[PlayerScoreManager] Added new entry: {newEntry.playerName} with score {newEntry.points}. Saved to {filename}.");

		// Reload the leaderboard from the JSON file
		entries = FileHandler.ReadListFromJSON<PlayerScoreInput>(filename);

		// Convert PlayerScoreInput to HighScoreElement and raise the event
		var highScoreElements = new List<HighScoreElement>();
		foreach (var entry in entries)
		{
			highScoreElements.Add(new HighScoreElement(entry.playerName, entry.points));
		}

		EventBus.Instance.Raise(new HighscoreListChangedEvent(highScoreElements));
		EventBus.Instance.Raise(new GameStateChangedEvent(GameState.GameOver));

		_scoreInputPanel.SetActive(false);
	}

	public bool PlayerQualifiesForLeaderboard(int playerScore)
	{
		// If the leaderboard has fewer entries than maxCount, the player qualifies
		if (entries.Count < maxCount)
		{
			return true;
		}

		// If the leaderboard is full, check if the player's score is greater than the lowest score
		int lowestScore = entries[entries.Count - 1].points; // The last entry in the sorted list has the lowest score
		return playerScore > lowestScore;
	}

}