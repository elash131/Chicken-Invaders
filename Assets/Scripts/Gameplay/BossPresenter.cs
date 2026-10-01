using UnityEngine;

/// <summary>
/// Animates the layered Mother Hen artwork authored in her prefab: flapping wings, stepping feet,
/// armour damage, hit flashes and the defeat show. It owns no rules; BossController tells it what
/// happened and when.
/// </summary>
public sealed class BossPresenter : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private Transform _leftWing;
    [SerializeField] private Transform _rightWing;
    [SerializeField] private Transform _leftFoot;
    [SerializeField] private Transform _rightFoot;

    [Header("Damage")]
    [SerializeField] private SpriteRenderer _body;
    [Tooltip("Body art from unharmed to most damaged. Health is split evenly between the stages.")]
    [SerializeField] private Sprite[] _damageStages;

    [Header("Defeat Show")]
    [SerializeField] private ExplosionEffect _explosionPrefab;
    [SerializeField, Min(1)] private int _blastCount = 6;
    [SerializeField, Min(0.05f)] private float _blastInterval = 0.14f;
    [SerializeField, Min(1f)] private float _swellScale = 1.35f;
    [SerializeField, Min(1f)] private float _finalBlastSize = 7f;

    private SpriteRenderer[] _renderers;
    private ExplosionEffect[] _blasts = System.Array.Empty<ExplosionEffect>();
    private Vector3 _visualScale;
    private Vector3 _visualPosition;
    private float _defeatDuration = 1f;
    private float _hitFlashUntil;
    private float _defeatStartedAt = -1f;
    private float _nextBlastAt;
    private int _nextBlast;
    private int _damageStage;
    private bool _enraged;
    private bool _windingUp;
    private bool _exploded;

    public void Initialize(BossConfig config)
    {
        if (_renderers != null) return;
        _defeatDuration = Mathf.Max(0.01f, config.DefeatDuration);
        _visualScale = _visualRoot.localScale;
        _visualPosition = _visualRoot.localPosition;
        // The prefab starts inactive, so Awake has not run yet; include inactive children.
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        CreateBlasts();
        gameObject.SetActive(false);
    }

    // Created once and replayed, so the defeat show never instantiates during play. They are not
    // children of the boss, because the final blast must keep playing after she is hidden.
    private void CreateBlasts()
    {
        if (_explosionPrefab == null) return;
        _blasts = new ExplosionEffect[_blastCount];
        for (var i = 0; i < _blastCount; i++)
        {
            _blasts[i] = Instantiate(_explosionPrefab);
            _blasts[i].name = "Mother Hen Blast";
            _blasts[i].Stop();
        }
    }

    public void Show()
    {
        _enraged = false;
        _windingUp = false;
        _exploded = false;
        _hitFlashUntil = 0f;
        _defeatStartedAt = -1f;
        transform.localRotation = Quaternion.identity;
        _visualRoot.localScale = _visualScale;
        _visualRoot.localPosition = _visualPosition;
        SetDamageStage(0);
        SetColor(Color.white);
        SetVisible(true);
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    public void SetVolleyWarning(bool warning) => _windingUp = warning;

    public void ShowDamage(int health, int maxHealth)
    {
        if (_defeatStartedAt >= 0f) return;

        var stageCount = _damageStages != null ? _damageStages.Length : 0;
        var lost = 1f - health / (float)Mathf.Max(1, maxHealth);
        var stage = Mathf.Clamp((int)(lost * stageCount), 0, Mathf.Max(0, stageCount - 1));
        // A longer flash when the armour breaks, so the change of look reads as a big hit.
        _hitFlashUntil = Time.time + (stage != _damageStage ? 0.3f : 0.1f);
        SetDamageStage(stage);
    }

    private void SetDamageStage(int stage)
    {
        _damageStage = stage;
        if (_body != null && _damageStages != null && stage < _damageStages.Length)
            _body.sprite = _damageStages[stage];
    }

    public void Enrage()
    {
        _enraged = true;
        _windingUp = false;
        _hitFlashUntil = Time.time + 0.22f;
    }

    /// <summary>Starts the build-up: she swells, shakes and pops with small blasts.</summary>
    public void Defeat()
    {
        _windingUp = false;
        _defeatStartedAt = Time.time;
        _nextBlastAt = Time.time;
    }

    /// <summary>The final blast. The boss disappears inside it.</summary>
    public void Explode()
    {
        if (_exploded) return;
        _exploded = true;
        PlayBlast(transform.position, _finalBlastSize, 2.5f);
        SetVisible(false);
    }

    public void StopEffects()
    {
        foreach (var blast in _blasts) blast.Stop();
    }

    private void Update()
    {
        if (_renderers == null || _exploded) return;

        var time = Time.time;
        var dying = _defeatStartedAt >= 0f;
        var flapSpeed = dying ? 30f : _enraged ? 12f : _windingUp ? 16f : 7f;
        var flapAmount = _windingUp ? 24f : _enraged || dying ? 18f : 10f;
        var flap = Mathf.Sin(time * flapSpeed) * flapAmount;
        _leftWing.localRotation = Quaternion.Euler(0f, 0f, -12f + flap);
        _rightWing.localRotation = Quaternion.Euler(0f, 0f, 12f - flap);
        _leftFoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(time * 5f) * 5f);
        _rightFoot.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Sin(time * 5f) * 5f);

        if (dying)
        {
            UpdateDefeatShow(time);
            return;
        }

        if (time < _hitFlashUntil)
        {
            var pulse = Mathf.PingPong(time * 24f, 1f);
            SetColor(Color.Lerp(Color.white, new Color(1f, 0.25f, 0.2f), pulse));
        }
        else
        {
            SetColor(Color.white);
        }
    }

    private void UpdateDefeatShow(float time)
    {
        var progress = Mathf.Clamp01((time - _defeatStartedAt) / _defeatDuration);

        // Driven by time rather than Random, so the shake freezes with everything else on pause.
        var shake = 0.06f + 0.18f * progress;
        _visualRoot.localPosition = _visualPosition +
            new Vector3(Mathf.Sin(time * 53f), Mathf.Sin(time * 71f), 0f) * shake;
        _visualRoot.localScale = _visualScale * Mathf.Lerp(1f, _swellScale, progress * progress);

        var flicker = Mathf.PingPong(time * (8f + 30f * progress), 1f);
        SetColor(Color.Lerp(Color.white, new Color(1f, 0.45f, 0.2f), flicker));

        if (time < _nextBlastAt || _body == null) return;
        var bounds = _body.bounds;
        var point = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y));
        PlayBlast(point, Random.Range(1.2f, 2.2f), 1f);
        // Blasts come faster as the boom approaches.
        _nextBlastAt = time + _blastInterval * Mathf.Lerp(1f, 0.45f, progress);
    }

    private void PlayBlast(Vector2 position, float size, float duration)
    {
        if (_blasts.Length == 0) return;
        _blasts[_nextBlast].Play(position, size, duration);
        _nextBlast = (_nextBlast + 1) % _blasts.Length;
    }

    private void SetVisible(bool visible)
    {
        foreach (var spriteRenderer in _renderers) spriteRenderer.enabled = visible;
    }

    private void SetColor(Color color)
    {
        foreach (var spriteRenderer in _renderers) spriteRenderer.color = color;
    }

    private void OnDestroy()
    {
        foreach (var blast in _blasts)
        {
            if (blast != null) Destroy(blast.gameObject);
        }
    }
}
