using UnityEngine;
using System;

public class BulletBase : WeaponBase
{
	public event Action<BulletBase> OnBulletDestroyed;

	[SerializeField] GameObject _muzzleFlash, _hitEffect;
	[SerializeField] float _speed = 10f, _duration = 3f;
	[SerializeField] float _damage = 10f;
	private bool _hasHit = false;

	Rigidbody2D _rigidbody;
	Transform _transform;
	Camera _cam;
	bool _swappingX, _swappingY;
	bool _destroyed;
	Scorer _scorer;
	Timer _timer;

	void Awake()
	{
		_cam = Camera.main;
		_transform = transform;
		_rigidbody = GetComponent<Rigidbody2D>();
		_scorer = GetComponent<Scorer>();
		_timer = TimerManager.Instance.CreateTimer<CountdownTimer>();
	}

	void OnEnable()
	{
		_destroyed = false;
		_hasHit = false;
		if (_rigidbody != null)
		{
			_rigidbody.linearVelocity = transform.up * _speed;
			_rigidbody.angularVelocity = 0f;
		}
		_timer.OnTimerStop += DestroyBullet;
		_timer.Start(_duration);
	}

	void Update()
	{
		// Destroy bullet when it goes off-screen
		if (!ViewportHelper.Instance.IsOnScreen(_transform.position))
		{
			DestroyBullet();
			return;
		}

		// Handle expanded universe swap
		if (ViewportHelper.Instance.IsOnScreen(_transform.position))
		{
			_swappingX = _swappingY = false;
			return;
		}

		HandleExpandedUniverseSwap();
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (_hasHit) return;
		_hasHit = true;

		var health = other.collider.GetComponent<Health>();
		if (health != null)
		{
			health.ApplyDamage(_damage);
		}

		DestroyBullet();
	}

	void DestroyBullet()
	{
		if (_destroyed) return;
		_destroyed = true;
		_timer.OnTimerStop -= DestroyBullet;
		_timer.Stop();
		OnBulletDestroyed?.Invoke(this);
		gameObject.SetActive(false);
	}

	void HandleExpandedUniverseSwap()
	{
		if (_swappingX && _swappingY) return;
		var viewportPos = _cam.WorldToViewportPoint(_transform.position);
		var newPos = _transform.position;
		if (!_swappingX && (viewportPos.x > 1 || viewportPos.x < 0))
		{
			newPos.x = -newPos.x;
			_swappingX = true;
		}

		if (!_swappingY && (viewportPos.y > 1 || viewportPos.y < 0))
		{
			newPos.y = -newPos.y;
			_swappingY = true;
		}

		_transform.position = newPos;
	}

	public override void Fire(Vector3 position, Quaternion rotation, Transform parent)
	{
		transform.SetPositionAndRotation(position, rotation);
		transform.SetParent(parent);
		gameObject.SetActive(true);

		if (_rigidbody != null)
		{
			_rigidbody.linearVelocity = transform.up * _speed;
			_rigidbody.angularVelocity = 0f;
		}
	}
}
