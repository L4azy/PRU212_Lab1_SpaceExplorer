using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
#endif
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class AsteroidSpawner : MonoBehaviour
{
	[Header("Asteroid Settings")]
	[SerializeField] Asteroid[] _smallAsteroidPrefabs;
	[SerializeField] Asteroid[] _mediumAsteroidPrefabs;
	[SerializeField] Asteroid[] _largeAsteroidPrefabs;
	[SerializeField] GameObject _explosionPrefab;
	[SerializeField] int _asteroidsToSpawn = 4, _maxAsteroids = 10;
	[SerializeField] float _minSpawnDistanceFromPlayer = 2f;

	[Header("Star Settings")]
	[SerializeField] GameObject _starPrefab;
	[SerializeField] int _starsToSpawn = 3;

	readonly Dictionary<AsteroidSize, IObjectPool<Asteroid>> _asteroidPools = new();
	readonly List<Asteroid> _asteroids = new();
	Transform _transform;
	int SpawnCount => Math.Min(_asteroidsToSpawn + GameManager.Instance.Round - 1, _maxAsteroids);

	public void DestroyAsteroid(Asteroid asteroid, Vector3 position)
	{

		//Debug.Log($"DestroyAsteroid called for {asteroid.name} at position {position}");
		ExplosionSpawner.Instance.SpawnExplosion(position);
		SplitAsteroid(asteroid);
		ReleaseAsteroidToPool(asteroid);

		if (asteroid.Settings.Size == AsteroidSize.Large)
		{
			SpawnStars(position, _starsToSpawn);
		}

		if (_asteroids.Count == 0)
		{
			GameManager.Instance.RoundOver();
		}
	}

	void Awake()
	{
		_transform = transform;
		EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
	}

	void OnDestroy()
	{
		EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
	}

	void OnGameStateChanged(GameStateChangedEvent gameState)
	{
		if (gameState.GameState == GameState.StartFirstRound)
		{
			ReleaseAllAsteroids();
		}
		if (gameState.GameState is GameState.StartFirstRound or GameState.StartRound)			SpawnAsteroids();
	}

	//[ContextMenu("Spawn Asteroids")]
	void SpawnAsteroids()
	{
		_asteroids.Clear();
		var pool = GetPool(AsteroidSize.Large);
		for (var i = 0; i < SpawnCount; i++)
		{
			var asteroid = pool.Get();
			if (!asteroid) continue;
			var spawnPoint = GetRandomSpawnPoint();
			asteroid.Initialize(this, spawnPoint);
			_asteroids.Add(asteroid);
		}
	}

	void ReleaseAllAsteroids()
	{
		foreach (var asteroid in _asteroids)
		{
			asteroid.gameObject.SetActive(false);
			GetPool(asteroid.Settings.Size).Release(asteroid);
		}
		_asteroids.Clear();
	}

	Vector3 GetRandomSpawnPoint()
	{
		var playerPos = GameManager.Instance.PlayerShip?.transform.position ?? Vector3.zero;
		Vector3 spawnPoint;
		do
		{
			spawnPoint = ViewportHelper.Instance.GetRandomVisiblePosition();
		} while (Vector3.Distance(spawnPoint, playerPos) < _minSpawnDistanceFromPlayer);

		return spawnPoint;
	}

	void SplitAsteroid(Asteroid asteroid)
	{
		if (asteroid.Settings.Size == AsteroidSize.Small) return;
		var pool = GetPool(asteroid.Settings.Size - 1);
		for (var i = 0; i < 2; i++)
		{
			var newAsteroid = pool.Get();
			if (!newAsteroid) continue;
			newAsteroid.Initialize(this, asteroid.transform.position);
			_asteroids.Add(newAsteroid);
		}
	}

	void SpawnStars(Vector3 position, int count)
	{
		for (int i = 0; i < count; i++)
		{
			GameObject star = Instantiate(_starPrefab, position, Quaternion.identity);
			Rigidbody2D rb = star.GetComponent<Rigidbody2D>();
			if (rb != null)
			{
				// Give each star a gentle random drift in any direction
				rb.gravityScale = 0f; // Ensure no gravity
				rb.linearVelocity = Random.insideUnitCircle.normalized * Random.Range(0.5f, 1.5f);
			}
		}
	}


	void ReleaseAsteroidToPool(Asteroid asteroid)
	{
		asteroid.gameObject.SetActive(false);
		var index = _asteroids.IndexOf(asteroid);
		_asteroids.RemoveAt(index);
		GetPool(asteroid.Settings.Size).Release(asteroid);
	}

	IObjectPool<Asteroid> GetPool(AsteroidSize size)
	{
		if (_asteroidPools.TryGetValue(size, out var pool)) return pool;
		pool = new ObjectPool<Asteroid>(
			() => InstantiateAsteroid(size), OnTakeAsteroidFromPool, OnReturnAsteroidToPool, OnDestroyAsteroid,
			collectionCheck: true, 20, 100);
		_asteroidPools.Add(size, pool);
		return pool;
	}

	Asteroid InstantiateAsteroid(AsteroidSize size)
	{
		var prefab = GetRandomPrefab(size);
		if (!prefab)
		{
			//Debug.LogError("Asteroid prefab is null.", this);
			return null;
		}
		var asteroid = Instantiate(prefab, _transform);
		asteroid.gameObject.SetActive(false);
		return asteroid;
	}

	void OnTakeAsteroidFromPool(Asteroid asteroid)
	{
	}

	void OnReturnAsteroidToPool(Asteroid asteroid)
	{
		asteroid.gameObject.SetActive(false);
	}

	void OnDestroyAsteroid(Asteroid asteroid)
	{
		Destroy(asteroid);
	}

	Asteroid GetRandomPrefab(AsteroidSize size)
	{
		return size switch
		{
			AsteroidSize.Small => _smallAsteroidPrefabs[Random.Range(0, _smallAsteroidPrefabs.Length)],
			AsteroidSize.Medium => _mediumAsteroidPrefabs[Random.Range(0, _mediumAsteroidPrefabs.Length)],
			AsteroidSize.Large => _largeAsteroidPrefabs[Random.Range(0, _largeAsteroidPrefabs.Length)],
			_ => throw new ArgumentOutOfRangeException()
		};
	}

//#if UNITY_EDITOR
//	void Update()
//	{
//		if (!Mouse.current.leftButton.wasPressedThisFrame) return;
//		DestroyAllAsteroid();
//	}

//	[ContextMenu("Destroy all asteroids")]
//	public void DestroyAllAsteroid()
//	{
//		var asteroids = _asteroids.ToList();
//		foreach (var asteroid in asteroids)
//		{
//			DestroyAsteroid(asteroid, asteroid.transform.position);
//		}
//	}
//#endif
}
