using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GhostSpawner : SingletonMonoBehaviour<GhostSpawner>
{
	[SerializeField] int _initPoolSize = 8, _maxPoolSize = 8;
	[SerializeField] Transform _parentTransform;

	readonly Dictionary<string, IObjectPool<Ghost>> _ghostPools = new();
	Ghost _prefab;
	readonly Vector3 _offScreen = new(999, 999, 0);

	IObjectPool<Ghost> GetPool(Ghost ghostPrefab)
	{
		if (_ghostPools.TryGetValue(ghostPrefab.name, out var pool)) return pool;
		pool = GetPool();
		_ghostPools.Add(ghostPrefab.name, pool);
		return pool;
	}

	IObjectPool<Ghost> GetPool()
	{
		var pool = new ObjectPool<Ghost>(
			InstantiateGhost, OnTakeGhostFromPool, OnReturnGhostToPool, OnDestroyGhost,
			collectionCheck: true, _initPoolSize, _maxPoolSize);

		return pool;
	}

	Ghost InstantiateGhost()
	{
		var ghost = Instantiate(_prefab, _parentTransform);
		ghost.gameObject.SetActive(false);
		ghost.transform.position = Vector3.zero;
		ghost.name = _prefab.name;
		return ghost;
	}

	void OnTakeGhostFromPool(Ghost ghost)
	{
		ghost?.gameObject.SetActive(false);
		ghost.transform.position = Vector3.zero;
	}

	void OnReturnGhostToPool(Ghost ghost)
	{
		if (!ghost) return;
		ghost.gameObject.SetActive(false);
		ghost.transform.position = Vector3.zero;
	}

	void OnDestroyGhost(Ghost ghost)
		=> Destroy(ghost.gameObject);

	public Ghost SpawnGhost(GhostParent ghostParent, Ghost.GhostPosition ghostPos)
	{
		_prefab = ghostParent.GhostPrefab;
		var ghost = GetPool(_prefab).Get();
		ghost.Init(ghostParent, ghostPos);
		return ghost;
	}

	public void ReleaseGhost(Ghost ghost)
	{
		if (!ghost) return;
		ghost.gameObject.SetActive(false);
		ghost.transform.position = _offScreen;
		GetPool(ghost).Release(ghost);
	}

}

