using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;
	private static readonly object _lock = new();
	private static bool _isApplicationQuitting = false;

	public static T Instance
	{
		get
		{
			if (_isApplicationQuitting)
				return null;

			lock (_lock)
			{
				if (_instance == null)
				{
					// Look for an existing one
					_instance = FindFirstObjectByType<T>();

					// Create one if there is none
					if (_instance == null)
					{
						GameObject singleton = new(typeof(T).Name);
						_instance = singleton.AddComponent<T>();
						DontDestroyOnLoad(singleton); // Prevents the object from being destroyed when loading a new scene

						Debug.Log($"[SingletonMonoBehaviour] Created instance of singleton {typeof(T).Name}.");
					}
					else
						Debug.Log($"[SingletonMonoBehaviour] Found existing instance of singleton {typeof(T).Name}.");

				}

				return _instance;
			}
		}
	}

	// Make sure that singleton instance is reset on app quit
	protected virtual void OnApplicationQuit()
		=> _isApplicationQuitting = true;
	
	// Reset the quitting flag when a new instance is created
	protected virtual void OnDestroy()
	{
		if (_instance == this)
			_isApplicationQuitting = true;
	}

	// Override Awake in subclasses to add initialization logic
	protected virtual void Awake()
	{
		// Prevent multiple instances from existing
		if (_instance == null)
			_instance = this as T;

		else if (_instance != this)
		{
			Debug.LogWarning($"[SingletonMonoBehaviour] Instance of singleton {typeof(T).Name} already exists. Destroying duplicate.");
			Destroy(gameObject);
		}
	}

}
