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

    [Header("Presentation")]
    [SerializeField, Min(0f)] private float _enrageDelay = 0.45f;
    [SerializeField, Min(0f)] private float _defeatDuration = 1.1f;
    [SerializeField, Min(0.1f)] private float _visualScale = 0.72f;
    [SerializeField] private Vector2 _colliderSize = new(2.2f, 2.45f);
    [SerializeField] private Material _spriteMaterial;
    [SerializeField] private Sprite _bodyBack;
    [SerializeField] private Sprite _bodyFront;
    [SerializeField] private Sprite _leftFoot;
    [SerializeField] private Sprite _rightFoot;
    [SerializeField] private Sprite _wing;
    [SerializeField] private Sprite _rightWing;
    [SerializeField] private Sprite[] _headFrames;
    [SerializeField] private Vector2 _bodyPosition;
    [SerializeField] private Vector2 _leftFootPosition = new(-0.62f, -1.05f);
    [SerializeField] private Vector2 _rightFootPosition = new(0.62f, -1.05f);
    [SerializeField] private Vector2 _leftWingPosition = new(-0.88f, 0.1f);
    [SerializeField] private Vector2 _rightWingPosition = new(0.88f, 0.1f);
    [SerializeField] private Vector2 _headPosition = new(0f, 0.82f);
    [SerializeField] private Vector2 _leftWingScale = new(0.78f, 0.78f);
    [SerializeField] private Vector2 _rightWingScale = new(-0.78f, 0.78f);
    [SerializeField, Min(0)] private int _normalHeadIndex = 5;
    [SerializeField, Min(0)] private int _windupHeadIndex = 1;
    [SerializeField, Min(0)] private int _enragedHeadIndex;
    [SerializeField, Min(0)] private int _hurtHeadIndex = 7;
    [SerializeField, Min(0)] private int _defeatedHeadIndex = 10;

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
    public float VisualScale => _visualScale;
    public Vector2 ColliderSize => _colliderSize;
    public Material SpriteMaterial => _spriteMaterial;
    public Sprite BodyBack => _bodyBack;
    public Sprite BodyFront => _bodyFront;
    public Sprite LeftFoot => _leftFoot;
    public Sprite RightFoot => _rightFoot;
    public Sprite Wing => _wing;
    public Sprite RightWing => _rightWing != null ? _rightWing : _wing;
    public Sprite[] HeadFrames => _headFrames;
    public Vector2 BodyPosition => _bodyPosition;
    public Vector2 LeftFootPosition => _leftFootPosition;
    public Vector2 RightFootPosition => _rightFootPosition;
    public Vector2 LeftWingPosition => _leftWingPosition;
    public Vector2 RightWingPosition => _rightWingPosition;
    public Vector2 HeadPosition => _headPosition;
    public Vector2 LeftWingScale => _leftWingScale;
    public Vector2 RightWingScale => _rightWingScale;
    public int NormalHeadIndex => _normalHeadIndex;
    public int WindupHeadIndex => _windupHeadIndex;
    public int EnragedHeadIndex => _enragedHeadIndex;
    public int HurtHeadIndex => _hurtHeadIndex;
    public int DefeatedHeadIndex => _defeatedHeadIndex;

    public bool IsConfigured => (_bodyBack != null || _bodyFront != null) && _leftFoot != null &&
        _rightFoot != null && _wing != null && _rightWing != null && _spriteMaterial != null &&
        _colliderSize.x > 0f && _colliderSize.y > 0f;
}
