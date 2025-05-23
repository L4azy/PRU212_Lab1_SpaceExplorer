using UnityEngine;
using System;

public class BulletBase : MonoBehaviour
{
    public event Action<BulletBase> OnBulletDestroyed;

    [SerializeField] GameObject _muzzleFlash, _hitEffect;
    [SerializeField] float _speed = 100f, _duration = 3f;

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
		_rigidbody.AddForce(_transform.up * _speed);
		_timer.OnTimerStop += DestroyBullet;
		_timer.Start(_duration);
	}

	void Update()
	{
		if (ViewportHelper.Instance.IsOnScreen(_transform.up * _speed))
		{
			_swappingX = _swappingY = false;
			return;
		}

		HandleExpandedUniverseSwap();
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		DestroyBullet();
		_scorer?.ScorePoints(other);
	}

	void DestroyBullet()
	{
		if (_destroyed) return;
		_destroyed = true;
		_timer.OnTimerStop -= DestroyBullet;
		_timer.Stop();
		OnBulletDestroyed?.Invoke(this);
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

}
