using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A full-screen red flash when the flock breaks through and hits the ship. Listens to GameManager
/// and owns no rules; the image is invisible and ignores clicks the rest of the time.
/// </summary>
[RequireComponent(typeof(Image))]
public sealed class ImpactFlash : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float _peakAlpha = 0.65f;
    [SerializeField, Min(0.05f)] private float _fadeDuration = 0.6f;

    private Image _image;
    private IGameManager _game;
    private float _flashStartedAt = float.NegativeInfinity;

    private void Awake()
    {
        _image = GetComponent<Image>();
        SetAlpha(0f);
    }

    private void Start()
    {
        _game = GameManager.Instance;
        if (_game != null) _game.OnBreakthroughImpact += Flash;
    }

    private void Flash() => _flashStartedAt = Time.unscaledTime;

    private void Update()
    {
        var progress = (Time.unscaledTime - _flashStartedAt) / _fadeDuration;
        SetAlpha(progress < 1f ? _peakAlpha * (1f - progress) : 0f);
    }

    private void SetAlpha(float alpha)
    {
        if (Mathf.Approximately(_image.color.a, alpha)) return;
        var color = _image.color;
        color.a = alpha;
        _image.color = color;
    }

    private void OnDestroy()
    {
        if (_game != null) _game.OnBreakthroughImpact -= Flash;
    }
}
