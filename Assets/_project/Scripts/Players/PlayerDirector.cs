using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDirector : MonoBehaviour
{
	[SerializeField] PlayerShip _playerShip;
	[SerializeField] float _fireDelay = 0.15f, _invulnerabilityDuration = 3f;

	PlayerInputBase _playerInput;
	bool _fireEnabled, _hyperspaceEnabled, _isInvulnerable;
	Timer _enableFireTimer;
	Timer _enableHyperspaceTimer;
	Timer _cancelInvulnerabilityTimer;

	void Awake()
	{
		_playerInput = gameObject.AddComponent<PlayerKeyboardInput>();
	}

	void OnEnable()
	{
		CreateTimers();
		EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
	}

	void OnDisable()
	{
		EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
		ReleaseTimers();
	}

	void Update()
	{
		if (_playerInput == null) return;
		CheckInvulnerability();
		HandleRotationInput();
		HandleThrustInput();
		HandleFireInput();
		HandleHyperspaceInput();
	}

	void CreateTimers()
	{
		_enableFireTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
		_enableHyperspaceTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
		_cancelInvulnerabilityTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
	}

	void ReleaseTimers()
	{
		TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_enableFireTimer);
		TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_enableHyperspaceTimer);
		TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_cancelInvulnerabilityTimer);
	}

	void OnGameStateChanged(GameStateChangedEvent gameStateChangedEvent)
	{
		switch (gameStateChangedEvent.GameState)
		{
			case GameState.StartFirstRound:
				_playerShip.DisableShip();
				break;
			case GameState.ShipSpawned:
				_playerShip.ResetShipToStartPosition();
				_playerShip.EnableRenderer();
				EnableInvulnerability();
				_fireEnabled = true;
				_hyperspaceEnabled = false;
				_enableHyperspaceTimer.OnTimerStop += EnableHyperspace;
				_enableHyperspaceTimer.Start(3f);
				break;
			case GameState.PlayerDied:
			case GameState.GameOver:
				_playerShip.DisableShip();
				break;
		}
	}



	void CheckInvulnerability()
	{
		if (!_isInvulnerable) return;
		if (!_playerInput.AnyInputThisFrame) return;
		CancelInvulnerability();
	}

	void EnableInvulnerability()
	{
		if (_isInvulnerable) return;
		_isInvulnerable = true;
		_cancelInvulnerabilityTimer.OnTimerStop += CancelInvulnerability;
		_cancelInvulnerabilityTimer.Start(_invulnerabilityDuration);
		_playerShip.EnableInvulnerability();
	}

	void CancelInvulnerability()
	{
		if (!_isInvulnerable) return;
		_isInvulnerable = false;
		_cancelInvulnerabilityTimer.OnTimerStop -= CancelInvulnerability;
		_cancelInvulnerabilityTimer.Stop();
		_playerShip.CancelInvulnerability();
	}

	void HandleRotationInput()
	{
		var rotationInput = _playerInput.GetRotationInput();
		if (Mathf.Approximately(rotationInput, 0f)) return;
		_playerShip.Rotate(rotationInput);
	}

	void HandleThrustInput()
	{
		_playerShip.SetThrust(_playerInput.GetThrustInput());
	}

	void EnableFire()
	{
		_fireEnabled = true;
		_enableFireTimer.OnTimerStop -= EnableFire;
		_enableFireTimer.Stop();
	}

	void DisableFire()
	{
		_fireEnabled = false;
		_enableFireTimer.OnTimerStop += EnableFire;
		_enableFireTimer.Start(_fireDelay);
	}

	void HandleFireInput()
	{
		if (!_fireEnabled) return;
		if (!_playerInput.GetFireInput()) return;
		//Debug.Log("Fired bullets");
		DisableFire();
		_playerShip.FireBullet();
	}

	void EnableHyperspace()
	{
		_hyperspaceEnabled = true;
		_enableHyperspaceTimer.OnTimerStop -= EnableHyperspace;
		_enableFireTimer.Stop();
	}

	void DisableHyperspace()
	{
		_hyperspaceEnabled = false;
		_enableHyperspaceTimer.OnTimerStop += EnableHyperspace;
		_enableHyperspaceTimer.Start(3f);
	}

	void HandleHyperspaceInput()
	{
		if (!_hyperspaceEnabled) return;
		if (!_playerInput.GetHyperspaceInput()) return;
		DisableHyperspace();
		_playerShip.EnterHyperspace();
	}
}
