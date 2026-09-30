using UnityEngine;

[CreateAssetMenu(fileName = "GameBalance", menuName = "Chicken Invaders/Game Balance")]
public sealed class GameBalanceConfig : ScriptableObject
{
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

    [Header("Scoring")]
    [SerializeField, Min(0)] private int _chickenScore = 100;

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
    public int ChickenScore => _chickenScore;
}
