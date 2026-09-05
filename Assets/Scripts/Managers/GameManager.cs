using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Owns the run: score, lives and the start / game-over transitions. Everything that needs to
/// react to those changes subscribes to the events below rather than polling the state, so the
/// UI and the audio can be replaced without touching a single rule in here.
/// </summary>
public class GameManager : Singleton<GameManager>, IGameManager
{
    [Header("Run")]
    [SerializeField] private int _startingLives = 3;
    [SerializeField] private float _respawnDelay = 1.5f;

    private int _score;
    private int _highScore;
    private int _lives;
    private bool _gameOver;
    private bool _playerAlive;

    public int  Score       => _score;
    public int  HighScore   => _highScore;
    public int  Lives       => _lives;
    public bool GameOver    => _gameOver;
    public bool PlayerAlive => _playerAlive;

    public event Action<int, int> OnScoreChanged;
    public event Action<int>      OnLivesChanged;
    public event Action           OnGameStarted;
    public event Action           OnGameOver;
    public event Action           OnPlayerDied;

    private void Awake()
    {
        // Awake is for state this object owns and that depends on nothing else in the scene.
        _highScore = PlayerPrefs.GetInt(Constants.HighScoreKey, 0);
    }

    private void Start()
    {
        // Other objects register and subscribe in their own Start, and Unity does not guarantee
        // the order between them - so hold one frame before kicking the run off, or the first
        // OnGameStarted would fire before anyone is listening.
        StartCoroutine(StartGameNextFrame());
    }

    private IEnumerator StartGameNextFrame()
    {
        yield return null;
        StartGame();
    }

    public void StartGame()
    {
        _gameOver = false;
        _playerAlive = true;
        _score = 0;
        _lives = _startingLives;

        OnScoreChanged?.Invoke(_score, _highScore);
        OnLivesChanged?.Invoke(_lives);
        OnGameStarted?.Invoke();
    }

    public void RestartGame()
    {
        CancelInvoke(nameof(RespawnPlayer));
        StartGame();
    }

    public void AddScore(int amount)
    {
        _score += amount;

        if (_score > _highScore)
        {
            _highScore = _score;
            PlayerPrefs.SetInt(Constants.HighScoreKey, _highScore);
            PlayerPrefs.Save();
        }

        OnScoreChanged?.Invoke(_score, _highScore);
    }

    public void OnPlayerHit()
    {
        // Guard first: an egg and a chicken can both reach the player on the same frame, and
        // without this one collision would cost two lives.
        if (_gameOver || !_playerAlive)
        {
            return;
        }

        _playerAlive = false;
        _lives--;

        OnLivesChanged?.Invoke(_lives);
        OnPlayerDied?.Invoke();

        if (_lives <= 0)
        {
            _gameOver = true;
            OnGameOver?.Invoke();
        }
        else
        {
            Invoke(nameof(RespawnPlayer), _respawnDelay);
        }
    }

    private void RespawnPlayer()
    {
        if (_gameOver)
        {
            return;
        }

        _playerAlive = true;
        PlayerController.Instance.RespawnPlayer();
    }
}
