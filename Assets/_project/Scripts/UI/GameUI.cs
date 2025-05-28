using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static HighscoreListChangedEvent;

public class GameUI : MonoBehaviour
{
	[SerializeField] TMP_Text _scoreText, _highScoreText, _gameOverText, _playAgainText;
	//[SerializeField] Button _playAgainButton;
	[SerializeField] Transform _playerLivesParent;
	[SerializeField] UIButton _settingsButton;

	[SerializeField] TMP_InputField _playerNameInput;
	[SerializeField] GameObject _leaderboardPanel;
	[SerializeField] TMP_Text _leaderboardText;

	string _leaderboardFilename = "leaderboard.json";

	Timer _showPlayAgainPromptTimer;

	void OnEnable()
	{
		if (EventBus.Instance == null)
		{
			Debug.LogWarning("[GameUI] EventBus.Instance is null in OnEnable. Skipping subscription.");
			return;
		}

		EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
		EventBus.Instance.Subscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
		EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
		EventBus.Instance.Subscribe<LeaderboardUpdatedEvent>(OnLeaderboardUpdated);
		_settingsButton.Init(LoadSettingsScene);
		UpdatePlayerLives(3);
		_gameOverText.enabled = false;
		_playAgainText.enabled = false;
		_showPlayAgainPromptTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
	}


	void Start()
	{
		DisplayLowestLeaderboardScore();
	}

	void OnDisable()
	{
		if (EventBus.Instance == null)
		{
			Debug.LogWarning("[GameUI] EventBus.Instance is null in OnDisable. Skipping unsubscription.");
			return;
		}

		EventBus.Instance.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
		EventBus.Instance.Unsubscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
		EventBus.Instance.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
		EventBus.Instance.Unsubscribe<LeaderboardUpdatedEvent>(OnLeaderboardUpdated);
		_showPlayAgainPromptTimer.OnTimerStop -= ShowPlayAgainPrompt;
		TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_showPlayAgainPromptTimer);
	}


	void DisplayLowestLeaderboardScore()
	{
		// Use FileHandler to read the leaderboard JSON
		List<HighScoreElement> scores = FileHandler.ReadListFromJSON<HighScoreElement>(_leaderboardFilename);

		if (scores == null || scores.Count == 0)
		{
			Debug.LogWarning("[GameUI] No scores found in the leaderboard.");
			_highScoreText.text = "N/A";
			return;
		}

		// Find the lowest score
		int minScore = int.MaxValue;
		foreach (var score in scores)
		{
			if (score.points < minScore)
			{
				minScore = score.points;
			}
		}

		// Update the high score text
		_highScoreText.text = minScore.ToString();
		Debug.Log($"[GameUI] Lowest score in leaderboard: {minScore}");
	}

	void Update()
	{
		if (!_playAgainText.enabled) return;
		var keyboard = Keyboard.current;
		if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
		{
			RestartGame();
		}
	}

	void RestartGame()
	{
		GameManager.Instance.RestartGame();
	}

	void OnGameStateChanged(GameStateChangedEvent gameStateChangedEvent)
	{
		if (gameStateChangedEvent.GameState == GameState.GameOver)
		{
			_gameOverText.enabled = true;
			_showPlayAgainPromptTimer.OnTimerStop += ShowPlayAgainPrompt;
			_showPlayAgainPromptTimer.Start(3f);
			DisplayLowestLeaderboardScore();
			return;
		}
		_showPlayAgainPromptTimer.OnTimerStop -= ShowPlayAgainPrompt;
		_showPlayAgainPromptTimer.Stop();
		_gameOverText.enabled = false;
		_playAgainText.enabled = false;
		//_playAgainButton.gameObject.SetActive(false);
	}

	void LoadSettingsScene()
	{
		EventBus.Instance.Subscribe<SettingsSceneClosedEvent>(ResumeGame);
		GameManager.Instance.PauseGame();
		SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
	}

	void ResumeGame(SettingsSceneClosedEvent _)
	{
		EventBus.Instance.Unsubscribe<SettingsSceneClosedEvent>(ResumeGame);
		GameManager.Instance.ResumeGame();
	}

	void ShowPlayAgainPrompt()
	{
		_playAgainText.enabled = true;
	}

	void OnScoreChanged(ScoreChangedEvent scoreChangedEvent)
	{
		// Safely parse the high score text or default to 0 if parsing fails
		int highScore;
		if (!int.TryParse(_highScoreText.text, out highScore))
		{
			Debug.LogWarning($"[GameUI] Failed to parse _highScoreText.text: {_highScoreText.text}. Defaulting to 0.");
			highScore = 0;
		}

		// Update the score and high score
		UpdateScore(scoreChangedEvent.Score, highScore);
	}


	void OnPlayerLivesChanged(PlayerLivesChangedEvent playerLivesChangedEvent)
	{
		UpdatePlayerLives(playerLivesChangedEvent.Lives);

		if (playerLivesChangedEvent.Lives <= 0)			EventBus.Instance.Raise(new GameStateChangedEvent(GameState.GameOver)); // Trigger game over sequence
	}


	void UpdateScore(int score, int highScore)
	{
		_scoreText.text = score.ToString();
		_highScoreText.text = highScore.ToString();
	}

	void UpdatePlayerLives(int lives)
	{
		for (var i = 0; i < _playerLivesParent.childCount; i++)
		{
			_playerLivesParent.GetChild(i).gameObject.SetActive(i < lives);
		}
	}
	void OnLeaderboardUpdated(LeaderboardUpdatedEvent _)
	{
		DisplayLowestLeaderboardScore();
	}
}
