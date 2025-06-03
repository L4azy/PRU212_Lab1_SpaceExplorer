using System;
using UnityEngine;

public class PlayerShip : MonoBehaviour, ICollisionParent
{
	[Header("Ship Settings")]
	[SerializeField] float _turnSpeed = 200f, _thrustSpeed = 120f;
	[SerializeField] GameObject _exhaust;
	[SerializeField] PlayerWeapons _playerWeapons;

	bool _thrusting;
	Rigidbody2D _rigidBody;
	Renderer _renderer;
	Collider2D _collider;
	GhostParent _ghostParent;
	Scorer _scorer;

	public void Collided(Collision2D collision)
	{
		//Debug.Log($"{name} collided with {collision.gameObject.name}", this);
		ExplosionSpawner.Instance.SpawnExplosion(collision.gameObject.transform.position);
		DisableShip();
		ResetShipToStartPosition();
		_scorer?.ScorePoints(collision);
		GameManager.Instance.PlayerDied();
	}

	public void FireBullet()
	{
		_playerWeapons.FireBullet();
	}

	public void DisableShip()
	{
		_renderer.enabled = false;
		_collider.enabled = false;
		_exhaust.SetActive(false);
		_ghostParent.EnableGhosts(false);
	}

	public void EnableRenderer()
	{
		_renderer.enabled = true;
	}

	public void ResetShipToStartPosition()
	{
		Debug.Log("Resetting ship to start position.", this);
		transform.position = Vector3.zero;
		_rigidBody.linearVelocity = Vector2.zero;
		_rigidBody.angularVelocity = 0f;
		transform.localRotation = Quaternion.identity;
	}

	public void EnableInvulnerability()
	{
		_collider.enabled = false;
		_ghostParent.EnableGhosts(false);
		SetShipAlpha(0.25f);
	}

	public void CancelInvulnerability()
	{
		_ghostParent.EnableGhosts();
		_collider.enabled = true;
		SetShipAlpha(1f);
	}

	public void SetThrust(bool thrusting)
	{
		_thrusting = thrusting;
	}

	public void EnterHyperspace()
	{
		transform.position = ViewportHelper.Instance.GetRandomVisiblePosition();
	}

	void Awake()
	{
		_renderer = GetComponent<Renderer>();
		_collider = GetComponent<Collider2D>();
		_rigidBody = GetComponent<Rigidbody2D>();
		_ghostParent = GetComponent<GhostParent>();
		_scorer = GetComponent<Scorer>();
	}

	void FixedUpdate()
	{
		HandleThrust();
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		Collided(other);
	}

	void SetShipAlpha(float alpha)
	{
		var color = _renderer.material.color;
		color.a = alpha;
		_renderer.material.color = color;
	}

	public void Rotate(float rotationInput)
	{
		var rotateAmount = rotationInput * _turnSpeed * Time.deltaTime;
		transform.Rotate(0, 0, rotateAmount);
	}

	void HandleThrust()
	{
		if (!_thrusting)
		{
			_exhaust.gameObject.SetActive(false);
			return;
		}
		_exhaust.gameObject.SetActive(true);
		var thrustAmount = _thrustSpeed * Time.fixedDeltaTime;
		var force = transform.up * thrustAmount;
		_rigidBody.AddForce(force);
	}
}