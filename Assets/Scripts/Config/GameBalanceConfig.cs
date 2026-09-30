using UnityEngine;

[CreateAssetMenu(fileName = "GameBalance", menuName = "Chicken Invaders/Game Balance")]
public sealed class GameBalanceConfig : ScriptableObject
{
    [Header("Run")]
    [SerializeField, Min(1)] private int _startingLives = 3;
    [SerializeField, Min(0f)] private float _respawnDelay = 1.5f;
    [SerializeField, Min(0f)] private float _invulnerabilityDuration = 2.5f;
    [SerializeField, Min(0f)] private float _restartLockout = 0.5f;

    public int StartingLives => _startingLives;
    public float RespawnDelay => _respawnDelay;
    public float InvulnerabilityDuration => _invulnerabilityDuration;
    public float RestartLockout => _restartLockout;

    [Header("Chicken Formation")]
    [SerializeField, Min(0.1f)] private float _formationSpeed = 1.5f;
    [SerializeField, Min(0f)] private float _speedIncreasePerKill = 0.15f;
    [SerializeField, Min(0.01f)] private float _descendStep = 0.12f;
    [SerializeField, Min(0.1f)] private float _columnSpacing = 1.35f;
    [SerializeField, Min(0.1f)] private float _rowSpacing = 1.15f;
    [SerializeField, Min(0f)] private float _screenSidePadding = 0.25f;
    [SerializeField, Min(0f)] private float _screenTopPadding = 1f;

    [Header("Chicken Entry")]
    [SerializeField, Min(0.05f)] private float _entryDuration = 0.8f;
    [SerializeField, Min(0f)] private float _entryStagger = 0.1f;
    [SerializeField, Min(0f)] private float _entrySideDistance = 1.5f;
    [SerializeField, Min(0f)] private float _waveDelay = 1f;

    [Header("Chicken Eggs")]
    [SerializeField] private EggProjectile _eggPrefab;
    [SerializeField, Min(0f)] private float _eggRatePerShooter = 0.15f;
    [SerializeField, Min(0.1f)] private float _eggSpeed = 6f;
    [SerializeField, Min(0f)] private float _eggSafetyDistance = 2.5f;
    [SerializeField, Min(0.1f)] private float _eggLifetime = 5f;
    [SerializeField, Min(0.01f)] private float _eggBreakFrameDuration = 0.04f;
    [SerializeField, Min(0f)] private float _brokenEggHoldDuration = 0.5f;
    [SerializeField, Min(1)] private int _eggPoolPrewarmCount = 8;
    [SerializeField, Min(1)] private int _eggPoolMaxRetained = 24;

    [Header("Scoring")]
    [SerializeField, Min(0)] private int _chickenScore = 100;

    [Header("Presentation")]
    [SerializeField] private PlayerExplosionEffect _playerExplosionPrefab;

    public float FormationSpeed => _formationSpeed;
    public float SpeedIncreasePerKill => _speedIncreasePerKill;
    public float DescendStep => _descendStep;
    public float ColumnSpacing => _columnSpacing;
    public float RowSpacing => _rowSpacing;
    public float ScreenSidePadding => _screenSidePadding;
    public float ScreenTopPadding => _screenTopPadding;
    public float EntryDuration => _entryDuration;
    public float EntryStagger => _entryStagger;
    public float EntrySideDistance => _entrySideDistance;
    public float WaveDelay => _waveDelay;
    public EggProjectile EggPrefab => _eggPrefab;
    public float EggRatePerShooter => _eggRatePerShooter;
    public float EggSpeed => _eggSpeed;
    public float EggSafetyDistance => _eggSafetyDistance;
    public float EggLifetime => _eggLifetime;
    public float EggBreakFrameDuration => _eggBreakFrameDuration;
    public float BrokenEggHoldDuration => _brokenEggHoldDuration;
    public int EggPoolPrewarmCount => _eggPoolPrewarmCount;
    public int EggPoolMaxRetained => _eggPoolMaxRetained;
    public int ChickenScore => _chickenScore;
    public PlayerExplosionEffect PlayerExplosionPrefab => _playerExplosionPrefab;
}
