using UnityEngine;

[CreateAssetMenu(fileName = "MotherHen", menuName = "Chicken Invaders/Boss")]
public sealed class BossConfig : ScriptableObject
{
    [Header("Rules")]
    [SerializeField, Min(1)] private int _health = 30;
    [SerializeField, Min(0)] private int _score = 500;

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float _entryDuration = 1.2f;
    [SerializeField, Min(0.1f)] private float _phaseOneSpeed = 1.4f;
    [SerializeField, Min(0.1f)] private float _phaseTwoSpeed = 2.2f;
    [SerializeField, Min(0f)] private float _sidePadding = 0.35f;
    [SerializeField, Min(0f)] private float _topPadding = 1.55f;

    [Header("Phase One - Aimed Volley")]
    [SerializeField, Min(0.1f)] private float _volleyInterval = 2f;
    [SerializeField, Min(0f)] private float _volleyWarning = 0.55f;
    [SerializeField, Range(1, 7)] private int _volleyEggCount = 3;
    [SerializeField, Range(0f, 45f)] private float _volleySpread = 14f;
    [SerializeField, Min(0.1f)] private float _volleyEggSpeedMultiplier = 0.95f;

    [Header("Phase Two - Egg Rain")]
    [SerializeField, Min(0.05f)] private float _rainInterval = 0.32f;
    [SerializeField, Min(0.1f)] private float _rainEggSpeedMultiplier = 1.05f;
    [SerializeField, Min(0f)] private float _rainHorizontalJitter = 0.45f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float _enrageDelay = 0.45f;
    [Tooltip("Build-up before the final blast.")]
    [SerializeField, Min(0f)] private float _defeatDuration = 2.4f;
    [Tooltip("Time between the final blast and the Victory screen.")]
    [SerializeField, Min(0f)] private float _victoryDelay = 1.2f;

    [Header("Ring Burst")]
    [Tooltip("Seconds between ring bursts, in both phases.")]
    [SerializeField, Min(0.5f)] private float _burstInterval = 6f;
    [SerializeField, Min(0f)] private float _burstWarning = 0.6f;
    [SerializeField, Range(3, 15)] private int _burstEggCount = 9;
    [Tooltip("The fan of eggs, in degrees, centred straight down.")]
    [SerializeField, Range(30f, 180f)] private float _burstArc = 140f;
    [SerializeField, Min(0.1f)] private float _burstEggSpeedMultiplier = 0.8f;

    [Header("Chick Escort")]
    [Tooltip("The small formation she calls at half health. Empty means no escort.")]
    [SerializeField] private WaveConfig _escortWave;
    [Tooltip("How far below the top of the screen the escort row flies.")]
    [SerializeField, Min(0f)] private float _escortTopPadding = 4.2f;

    [Header("Feast")]
    [Tooltip("During the defeat build-up she throws one piece of food this often.")]
    [SerializeField, Min(0.05f)] private float _feastInterval = 0.35f;
    [Tooltip("Pieces of food thrown out by the final blast.")]
    [SerializeField, Min(0)] private int _feastBurst = 6;

    public float BurstInterval => _burstInterval;
    public float BurstWarning => _burstWarning;
    public int BurstEggCount => _burstEggCount;
    public float BurstArc => _burstArc;
    public float BurstEggSpeedMultiplier => _burstEggSpeedMultiplier;
    public WaveConfig EscortWave => _escortWave;
    public float EscortTopPadding => _escortTopPadding;
    public int Health => _health;
    public int Score => _score;
    public float EntryDuration => _entryDuration;
    public float PhaseOneSpeed => _phaseOneSpeed;
    public float PhaseTwoSpeed => _phaseTwoSpeed;
    public float SidePadding => _sidePadding;
    public float TopPadding => _topPadding;
    public float VolleyInterval => _volleyInterval;
    public float VolleyWarning => _volleyWarning;
    public int VolleyEggCount => _volleyEggCount;
    public float VolleySpread => _volleySpread;
    public float VolleyEggSpeedMultiplier => _volleyEggSpeedMultiplier;
    public float RainInterval => _rainInterval;
    public float RainEggSpeedMultiplier => _rainEggSpeedMultiplier;
    public float RainHorizontalJitter => _rainHorizontalJitter;
    public float EnrageDelay => _enrageDelay;
    public float DefeatDuration => _defeatDuration;
    public float VictoryDelay => _victoryDelay;
    public float FeastInterval => _feastInterval;
    public int FeastBurst => _feastBurst;
}
