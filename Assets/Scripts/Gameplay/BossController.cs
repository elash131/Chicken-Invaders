using UnityEngine;

/// <summary>Owns Mother Hen health, movement, attacks and guarded defeat.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(BossPresenter))]
public sealed class BossController : MonoBehaviour, IDamageable
{
    private enum BossPhase
    {
        Inactive,
        Entering,
        Volley,
        Rain,
        Defeated
    }

    private GameManager _game;
    private PlayerController _player;
    private EggPool _eggs;
    private BossCameraFeedback _cameraFeedback;
    private BossConfig _config;
    private Camera _camera;
    private Rigidbody2D _body;
    private BoxCollider2D _collider;
    private BossPresenter _presenter;
    private BossPhase _phase;
    private Vector2 _entryStart;
    private Vector2 _combatPosition;
    private float _entryElapsed;
    private float _direction = 1f;
    private float _nextAttackAt;
    private float _volleyAt;
    private Vector2 _lockedTarget;
    private bool _windingUp;
    private int _health;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    public bool IsActive => _phase != BossPhase.Inactive;
    public int Health => _health;

    public bool Initialize(
        GameManager game,
        PlayerController player,
        EggPool eggs,
        BossCameraFeedback cameraFeedback,
        BossConfig config,
        Camera gameplayCamera)
    {
        if (_config != null) return true;
        if (game == null || player == null || eggs == null || config == null ||
            gameplayCamera == null || !config.IsConfigured)
        {
            Debug.LogError("Mother Hen needs configured game, player, egg pool, camera and boss data.", this);
            return false;
        }

        _game = game;
        _player = player;
        _eggs = eggs;
        _cameraFeedback = cameraFeedback;
        _config = config;
        _camera = gameplayCamera;
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _presenter = GetComponent<BossPresenter>();

        gameObject.layer = LayerMask.NameToLayer(Constants.EnemyLayer);
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.gravityScale = 0f;
        _body.freezeRotation = true;
        _collider.isTrigger = true;
        _collider.size = config.ColliderSize;
        _collider.offset = new Vector2(0f, -0.05f);
        _collider.enabled = false;
        _presenter.Initialize(config);
        _phase = BossPhase.Inactive;
        _game.OnStateChanged += HandleStateChanged;
        return true;
    }

    public void StartEncounter()
    {
        if (_config == null || _phase != BossPhase.Inactive || _game.State != GameState.BossFight) return;

        _health = _config.Health;
        _direction = Random.value < 0.5f ? -1f : 1f;
        _entryElapsed = 0f;
        _windingUp = false;
        RecalculateCombatPosition();
        _entryStart = new Vector2(_combatPosition.x,
            _camera.ViewportToWorldPoint(Vector3.up).y + _collider.size.y);
        SetPosition(_entryStart);
        _phase = BossPhase.Entering;
        _presenter.Show();
        _game.ReportBossHealth(_health, _config.Health);
        RememberScreenSize();
    }

    private void Update()
    {
        if (_config == null) return;

        if (_phase == BossPhase.Defeated)
        {
            if (Time.time >= _nextAttackAt) StopEncounter();
            return;
        }

        if (!_game.CanEnemiesAct) return;

        if (_phase == BossPhase.Volley) UpdateVolley();
        else if (_phase == BossPhase.Rain) UpdateRain();
    }

    private void FixedUpdate()
    {
        if (_config == null || !_game.CanEnemiesAct) return;

        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            var previousY = _combatPosition.y;
            RecalculateCombatPosition();
            if (_phase != BossPhase.Entering) _combatPosition.y = previousY;
            RememberScreenSize();
        }

        if (_phase == BossPhase.Entering) UpdateEntrance();
        else if (_phase == BossPhase.Volley || _phase == BossPhase.Rain) UpdateSweep();
    }

    private void UpdateEntrance()
    {
        _entryElapsed += Time.fixedDeltaTime;
        var progress = Mathf.Clamp01(_entryElapsed / Mathf.Max(0.01f, _config.EntryDuration));
        SetPosition(Vector2.Lerp(_entryStart, _combatPosition, Mathf.SmoothStep(0f, 1f, progress)));
        if (progress < 1f) return;

        _phase = BossPhase.Volley;
        _collider.enabled = true;
        _nextAttackAt = Time.time + _config.VolleyInterval;
    }

    private void UpdateSweep()
    {
        var halfWidth = _collider.bounds.extents.x;
        var left = _camera.ViewportToWorldPoint(Vector3.zero).x + _config.SidePadding + halfWidth;
        var right = _camera.ViewportToWorldPoint(Vector3.right).x - _config.SidePadding - halfWidth;
        var speed = _phase == BossPhase.Rain ? _config.PhaseTwoSpeed : _config.PhaseOneSpeed;
        var nextX = _body.position.x + _direction * speed * Time.fixedDeltaTime;
        if (nextX <= left || nextX >= right)
        {
            _direction *= -1f;
            nextX = Mathf.Clamp(nextX, left, right);
        }
        SetPosition(new Vector2(nextX, _combatPosition.y));
    }

    private void UpdateVolley()
    {
        if (!_game.PlayerAlive)
        {
            if (_windingUp)
            {
                _windingUp = false;
                _presenter.SetVolleyWarning(false);
                _nextAttackAt = Time.time + _config.VolleyInterval;
            }

            return;
        }

        if (_windingUp)
        {
            if (Time.time < _volleyAt) return;
            _windingUp = false;
            _presenter.SetVolleyWarning(false);
            FireVolley();
            _nextAttackAt = Time.time + _config.VolleyInterval;
            return;
        }

        if (Time.time < _nextAttackAt || !_game.PlayerAlive) return;
        _lockedTarget = _player.transform.position;
        _windingUp = true;
        _volleyAt = Time.time + _config.VolleyWarning;
        _presenter.SetVolleyWarning(true);
    }

    private void FireVolley()
    {
        var origin = EggOrigin;
        var direction = (_lockedTarget - origin).normalized;
        if (direction.y > -0.2f) direction.y = -0.2f;
        direction.Normalize();
        var count = Mathf.Max(1, _config.VolleyEggCount);
        for (var i = 0; i < count; i++)
        {
            var offset = count == 1 ? 0f : Mathf.Lerp(-_config.VolleySpread, _config.VolleySpread, i / (count - 1f));
            var spreadDirection = Quaternion.Euler(0f, 0f, offset) * direction;
            if (spreadDirection.y < -0.05f)
                _eggs.Fire(origin, spreadDirection, _config.VolleyEggSpeedMultiplier);
        }
    }

    private void UpdateRain()
    {
        if (Time.time < _nextAttackAt) return;
        var origin = EggOrigin;
        origin.x += Random.Range(-_config.RainHorizontalJitter, _config.RainHorizontalJitter);
        _eggs.Fire(origin, Vector2.down, _config.RainEggSpeedMultiplier);
        _nextAttackAt = Time.time + _config.RainInterval;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || _config == null || !_game.CanDamageEnemies ||
            (_phase != BossPhase.Volley && _phase != BossPhase.Rain)) return;

        _health = Mathf.Max(0, _health - amount);
        _presenter.FlashHit();
        _game.ReportBossHealth(_health, _config.Health);

        if (_health == 0)
        {
            Defeat();
        }
        else if (_phase == BossPhase.Volley && _health <= _config.Health / 2)
        {
            _phase = BossPhase.Rain;
            _windingUp = false;
            _presenter.Enrage();
            _cameraFeedback?.PlayEnrage();
            _nextAttackAt = Time.time + _config.EnrageDelay;
        }
    }

    private void Defeat()
    {
        if (_phase == BossPhase.Defeated) return;
        _phase = BossPhase.Defeated;
        _collider.enabled = false;
        _windingUp = false;
        _eggs.ReleaseAll();
        _presenter.Defeat();
        _nextAttackAt = Time.time + _config.DefeatDuration;
        _game.AddScore(_config.Score);
        _game.OnBossDefeated();
    }

    public void StopEncounter()
    {
        if (_config == null) return;
        _phase = BossPhase.Inactive;
        _health = 0;
        _windingUp = false;
        _collider.enabled = false;
        _body.linearVelocity = Vector2.zero;
        _presenter.Hide();
        _game.ReportBossHealth(0, _config.Health);
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Menu || state == GameState.WaveIntro || state == GameState.GameOver)
            StopEncounter();
    }

    private Vector2 EggOrigin => new(_body.position.x, _collider.bounds.min.y + 0.05f);

    private void RecalculateCombatPosition()
    {
        var top = _camera.ViewportToWorldPoint(Vector3.up).y;
        _combatPosition = new Vector2(_camera.transform.position.x, top - _config.TopPadding);
    }

    private void SetPosition(Vector2 position)
    {
        transform.position = position;
        _body.position = position;
    }

    private void RememberScreenSize()
    {
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }

    private void OnDestroy()
    {
        if (_game != null) _game.OnStateChanged -= HandleStateChanged;
    }
}
