using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Owns the player-bullet pool. Enemy eggs will use a separate pool when they are implemented;
/// different projectile owners must never release objects into one another's collections.
/// </summary>
public sealed class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile _bulletPrefab;
    [SerializeField, Min(1)] private int _prewarmCount = 12;
    [SerializeField, Min(1)] private int _maxRetained = 32;

    private readonly HashSet<Projectile> _activeProjectiles = new();
    private readonly List<Projectile> _releaseScratch = new();

    private ObjectPool<Projectile> _pool;
    private IGameManager _gameManager;
    private Camera _gameplayCamera;
    private bool _isShuttingDown;
    public bool IsReady => _pool != null && !_isShuttingDown;
    public bool CanDamage => _gameManager != null && _gameManager.CanDamageEnemies;

    private void Start()
    {
        if (_bulletPrefab == null)
        {
            Debug.LogError("ProjectilePool has no player-bullet prefab assigned.", this);
            enabled = false;
            return;
        }

        _gameplayCamera = Camera.main;
        if (_gameplayCamera == null)
        {
            Debug.LogError("ProjectilePool needs a camera tagged MainCamera.", this);
            enabled = false;
            return;
        }

        ConfigureCollisionLayers();

        _pool = new ObjectPool<Projectile>(
            CreateProjectile,
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile,
            collectionCheck: true,
            defaultCapacity: _prewarmCount,
            maxSize: _maxRetained);

        Prewarm();

        _gameManager = GameManager.Instance;
        _gameManager.OnGameStarted += ReleaseAll;
        _gameManager.OnPlayerDied += ReleaseAll;
        _gameManager.OnGameOver += ReleaseAll;
    }

    public void Fire(Vector2 position, Vector2 direction, float speed, int damage, float lifetime)
    {
        if (_pool == null || _isShuttingDown || _gameManager == null || !_gameManager.CanControlPlayer)
        {
            return;
        }

        var projectile = _pool.Get();
        projectile.Launch(this, _gameplayCamera, position, direction, speed, damage, lifetime);
    }

    public void Release(Projectile projectile)
    {
        if (_pool == null || projectile == null || !_activeProjectiles.Contains(projectile))
        {
            return;
        }

        _pool.Release(projectile);
    }

    public void ReleaseAll()
    {
        if (_pool == null || _activeProjectiles.Count == 0)
        {
            return;
        }

        _releaseScratch.Clear();
        _releaseScratch.AddRange(_activeProjectiles);

        foreach (var projectile in _releaseScratch)
        {
            if (projectile != null && _activeProjectiles.Contains(projectile))
            {
                _pool.Release(projectile);
            }
        }

        _releaseScratch.Clear();
    }

    private Projectile CreateProjectile()
    {
        var projectile = Instantiate(_bulletPrefab, transform);
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    private void OnGetProjectile(Projectile projectile)
    {
        _activeProjectiles.Add(projectile);
    }

    private void OnReleaseProjectile(Projectile projectile)
    {
        _activeProjectiles.Remove(projectile);
        projectile.ResetForPool();
    }

    private void OnDestroyProjectile(Projectile projectile)
    {
        _activeProjectiles.Remove(projectile);
        if (projectile != null)
        {
            Destroy(projectile.gameObject);
        }
    }

    private void Prewarm()
    {
        _releaseScratch.Clear();

        for (var i = 0; i < _prewarmCount; i++)
        {
            _releaseScratch.Add(_pool.Get());
        }

        foreach (var projectile in _releaseScratch)
        {
            _pool.Release(projectile);
        }

        _releaseScratch.Clear();
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

        ReleaseAll();
        _pool?.Dispose();
        _pool = null;
    }
}
