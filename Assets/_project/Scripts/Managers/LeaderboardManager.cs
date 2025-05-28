using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
	List<HighScoreElement> _highscoreList = new();
	[SerializeField] int maxCount = 5;
	[SerializeField] string filename;

	private void Awake()
	{
		LoadHighscores();
	}

	private void LoadHighscores()
	{
		_highscoreList = FileHandler.ReadListFromJSON<HighScoreElement>(filename);

		Debug.Log("[LeaderboardManager] Loaded high scores from JSON:");
		foreach (var highScore in _highscoreList)
		{
			Debug.Log($"Player: {highScore.playerName}, Points: {highScore.points}");
		}

		while (_highscoreList.Count > maxCount)
		{
			_highscoreList.RemoveAt(maxCount);
		}

		Debug.Log("[LeaderboardManager] Raising HighscoreListChangedEvent via EventBus.");
		EventBus.Instance.Raise(new HighscoreListChangedEvent(_highscoreList));
	}


	private void SaveHighscore()
	{
		FileHandler.SaveToJSON<HighScoreElement>(_highscoreList, filename);
	}

	public void AddHighscoreIfPossible(HighScoreElement element)
	{
		for (int i = 0; i < maxCount; i++)
		{
			if (i >= _highscoreList.Count || element.points > _highscoreList[i].points)
			{
				// Add new high score
				_highscoreList.Insert(i, element);

				while (_highscoreList.Count > maxCount)
				{
					_highscoreList.RemoveAt(maxCount);
				}

				SaveHighscore();

				Debug.Log("[LeaderboardManager] Raising HighscoreListChangedEvent via EventBus.");
				EventBus.Instance.Raise(new HighscoreListChangedEvent(_highscoreList));

				break;
			}
		}
	}
}
