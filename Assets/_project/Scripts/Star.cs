using UnityEngine;

public class Star : MonoBehaviour, IScoreable
{
	public float lifetime = 20f;
	public int _pointValue = 5;

	public int PointValue => _pointValue;

	void Start()
	{
		Destroy(gameObject, lifetime);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		// Check if the player collected the star
		if (other.TryGetComponent<Scorer>(out var scorer))
		{
			// Use a dummy Collision2D to call ScorePoints, or call GameManager directly
			GameManager.Instance.AddPoints(PointValue);
			Destroy(gameObject);
		}
	}
}
