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

    // Speed, step-down and egg rate differ per wave and live in each WaveConfig.
    [Header("Chicken Formation")]
    [SerializeField, Min(0.1f)] private float _columnSpacing = 1.35f;
    [SerializeField, Min(0.1f)] private float _rowSpacing = 1.15f;
    [SerializeField, Min(0f)] private float _screenSidePadding = 0.25f;
    [SerializeField, Min(0f)] private float _screenTopPadding = 1f;
    [Tooltip("Height above the ship. A chicken reaching it ends the run.")]
    [SerializeField, Min(0f)] private float _loseLineHeight = 0.8f;

    [Header("Chicken Entry")]
    [SerializeField, Min(0.05f)] private float _entryDuration = 0.8f;
    [SerializeField, Min(0f)] private float _entryStagger = 0.1f;
    [SerializeField, Min(0f)] private float _entrySideDistance = 1.5f;
    [SerializeField, Min(0f)] private float _waveDelay = 1f;

    [Header("Chicken Eggs")]
    [SerializeField] private EggProjectile _eggPrefab;
    [SerializeField, Min(0.1f)] private float _eggSpeed = 6f;
    [SerializeField, Min(0f)] private float _eggSafetyDistance = 2.5f;
    [SerializeField, Min(0.1f)] private float _eggLifetime = 5f;
    [SerializeField, Min(0.01f)] private float _eggBreakFrameDuration = 0.04f;
    [SerializeField, Min(0f)] private float _brokenEggHoldDuration = 0.5f;
    [SerializeField, Min(1)] private int _eggPoolPrewarmCount = 8;
    [SerializeField, Min(1)] private int _eggPoolMaxRetained = 24;

    [Header("Dive Bombers")]
    [Tooltip("The wobble that warns a chicken is about to dive.")]
    [SerializeField, Min(0f)] private float _diveWarning = 0.45f;
    [SerializeField, Min(0.1f)] private float _diveSwoopDuration = 1.4f;
    [SerializeField, Min(0.1f)] private float _diveReturnDuration = 1.1f;
    [Tooltip("How far sideways the swoop curves before it heads for the ship.")]
    [SerializeField, Min(0f)] private float _diveSwing = 2.5f;

    public float DiveWarning => _diveWarning;
    public float DiveSwoopDuration => _diveSwoopDuration;
    public float DiveReturnDuration => _diveReturnDuration;
    public float DiveSwing => _diveSwing;

    [Header("Scoring")]
    [SerializeField, Min(0)] private int _chickenScore = 100;

    [Header("Presentation")]
    [SerializeField] private ExplosionEffect _playerExplosionPrefab;

    [Header("Boss")]
    [SerializeField] private BossConfig _boss;

    [Header("Boss Camera Feedback")]
    [SerializeField] private CameraCue _bossEntranceCamera = new(0.9f, 0.045f, 0.025f);
    [SerializeField] private CameraCue _bossEnrageCamera = new(0.35f, 0.02f, 0.04f);
    [SerializeField] private CameraCue _bossDefeatCamera = new(1.1f, 0.06f, 0.065f);

    [System.Serializable]
    public struct CameraCue
    {
        [SerializeField, Min(0f)] private float _duration;
        [SerializeField, Range(0f, 0.1f)] private float _zoomOutFraction;
        [SerializeField, Range(0f, 0.1f)] private float _shakeDistance;

        public float Duration => Mathf.Max(0f, _duration);
        public float ZoomOutFraction => Mathf.Clamp(_zoomOutFraction, 0f, 0.1f);
        public float ShakeDistance => Mathf.Clamp(_shakeDistance, 0f, 0.1f);

        public CameraCue(float duration, float zoomOutFraction, float shakeDistance)
        {
            _duration = duration;
            _zoomOutFraction = zoomOutFraction;
            _shakeDistance = shakeDistance;
        }
    }

    public float ColumnSpacing => _columnSpacing;
    public float RowSpacing => _rowSpacing;
    public float ScreenSidePadding => _screenSidePadding;
    public float ScreenTopPadding => _screenTopPadding;
    public float LoseLineHeight => _loseLineHeight;
    public float EntryDuration => _entryDuration;
    public float EntryStagger => _entryStagger;
    public float EntrySideDistance => _entrySideDistance;
    public float WaveDelay => _waveDelay;
    public EggProjectile EggPrefab => _eggPrefab;
    public float EggSpeed => _eggSpeed;
    public float EggSafetyDistance => _eggSafetyDistance;
    public float EggLifetime => _eggLifetime;
    public float EggBreakFrameDuration => _eggBreakFrameDuration;
    public float BrokenEggHoldDuration => _brokenEggHoldDuration;
    public int EggPoolPrewarmCount => _eggPoolPrewarmCount;
    public int EggPoolMaxRetained => _eggPoolMaxRetained;
    public int ChickenScore => _chickenScore;
    public ExplosionEffect PlayerExplosionPrefab => _playerExplosionPrefab;
    public BossConfig Boss => _boss;
    public CameraCue BossEntranceCamera => _bossEntranceCamera;
    public CameraCue BossEnrageCamera => _bossEnrageCamera;
    public CameraCue BossDefeatCamera => _bossDefeatCamera;
}
