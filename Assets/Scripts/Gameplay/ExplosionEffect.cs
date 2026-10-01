using UnityEngine;

/// <summary>
/// A short, reusable 2D flash that expands and fades, then hides itself. Used for the ship's death
/// and for Mother Hen's defeat; callers keep their instances and replay them instead of spawning.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField, Min(0.01f)] private float _duration = 0.55f;
    [SerializeField, Min(0f)] private float _startScale = 0.2f;
    [SerializeField, Min(0f)] private float _endScale = 0.85f;

    private Color _baseColor;
    private float _elapsed;
    private float _playDuration;
    private float _sizeMultiplier = 1f;
    private bool _playing;

    private void Awake()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseColor = _spriteRenderer.color;
    }

    public void Play(Vector2 worldPosition, float sizeMultiplier = 1f, float durationMultiplier = 1f)
    {
        _sizeMultiplier = sizeMultiplier;
        _playDuration = _duration * Mathf.Max(0.1f, durationMultiplier);
        transform.position = worldPosition;
        transform.localScale = Vector3.one * (_startScale * _sizeMultiplier);
        _elapsed = 0f;
        _playing = true;

        var color = _baseColor;
        color.a = 1f;
        _spriteRenderer.color = color;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!_playing) return;

        _elapsed += Time.deltaTime;
        var progress = Mathf.Clamp01(_elapsed / _playDuration);
        var eased = Mathf.SmoothStep(0f, 1f, progress);
        transform.localScale = Vector3.one * (Mathf.Lerp(_startScale, _endScale, eased) * _sizeMultiplier);

        var color = _baseColor;
        color.a = 1f - eased;
        _spriteRenderer.color = color;

        if (progress >= 1f) Stop();
    }

    public void Stop()
    {
        _playing = false;
        _elapsed = 0f;
        gameObject.SetActive(false);
    }
}
