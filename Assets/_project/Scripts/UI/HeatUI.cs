using UnityEngine;
using UnityEngine.UI;

public class HeatUI : MonoBehaviour
{
	[SerializeField] Transform _target; // Target for following
	[SerializeField] Canvas _canvas;
	[SerializeField] Vector3 _screenOffset; // Offset in screen pixels

	[Header("Bar UI")]
	[SerializeField] Image _overheatBar;
	[SerializeField] Color _coolColor = Color.cyan;
	[SerializeField] Color _hotColor = Color.red;
	[SerializeField] Color _overheatedColor = Color.yellow;

	public void UpdateHeatBar(float currentHeat, float maxHeat, bool isOverheated)
	{
		if (_overheatBar == null) return;

		float fill = Mathf.Clamp01(currentHeat / maxHeat);
		_overheatBar.fillAmount = fill;

		if (isOverheated)
			_overheatBar.color = _overheatedColor;
		else
			_overheatBar.color = Color.Lerp(_coolColor, _hotColor, fill);
	}

	void LateUpdate()
	{
		if (_target != null && _canvas != null)
		{
			Vector3 screenPos = Camera.main.WorldToScreenPoint(_target.position);
			transform.position = screenPos + _screenOffset;
		}
	}
}
