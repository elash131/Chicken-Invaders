using UnityEngine;

/// <summary>
/// Owns the player-bullet pool for every weapon; each launch takes its look and behaviour from a
/// WeaponConfig. Enemy eggs use their own EggPool, so the two never mix collections.
/// </summary>
public sealed class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile _bulletPrefab;
    [SerializeField, Min(1)] private int _prewarmCount = 12;
    [SerializeField, Min(1)] private int _maxRetained = 32;

    private TrackedPool<Projectile> _pool;
    private IGameManager _gameManager;
    private Camera _gameplayCamera;
    private bool _isShuttingDown;

    public bool IsReady => _pool != null && !_isShuttingDown;
    public bool CanDamage => _gameManager != null && _gameManager.CanDamageEnemies;

    private void Start()
    {
        _gameplayCamera = Camera.main;
        if (_bulletPrefab == null || _gameplayCamera == null)
        {
            Debug.LogError("ProjectilePool needs a player-bullet prefab and a camera tagged MainCamera.", this);
            enabled = false;
            return;
        }

        ConfigureCollisionLayers();
        _pool = new TrackedPool<Projectile>(CreateProjectile, projectile => projectile.ResetForPool(),
            _prewarmCount, _maxRetained);

        _gameManager = GameManager.Instance;
        _gameManager.OnGameStarted += ReleaseAll;
        _gameManager.OnPlayerDied += ReleaseAll;
        _gameManager.OnGameOver += ReleaseAll;
    }

    public void Fire(Vector2 position, Vector2 direction, WeaponConfig weapon)
    {
        if (!IsReady || _gameManager == null || !_gameManager.CanControlPlayer) return;
        _pool.Get().Launch(this, _gameplayCamera, position, direction, weapon);
    }

    public void Release(Projectile projectile) => _pool?.Release(projectile);

    public void ReleaseAll() => _pool?.ReleaseAll();

    private Projectile CreateProjectile()
    {
        var projectile = Instantiate(_bulletPrefab, transform);
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    private static void ConfigureCollisionLayers()
    {
        var playerLayer = LayerMask.NameToLayer(Constants.PlayerLayer);
        var projectileLayer = LayerMask.NameToLayer(Constants.PlayerProjectileLayer);

        if (playerLayer < 0 || projectileLayer < 0)
        {
            Debug.LogError("Player and PlayerProjectile layers must be configured before play.");
            return;
        }

        Physics2D.IgnoreLayerCollision(projectileLayer, playerLayer, true);
        Physics2D.IgnoreLayerCollision(projectileLayer, projectileLayer, true);
    }

    private void OnDestroy()
    {
        _isShuttingDown = true;

        if (_gameManager != null)
        {
            _gameManager.OnGameStarted -= ReleaseAll;
            _gameManager.OnPlayerDied -= ReleaseAll;
            _gameManager.OnGameOver -= ReleaseAll;
        }

        _pool?.Dispose();
        _pool = null;
    }
}
