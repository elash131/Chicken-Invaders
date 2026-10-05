using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// A camera-child starfield that tiles across the whole view, including wider windows.
/// Extra rows cover a full scroll cycle; whole tiles keep the wrap seamless.
/// </summary>
[ExecuteAlways, RequireComponent(typeof(SpriteRenderer))]
public sealed class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 1.2f;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Camera _camera;
    private float _scrollOffset;

    private void OnEnable()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        _camera = GetComponentInParent<Camera>();
        FitToCamera();
        RenderPipelineManager.beginCameraRendering += BeforeCameraRendering;
    }

    private void OnDisable() => RenderPipelineManager.beginCameraRendering -= BeforeCameraRendering;

    private void BeforeCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
    {
        // Also refresh when the editor Game view changes size without entering Play mode.
        if (renderingCamera == _camera) FitToCamera();
    }

    private void Update()
    {
        if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;
        var tileHeight = _spriteRenderer.sprite.bounds.size.y;
        if (Application.IsPlaying(gameObject) && tileHeight > 0f)
        {
            _scrollOffset = Mathf.Repeat(_scrollOffset + _scrollSpeed * Time.deltaTime, tileHeight);
            var position = transform.localPosition;
            position.y = -_scrollOffset;
            transform.localPosition = position;
        }
        FitToCamera();
    }

    private void FitToCamera()
    {
        if (_camera == null || !_camera.orthographic ||
            _spriteRenderer == null || _spriteRenderer.sprite == null) return;

        var tile = _spriteRenderer.sprite.bounds.size;
        if (tile.x <= 0f || tile.y <= 0f) return;

        // Overscan also covers the brief zoom-out cues. Tiling preserves the artwork's proportions.
        var height = 2f * _camera.orthographicSize * 1.2f;
        var width = height * _camera.aspect;
        var size = new Vector2(
            Mathf.Ceil(width / tile.x) * tile.x,
            (Mathf.Ceil(height / tile.y) + 2f) * tile.y);
        if (_spriteRenderer.drawMode != SpriteDrawMode.Tiled)
            _spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        if (_spriteRenderer.size != size) _spriteRenderer.size = size;
    }
}
