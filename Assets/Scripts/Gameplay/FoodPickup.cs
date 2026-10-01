using UnityEngine;

/// <summary>
/// One pooled piece of food: it pops up, spins and falls, bounces once on the floor, rests there
/// and blinks before vanishing. FoodManager decides when it is caught and returns it to the pool.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class FoodPickup : MonoBehaviour
{
    private enum FoodState
    {
        Pooled,
        Falling,
        Resting
    }

    [SerializeField] private SpriteRenderer _spriteRenderer;

    private FoodManager _owner;
    private FoodConfig _config;
    private Camera _camera;
    private FoodState _state;
    private Vector2 _velocity;
    private float _spinSpeed;
    private float _vanishAt;
    private bool _bounced;

    public int Points { get; private set; }
    public bool IsHerring { get; private set; }
    public int TierIndex { get; private set; }
    public bool IsActive => _state != FoodState.Pooled;
    public Bounds Bounds => _spriteRenderer.bounds;

    private void Awake()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Launch(FoodManager owner, FoodConfig config, Camera gameplayCamera, Vector2 position,
        Sprite sprite, int points, int tierIndex, bool isHerring, float popMultiplier)
    {
        _owner = owner;
        _config = config;
        _camera = gameplayCamera;
        _spriteRenderer.sprite = sprite;
        _spriteRenderer.enabled = true;
        Points = points;
        TierIndex = tierIndex;
        IsHerring = isHerring;
        _bounced = false;
        _state = FoodState.Falling;

        var spread = config.HorizontalSpread * popMultiplier;
        _velocity = new Vector2(Random.Range(-spread, spread), config.PopSpeed * popMultiplier);
        _spinSpeed = Random.Range(-config.MaxSpinSpeed, config.MaxSpinSpeed);
        transform.SetPositionAndRotation(position, Quaternion.identity);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_state == FoodState.Falling) Fall();
        else if (_state == FoodState.Resting) Rest();
    }

    private void Fall()
    {
        var dt = Time.deltaTime;
        _velocity.y = Mathf.Max(_velocity.y - _config.Gravity * dt, -_config.MaxFallSpeed);
        transform.position += (Vector3)(_velocity * dt);
        transform.Rotate(0f, 0f, _spinSpeed * dt);

        var left = _camera.ViewportToWorldPoint(Vector3.zero).x;
        var right = _camera.ViewportToWorldPoint(Vector3.right).x;
        var x = transform.position.x;
        if (x < left || x > right)
        {
            _owner.Release(this);
            return;
        }

        var floor = _camera.ViewportToWorldPoint(Vector3.zero).y;
        if (_velocity.y >= 0f || _spriteRenderer.bounds.min.y > floor) return;

        if (!_bounced)
        {
            _bounced = true;
            _velocity = new Vector2(_velocity.x * 0.5f, -_velocity.y * _config.Bounce);
            _spinSpeed *= 0.5f;
            return;
        }

        // Settle upright on the floor so it reads clearly as something to collect.
        transform.rotation = Quaternion.identity;
        transform.position += Vector3.up * (floor - _spriteRenderer.bounds.min.y);
        _state = FoodState.Resting;
        _vanishAt = Time.time + _config.FloorLifetime;
    }

    private void Rest()
    {
        var remaining = _vanishAt - Time.time;
        if (remaining <= 0f)
        {
            _owner.Release(this);
            return;
        }

        // Time-based rather than toggled per frame, so the blink freezes with everything else on pause.
        _spriteRenderer.enabled = remaining > _config.BlinkDuration || Mathf.Repeat(Time.time, 0.2f) < 0.12f;
    }

    public void ResetForPool()
    {
        _state = FoodState.Pooled;
        _owner = null;
        _config = null;
        _camera = null;
        Points = 0;
        IsHerring = false;
        gameObject.SetActive(false);
    }
}
