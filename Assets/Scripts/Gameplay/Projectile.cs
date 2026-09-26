using UnityEngine;

/// <summary>
/// One pooled projectile flight. The owning pool configures every launch and is the only system
/// allowed to release it, so a hit and an expiry on the same frame cannot return it twice.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public sealed class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody2D;

    private ProjectilePool _owner;
    private Camera _gameplayCamera;
    private float _expiresAt;
    private int _damage;
    private bool _isLive;

    private void Awake()
    {
        if (_rigidbody2D == null)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }
    }

    public void Launch(
        ProjectilePool owner,
        Camera gameplayCamera,
        Vector2 position,
        Vector2 direction,
        float speed,
        int damage,
        float lifetime)
    {
        _owner = owner;
        _gameplayCamera = gameplayCamera;
        _damage = damage;
        _expiresAt = Time.time + lifetime;
        _isLive = true;

        transform.position = position;
        _rigidbody2D.position = position;
        _rigidbody2D.linearVelocity = Vector2.zero;

        gameObject.SetActive(true);
        _rigidbody2D.linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        if (!_isLive)
        {
            return;
        }

        if (Time.time >= _expiresAt || IsOutsideGameplayView())
        {
            ReturnToPool();
        }
    }

    private bool IsOutsideGameplayView()
    {
        if (_gameplayCamera == null)
        {
            return false;
        }

        var viewport = _gameplayCamera.WorldToViewportPoint(transform.position);
        return viewport.z < 0f || viewport.y > 1.05f || viewport.x < -0.05f || viewport.x > 1.05f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isLive || !other.TryGetComponent<IDamageable>(out var target))
        {
            return;
        }

        // Consume first: TakeDamage may synchronously disable or destroy the receiver and can
        // indirectly trigger more physics callbacks before this callback finishes.
        var owner = _owner;
        _isLive = false;
        target.TakeDamage(_damage);
        owner.Release(this);
    }

    private void ReturnToPool()
    {
        if (!_isLive)
        {
            return;
        }

        var owner = _owner;
        _isLive = false;
        owner.Release(this);
    }

    public void ResetForPool()
    {
        _isLive = false;
        _damage = 0;
        _expiresAt = 0f;
        _gameplayCamera = null;
        _owner = null;

        if (_rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
        }

        gameObject.SetActive(false);
    }
}
