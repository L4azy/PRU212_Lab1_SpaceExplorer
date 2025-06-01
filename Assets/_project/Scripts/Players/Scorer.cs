using UnityEngine;

public class Scorer : MonoBehaviour
{
	public void ScorePoints(Collision2D other)
	{
		IScoreable scoreable = null;

		if (other != null)
		{
			other.gameObject.TryGetComponent<IScoreable>(out scoreable);
		}
		else
		{
			// Fallback: try to get IScoreable from this GameObject
			TryGetComponent<IScoreable>(out scoreable);
		}

		if (scoreable != null)
		{
			GameManager.Instance.AddPoints(scoreable.PointValue);
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent<IScoreable>(out var scoreable))
		{
			GameManager.Instance.AddPoints(scoreable.PointValue);
			Destroy(other.gameObject); // Destroy the star when collected
		}
	}
}
