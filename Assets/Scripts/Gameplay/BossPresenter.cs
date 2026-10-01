using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns the layered Mother Hen artwork and short, bounded visual feedback.</summary>
public sealed class BossPresenter : MonoBehaviour
{
    private readonly List<SpriteRenderer> _renderers = new(7);

    private BossConfig _config;
    private Transform _visualRoot;
    private Transform _head;
    private Transform _leftWing;
    private Transform _rightWing;
    private Transform _leftFoot;
    private Transform _rightFoot;
    private SpriteRenderer _headRenderer;
    private Color _baseColor = Color.white;
    private float _hitFlashUntil;
    private float _defeatStartedAt = -1f;
    private bool _enraged;
    private bool _windingUp;

    public void Initialize(BossConfig config)
    {
        if (_config != null) return;
        _config = config;
        BuildVisuals();
        gameObject.SetActive(false);
    }

    private void BuildVisuals()
    {
        _visualRoot = new GameObject("Visuals").transform;
        _visualRoot.SetParent(transform, false);
        _visualRoot.localScale = Vector3.one * _config.VisualScale;

        if (_config.BodyBack != null)
            CreatePart("Body Back", _config.BodyBack, _config.BodyPosition, 0, _visualRoot);
        _leftFoot = CreatePart("Left Foot", _config.LeftFoot, _config.LeftFootPosition, 1, _visualRoot).transform;
        _rightFoot = CreatePart("Right Foot", _config.RightFoot, _config.RightFootPosition, 1, _visualRoot).transform;
        if (_config.BodyFront != null)
            CreatePart("Body Front", _config.BodyFront, _config.BodyPosition, 2, _visualRoot);

        _leftWing = CreatePart("Left Wing", _config.Wing, _config.LeftWingPosition, 3, _visualRoot).transform;
        _leftWing.localScale = ToScale(_config.LeftWingScale);
        _rightWing = CreatePart("Right Wing", _config.RightWing, _config.RightWingPosition, 3, _visualRoot).transform;
        _rightWing.localScale = ToScale(_config.RightWingScale);

        var normalHead = GetHead(_config.NormalHeadIndex);
        if (normalHead != null)
        {
            _headRenderer = CreatePart("Head", normalHead, _config.HeadPosition, 4, _visualRoot);
            _head = _headRenderer.transform;
        }
    }

    private SpriteRenderer CreatePart(string partName, Sprite sprite, Vector2 position, int order, Transform parent)
    {
        var part = new GameObject(partName);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        var renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = _config.SpriteMaterial;
        renderer.sortingLayerName = "Chickens";
        renderer.sortingOrder = order;
        _renderers.Add(renderer);
        return renderer;
    }

    public void Show()
    {
        _enraged = false;
        _windingUp = false;
        _hitFlashUntil = 0f;
        _defeatStartedAt = -1f;
        transform.localRotation = Quaternion.identity;
        _visualRoot.localScale = Vector3.one * _config.VisualScale;
        SetHead(_config.NormalHeadIndex);
        SetColor(_baseColor);
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    public void SetVolleyWarning(bool warning)
    {
        _windingUp = warning;
        if (_defeatStartedAt < 0f)
            SetHead(warning ? _config.WindupHeadIndex :
                (_enraged ? _config.EnragedHeadIndex : _config.NormalHeadIndex));
    }

    public void FlashHit()
    {
        if (_defeatStartedAt >= 0f) return;
        _hitFlashUntil = Time.time + 0.1f;
        SetHead(_config.HurtHeadIndex);
    }

    public void Enrage()
    {
        _enraged = true;
        _windingUp = false;
        SetHead(_config.EnragedHeadIndex);
        _hitFlashUntil = Time.time + 0.22f;
    }

    public void Defeat()
    {
        _windingUp = false;
        _defeatStartedAt = Time.time;
        SetHead(_config.DefeatedHeadIndex);
    }

    private void Update()
    {
        if (_config == null || !gameObject.activeSelf) return;

        var time = Time.time;
        var flapSpeed = _enraged ? 12f : _windingUp ? 16f : 7f;
        var flapAmount = _windingUp ? 24f : _enraged ? 18f : 10f;
        var flap = Mathf.Sin(time * flapSpeed) * flapAmount;
        _leftWing.localRotation = Quaternion.Euler(0f, 0f, -12f + flap);
        _rightWing.localRotation = Quaternion.Euler(0f, 0f, 12f - flap);
        _leftFoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(time * 5f) * 5f);
        _rightFoot.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Sin(time * 5f) * 5f);
        if (_head != null)
        {
            var headPosition = _config.HeadPosition;
            _head.localPosition = new Vector3(headPosition.x,
                headPosition.y + Mathf.Sin(time * 4f) * 0.035f, 0f);
        }

        if (_defeatStartedAt >= 0f)
        {
            var progress = Mathf.Clamp01((time - _defeatStartedAt) / Mathf.Max(0.01f, _config.DefeatDuration));
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(progress * 40f) * (1f - progress) * 18f);
            _visualRoot.localScale = Vector3.one * _config.VisualScale * Mathf.Lerp(1f, 0.2f, progress * progress);
            SetColor(new Color(1f, Mathf.Lerp(1f, 0.25f, progress), Mathf.Lerp(1f, 0.1f, progress), 1f - progress));
            return;
        }

        if (time < _hitFlashUntil)
        {
            var pulse = Mathf.PingPong(time * 24f, 1f);
            SetColor(Color.Lerp(Color.white, new Color(1f, 0.25f, 0.2f), pulse));
        }
        else
        {
            SetColor(_baseColor);
            if (!_windingUp) SetHead(_enraged ? _config.EnragedHeadIndex : _config.NormalHeadIndex);
        }
    }

    private void SetColor(Color color)
    {
        foreach (var renderer in _renderers) renderer.color = color;
    }

    private void SetHead(int index)
    {
        if (_headRenderer != null) _headRenderer.sprite = GetHead(index);
    }

    private Sprite GetHead(int index)
    {
        if (_config.HeadFrames == null || _config.HeadFrames.Length == 0) return null;
        return _config.HeadFrames[Mathf.Clamp(index, 0, _config.HeadFrames.Length - 1)];
    }

    private static Vector3 ToScale(Vector2 scale) => new(scale.x, scale.y, 1f);
}
