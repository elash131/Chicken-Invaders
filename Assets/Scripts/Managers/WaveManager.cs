using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds and moves regular chicken formations. All chickens are registered explicitly, so wave
/// completion is a constant-time set check rather than a scene search.
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

    private sealed class EntryFlight
    {
        public Chicken Chicken;
        public Vector2 Start;
        public Vector2 Control;
        public float Delay;
        public float Elapsed;
        public bool Arrived;
    }

    [Header("Configuration")]
    [SerializeField] private GameBalanceConfig _balance;
    [SerializeField] private WaveConfig[] _waves;
    [SerializeField] private ChickenFactory _factory;

    private readonly HashSet<Chicken> _livingChickens = new();
    private readonly List<EntryFlight> _entryFlights = new();

    private Camera _gameplayCamera;
    private WavePhase _phase;
    private Vector2 _formationOrigin;
    private float _direction = 1f;
    private float _currentSpeed;
    private int _waveIndex;
    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private bool _isClearing;
    private GameManager _game;

    public bool IsReady { get; private set; }
    public GameBalanceConfig Balance => _balance;
    public int WaveCount => _waves.Length;
    public float WaveDelay => _balance.WaveDelay;
    public bool AcceptsDamage => _phase == WavePhase.Active && !_isClearing && _game != null && _game.CanDamageEnemies;
    public int CurrentWaveNumber => _waveIndex + 1;
    public int LivingChickenCount => _livingChickens.Count;

    private void Start()
    {
        _gameplayCamera = Camera.main;

        _game = GameManager.Instance;
        if (_gameplayCamera == null || _game == null || _balance == null || _factory == null || _waves == null || _waves.Length != 4)
        {
            Debug.LogError("WaveManager is missing its balance, factory or wave configuration.", this);
            enabled = false;
            return;
        }

        IsReady = System.Array.TrueForAll(_waves, wave => wave != null);
        if (!IsReady) Debug.LogError("All four wave configurations must be assigned.", this);
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

        _isClearing = false;
        _phase = WavePhase.Entering;
        _direction = 1f;
        _currentSpeed = _balance.FormationSpeed;
        _livingChickens.Clear();
        _entryFlights.Clear();

        RecalculateFormationOrigin();
        BuildFormation(config);

        if (_livingChickens.Count == 0)
        {
            Debug.LogError($"Wave {index + 1} could not create any chickens.", this);
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
        _formationOrigin = new Vector2(topCentre.x, topCentre.y - _balance.ScreenTopPadding);
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
                _entryFlights.Add(new EntryFlight
                {
                    Chicken = chicken,
                    Start = start,
                    Control = new Vector2(target.x - side * 1.5f, target.y + 1.25f),
                    Delay = order * _balance.EntryStagger
                });
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
        var arrivedCount = 0;
        foreach (var flight in _entryFlights)
        {
            if (flight.Chicken == null)
            {
                continue;
            }

            if (flight.Arrived)
            {
                arrivedCount++;
                continue;
            }

            flight.Elapsed += Time.fixedDeltaTime;
            if (flight.Elapsed < flight.Delay)
            {
                continue;
            }

            var progress = Mathf.Clamp01((flight.Elapsed - flight.Delay) / _balance.EntryDuration);
            var eased = Mathf.SmoothStep(0f, 1f, progress);
            var target = _formationOrigin + flight.Chicken.SlotOffset;
            flight.Chicken.MoveTo(QuadraticBezier(flight.Start, flight.Control, target, eased));

            if (progress < 1f)
            {
                continue;
            }

            flight.Arrived = true;
            flight.Chicken.MoveTo(target);
            arrivedCount++;
        }

        if (arrivedCount != _livingChickens.Count)
        {
            return;
        }

        _entryFlights.Clear();
        _phase = WavePhase.Active;
        SetAllColliders(true);
        _game.OnFormationReady();
    }

    private void UpdateActiveFormation()
    {
        if (_livingChickens.Count == 0)
        {
            return;
        }

        var left = float.PositiveInfinity;
        var right = float.NegativeInfinity;

        foreach (var chicken in _livingChickens)
        {
            if (chicken == null)
            {
                continue;
            }

            var centre = _formationOrigin.x + chicken.SlotOffset.x;
            left = Mathf.Min(left, centre - chicken.HalfWidth);
            right = Mathf.Max(right, centre + chicken.HalfWidth);
        }

        var leftWall = _gameplayCamera.ViewportToWorldPoint(Vector3.zero).x + _balance.ScreenSidePadding;
        var rightWall = _gameplayCamera.ViewportToWorldPoint(Vector3.right).x - _balance.ScreenSidePadding;
        var movement = _direction * _currentSpeed * Time.fixedDeltaTime;

        if ((_direction < 0f && left + movement <= leftWall) ||
            (_direction > 0f && right + movement >= rightWall))
        {
            _direction *= -1f;
            _formationOrigin.y -= _balance.DescendStep;
            movement = _direction * _currentSpeed * Time.fixedDeltaTime;
        }

        _formationOrigin.x += movement;
        MoveAllToFormationSlots();
    }

    private void MoveAllToFormationSlots()
    {
        foreach (var chicken in _livingChickens)
        {
            if (chicken != null)
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

        _currentSpeed += _balance.SpeedIncreasePerKill;
        _game.AddScore(_balance.ChickenScore);

        if (_livingChickens.Count > 0)
        {
            return;
        }

        _phase = WavePhase.Clearing;
        _game.OnWaveCleared();
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
        _entryFlights.Clear();
        _isClearing = false;
        _phase = WavePhase.Idle;
    }

    private static Vector2 QuadraticBezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        var remaining = 1f - t;
        return remaining * remaining * start + 2f * remaining * t * control + t * t * end;
    }

    private void OnDrawGizmosSelected()
    {
        if (_balance == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_formationOrigin, 0.12f);
    }
}
