using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds and moves chicken formations: the regular waves and Mother Hen's escort. All chickens are
/// tracked explicitly, so wave completion is a constant-time set check rather than a scene search.
/// The entrance, egg laying and dives are small helpers it owns: FormationEntry, FormationEggs and
/// ChickenDives.
/// </summary>
public sealed class WaveManager : MonoBehaviour
{
    private enum WavePhase
    {
        Idle,
        Entering,
        Active,
        Clearing
    }

    [Header("Configuration")]
    [SerializeField] private GameBalanceConfig _balance;
    [SerializeField] private WaveConfig[] _waves;
    [SerializeField] private ChickenFactory _factory;

    private readonly HashSet<Chicken> _livingChickens = new();

    private Camera _gameplayCamera;
    private WavePhase _phase;
    private Vector2 _formationOrigin;
    private float _direction = 1f;
    private float _currentSpeed;
    private int _waveIndex;
    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private bool _isClearing;
    private IGameManager _game;
    private PlayerController _player;
    private EggPool _eggPool;
    private WaveConfig _wave;
    private FormationEntry _entry;
    private FormationEggs _eggs;
    private ChickenDives _dives;
    private float _nextDiveTime;
    private float _topPadding;

    public bool IsReady { get; private set; }

    /// <summary>Where a chicken died. Food, feathers and anything else that reacts to a kill listen here.</summary>
    public event System.Action<Vector2> OnChickenKilled;
    public GameBalanceConfig Balance => _balance;
    public int WaveCount => _waves.Length;
    public float WaveDelay => _balance.WaveDelay;
    public bool AcceptsDamage => _phase == WavePhase.Active && !_isClearing && _game != null && _game.CanDamageEnemies;
    public EggPool EnemyEggs => _eggPool;
    private float LoseLineY => _player.transform.position.y + _balance.LoseLineHeight;

    private void Start()
    {
        _gameplayCamera = Camera.main;

        _game = GameManager.Instance;
        _player = PlayerController.Instance;
        if (_gameplayCamera == null || _game == null || _player == null || _balance == null ||
            _factory == null || _waves == null || _waves.Length == 0)
        {
            Debug.LogError("WaveManager is missing its balance, factory or wave configuration.", this);
            enabled = false;
            return;
        }

        _eggPool = GetComponent<EggPool>();
        if (_eggPool == null)
        {
            Debug.LogError("Managers needs an EggPool component next to WaveManager.", this);
            enabled = false;
            return;
        }

        IsReady = System.Array.TrueForAll(_waves, wave => wave != null) &&
                  _eggPool.Initialize(_balance, _gameplayCamera, _game);
        _dives = new ChickenDives(_balance, _gameplayCamera, _player, _eggPool,
            chicken => _formationOrigin + chicken.SlotOffset);
        _entry = new FormationEntry(_balance.EntryDuration);
        _eggs = new FormationEggs(_balance, _player, _eggPool, _dives);
        if (!IsReady) Debug.LogError("Every wave slot needs a WaveConfig.", this);
    }

    private void Update()
    {
        if (_phase != WavePhase.Active || _game == null || !_game.CanEnemiesAct || _eggs == null)
        {
            return;
        }

        _eggs.Tick();
    }

    public void StopCombat()
    {
        _phase = WavePhase.Idle;
        SetAllColliders(false);
    }

    public void StartWave(int index)
    {
        if (!IsReady || _game.State != GameState.WaveIntro) return;
        _waveIndex = index;
        if (index < 0 || index >= _waves.Length)
        {
            _phase = WavePhase.Idle;
            return;
        }

        var config = _waves[index];
        if (config == null)
        {
            Debug.LogError($"Wave {index + 1} has no WaveConfig.", this);
            _phase = WavePhase.Idle;
            return;
        }

        BeginFormation(config, _balance.ScreenTopPadding);
    }

    /// <summary>
    /// Mother Hen's chick escort: a small formation flown in during the boss fight. It reuses the
    /// whole wave system, so escorts sweep, lay eggs, dive, score and drop food like any chicken.
    /// </summary>
    public void StartEscort(WaveConfig config, float topPadding)
    {
        if (!IsReady || config == null || _game.State != GameState.BossFight) return;
        BeginFormation(config, topPadding);
    }

    /// <summary>Pops every remaining chicken in a puff of feathers without scoring them.</summary>
    public void ScatterFormation()
    {
        foreach (var chicken in _livingChickens)
        {
            if (chicken != null) FeatherBursts.Emit(chicken.transform.position, 8);
        }
        ClearFormation();
    }

    private void BeginFormation(WaveConfig config, float topPadding)
    {
        _wave = config;
        _topPadding = topPadding;
        _isClearing = false;
        _phase = WavePhase.Entering;
        _direction = 1f;
        _currentSpeed = config.FormationSpeed;
        _dives.Clear();
        _livingChickens.Clear();
        _entry.Clear();
        _eggs.Reset(config);

        RecalculateFormationOrigin();
        BuildFormation(config);

        if (_livingChickens.Count == 0)
        {
            Debug.LogError($"{config.name} could not create any chickens.", this);
            _phase = WavePhase.Idle;
        }
    }

    private void RecalculateFormationOrigin()
    {
        if (_gameplayCamera == null)
        {
            _gameplayCamera = Camera.main;
        }

        if (_gameplayCamera == null)
        {
            Debug.LogError("WaveManager needs a camera tagged MainCamera.", this);
            return;
        }

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        var topCentre = _gameplayCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f));
        _formationOrigin = new Vector2(topCentre.x, topCentre.y - _topPadding);
    }

    private void BuildFormation(WaveConfig config)
    {
        var order = 0;
        for (var row = 0; row < config.Rows; row++)
        {
            for (var orderColumn = 0; orderColumn < config.Columns; orderColumn++)
            {
                var column = row % 2 == 0 ? orderColumn : config.Columns - 1 - orderColumn;
                var slot = new Vector2(
                    (column - (config.Columns - 1) * 0.5f) * _balance.ColumnSpacing,
                    -row * _balance.RowSpacing);
                var target = _formationOrigin + slot;
                var entersFromLeft = order % 2 == 0;
                var edgeViewportX = entersFromLeft ? 0f : 1f;
                var edge = _gameplayCamera.ViewportToWorldPoint(new Vector3(edgeViewportX, 1f, 0f));
                var side = entersFromLeft ? -1f : 1f;
                var start = new Vector2(
                    edge.x + side * _balance.EntrySideDistance,
                    target.y + 1.2f + (order % 3) * 0.25f);

                var chicken = _factory.Create(
                    config.GetVariantForRow(row), this, row, column, slot, start);
                if (chicken == null)
                {
                    continue;
                }

                _livingChickens.Add(chicken);
                _eggs.Register(chicken);
                _entry.Add(chicken, start, new Vector2(target.x - side * 1.5f, target.y + 1.25f),
                    order * _balance.EntryStagger);
                order++;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_gameplayCamera == null || _game == null ||
            (_game.State != GameState.WaveIntro && !_game.CanEnemiesAct))
        {
            return;
        }

        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            HandleScreenResize();
        }

        if (_phase == WavePhase.Entering)
        {
            UpdateEntryFlights();
        }
        else if (_phase == WavePhase.Active)
        {
            UpdateActiveFormation();
            UpdateDives();
        }
    }

    private void UpdateDives()
    {
        if (_dives.Tick(Time.fixedDeltaTime, _wave.DiveSpeed)) _game.ReportPlayerHit();

        // New dives only start while the ship is in play, so a respawning player is not ambushed.
        if (!_wave.HasDivers || !_game.CanControlPlayer || Time.time < _nextDiveTime ||
            _dives.Count >= _wave.MaxDivers) return;

        _nextDiveTime = Time.time + _wave.DiveInterval * Random.Range(0.7f, 1.3f);
        var pick = Random.Range(0, _livingChickens.Count);
        foreach (var chicken in _livingChickens)
        {
            if (pick-- > 0 || _dives.IsDiving(chicken)) continue;
            _dives.Start(chicken);
            return;
        }
    }

    private void HandleScreenResize()
    {
        var oldOrigin = _formationOrigin;
        RecalculateFormationOrigin();

        // During combat retain the current descent depth; only recenter horizontally.
        if (_phase == WavePhase.Active)
        {
            _formationOrigin.y = oldOrigin.y;
        }
    }

    private void UpdateEntryFlights()
    {
        if (!_entry.Tick(Time.fixedDeltaTime, _formationOrigin)) return;

        _entry.Clear();
        _phase = WavePhase.Active;
        SetAllColliders(true);
        _nextDiveTime = Time.time + _wave.DiveInterval;
        _game.ReportFormationReady();
        _eggs.Begin();
    }

    private void UpdateActiveFormation()
    {
        if (_livingChickens.Count == 0)
        {
            return;
        }

        var left = float.PositiveInfinity;
        var right = float.NegativeInfinity;
        var bottom = float.PositiveInfinity;

        foreach (var chicken in _livingChickens)
        {
            if (chicken == null)
            {
                continue;
            }

            var centre = _formationOrigin.x + chicken.SlotOffset.x;
            left = Mathf.Min(left, centre - chicken.HalfWidth);
            right = Mathf.Max(right, centre + chicken.HalfWidth);
            bottom = Mathf.Min(bottom, _formationOrigin.y + chicken.SlotOffset.y - chicken.HalfHeight);
        }

        // Below this line a chicken can no longer be shot, so the run would never end.
        if (bottom <= LoseLineY)
        {
            _phase = WavePhase.Idle;
            _game.ReportLoseLineCrossed();
            return;
        }

        var leftWall = _gameplayCamera.ViewportToWorldPoint(Vector3.zero).x + _balance.ScreenSidePadding;
        var rightWall = _gameplayCamera.ViewportToWorldPoint(Vector3.right).x - _balance.ScreenSidePadding;
        var movement = _direction * _currentSpeed * Time.fixedDeltaTime;

        if ((_direction < 0f && left + movement <= leftWall) ||
            (_direction > 0f && right + movement >= rightWall))
        {
            _direction *= -1f;
            // In a window narrower than the flock it touches a wall every step; descending each
            // time would drop it onto the lose line within a second.
            if (right - left < rightWall - leftWall)
            {
                _formationOrigin.y -= _wave.DescendStep;
            }
            movement = _direction * _currentSpeed * Time.fixedDeltaTime;
        }

        _formationOrigin.x += movement;
        MoveAllToFormationSlots();
    }

    private void MoveAllToFormationSlots()
    {
        foreach (var chicken in _livingChickens)
        {
            if (chicken != null && !_dives.IsDiving(chicken))
            {
                chicken.MoveTo(_formationOrigin + chicken.SlotOffset);
            }
        }
    }

    public void HandleChickenKilled(Chicken chicken)
    {
        if (!AcceptsDamage || chicken == null || !_livingChickens.Remove(chicken))
        {
            return;
        }

        _dives.Remove(chicken);
        _currentSpeed += _wave.SpeedIncreasePerKill;
        _game.AddScore(_balance.ChickenScore);
        AudioManager.Play(SoundEffect.ChickenDie);
        OnChickenKilled?.Invoke(chicken.transform.position);

        _eggs.HandleRemoved(chicken, _livingChickens);

        if (_livingChickens.Count > 0)
        {
            return;
        }

        _phase = WavePhase.Clearing;
        _game.ReportWaveCleared();
    }

    private void SetAllColliders(bool active)
    {
        foreach (var chicken in _livingChickens)
        {
            if (chicken != null)
            {
                chicken.SetCombatActive(active);
            }
        }
    }

    public void ClearFormation()
    {
        _isClearing = true;
        _phase = WavePhase.Clearing;

        foreach (var chicken in _livingChickens)
        {
            if (chicken != null)
            {
                chicken.RemoveWithoutKill();
            }
        }

        _livingChickens.Clear();
        _entry?.Clear();
        _dives?.Clear();
        _eggs?.Reset(null);
        _isClearing = false;
        _phase = WavePhase.Idle;
    }

    private void OnDrawGizmosSelected()
    {
        if (_balance == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_formationOrigin, 0.12f);

        var player = _player != null ? _player : FindAnyObjectByType<PlayerController>();
        if (player == null) return;
        var y = player.transform.position.y + _balance.LoseLineHeight;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-10f, y, 0f), new Vector3(10f, y, 0f));
    }
}
