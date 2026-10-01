using UnityEngine;

/// <summary>Owns every enemy egg - formation, divers and Mother Hen - and is the only object allowed to return one.</summary>
public sealed class EggPool : MonoBehaviour
{
    private TrackedPool<EggProjectile> _pool;
    private EggProjectile _prefab;
    private Camera _gameplayCamera;
    private IGameManager _game;
    private float _speed;
    private float _lifetime;
    private float _breakFrameDuration;
    private float _brokenHoldDuration;
    private bool _isShuttingDown;

    public bool IsReady => _pool != null && !_isShuttingDown;

    public bool Initialize(GameBalanceConfig balance, Camera gameplayCamera, IGameManager game)
    {
        if (IsReady) return true;
        if (balance == null || balance.EggPrefab == null || gameplayCamera == null || game == null)
        {
            Debug.LogError("EggPool needs an egg prefab, gameplay camera and GameManager.", this);
            return false;
        }

        if (!ConfigureCollisionLayers()) return false;

        _prefab = balance.EggPrefab;
        _gameplayCamera = gameplayCamera;
        _game = game;
        _speed = balance.EggSpeed;
        _lifetime = balance.EggLifetime;
        _breakFrameDuration = balance.EggBreakFrameDuration;
        _brokenHoldDuration = balance.BrokenEggHoldDuration;

        _pool = new TrackedPool<EggProjectile>(CreateEgg, egg => egg.ResetForPool(),
            balance.EggPoolPrewarmCount, balance.EggPoolMaxRetained);
        _game.OnGameStarted += ReleaseAll;
        _game.OnPlayerDied += ReleaseAll;
        _game.OnStateChanged += HandleStateChanged;
        return true;
    }

    public void Fire(Vector2 position, Vector2 direction, float speedMultiplier = 1f)
    {
        if (!IsReady || _game == null || !_game.CanEnemiesAct) return;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.down;

        var egg = _pool.Get();
        egg.Launch(
            this,
            _gameplayCamera,
            position,
            direction.normalized,
            _speed * Mathf.Max(0.1f, speedMultiplier),
            _lifetime,
            _breakFrameDuration,
            _brokenHoldDuration);
    }

    public void Release(EggProjectile egg) => _pool?.Release(egg);

    public void HandlePlayerHit(EggProjectile egg)
    {
        // Only an egg that is still out can hurt: a hit and an expiry on one frame count once.
        if (_pool == null || !_pool.IsOut(egg)) return;

        var canDamage = _game != null && _game.CanDamagePlayer;
        Release(egg);
        if (canDamage) _game.ReportPlayerHit();
    }

    public void ReleaseAll() => _pool?.ReleaseAll();

    private EggProjectile CreateEgg()
    {
        var egg = Instantiate(_prefab, transform);
        egg.gameObject.SetActive(false);
        return egg;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Menu || state == GameState.WaveIntro ||
            state == GameState.GameOver || state == GameState.Victory)
        {
            ReleaseAll();
        }
    }

    private static bool ConfigureCollisionLayers()
    {
        var player = LayerMask.NameToLayer(Constants.PlayerLayer);
        var playerProjectile = LayerMask.NameToLayer(Constants.PlayerProjectileLayer);
        var enemy = LayerMask.NameToLayer(Constants.EnemyLayer);
        var egg = LayerMask.NameToLayer(Constants.EnemyProjectileLayer);
        if (player < 0 || playerProjectile < 0 || enemy < 0 || egg < 0)
        {
            Debug.LogError("Player, PlayerProjectile, Enemy and EnemyProjectile layers must be configured.");
            return false;
        }

        Physics2D.IgnoreLayerCollision(egg, player, false);
        Physics2D.IgnoreLayerCollision(egg, playerProjectile, true);
        Physics2D.IgnoreLayerCollision(egg, egg, true);
        Physics2D.IgnoreLayerCollision(egg, enemy, true);
        return true;
    }

    private void OnDestroy()
    {
        _isShuttingDown = true;
        if (_game != null)
        {
            _game.OnGameStarted -= ReleaseAll;
            _game.OnPlayerDied -= ReleaseAll;
            _game.OnStateChanged -= HandleStateChanged;
        }

        _pool?.Dispose();
        _pool = null;
    }
}
