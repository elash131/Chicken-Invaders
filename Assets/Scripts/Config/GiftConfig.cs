using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Gifts", menuName = "Chicken Invaders/Gifts")]
public sealed class GiftConfig : ScriptableObject
{
    [Header("Look")]
    [SerializeField] private Sprite[] _frames = Array.Empty<Sprite>();
    [SerializeField, Min(0f)] private float _frameRate = 10f;
    [SerializeField, Min(0.1f)] private float _scale = 1f;

    [Header("Drops")]
    [Tooltip("Chance that a chicken kill also drops a gift. Only one gift is ever falling at a time.")]
    [SerializeField, Range(0f, 1f)] private float _dropChance = 0.04f;
    [Tooltip("A gift is guaranteed after a random number of kills without one, picked from this range, " +
             "so every player meets them but never at the same kill twice.")]
    [SerializeField] private Vector2Int _guaranteedAfterKills = new(10, 18);

    [Header("Contents")]
    [Tooltip("A gift holds one of these weapons or a shield, drawn from a shuffled bag so nothing repeats until all have appeared.")]
    [SerializeField] private WeaponConfig[] _weapons = Array.Empty<WeaponConfig>();
    [SerializeField, Min(1f)] private float _weaponDuration = 8f;
    [Tooltip("The shield lasts this long, or until it absorbs a hit.")]
    [SerializeField, Min(1f)] private float _shieldDuration = 12f;

    public Sprite[] Frames => _frames;
    public float FrameRate => _frameRate;
    public float Scale => _scale;
    public float DropChance => _dropChance;
    public int RollGuarantee() => UnityEngine.Random.Range(_guaranteedAfterKills.x, _guaranteedAfterKills.y + 1);
    public WeaponConfig[] Weapons => _weapons;
    public float WeaponDuration => _weaponDuration;
    public float ShieldDuration => _shieldDuration;
}
