using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class LeaderboardEntry
{
	public string playerName;
	public int score;
}

[Serializable]
public class LeaderboardData
{
	public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class LeaderboardManager : MonoBehaviour
{
	public static LeaderboardManager Instance { get; private set; }
	private const int MaxEntries = 5;
	private string _savePath;
	private LeaderboardData _data = new LeaderboardData();

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			_savePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
			Load();
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public List<LeaderboardEntry> GetEntries() => _data.entries;

	public void AddEntry(string playerName, int score)
	{
		_data.entries.Add(new LeaderboardEntry { playerName = playerName, score = score });
		_data.entries.Sort((a, b) => b.score.CompareTo(a.score));
		if (_data.entries.Count > MaxEntries)
			_data.entries.RemoveAt(_data.entries.Count - 1);
		Save();
	}

	private void Save()
	{
		File.WriteAllText(_savePath, JsonUtility.ToJson(_data, true));
	}

	private void Load()
	{
		if (File.Exists(_savePath))
		{
			_data = JsonUtility.FromJson<LeaderboardData>(File.ReadAllText(_savePath));
		}
	}

	public void ClearLeaderboard()
	{
		_data.entries.Clear();
		if (File.Exists(_savePath))
			File.Delete(_savePath);
	}

}
