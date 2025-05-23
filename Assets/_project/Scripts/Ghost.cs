using UnityEngine;

public class Ghost : MonoBehaviour
{
	/// The ghost's position relative to the player
	public enum GhostPosition
    {
        UpperRight = 0,
		MiddleRight,
        LowerRight,
        LowerMiddle,
		LowerLeft,
		MiddleLeft,
        UpperLeft,
		UpperMiddle
	}

	PolygonCollider2D _collider;
	Renderer _renderer;

	Transform _parentTransform;
	GhostParent _ghostParent;
	GhostPosition _ghostPosition;
	Transform _transform;
	ICollisionParent _collisionParent;

	public void Init(GhostParent ghostParent, GhostPosition ghostPos)
	{
		_ghostParent = ghostParent;
		_ghostParent.TryGetComponent(out _collisionParent);
		_parentTransform = ghostParent.transform;
		_ghostPosition = ghostPos;
		_transform.localScale = ghostParent.transform.localScale;
		RepositionGhost();
		gameObject.SetActive(true);
	}

	void Awake()
	{
		_collider = GetComponent<PolygonCollider2D>();
		_renderer = GetComponent<SpriteRenderer>();
		_transform = transform;
	}

	void OnEnable()
		=> EnableComponents();

	void OnDisable()
		=> DisableComponents();

	void Update()
		=> RepositionGhost();

	void OnCollisionEnter2D(Collision2D collision)
		=> _collisionParent?.Collided(collision);

	void EnableComponents()
	{
		_collider.enabled = true;
		_renderer.enabled = true;
	}

	void DisableComponents()
	{
		_collider.enabled = false;
		_renderer.enabled = false;
	}

	void RepositionGhost()
	{
		if (_parentTransform == null) return;
		_transform.SetPositionAndRotation(_parentTransform.position + GhostOffset, _parentTransform.rotation);
		_collider.enabled = _renderer.isVisible;
	}

	Vector3 GhostOffset
	{
		get
		{
			var xOffset = 0f;
			var yOffset = 0f;

			xOffset = _ghostPosition switch
			{
				// Calculate xOffset
				GhostPosition.MiddleRight or GhostPosition.LowerRight or
					GhostPosition.UpperRight => ViewportHelper.Instance.ScreenWidth,
				GhostPosition.MiddleLeft or GhostPosition.LowerLeft or
					GhostPosition.UpperLeft => -ViewportHelper.Instance.ScreenWidth,
				_ => xOffset
			};

			yOffset = _ghostPosition switch
			{
				// Calculate yOffset
				GhostPosition.UpperRight or GhostPosition.UpperMiddle or
					GhostPosition.UpperLeft => ViewportHelper.Instance.ScreenHeight,
				GhostPosition.LowerRight or GhostPosition.LowerMiddle or
					GhostPosition.LowerLeft => -ViewportHelper.Instance.ScreenHeight,
				_ => yOffset
			};

			return new Vector3(xOffset, yOffset, 0f);
		}
	}
}
