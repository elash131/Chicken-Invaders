using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Owns run rules. Other systems report events and read permissions.</summary>
public class GameManager : Singleton<GameManager>, IGameManager
{
    [Header("Scene References")]
    [SerializeField] private BossController _boss;
    [SerializeField] private BossCameraFeedback _cameraFeedback;

    private GameState _state = GameState.Menu;
    private GameState _stateBeforePause;
    private GameState _combatBeforeRespawn;
    private int _score;
    private int _highScore;
    private int _lives;
    private int _waveIndex;
    private float _restartAllowedAt;
    private bool _invulnerable;
    private bool _bestNeedsSaving;
    private bool _ready;
    private Coroutine _respawnRoutine;
    private Coroutine _protectionRoutine;
    private Coroutine _waveRoutine;
    private PlayerController _player;
    private WaveManager _waves;
    private ProjectilePool _projectiles;
    private InputAction _restart;
    private InputAction _pause;

    public GameState State => _state;
    public int Score => _score;
    public int HighScore => _highScore;
    public int Lives => _lives;
    public int CurrentWaveNumber => _waveIndex + 1;
    public bool GameOver => _state == GameState.GameOver;
    public bool PlayerAlive => CanControlPlayer;
    public bool CanControlPlayer => _state == GameState.Playing || _state == GameState.BossFight;
    // Between waves the ship may move and collect food, but not shoot.
    public bool CanMovePlayer => CanControlPlayer || _state == GameState.WaveIntro;
    public bool CanEnemiesAct => CanControlPlayer || _state == GameState.Respawning;
    public bool CanDamageEnemies => CanEnemiesAct;
    public bool CanDamagePlayer => CanControlPlayer && !_invulnerable;
    public bool CanRestart => _ready && (_state == GameState.Victory ||
        (_state == GameState.GameOver && Time.unscaledTime >= _restartAllowedAt));

    public event Action<GameState> OnStateChanged;
    public event Action<int, int> OnScoreChanged;
    public event Action<int> OnLivesChanged;
    public event Action OnGameStarted;
    public event Action OnGameOver;
    public event Action OnPlayerDied;
    public event Action<int, int> OnBossHealthChanged;

    protected override void Awake()
    {
        base.Awake();
        if (!enabled) return;
        _highScore = PlayerPrefs.GetInt(Constants.HighScoreKey, 0);
        var map = InputSystem.actions.FindActionMap(Constants.PlayerActionMap, true);
        _restart = map.FindAction(Constants.RestartAction, true);
        _pause = map.FindAction(Constants.PauseAction, true);
    }

    private IEnumerator Start()
    {
        // Pools and listeners finish initialization before Play can be accepted.
        yield return null;
        _player = PlayerController.Instance;
        _waves = GetComponent<WaveManager>();
        _projectiles = GetComponent<ProjectilePool>();
        if (_player == null || _waves == null || !_waves.IsReady || _projectiles == null || !_projectiles.IsReady)
        {
            Debug.LogError("GameManager needs a player and configured wave and projectile systems on Managers.", this);
            enabled = false;
            yield break;
        }

        var deathPresenter = GetComponent<PlayerDeathPresenter>();
        if (deathPresenter != null) deathPresenter.Initialize(this, _player, _waves.Balance.PlayerExplosionPrefab);
        else Debug.LogError("Managers needs a PlayerDeathPresenter component.", this);

        if (_cameraFeedback != null) _cameraFeedback.Initialize(this, _waves.Balance);

        if (_boss == null || !_boss.Initialize(this, _player, _waves.EnemyEggs, GetComponent<FoodManager>(),
                _cameraFeedback, _waves.Balance.Boss, Camera.main))
        {
            Debug.LogError("GameManager needs the Mother Hen from the scene assigned.", this);
            enabled = false;
            yield break;
        }

        _player.HidePlayer();
        _ready = true;
    }

    private void Update()
    {
        if (_pause.WasPressedThisFrame())
        {
            if (_state == GameState.Paused) ResumeGame();
            else PauseGame();
        }
        if (_restart.WasPressedThisFrame()) RestartGame();
    }

    public void StartGame()
    {
        if (_ready && _state == GameState.Menu) BeginRun();
    }

    public void RestartGame()
    {
        if (CanRestart) BeginRun();
    }

    private void BeginRun()
    {
        CancelTimedWork();
        _boss.StopEncounter();
        _projectiles.ReleaseAll();
        _waves.ClearFormation();
        _score = 0;
        _lives = _waves.Balance.StartingLives;
        _waveIndex = 0;
        _player.RespawnPlayer();
        ChangeState(GameState.WaveIntro);
        OnScoreChanged?.Invoke(_score, _highScore);
        OnLivesChanged?.Invoke(_lives);
        OnGameStarted?.Invoke();
        _waves.StartWave(_waveIndex);
    }

    public void ReportFormationReady()
    {
        if (_state == GameState.WaveIntro) ChangeState(GameState.Playing);
    }

    public void ReportWaveCleared()
    {
        if (_state != GameState.Playing &&
            !(_state == GameState.Respawning && _combatBeforeRespawn == GameState.Playing)) return;
        // A remaining bullet can clear the board while the ship is respawning.
        if (_waveRoutine == null) _waveRoutine = StartCoroutine(AdvanceWave());
    }

    private IEnumerator AdvanceWave()
    {
        yield return new WaitUntil(() => _state == GameState.Playing);
        _waveIndex++;
        ChangeState(GameState.WaveIntro);
        _projectiles.ReleaseAll();
        yield return new WaitForSeconds(_waves.WaveDelay);
        yield return new WaitUntil(() => _state != GameState.Paused);
        _waveRoutine = null;
        if (_waveIndex < _waves.WaveCount)
        {
            _waves.StartWave(_waveIndex);
        }
        else
        {
            ChangeState(GameState.BossFight);
            _boss.StartEncounter();
        }
    }

    public void AddScore(int amount)
    {
        if (!(CanDamageEnemies || CanMovePlayer) || amount <= 0) return;
        _score += amount;
        if (_score > _highScore)
        {
            _highScore = _score;
            PlayerPrefs.SetInt(Constants.HighScoreKey, _highScore);
            _bestNeedsSaving = true;
        }
        OnScoreChanged?.Invoke(_score, _highScore);
    }

    public void ReportPlayerHit()
    {
        if (!CanDamagePlayer) return;
        _combatBeforeRespawn = _state;
        _lives--;
        ChangeState(GameState.Respawning);
        _player.HidePlayer();
        OnLivesChanged?.Invoke(_lives);
        OnPlayerDied?.Invoke();
        if (_lives == 0) EndRun(GameState.GameOver);
        else _respawnRoutine = StartCoroutine(RespawnPlayer());
    }

    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(_waves.Balance.RespawnDelay);
        yield return new WaitUntil(() => _state != GameState.Paused);
        _respawnRoutine = null;
        _player.RespawnPlayer();
        _invulnerable = true;
        _player.SetInvulnerable(true);
        ChangeState(_combatBeforeRespawn);
        _protectionRoutine = StartCoroutine(EndProtectionAfterDelay());
    }

    private IEnumerator EndProtectionAfterDelay()
    {
        yield return new WaitForSeconds(_waves.Balance.InvulnerabilityDuration);
        yield return new WaitUntil(() => _state != GameState.Paused);
        _invulnerable = false;
        _player.SetInvulnerable(false);
        _protectionRoutine = null;
    }

    public void ReportLoseLineCrossed()
    {
        if (_state == GameState.Playing ||
            (_state == GameState.Respawning && _combatBeforeRespawn == GameState.Playing))
            EndRun(GameState.GameOver);
    }

    public void ReportBossDefeated()
    {
        if (_state == GameState.BossFight ||
            (_state == GameState.Respawning && _combatBeforeRespawn == GameState.BossFight))
            EndRun(GameState.Victory);
    }

    public void ReportBossHealth(int current, int maximum)
    {
        OnBossHealthChanged?.Invoke(Mathf.Max(0, current), Mathf.Max(1, maximum));
    }

    private void EndRun(GameState result)
    {
        _restartAllowedAt = Time.unscaledTime + _waves.Balance.RestartLockout;
        ChangeState(result);
        CancelTimedWork();
        _waves.StopCombat();
        if (result != GameState.Victory) _boss.StopEncounter();
        _projectiles.ReleaseAll();
        _player.HidePlayer();
        SaveBest();
        if (result == GameState.GameOver) OnGameOver?.Invoke();
    }

    public void PauseGame()
    {
        if (!_ready || (!CanEnemiesAct && _state != GameState.WaveIntro)) return;
        _stateBeforePause = _state;
        ChangeState(GameState.Paused);
        SaveBest();
    }

    public void ResumeGame()
    {
        if (_state == GameState.Paused) ChangeState(_stateBeforePause);
    }

    public void ReturnToMenu()
    {
        if (!_ready || (_state != GameState.Paused && _state != GameState.GameOver &&
            _state != GameState.Victory && _state != GameState.BossFight)) return;
        ChangeState(GameState.Menu);
        CancelTimedWork();
        _boss.StopEncounter();
        _waves.ClearFormation();
        _projectiles.ReleaseAll();
        _player.HidePlayer();
        SaveBest();
    }

    private void ChangeState(GameState next)
    {
        if (_state == next) return;
        _state = next;
        Time.timeScale = next == GameState.Paused ? 0f : 1f;
        OnStateChanged?.Invoke(next);
    }

    private void CancelTimedWork()
    {
        if (_respawnRoutine != null) StopCoroutine(_respawnRoutine);
        if (_protectionRoutine != null) StopCoroutine(_protectionRoutine);
        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        _respawnRoutine = _protectionRoutine = _waveRoutine = null;
        _invulnerable = false;
        if (_player != null) _player.SetInvulnerable(false);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) PauseGame();
    }

    private void SaveBest()
    {
        if (!_bestNeedsSaving) return;
        PlayerPrefs.Save();
        _bestNeedsSaving = false;
    }

    private void OnApplicationQuit() => SaveBest();

    protected override void OnDestroy()
    {
        if (HasInstance && Instance == this)
        {
            CancelTimedWork();
            Time.timeScale = 1f;
            SaveBest();
        }
        base.OnDestroy();
    }
}
