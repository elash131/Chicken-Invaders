using UnityEngine;

/// <summary>A pooled enemy egg with guarded flight, impact and floor-break states.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public sealed class EggProjectile : MonoBehaviour
{
    private enum EggState
    {
        Pooled,
        Flying,
        Breaking,
        Broken,
        Consumed
    }

    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Collider2D _collider2D;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite[] _breakFrames;

    private EggPool _owner;
    private Camera _gameplayCamera;
    private Sprite _flightSprite;
    private EggState _state;
    private float _expiresAt;
    private float _breakFrameDuration;
    private float _brokenHoldDuration;
    private float _nextBreakFrameAt;
    private float _releaseAt;
    private float _floorY;
    private int _breakFrameIndex;

    private void Awake()
    {
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();
        if (_collider2D == null) _collider2D = GetComponent<Collider2D>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        _flightSprite = _spriteRenderer.sprite;
    }

    public void Launch(
        EggPool owner,
        Camera gameplayCamera,
        Vector2 position,
        float speed,
        float lifetime,
        float breakFrameDuration,
        float brokenHoldDuration)
    {
        _owner = owner;
        _gameplayCamera = gameplayCamera;
        _expiresAt = Time.time + lifetime;
        _breakFrameDuration = breakFrameDuration;
        _brokenHoldDuration = brokenHoldDuration;
        _breakFrameIndex = 0;
        _state = EggState.Flying;

        transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        transform.position = position;
        _rigidbody2D.position = position;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
        _rigidbody2D.simulated = true;
        _collider2D.enabled = true;
        _spriteRenderer.sprite = _flightSprite;
        gameObject.SetActive(true);
        _rigidbody2D.linearVelocity = Vector2.down * speed;
    }

    private void Update()
    {
        if (_state == EggState.Flying)
        {
            if (HasReachedFloor()) BeginBreak();
            else if (Time.time >= _expiresAt || IsOutsideHorizontalView()) ReturnToPool();
        }
        else if (_state == EggState.Breaking && Time.time >= _nextBreakFrameAt)
        {
            AdvanceBreakAnimation();
        }
        else if (_state == EggState.Broken && Time.time >= _releaseAt)
        {
            ReturnToPool();
        }
    }

    private bool HasReachedFloor()
    {
        if (_gameplayCamera == null) return false;
        _floorY = _gameplayCamera.ViewportToWorldPoint(Vector3.zero).y;
        return _spriteRenderer.bounds.min.y <= _floorY;
    }

    private bool IsOutsideHorizontalView()
    {
        if (_gameplayCamera == null) return false;
        var viewport = _gameplayCamera.WorldToViewportPoint(transform.position);
        return viewport.z < 0f || viewport.x < -0.05f || viewport.x > 1.05f || viewport.y > 1.05f;
    }

    private void BeginBreak()
    {
        _state = EggState.Breaking;
        _collider2D.enabled = false;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
        _rigidbody2D.simulated = false;
        transform.rotation = Quaternion.identity;

        if (_breakFrames == null || _breakFrames.Length == 0)
        {
            ReturnToPool();
            return;
        }

        _breakFrameIndex = 0;
        SetBreakFrame(_breakFrames[0]);
        _nextBreakFrameAt = Time.time + _breakFrameDuration;
    }

    private void AdvanceBreakAnimation()
    {
        _breakFrameIndex++;
        if (_breakFrameIndex >= _breakFrames.Length)
        {
            _state = EggState.Broken;
            _releaseAt = Time.time + _brokenHoldDuration;
            return;
        }

        SetBreakFrame(_breakFrames[_breakFrameIndex]);
        _nextBreakFrameAt = Time.time + _breakFrameDuration;
    }

    private void SetBreakFrame(Sprite frame)
    {
        _spriteRenderer.sprite = frame;
        var position = transform.position;
        position.y += _floorY - _spriteRenderer.bounds.min.y;
        transform.position = position;
        _rigidbody2D.position = position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_state != EggState.Flying || _owner == null ||
            other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        var owner = _owner;
        _state = EggState.Consumed;
        _collider2D.enabled = false;
        _rigidbody2D.linearVelocity = Vector2.zero;
        owner.HandlePlayerHit(this);
    }

    private void ReturnToPool()
    {
        if (_state == EggState.Pooled || _state == EggState.Consumed) return;
        var owner = _owner;
        _state = EggState.Consumed;
        owner?.Release(this);
    }

    public void ResetForPool()
    {
        _state = EggState.Pooled;
        _owner = null;
        _gameplayCamera = null;
        _expiresAt = 0f;
        _breakFrameDuration = 0f;
        _brokenHoldDuration = 0f;
        _nextBreakFrameAt = 0f;
        _releaseAt = 0f;
        _breakFrameIndex = 0;
        _floorY = 0f;

        if (_rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
            _rigidbody2D.simulated = false;
        }
        if (_collider2D != null) _collider2D.enabled = false;
        if (_spriteRenderer != null) _spriteRenderer.sprite = _flightSprite;
        gameObject.SetActive(false);
    }
}
