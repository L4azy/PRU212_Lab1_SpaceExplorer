using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GhostParent : MonoBehaviour
{
	[SerializeField] Ghost _ghostPrefab = null;
	[SerializeField] float _swapDelay = 2f;

	public Ghost GhostPrefab => _ghostPrefab;

	public void EnableGhosts(bool enable = true)
	{
		foreach (var ghost in _ghosts)
			ghost.gameObject.SetActive(enable);
	}

	readonly List<Ghost> _ghosts = new();
	Camera _mainCam;
	Transform _transform;
	Renderer _renderer;
	Collider2D _collider;
	Timer _swapXTimer, _swapYTimer;

	bool CanSwapX { get; set; }
	bool CanSwapY { get; set; }

	private void Awake()
	{
		_mainCam = Camera.main;
		_transform = transform;
		_renderer = GetComponent<Renderer>();
		_collider = GetComponent<Collider2D>();
	}

	void OnEnable()
	{
		CreateGhosts();
		CanSwapX = CanSwapY = true;
		CreateTimers();
		EnableComponents();
	}

	void Update()
		=> HandleScreenWrap();

	void OnDisable()
	{
		ReleaseGhosts();
		DisableComponents();
		ReleaseTimers();
	}

	void CreateGhosts()
	{
		foreach (Ghost.GhostPosition pos in Enum.GetValues(typeof(Ghost.GhostPosition)))
		{
			var ghost = GhostSpawner.Instance.SpawnGhost(this, pos);
			_ghosts.Add(ghost);
		}
	}

	void ReleaseGhosts()
	{
		foreach (var ghost in _ghosts)
		{
			GhostSpawner.Instance?.ReleaseGhost(ghost);
		}

		_ghosts?.Clear();
	}

	void CreateTimers()
	{
		_swapXTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
		_swapYTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
		_swapXTimer.OnTimerStop += OnSwapXTimerStop;
		_swapYTimer.OnTimerStop += OnSwapYTimerStop;
	}

	void ReleaseTimers()
	{
		_swapXTimer.OnTimerStop -= OnSwapXTimerStop;
		_swapYTimer.OnTimerStop -= OnSwapYTimerStop;
		TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_swapXTimer);
		TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_swapYTimer);
	}

	void OnSwapXTimerStop()
		=> CanSwapX = true;

	void OnSwapYTimerStop()
		=> CanSwapY = true;

	void EnableComponents()
	{
		_collider.enabled = true;
		_renderer.enabled = true;
	}

	void DisableComponents()
	{
		_collider.enabled = false;
		_renderer.enabled = false;
	}

	// Check if player is on screen
	void HandleScreenWrap()
	{
		if (ViewportHelper.Instance.IsOnScreen(_transform)) return;
		SwapWithGhost();
	}

	// Determine which ghost to swap with
	private void SwapWithGhost()
	{
		var viewportPos = _mainCam.WorldToViewportPoint(_transform.position);
		var newPos = _transform.position;

		if (CanSwapX && viewportPos.x is > 1 or < 0)
		{
			var ghostPos = viewportPos.x > 1
				? Ghost.GhostPosition.MiddleLeft
				: Ghost.GhostPosition.MiddleRight;

			var ghost = _ghosts.SafeGetByIndex((int)ghostPos);
			if (ghost == null) return;

			newPos.x = ghost.transform.position.x;
			CanSwapX = false;
			_swapXTimer.Start(_swapDelay);
		}

		if (CanSwapY && viewportPos.y is > 1 or < 0)
		{
			var ghostPos = viewportPos.y > 1
				? Ghost.GhostPosition.LowerMiddle
				: Ghost.GhostPosition.UpperMiddle;

			var ghost = _ghosts.SafeGetByIndex((int)ghostPos);
			if (ghost == null) return;

			newPos.y = ghost.transform.position.y;
			CanSwapY = false;
			_swapYTimer.Start(_swapDelay);
		}

		_transform.position = newPos;
	}
}
