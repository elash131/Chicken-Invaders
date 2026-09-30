using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Presents run events and forwards button commands; owns no game rules.</summary>
public sealed class GameUIManager : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject _shade;
    [SerializeField] private GameObject _logo;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _livesText;
    [SerializeField] private TextMeshProUGUI _waveText;

    [Header("Menu")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _captionText;
    [SerializeField] private TextMeshProUGUI _bestText;
    [SerializeField] private TextMeshProUGUI _controlsText;
    [SerializeField] private Button _primaryButton;
    [SerializeField] private TextMeshProUGUI _primaryButtonText;
    [SerializeField] private Button _menuButton;

    private GameManager _gameManager;
    private Coroutine _unlockRoutine;

    // The presentation is authored as a prefab. Only this small bootstrap is created in code.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateUI()
    {
        if (GameManager.Instance == null || FindAnyObjectByType<GameUIManager>() != null)
        {
            return;
        }

        var prefab = Resources.Load<GameUIManager>("RunUI");
        if (prefab != null)
        {
            Instantiate(prefab);
        }
        else
        {
            Debug.LogError("The RunUI prefab is missing from Assets/Resources.");
        }
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        if (_gameManager == null)
        {
            Debug.LogError("GameUIManager requires a GameManager.", this);
            enabled = false;
            return;
        }

        _primaryButton.onClick.AddListener(PressedPrimaryButton);
        _menuButton.onClick.AddListener(_gameManager.ReturnToMenu);
        _gameManager.OnStateChanged += RefreshState;
        _gameManager.OnScoreChanged += RefreshScore;
        _gameManager.OnLivesChanged += RefreshLives;
        RefreshState(_gameManager.State);
    }

    private void RefreshScore(int score, int best) => RefreshHUD();
    private void RefreshLives(int lives) => RefreshHUD();

    private void RefreshHUD()
    {
        _scoreText.text = $"<b>{_gameManager.Score:000000}</b>\n<size=22>BEST  {_gameManager.HighScore:000000}</size>";
        _livesText.text = $"LIVES  <b>{_gameManager.Lives}</b>";
        _waveText.text = _gameManager.CurrentWaveNumber <= 4
            ? $"WAVE  {_gameManager.CurrentWaveNumber} / 4"
            : "MOTHER HEN";
        _bestText.text = $"PERSONAL BEST\n<b>{_gameManager.HighScore:000000}</b>";
    }

    private void RefreshState(GameState state)
    {
        if (_unlockRoutine != null)
        {
            StopCoroutine(_unlockRoutine);
        }
        _unlockRoutine = null;

        var menu = state == GameState.Menu;
        var overlay = menu || state == GameState.Paused || state == GameState.GameOver ||
            state == GameState.Victory || state == GameState.BossFight;

        _shade.SetActive(overlay);
        _logo.SetActive(menu);
        _statusText.gameObject.SetActive(!menu);
        _captionText.gameObject.SetActive(overlay);
        _bestText.gameObject.SetActive(overlay);
        _controlsText.gameObject.SetActive(overlay);
        _scoreText.gameObject.SetActive(!menu);
        _livesText.gameObject.SetActive(!menu);
        _waveText.gameObject.SetActive(!menu);
        _primaryButton.gameObject.SetActive(menu || state == GameState.Paused ||
            state == GameState.GameOver || state == GameState.Victory);
        _menuButton.gameObject.SetActive(overlay && !menu);

        _primaryButton.interactable = state != GameState.GameOver;
        _primaryButtonText.text = menu ? "PLAY" : state == GameState.Paused ? "RESUME" : "PLAY AGAIN";
        _captionText.text = "Four waves. One Mother Hen.\nMake every shot count.";

        switch (state)
        {
            case GameState.WaveIntro:
                _statusText.text = $"WAVE {_gameManager.CurrentWaveNumber}";
                break;
            case GameState.Respawning:
                _statusText.text = "GET READY";
                break;
            case GameState.Paused:
                _statusText.text = "PAUSED";
                _captionText.text = "Take a breath.\nYour flight continues when you're ready.";
                break;
            case GameState.GameOver:
                _statusText.text = "GAME OVER";
                _captionText.text = $"FINAL SCORE   {_gameManager.Score:000000}\nOne more flight?";
                break;
            case GameState.Victory:
                _statusText.text = "VICTORY";
                _captionText.text = $"FINAL SCORE   {_gameManager.Score:000000}\nThe skies are yours.";
                break;
            case GameState.BossFight:
                _statusText.text = "FOUR WAVES\nCLEARED";
                _captionText.text = "Boss gameplay is coming next.";
                break;
            default:
                _statusText.text = string.Empty;
                break;
        }

        RefreshHUD();
        SelectVisibleButton();

        if (state == GameState.GameOver)
        {
            _unlockRoutine = StartCoroutine(UnlockRestart());
        }
    }

    private IEnumerator UnlockRestart()
    {
        yield return new WaitUntil(() => _gameManager.CanRestart);
        _primaryButton.interactable = true;
        SelectVisibleButton();
        _unlockRoutine = null;
    }

    private void PressedPrimaryButton()
    {
        if (_gameManager.State == GameState.Menu)
        {
            _gameManager.StartGame();
        }
        else if (_gameManager.State == GameState.Paused)
        {
            _gameManager.ResumeGame();
        }
        else
        {
            _gameManager.RestartGame();
        }
    }

    private void SelectVisibleButton()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        var selected = _primaryButton.gameObject.activeSelf ? _primaryButton.gameObject :
            _menuButton.gameObject.activeSelf ? _menuButton.gameObject : null;
        EventSystem.current.SetSelectedGameObject(selected);
    }

    private void OnDestroy()
    {
        if (_gameManager == null)
        {
            return;
        }

        _primaryButton.onClick.RemoveListener(PressedPrimaryButton);
        _menuButton.onClick.RemoveListener(_gameManager.ReturnToMenu);
        _gameManager.OnStateChanged -= RefreshState;
        _gameManager.OnScoreChanged -= RefreshScore;
        _gameManager.OnLivesChanged -= RefreshLives;
    }
}
