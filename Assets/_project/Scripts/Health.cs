using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] float _maxHealth = 100f;
    float _currentHealth;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsDead => _currentHealth <= 0f;

    public event Action OnDeath;

    void Awake()
    {
        _currentHealth = _maxHealth;
    }

	public void ApplyDamage(float amount)
	{
		if (IsDead) return;

		Debug.Log($"{gameObject.name} takes {amount:F2} damage. Health before: {_currentHealth:F2}");

		_currentHealth -= amount;

		var asteroid = GetComponent<Asteroid>();
		if (asteroid != null)
			asteroid.FlashOnDamage();

		if (_currentHealth <= 0f)
		{
			_currentHealth = 0f;
			Die();
		}
	}


	public void Heal(float amount)
    {
        if (IsDead) return;

        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
    }

	void Die()
	{
		OnDeath?.Invoke();
		var asteroid = GetComponent<Asteroid>();
		if (asteroid != null)
		{
			// Award points here
			var scorer = GetComponent<Scorer>();
			if (scorer != null)
			{
				scorer.ScorePoints(null); // Or pass relevant info if needed
			}

			var spawner = FindFirstObjectByType<AsteroidSpawner>();
			if (spawner != null)
			{
				spawner.DestroyAsteroid(asteroid, transform.position);
			}
		}
		Destroy(gameObject); // Or just deactivate if pooling
	}


}
