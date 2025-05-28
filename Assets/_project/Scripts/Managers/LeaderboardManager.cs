using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HighscoreListChangedEvent;

public class LeaderboardManager : MonoBehaviour
{
	private List<HighScoreElement> _highscoreList = new();
	[SerializeField] private int maxCount;
	[SerializeField] private string filename;

	public IReadOnlyList<HighScoreElement> HighscoreList => _highscoreList.ToList();


	private void Awake()
	{
		Debug.Log("[LeaderboardManager] Awake called.");
		LoadHighscores();
	}

	private void LoadHighscores()
	{
		Debug.Log("[LeaderboardManager] LoadHighscores called.");
		try
		{
			_highscoreList = FileHandler.ReadListFromJSON<HighScoreElement>(filename);
		}
		catch (Exception ex)
		{
			Debug.LogError($"[LeaderboardManager] Exception during LoadHighscores: {ex.Message}");
			_highscoreList = new List<HighScoreElement>();
		}

		Debug.Log($"[LeaderboardManager] Loaded {_highscoreList.Count} high scores from JSON.");
		foreach (var highScore in _highscoreList)
		{
			Debug.Log($"Player: {highScore.playerName}, Points: {highScore.points}");
		}

		TrimHighscores();
		SaveHighscore();
		RaiseHighscoreListChangedEvent();
		EventBus.Instance.Raise(new LeaderboardUpdatedEvent());
	}



	private void SaveHighscore()
	{
		FileHandler.SaveToJSON<HighScoreElement>(_highscoreList, filename);
		Debug.Log("[LeaderboardManager] Saved high scores to JSON.");
	}

	public void AddHighscoreIfPossible(HighScoreElement element)
	{
		for (int i = 0; i < maxCount; i++)
		{
			if (i >= _highscoreList.Count || element.points > _highscoreList[i].points)
			{
				_highscoreList.Insert(i, element);

				while (_highscoreList.Count > maxCount)
				{
					_highscoreList.RemoveAt(maxCount);
				}

				SaveHighscore();

				Debug.Log("[LeaderboardManager] Raising LeaderboardUpdatedEvent via EventBus.");
				EventBus.Instance.Raise(new LeaderboardUpdatedEvent());

				break;
			}
		}
	}


	private void TrimHighscores()
	{
		Debug.Log("[LeaderboardManager] TrimHighscores called.");
		Debug.Log($"[LeaderboardManager] Initial list count: {_highscoreList.Count}, maxCount: {maxCount}");

		// Sort the list in descending order
		_highscoreList.Sort((a, b) => b.points.CompareTo(a.points));

		Debug.Log($"[LeaderboardManager] Trimming highscore list. Initial count: {_highscoreList.Count}");

		// Trim the list to maxCount
		if (_highscoreList.Count > maxCount)
		{
			_highscoreList.RemoveRange(maxCount, _highscoreList.Count - maxCount);
			Debug.Log($"[LeaderboardManager] Highscore list trimmed to {_highscoreList.Count} entries.");
		}
		else
		{
			Debug.Log("[LeaderboardManager] No trimming needed.");
		}
	}


	private void RaiseHighscoreListChangedEvent()
	{
		Debug.Log($"[LeaderboardManager] Raising HighscoreListChangedEvent with {HighscoreList.Count} entries.");
		EventBus.Instance.Raise(new HighscoreListChangedEvent(HighscoreList.ToList()));
	}

}
