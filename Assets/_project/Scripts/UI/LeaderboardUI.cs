using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
	[SerializeField] GameObject _panel;
	[SerializeField] GameObject _leaderboardUIElementPrefab;
	[SerializeField] Transform _elementWrapper;

	List<GameObject> uiElements = new();

	private void OnEnable()
	{
		Debug.Log("[LeaderboardUI] Subscribing to HighscoreListChangedEvent via EventBus.");
		EventBus.Instance.Subscribe<HighscoreListChangedEvent>(OnHighscoreListChanged);
	}

	private void OnDisable()
	{
		if (EventBus.Instance == null)
		{
			Debug.LogWarning("[LeaderboardUI] EventBus.Instance is null in OnDisable. Skipping unsubscription.");
			return;
		}

		Debug.Log("[LeaderboardUI] Unsubscribing from HighscoreListChangedEvent via EventBus.");
		EventBus.Instance.Unsubscribe<HighscoreListChangedEvent>(OnHighscoreListChanged);
	}


	private void OnHighscoreListChanged(HighscoreListChangedEvent eventArgs)
	{
		Debug.Log($"[LeaderboardUI] Received HighscoreListChangedEvent with {eventArgs.HighscoreList.Count} elements.");
		UpdateUI(eventArgs.HighscoreList);
	}

	public void ShowPanel()
	{
		_panel.SetActive(true);
	}

	public void ClosePanel()
	{
		_panel.SetActive(false);
	}

	private void UpdateUI(List<HighScoreElement> list)
{
    Debug.Log($"UpdateUI called with {list.Count} elements.");

    // Clear existing UI elements
    foreach (var element in uiElements)
    {
        Destroy(element.gameObject);
    }
    uiElements.Clear();

    for (int i = 0; i < list.Count; i++)
    {
        HighScoreElement el = list[i];
        Debug.Log($"Element {i}: {el.playerName}, {el.points}");

        if (el != null)
        {
            Debug.Log($"Instantiating new UI element for {el.playerName}.");
            // Instantiate new entry
            var inst = Instantiate(_leaderboardUIElementPrefab, Vector3.zero, Quaternion.identity);
            inst.transform.SetParent(_elementWrapper, false);

            // Update name & points
            var texts = inst.GetComponentsInChildren<Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = el.playerName;
                texts[1].text = el.points.ToString();
            }
            else
            {
                Debug.LogError("Prefab does not have enough Text components to display player name and points.");
            }

            uiElements.Add(inst);
        }
    }
}

}
