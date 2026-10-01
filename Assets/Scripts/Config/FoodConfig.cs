using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Chicken Invaders/Food")]
public sealed class FoodConfig : ScriptableObject
{
    [Serializable]
    public sealed class FoodTier
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField, Min(0)] private int _points = 50;
        [Tooltip("Kill streak needed for a chicken to drop this food instead of the tier below.")]
        [SerializeField, Min(1)] private int _minStreak = 1;

        public Sprite Sprite => _sprite;
        public int Points => _points;
        public int MinStreak => _minStreak;
    }

    [Header("Feast Streak")]
    [Tooltip("Lowest streak first. A chicken drops the best tier its kill streak has reached.")]
    [SerializeField] private FoodTier[] _tiers = Array.Empty<FoodTier>();
    [Tooltip("A kill within this many seconds of the previous one keeps the streak going.")]
    [SerializeField, Min(0.1f)] private float _streakWindow = 1.2f;
    [Tooltip("Mother Hen's feast picks from this tier upwards.")]
    [SerializeField, Min(0)] private int _feastLowestTier = 2;

    [Header("Red Herring")]
    [SerializeField] private Sprite _herringSprite;
    [Tooltip("Chance that any drop is a worthless red herring instead.")]
    [SerializeField, Range(0f, 0.2f)] private float _herringChance = 0.03f;

    [Header("Fall")]
    [SerializeField, Min(0f)] private float _popSpeed = 2.5f;
    [SerializeField, Min(0f)] private float _horizontalSpread = 1.2f;
    [SerializeField, Min(0.1f)] private float _gravity = 7f;
    [SerializeField, Min(0.1f)] private float _maxFallSpeed = 4.5f;
    [SerializeField, Min(0f)] private float _maxSpinSpeed = 240f;
    [Tooltip("Speed kept after the single bounce on the floor.")]
    [SerializeField, Range(0f, 1f)] private float _bounce = 0.35f;

    [Header("On The Floor")]
    [SerializeField, Min(0f)] private float _floorLifetime = 2.5f;
    [Tooltip("The last part of the floor lifetime blinks as a warning that it is about to vanish.")]
    [SerializeField, Min(0f)] private float _blinkDuration = 1f;

    [Header("Pool")]
    [SerializeField, Min(1)] private int _poolPrewarm = 16;
    [SerializeField, Min(1)] private int _poolMaxRetained = 48;

    public FoodTier[] Tiers => _tiers;
    public float StreakWindow => _streakWindow;
    public int FeastLowestTier => _feastLowestTier;
    public Sprite HerringSprite => _herringSprite;
    public float HerringChance => _herringChance;
    public float PopSpeed => _popSpeed;
    public float HorizontalSpread => _horizontalSpread;
    public float Gravity => _gravity;
    public float MaxFallSpeed => _maxFallSpeed;
    public float MaxSpinSpeed => _maxSpinSpeed;
    public float Bounce => _bounce;
    public float FloorLifetime => _floorLifetime;
    public float BlinkDuration => _blinkDuration;
    public int PoolPrewarm => _poolPrewarm;
    public int PoolMaxRetained => _poolMaxRetained;
}
