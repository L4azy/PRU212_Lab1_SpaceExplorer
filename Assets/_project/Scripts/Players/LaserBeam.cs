using UnityEngine;
using System.Collections.Generic;

public class LaserBeam : WeaponBase
{
	[Header("Laser Settings")]
	[SerializeField] float _duration = 1.0f;
	[SerializeField] float _damagePerSecond = 20f;
	[SerializeField] float _beamLength = 50f;
	[SerializeField] LayerMask _hitMask;

	[Header("Visuals")]
	[SerializeField] LineRenderer _lineRenderer;
	[SerializeField] float _beamWidth = 0.2f; // Width of the laser beam
	[SerializeField] Material _beamMaterial; // Material for the laser beam

	[Header("Audio")]
	[SerializeField] AudioClip _laserFireClip;
	[SerializeField] AudioClip _laserHitClip;
	[SerializeField] AudioSource _audioSource;


	Transform _muzzle;
	private CountdownTimer _lifetimeTimer;

	static readonly HashSet<Health> _damagedThisFrame = new();

	public void Init(Transform muzzle)
	{
		_muzzle = muzzle;
	}

	void OnEnable()
	{
		if (_lifetimeTimer == null)
		{
			_lifetimeTimer = new CountdownTimer();
			_lifetimeTimer.OnTimerStop += OnLifetimeEnded;
		}
		_lifetimeTimer.SetInitialTime(_duration);
		_lifetimeTimer.Reset();
		_lifetimeTimer.Start();

		if (_lineRenderer != null)
		{
			_lineRenderer.enabled = true;
			_lineRenderer.startWidth = _beamWidth;
			_lineRenderer.endWidth = _beamWidth;
			_lineRenderer.material = _beamMaterial;
		}
	}

	void Update()
	{
		_damagedThisFrame.Clear();
		_lifetimeTimer?.Tick(Time.deltaTime);

		if (_muzzle == null)
		{
			Debug.LogWarning("LaserBeam has no muzzle reference!");
			gameObject.SetActive(false);
			return;
		}

		transform.position = _muzzle.position;
		transform.rotation = _muzzle.rotation;

		Vector2 origin = _muzzle.position;
		Vector2 direction = _muzzle.up;

		Physics2D.queriesHitTriggers = true;

		// Exclude GhostOffscreen layer from the hit mask
		int ghostOffscreenLayer = LayerMask.NameToLayer("Offscreen");
		int mask = _hitMask & ~(1 << ghostOffscreenLayer);

		float beamEnd = _beamLength;
		RaycastHit2D hit = Physics2D.Raycast(origin, direction, _beamLength, mask);

		if (hit.collider != null)
		{
			//Debug.Log("Laser hit: " + hit.collider.gameObject.name + " on layer " + LayerMask.LayerToName(hit.collider.gameObject.layer));

			beamEnd = hit.distance;

			var health = hit.collider.GetComponent<Health>();
			if (health != null && !_damagedThisFrame.Contains(health))
			{
				health.ApplyDamage(_damagePerSecond * Time.deltaTime);
				_damagedThisFrame.Add(health);
			}
		}

		if (_lineRenderer != null)
		{
			_lineRenderer.SetPosition(0, origin);
			_lineRenderer.SetPosition(1, origin + direction * beamEnd);
		}

		Debug.DrawLine(origin, origin + direction * beamEnd, Color.red, 0, false);
	}


	void OnDisable()
	{
		if (_lineRenderer != null)
		{
			_lineRenderer.enabled = false;
		}
	}

	private void OnLifetimeEnded()
	{
		gameObject.SetActive(false);
	}

	public override void Fire(Vector3 position, Quaternion rotation, Transform parent)
	{
		transform.SetPositionAndRotation(position, rotation);
		transform.SetParent(parent);
		gameObject.SetActive(true);

		if (_audioSource != null && _laserFireClip != null)
			_audioSource.PlayOneShot(_laserFireClip);
	}

}
