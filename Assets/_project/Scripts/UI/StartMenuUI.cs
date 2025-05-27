using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
#if !UNITY_IOS && !UNITY_ANDROID
using UnityEngine.InputSystem;
#endif

public class StartMenuUI : MonoBehaviour
{
	[SerializeField] Image _fadePanel;
	[SerializeField] GameObject _controlsPanel, _pressSpaceText;
	//[SerializeField] Button _playButton;
	[SerializeField] TMP_Text _rotateControlsText, _thrustFireControlsText, _hyperspaceControlText;
	[SerializeField] AudioSource _audioSource;
	[SerializeField] UIButton _settingsButton;
	PlayerInputBase _playerInput;
	PlayerKeyboardInput _playerKeyboardInput;

	void Awake()
	{
		_playerInput = gameObject.AddComponent<PlayerKeyboardInput>();
		_playerKeyboardInput = (PlayerKeyboardInput)_playerInput;
		//_playButton.gameObject.SetActive(false);
		_pressSpaceText.SetActive(true);
		ResetFadePanel();
	}

	void OnEnable()
	{
		_audioSource.volume = 1f;
		ResetFadePanel();
		_fadePanel.DOFade(0f, 1f);
		_settingsButton?.Init(LoadSettingsScene);
		UpdateControlsText();
	}

	void OnDisable()
	{
		EventBus.Instance?.Unsubscribe<SettingsSavedEvent>(OnSettingsUpdated);
	}

	void Update()
	{
		var keyboard = Keyboard.current;
		if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
		{
			Debug.Log("Space key pressed! Starting game...");
			StartGame();
		}
	}

	void StartGame()
	{
		_fadePanel.raycastTarget = true;
		_audioSource.DOFade(0f, 2f);
		_fadePanel.DOFade(1f, 1f).OnComplete(() =>
		{
			SceneManager.LoadScene("GameScreen");
		});
	}

	void LoadSettingsScene()
	{
		SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
	}

	void OnSettingsUpdated(SettingsSavedEvent _)
	{
		_playerKeyboardInput.LoadKeyBindings();
		UpdateControlsText();
	}

	void UpdateControlsText()
	{
		_controlsPanel.SetActive(true);
		var rotateLeftKey = _playerKeyboardInput.RotateLeftKey;
		var rotateRightKey = _playerKeyboardInput.RotateRightKey;
		var thrustKey = _playerKeyboardInput.ThrustKey;
		var fireKey = _playerKeyboardInput.FireKey;
		var hyperspaceKey = _playerKeyboardInput.HyperspaceKey;

		_rotateControlsText.text = $"{rotateLeftKey} / {rotateRightKey} - Rotate Left / Right";
		_thrustFireControlsText.text = $"{thrustKey} / {fireKey} - Thrust Fire";
		_hyperspaceControlText.text = $"{hyperspaceKey} - Hyperspace";
	}

	public void ResetFadePanel()
	{
		if (_fadePanel != null)
		{
			var color = _fadePanel.color;
			color.a = 0f; // Set alpha to 0
			_fadePanel.color = color;

			// Disable raycast target to ensure it doesn't block input
			_fadePanel.raycastTarget = false;
		}
	}

}
