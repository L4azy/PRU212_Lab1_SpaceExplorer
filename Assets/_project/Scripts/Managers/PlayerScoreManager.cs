using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class PlayerScoreManager : MonoBehaviour
{
	[SerializeField] TMP_InputField _scoreInput;
	[SerializeField] string filename;
	[SerializeField] GameObject _scoreInputPanel;

	List<PlayerScoreInput> entries = new();

	private void Start()
	{
		entries = FileHandler.ReadListFromJSON<PlayerScoreInput>(filename);
		_scoreInputPanel.SetActive(false);
	}

	public void AddNameToList()
	{
		// Ensure the input field is not empty
		if (string.IsNullOrWhiteSpace(_scoreInput.text))
		{
			//Debug.LogWarning("Score input is empty. Cannot add to the list.");
			return;
		}

		// Add the new entry to the list
		var newEntry = new PlayerScoreInput(_scoreInput.text, GameManager.Instance.HighScore);
		entries.Add(newEntry);

		// Clear the input field
		_scoreInput.text = "";

		// Save the updated list to the JSON file
		FileHandler.SaveToJSON<PlayerScoreInput>(entries, filename);

		// Debug to confirm the save operation
		Debug.Log($"Added new entry: {newEntry.playerName} with score {newEntry.points}. Saved to {filename}.");

		_scoreInputPanel.SetActive(false);
	}

}