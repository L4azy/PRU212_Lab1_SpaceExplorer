using UnityEngine;
using UnityEngine.VFX;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
	[SerializeField] float _spawnShipDelayTime = 2f;
	[SerializeField] int _pointsForExtraLife = 10000;
	public int Round { get; private set; }
	public int Score { get; private set; }
	public int HighScore { get; private set; }
	public PlayerShip PlayerShip => _playerShip;

	GameState _gameState = GameState.StartGame;
	int Lives { get; set; }
	int _nextExtraLifeScore;
	PlayerShip _playerShip;
	Timer _nextRoundTimer, _spawnShipTimer;

	public void RestartGame()
	{
		StartGame();
	}

	protected override void Awake()
	{
		base.Awake();
		_playerShip = FindObjectOfType<PlayerShip>();
	}

	void Start()
	{
		HighScore = SettingsManager.Instance.GetSetting<int>("HighScore", 0);
		StartGame();
	}

	void StartFirstRound()
	{
		CreateTimers();
		++Round;
		AddPoints(0);
		SetGameState(GameState.StartFirstRound);
		StartSpawnShipTimer();
	}

	public void RoundOver()
	{
		SetGameState(GameState.RoundOver);
		StartNextRoundTimer();
	}

	public void PlayerDied()
	{
		Debug.Log("Player died");
		EventBus.Instance.Raise(new StopAllMusicEvent());
		if (Lives > 0)
		{
			SetGameState(GameState.PlayerDied);
			StartSpawnShipTimer();
			return;
		}
		GameOver();
	}

	void StartNextRoundTimer()
	{
		_nextRoundTimer.Start(3f);
	}

	void StartSpawnShipTimer()
	{
		_spawnShipTimer.Start(_spawnShipDelayTime);
	}

	void GameOver()
	{
		ReleaseTimers();
		SetGameState(GameState.GameOver);
		EventBus.Instance.Raise(new PlayMusicEvent("GameOver"));
		SettingsManager.Instance.SetSetting("HighScore", HighScore.ToString(), true);
	}

	public int GetFinalScore() => Score;

	public void AddPoints(int points)
	{
		Score += points;
		if (Score > HighScore) HighScore = Score;
		Debug.Log($"Score: {Score}, HighScore: {HighScore}");
		EventBus.Instance.Raise(new ScoreChangedEvent(Score, HighScore));
		CheckForExtraLife();
	}

	public void PauseGame()
	{
		Time.timeScale = 0;
	}

	public void ResumeGame()
	{
		Time.timeScale = 1;
	}

	void CheckForExtraLife()
	{
		if (Score < _nextExtraLifeScore) return;
		_nextExtraLifeScore += _pointsForExtraLife;
		SfxManager.Instance.PlayClip(SoundEffectsClip.ExtraLife);
		EventBus.Instance.Raise(new PlayerLivesChangedEvent(++Lives));
	}

	void CreateTimers()
	{
		_nextRoundTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
		_spawnShipTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
		_nextRoundTimer.OnTimerStop += StartNextRound;
		_spawnShipTimer.OnTimerStop += SpawnShip;
	}

	void ReleaseTimers()
	{
		_nextRoundTimer.OnTimerStop -= StartNextRound;
		_spawnShipTimer.OnTimerStop -= SpawnShip;
		TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_nextRoundTimer);
		TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_spawnShipTimer);
	}

	void StartNextRound()
	{
		++Round;
		SetGameState(GameState.StartRound);
	}

	void StartGame()
	{
		Lives = 3;
		Score = 0;
		Round = 0;
		_nextExtraLifeScore = _pointsForExtraLife;
		StartFirstRound();
	}

	void SpawnShip()
	{
		Debug.Log("Spawning ship");
		EventBus.Instance.Raise(new PlayerLivesChangedEvent(--Lives));
		SetGameState(GameState.ShipSpawned);
		EventBus.Instance.Raise(new PlayMusicEvent("Game"));
	}

	void SetGameState(GameState gameState)
	{
		_gameState = gameState;
		EventBus.Instance.Raise(new GameStateChangedEvent(_gameState));
	}
}

public enum GameState
{
	StartGame,
	StartFirstRound,
	StartRound,
	ShipSpawned,
	PlayerDied,
	RoundOver,
	GameOver,
}
