using UnityEngine;

/// <summary>
/// Keeps the gameplay view at one fixed aspect ratio by shrinking the camera's viewport and leaving
/// black bars. Every window size then shows exactly the same world, so formation width, ship lane,
/// boss path and background coverage never depend on the player's resolution.
/// </summary>
[DefaultExecutionOrder(-100), RequireComponent(typeof(Camera))]
public sealed class LetterboxCamera : MonoBehaviour
{
    [SerializeField] private Vector2 _targetAspect = new(16f, 9f);

    private Camera _camera;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    // Awake, so systems that read the camera in Start already see the final viewport.
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        Apply();
    }

    private void Update()
    {
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight) Apply();
    }

    private void Apply()
    {
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;

        var target = _targetAspect.x / _targetAspect.y;
        var window = Screen.width / (float)Mathf.Max(1, Screen.height);

        if (window > target)
        {
            // Wider than the game: bars left and right.
            var width = target / window;
            _camera.rect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
        }
        else
        {
            // Taller than the game: bars top and bottom.
            var height = window / target;
            _camera.rect = new Rect(0f, (1f - height) * 0.5f, 1f, height);
        }
    }
}
