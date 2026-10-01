using UnityEngine;

/// <summary>
/// One regular enemy in a formation. Movement and wave rules belong to WaveManager; the chicken
/// only owns its health and reports one, guarded death.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class Chicken : MonoBehaviour, IDamageable
{
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Collider2D _collider2D;

    private WaveManager _owner;
    private int _health;
    private bool _isDead;

    public int Row { get; private set; }
    public int Column { get; private set; }
    public Vector2 SlotOffset { get; private set; }
    public float HalfWidth => _collider2D != null ? _collider2D.bounds.extents.x : 0.5f;
    public float HalfHeight => _collider2D != null ? _collider2D.bounds.extents.y : 0.5f;
    public Vector2 EggSpawnPosition => _collider2D != null
        ? new Vector2(_collider2D.bounds.center.x, _collider2D.bounds.min.y)
        : (Vector2)transform.position;

    private void Awake()
    {
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();
        if (_collider2D == null) _collider2D = GetComponent<Collider2D>();
    }

    public void Initialize(
        WaveManager owner,
        int row,
        int column,
        Vector2 slotOffset,
        Vector2 spawnPosition)
    {
        _owner = owner;
        Row = row;
        Column = column;
        SlotOffset = slotOffset;
        _health = 1;
        _isDead = false;

        transform.position = spawnPosition;
        _rigidbody2D.position = spawnPosition;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _collider2D.enabled = false;
    }

    public void SetCombatActive(bool active)
    {
        if (!_isDead && _collider2D != null)
        {
            _collider2D.enabled = active;
        }
    }

    public void MoveTo(Vector2 worldPosition)
    {
        if (!_isDead)
        {
            _rigidbody2D.MovePosition(worldPosition);
        }
    }

    public void TakeDamage(int amount)
    {
        if (_isDead || amount <= 0 || _owner == null || !_owner.AcceptsDamage)
        {
            return;
        }

        _health -= amount;
        if (_health > 0)
        {
            return;
        }

        // Guard and disable first. Two projectiles can overlap this collider during one physics step.
        _isDead = true;
        _collider2D.enabled = false;

        var owner = _owner;
        _owner = null;
        owner.HandleChickenKilled(this);
        Destroy(gameObject);
    }

    public void RemoveWithoutKill()
    {
        _isDead = true;
        _owner = null;

        if (_collider2D != null)
        {
            _collider2D.enabled = false;
        }

        Destroy(gameObject);
    }
}
