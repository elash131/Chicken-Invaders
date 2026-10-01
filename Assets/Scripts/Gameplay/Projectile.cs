using UnityEngine;

/// <summary>
/// One pooled projectile flight. The owning pool configures every launch from a WeaponConfig and is
/// the only system allowed to release it, so a hit and an expiry on the same frame cannot return it
/// twice.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public sealed class Projectile : MonoBehaviour
{
    // The hitbox is a little smaller than the artwork, so glow and trails never score a hit.
    private const float HitboxFraction = 0.8f;

    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private ProjectilePool _owner;
    private Camera _gameplayCamera;
    private WeaponConfig _weapon;
    private float _launchedAt;
    private float _expiresAt;
    private bool _isLive;

    private void Awake()
    {
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();
        if (_collider == null) _collider = GetComponent<BoxCollider2D>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Launch(ProjectilePool owner, Camera gameplayCamera, Vector2 position, Vector2 direction,
        WeaponConfig weapon)
    {
        _owner = owner;
        _gameplayCamera = gameplayCamera;
        _weapon = weapon;
        _launchedAt = Time.time;
        _expiresAt = Time.time + weapon.Lifetime;
        _isLive = true;

        direction.Normalize();
        var rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = Vector3.one * weapon.Scale;
        _rigidbody2D.position = position;
        _rigidbody2D.rotation = rotation.eulerAngles.z;
        ShowFrame(0);

        gameObject.SetActive(true);
        _rigidbody2D.linearVelocity = direction * weapon.Speed;
    }

    private void ShowFrame(int index)
    {
        var frames = _weapon.Frames;
        if (frames.Length == 0) return;
        _spriteRenderer.sprite = frames[index % frames.Length];
        _collider.size = (Vector2)_spriteRenderer.sprite.bounds.size * HitboxFraction;
    }

    private void Update()
    {
        if (!_isLive) return;

        if (_weapon.Frames.Length > 1)
        {
            _spriteRenderer.sprite = _weapon.Frames[(int)((Time.time - _launchedAt) * _weapon.FrameRate) % _weapon.Frames.Length];
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
        if (!_isLive || _owner == null || !_owner.CanDamage || !other.TryGetComponent<IDamageable>(out var target))
        {
            return;
        }

        if (_weapon.Piercing)
        {
            target.TakeDamage(_weapon.Damage);
            return;
        }

        // Consume first: TakeDamage may synchronously disable or destroy the receiver and can
        // indirectly trigger more physics callbacks before this callback finishes.
        var owner = _owner;
        _isLive = false;
        target.TakeDamage(_weapon.Damage);
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
        _weapon = null;
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
