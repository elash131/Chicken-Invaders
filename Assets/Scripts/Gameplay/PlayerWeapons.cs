using System;
using UnityEngine;

/// <summary>
/// The ship's loadout: the weapon it fires, how long a gift weapon has left, and the shield.
/// PlayerController decides when the player is pulling the trigger; this decides what comes out.
/// Whether a hit is absorbed is still GameManager's rule - it asks <see cref="TryAbsorbHit"/>.
/// </summary>
public sealed class PlayerWeapons : MonoBehaviour
{
    [SerializeField] private WeaponConfig _defaultWeapon;
    [SerializeField] private ProjectilePool _projectilePool;
    [SerializeField] private Transform _firePoint;
    [Tooltip("The bubble drawn around the ship while the shield is up.")]
    [SerializeField] private SpriteRenderer _shieldVisual;

    private WeaponConfig _weapon;
    private float _weaponEndsAt;
    private float _nextFireTime;
    private bool _hasShield;
    private float _shieldEndsAt;
    private IGameManager _game;
    private PlayerController _player;

    public WeaponConfig Current => _weapon;
    public bool HasGiftWeapon => _weapon != _defaultWeapon;
    public float TimeLeft => HasGiftWeapon ? Mathf.Max(0f, _weaponEndsAt - Time.time) : 0f;
    public bool HasShield => _hasShield;
    public float ShieldTimeLeft => _hasShield ? Mathf.Max(0f, _shieldEndsAt - Time.time) : 0f;

    public event Action<WeaponConfig> OnWeaponChanged;
    public event Action<bool> OnShieldChanged;

    private void Awake()
    {
        _weapon = _defaultWeapon;
        _player = GetComponent<PlayerController>();
        if (_shieldVisual != null) _shieldVisual.enabled = false;
    }

    private void Start()
    {
        _game = GameManager.Instance;
        if (_defaultWeapon == null || _projectilePool == null || _firePoint == null || _game == null)
        {
            Debug.LogError("PlayerWeapons needs a default weapon, the projectile pool and a fire point.", this);
            enabled = false;
            return;
        }

        _game.OnGameStarted += ResetLoadout;
        _game.OnPlayerDied += DropGiftWeapon;
        _game.OnStateChanged += HandleStateChanged;
    }

    private void Update()
    {
        if (HasGiftWeapon && Time.time >= _weaponEndsAt)
        {
            AudioManager.Play(SoundEffect.WeaponExpire);
            SetWeapon(_defaultWeapon);
        }

        if (_hasShield && Time.time >= _shieldEndsAt)
        {
            AudioManager.Play(SoundEffect.WeaponExpire);
            SetShield(false);
        }

        if (_shieldVisual == null) return;
        // The bubble follows the ship's visibility, so it never floats alone on a result screen.
        _shieldVisual.enabled = _hasShield && _player.IsVisible;
        if (!_shieldVisual.enabled) return;

        // A slow breathing pulse, so the bubble reads as active rather than as part of the ship.
        var pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 4f);
        _shieldVisual.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.45f, 0.85f, pulse));
    }

    /// <summary>Fires the current weapon if its cooldown allows: one bullet or a fanned spread.</summary>
    public void Fire()
    {
        if (!enabled || Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + _weapon.Cooldown;

        var count = _weapon.BulletsPerShot;
        var firstAngle = -_weapon.SpreadAngle * (count - 1) * 0.5f;
        for (var i = 0; i < count; i++)
        {
            var direction = Quaternion.Euler(0f, 0f, firstAngle + i * _weapon.SpreadAngle) * Vector2.up;
            _projectilePool.Fire(_firePoint.position, direction, _weapon);
        }
        AudioManager.Play(_weapon.ShootSound);
    }

    public void EquipGiftWeapon(WeaponConfig weapon, float duration)
    {
        if (weapon == null) return;
        _weaponEndsAt = Time.time + duration;
        SetWeapon(weapon);
    }

    public void GiveShield(float duration)
    {
        _shieldEndsAt = Time.time + duration;
        SetShield(true);
    }

    /// <summary>Spends the shield on a hit. Returns false when there was no shield to spend.</summary>
    public bool TryAbsorbHit()
    {
        if (!_hasShield) return false;
        SetShield(false);
        AudioManager.Play(SoundEffect.ShieldBreak);
        FeatherBursts.Emit(transform.position, 6);
        return true;
    }

    private void SetWeapon(WeaponConfig weapon)
    {
        _weapon = weapon;
        OnWeaponChanged?.Invoke(weapon);
    }

    private void SetShield(bool on)
    {
        _hasShield = on;
        OnShieldChanged?.Invoke(on);
    }

    private void DropGiftWeapon()
    {
        if (HasGiftWeapon) SetWeapon(_defaultWeapon);
    }

    private void ResetLoadout()
    {
        SetWeapon(_defaultWeapon);
        SetShield(false);
        _nextFireTime = 0f;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Menu) ResetLoadout();
    }

    private void OnDestroy()
    {
        if (_game == null) return;
        _game.OnGameStarted -= ResetLoadout;
        _game.OnPlayerDied -= DropGiftWeapon;
        _game.OnStateChanged -= HandleStateChanged;
    }
}
