using UnityEngine;

/// <summary>A gentle up-and-down float for a UI element, such as the title logo.</summary>
[RequireComponent(typeof(RectTransform))]
public sealed class UIBob : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _height = 8f;
    [SerializeField, Min(0f)] private float _speed = 1.3f;

    private RectTransform _rect;
    private Vector2 _restPosition;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _restPosition = _rect.anchoredPosition;
    }

    // Unscaled time, so the menu keeps breathing even while the game is paused.
    private void Update()
    {
        _rect.anchoredPosition = _restPosition + Vector2.up * (Mathf.Sin(Time.unscaledTime * _speed) * _height);
    }
}
