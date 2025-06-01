using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class PlayerWeapons : MonoBehaviour
{
	[Header("Bullet Settings")]
	[SerializeField] BulletBase _bulletPrefab;
	[SerializeField] WeaponBase _laserPrefab;
	[SerializeField] Transform _muzzle, _bulletsParent;
	[SerializeField] SoundEffectsClip _fireSound = SoundEffectsClip.PlayerBulletFire;

	[Header("Overload Settings")]
	[SerializeField] int _maxHeat = 130;
	[SerializeField] int _heatPerShot = 10;
	[SerializeField] float _overheatCooldown = 2f;
	[SerializeField] float _laserDamage = 50f;
	[SerializeField] float _fireSoundVolume = 0.25f;
	[SerializeField] float _heatDissipationRate = 15f; // heat units per second

	[Header("Heat Bar")]
	[SerializeField] HeatUI _heatUI;

	IObjectPool<BulletBase> _bulletPool;
	Rigidbody2D _rigidBody;

	float _currentHeat = 0f;
	bool _isOverheated = false;

	CountdownTimer _overheatTimer;

	public void FireBullet()
	{
		if (_isOverheated) return;

		_currentHeat += _heatPerShot;
		if (_currentHeat >= _maxHeat)
		{
			Overload();
			return;
		}

		UpdateOverheatBar();

		SfxManager.Instance.PlayClip(_fireSound, _fireSoundVolume);
		var bullet = _bulletPool.Get();
		bullet.transform.position = _muzzle.position;
		bullet.transform.rotation = transform.rotation;
		bullet.OnBulletDestroyed += DestroyBullet;
		bullet.gameObject.SetActive(true);
		if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
		{
			rb.linearVelocity += _rigidBody.linearVelocity;
		}
	}

	void Overload()
	{
		// Fire the laser visually and functionally
		if (_laserPrefab != null)
		{
			var laserInstance = Instantiate(_laserPrefab, _muzzle.position, _muzzle.rotation, _bulletsParent);
			var laserBeam = laserInstance.GetComponent<LaserBeam>();
			if (laserBeam != null)
			{
				laserBeam.Init(_muzzle);
			}
			laserInstance.Fire(_muzzle.position, _muzzle.rotation, _bulletsParent);
		}

		_currentHeat = 0;
		_isOverheated = true;
		UpdateOverheatBar();
		_overheatTimer.Reset(_overheatCooldown);
		_overheatTimer.Start();
	}

	void OnOverheatCooldownFinished() => _isOverheated = false;

	void Awake()
	{
		_bulletPool = new ObjectPool<BulletBase>(
			SpawnBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet,
			true, 10, 20);
		_rigidBody = GetComponentInParent<Rigidbody2D>();

		_overheatTimer = new CountdownTimer();
		_overheatTimer.SetInitialTime(_overheatCooldown);
		_overheatTimer.OnTimerStop += OnOverheatCooldownFinished;
	}

	void Update()
	{
		if (_isOverheated)
		{
			_overheatTimer.Tick(Time.deltaTime);
		}
		else if (_currentHeat > 0f)
		{
			_currentHeat -= _heatDissipationRate * Time.deltaTime;
			if (_currentHeat < 0f) _currentHeat = 0f;
			UpdateOverheatBar();
		}

	}


	BulletBase SpawnBullet()
	{
		var bullet = Instantiate(_bulletPrefab, _muzzle.position, transform.rotation);
		bullet.transform.SetParent(_bulletsParent);
		return bullet;
	}

	void OnGetBullet(BulletBase bullet)
	{
		if (!bullet.TryGetComponent<Rigidbody2D>(out var rb)) return;
		rb.linearVelocity = Vector2.zero;
		rb.angularVelocity = 0f;
	}

	void OnReleaseBullet(BulletBase bullet)
	{
		if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
		{
			rb.linearVelocity = Vector2.zero;
			rb.angularVelocity = 0f;
		}
		bullet.gameObject.SetActive(false);
	}

	void OnDestroyBullet(BulletBase bullet) { }

	void DestroyBullet(BulletBase bullet)
	{
		bullet.OnBulletDestroyed -= DestroyBullet;
		bullet.gameObject.SetActive(false);
		_bulletPool.Release(bullet);
	}

	void UpdateOverheatBar()
	{
		if (_heatUI != null)
			_heatUI.UpdateHeatBar(_currentHeat, _maxHeat, _isOverheated);
	}

	public void ResetHeat()
	{
		_currentHeat = 0f;
		_isOverheated = false;
		UpdateOverheatBar();
	}

}
