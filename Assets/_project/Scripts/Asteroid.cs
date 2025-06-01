using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Asteroid : MonoBehaviour, ICollisionParent, IScoreable
{
	[SerializeField] AsteroidSettings _settings;

	[SerializeField] Color _damageColor = Color.yellow;
	[SerializeField] float _flashDuration = 0.1f;
	Color _originalColor;
	Coroutine _flashCoroutine;

	public AsteroidSettings Settings => _settings;

	Rigidbody2D _rigidBody;
	Transform _transform;
	bool _destroyed;
	AsteroidSpawner _asteroidSpawner;
	PolygonCollider2D _collider;
	Renderer _renderer;

	public void Initialize(AsteroidSpawner asteroidSpawner, Vector3 position)
	{
		//Debug.Log($"Asteroid spawned at {_transform.position}, moving to {position}");
		_asteroidSpawner = asteroidSpawner;
		_transform.position = position;
		_destroyed = false;
		EnableAndApplyForce();
	}

	public void Disable()
	{
		_collider.enabled = false;
		_renderer.enabled = false;
		_rigidBody.linearVelocity = Vector2.zero;
		_rigidBody.angularVelocity = 0f;
		gameObject.SetActive(false);
	}

	public void Collided(Collision2D collision)
	{
		
	}


	public int PointValue => _settings.Points;

	void Awake()
	{
		_transform = transform;
		_rigidBody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<PolygonCollider2D>();
		_renderer = GetComponent<Renderer>();
		if (_renderer != null)
			_originalColor = _renderer.material.color;
	}

	void OnEnable() => DisableComponents();

	void OnDisable() => DisableComponents();

	void OnCollisionEnter2D(Collision2D other) => Collided(other);

	public void FlashOnDamage()
	{
		if (_flashCoroutine != null)
			StopCoroutine(_flashCoroutine);
		_flashCoroutine = StartCoroutine(DamageFlashCoroutine());
	}

	IEnumerator DamageFlashCoroutine()
	{
		if (_renderer != null)
			_renderer.material.color = _damageColor;
		yield return new WaitForSeconds(_flashDuration);
		if (_renderer != null)
			_renderer.material.color = _originalColor;
	}

	void DisableComponents()
	{
		_collider.enabled = false;
		_renderer.enabled = false;
		_rigidBody.linearVelocity = Vector2.zero;
		_rigidBody.angularVelocity = 0f;
	}

	private void EnableAndApplyForce()
	{
		EnableAndApplyForce(gameObject);
	}

	void EnableAndApplyForce(GameObject gameObject)
	{
		gameObject.gameObject.SetActive(true);
		var force = Random.insideUnitCircle.normalized * GetRandomSpeed();
		_rigidBody.linearVelocity = Vector2.zero;
		_rigidBody.angularVelocity = 0f;
		_rigidBody.AddForce(force, ForceMode2D.Impulse);
		_rigidBody.AddTorque(Random.Range(_settings.MinimumRotation, _settings.MaximumRotation));
		_collider.enabled = true;
		_renderer.enabled = true;
	}

	float GetRandomSpeed()
		=> Random.Range(_settings.MinimumSpeed, _settings.MaximumSpeed + GameManager.Instance.Round * 0.1f);
	
}