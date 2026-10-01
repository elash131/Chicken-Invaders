using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "Chicken Invaders/Wave")]
public sealed class WaveConfig : ScriptableObject
{
    [Header("Formation")]
    [SerializeField, Min(1)] private int _rows = 2;
    [SerializeField, Min(1)] private int _columns = 5;
    [Tooltip("Prefab variant index for each row. Values repeat when the wave has more rows.")]
    [SerializeField] private int[] _rowVariants = { 0 };

    [Header("Difficulty")]
    [SerializeField, Min(0.1f)] private float _formationSpeed = 1.5f;
    [SerializeField, Min(0f)] private float _speedIncreasePerKill = 0.15f;
    [SerializeField, Min(0.01f)] private float _descendStep = 0.12f;
    [SerializeField, Min(0f)] private float _eggRatePerShooter = 0.15f;
    [SerializeField, Min(0.1f)] private float _eggSpeedMultiplier = 1f;

    [Header("Dive Bombers")]
    [Tooltip("Average seconds between dives. Zero means this wave has no dive bombers.")]
    [SerializeField, Min(0f)] private float _diveInterval;
    [SerializeField, Min(1)] private int _maxDivers = 1;
    [Tooltip("Multiplies the dive timings from GameBalance; above 1 dives faster.")]
    [SerializeField, Min(0.1f)] private float _diveSpeed = 1f;

    public int Rows => _rows;
    public int Columns => _columns;
    public float FormationSpeed => _formationSpeed;
    public float SpeedIncreasePerKill => _speedIncreasePerKill;
    public float DescendStep => _descendStep;
    public float EggRatePerShooter => _eggRatePerShooter;
    public float EggSpeedMultiplier => _eggSpeedMultiplier;
    public bool HasDivers => _diveInterval > 0f;
    public float DiveInterval => _diveInterval;
    public int MaxDivers => _maxDivers;
    public float DiveSpeed => _diveSpeed;

    public int GetVariantForRow(int row)
    {
        if (_rowVariants == null || _rowVariants.Length == 0)
        {
            return 0;
        }

        return _rowVariants[Mathf.Abs(row) % _rowVariants.Length];
    }
}
