using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
	public abstract void Fire(Vector3 position, Quaternion rotation, Transform parent);
}
