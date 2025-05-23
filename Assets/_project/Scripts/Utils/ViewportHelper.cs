using UnityEngine;

public class ViewportHelper : SingletonMonoBehaviour<ViewportHelper>
{
	//Get the screen
	Camera _cam => Camera.main;
	Vector3 ScreenBottomLeft => _cam.ViewportToWorldPoint(Vector3.zero);
	Vector3 ScreenTopRight => _cam.ViewportToWorldPoint(Vector3.one);
	public float ScreenWidth => ScreenTopRight.x - ScreenBottomLeft.x;
	public float ScreenHeight => ScreenTopRight.y - ScreenBottomLeft.y;

	public bool IsOnScreen(Transform trf) => IsOnScreen(trf.position);

	// Check if the position is within the screen bounds
	public bool IsOnScreen(Vector3 pos)
	{
		var isOnScreen = pos.x >= ScreenBottomLeft.x && 
						 pos.x <= ScreenTopRight.x &&
						 pos.y >= ScreenBottomLeft.y && 
						 pos.y <= ScreenTopRight.y;

		return isOnScreen;
	}

	public Vector3 GetRandomVisiblePosition()
	{
		var x = Random.Range(ScreenBottomLeft.x, ScreenTopRight.x);
		var y = Random.Range(ScreenBottomLeft.y, ScreenTopRight.y);
		return new Vector3(x, y, 0f);
	}
}
