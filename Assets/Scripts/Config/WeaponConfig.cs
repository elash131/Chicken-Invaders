using UnityEngine;

/// <summary>
/// One player weapon. The default Ion blaster and every gift weapon are assets of this type, so a
/// new weapon is a new asset rather than new code.
/// </summary>
[CreateAssetMenu(fileName = "Weapon", menuName = "Chicken Invaders/Weapon")]
public sealed class WeaponConfig : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string _displayName = "ION";
    [Tooltip("One short line for the How To Play screen.")]
    [SerializeField] private string _description;
    [SerializeField] private SoundEffect _shootSound = SoundEffect.Shoot;

    [Header("Bullet Look")]
    [Tooltip("Frames are drawn pointing right; the bullet is turned to face its flight.")]
    [SerializeField] private Sprite[] _frames = System.Array.Empty<Sprite>();
    [SerializeField, Min(0f)] private float _frameRate = 12f;
    [SerializeField, Min(0.05f)] private float _scale = 1f;

    [Header("Firing")]
    [SerializeField, Min(0.02f)] private float _cooldown = 0.18f;
    [SerializeField, Min(1)] private int _bulletsPerShot = 1;
    [Tooltip("Degrees between neighbouring bullets of one shot.")]
    [SerializeField, Range(0f, 45f)] private float _spreadAngle;

    [Header("Bullet")]
    [SerializeField, Min(0.1f)] private float _speed = 14f;
    [SerializeField, Min(1)] private int _damage = 1;
    [SerializeField, Min(0.1f)] private float _lifetime = 2f;
    [Tooltip("Piercing bullets keep flying after a hit.")]
    [SerializeField] private bool _piercing;

    public string DisplayName => _displayName;
    public string Description => _description;
    public SoundEffect ShootSound => _shootSound;
    public Sprite[] Frames => _frames;
    public float FrameRate => _frameRate;
    public float Scale => _scale;
    public float Cooldown => _cooldown;
    public int BulletsPerShot => _bulletsPerShot;
    public float SpreadAngle => _spreadAngle;
    public float Speed => _speed;
    public int Damage => _damage;
    public float Lifetime => _lifetime;
    public bool Piercing => _piercing;
}
