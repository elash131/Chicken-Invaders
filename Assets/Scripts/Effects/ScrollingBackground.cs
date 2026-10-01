using UnityEngine;

/// <summary>
/// Scrolls a seamless starfield downwards for ever.
///
/// The sprite is drawn with Draw Mode = Tiled and sized to cover more than the screen, so the
/// object repeats instead of being stretched. Once it has travelled exactly one tile height it
/// snaps back up by that same amount, which is invisible because the tile is seamless.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 1.2f;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Transform _transform;
    private float _startY;
    private float _tileHeight;

    private void Awake()
    {
        _transform = transform;
        _startY = _transform.position.y;

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // The snap distance is read from the sprite itself rather than typed into the Inspector.
        // A hand-entered value that is even slightly off is exactly what makes the seam visible
        // every few seconds instead of never.
        _tileHeight = _spriteRenderer.sprite.bounds.size.y;

        WarnIfSizeIsNotAWholeNumberOfTiles();
    }

    /// <summary>
    /// Tiled draw mode divides the renderer's Size into a whole number of tiles. If Size is not an
    /// exact multiple of the sprite, the tiles get resized to fit - so they are no longer
    /// <see cref="_tileHeight"/> tall, and snapping by that amount jumps.
    /// </summary>
    private void WarnIfSizeIsNotAWholeNumberOfTiles()
    {
        var tilesTall = _spriteRenderer.size.y / _tileHeight;
        if (Mathf.Abs(tilesTall - Mathf.Round(tilesTall)) > 0.001f)
        {
            Debug.LogWarning(
                $"{name}: SpriteRenderer Size Y is {_spriteRenderer.size.y}, which is " +
                $"{tilesTall:0.00} tiles. Set it to a whole multiple of {_tileHeight} or the " +
                "scroll will visibly jump.", this);
        }
    }

    private void Update()
    {
        var position = _transform.position;
        position.y -= _scrollSpeed * Time.deltaTime;

        if (position.y <= _startY - _tileHeight)
        {
            position.y += _tileHeight;
        }

        _transform.position = position;
    }
}
