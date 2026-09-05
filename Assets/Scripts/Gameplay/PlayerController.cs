using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The ship. Moves left and right along the bottom of the screen and nothing else - no thrust,
/// no inertia: input released means motion stopped on the same frame, so every death is readable.
///
/// The ship art is eight banking poses, not an animation: frame 0 leans fully left, the middle
/// frames are level, the last leans fully right. So the sprite is chosen from the current input
/// rather than played on a loop by an Animator - the ship banks because the player is turning,
/// which is the only reason it should ever bank.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Singleton<PlayerController>
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 8f;

    [Header("Banking")]
    [Tooltip("Ship poses, left-most lean first, level in the middle, right-most lean last.")]
    [SerializeField] private Sprite[] _bankFrames;

    [Tooltip("How fast the ship rolls into a turn, in bank units per second. Lower feels heavier.")]
    [SerializeField] private float _bankResponse = 6f;

    [Header("References")]
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private InputAction _move;

    private Vector3 _spawnPosition;
    private float _horizontal;
    private float _bank;
    private float _minX;
    private float _maxX;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    private void Awake()
    {
        // Local state and local references only - nothing here reaches out to another object.
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

        _spawnPosition = transform.position;

        var playerMap = InputSystem.actions.FindActionMap(Constants.PlayerActionMap, throwIfNotFound: true);
        _move = playerMap.FindAction(Constants.MoveAction, throwIfNotFound: true);
    }

    private void Start()
    {
        CalculateBounds();
    }

    /// <summary>
    /// The limits come from the camera, not from numbers typed into the Inspector. Hard-coding a
    /// world position works at exactly one aspect ratio and lets the ship walk off a wider screen.
    /// </summary>
    private void CalculateBounds()
    {
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;

        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No camera tagged MainCamera - the player cannot work out its limits.");
            return;
        }

        var halfScreenWidth = mainCamera.orthographicSize * mainCamera.aspect;
        var halfShipWidth = _spriteRenderer != null ? _spriteRenderer.bounds.extents.x : 0f;

        _minX = -halfScreenWidth + halfShipWidth;
        _maxX = halfScreenWidth - halfShipWidth;
    }

    private void Update()
    {
        // Resizing the window changes the aspect, which moves the walls.
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            CalculateBounds();
        }

        if (!GameManager.Instance.PlayerAlive)
        {
            return;
        }

        // Move is a Vector2 action; this game only uses its horizontal axis. Read in Update so the
        // pose responds on the frame the key goes down, then applied in FixedUpdate.
        _horizontal = _move.ReadValue<Vector2>().x;

        UpdateBankPose();
    }

    /// <summary>
    /// Eases the ship towards the pose its input asks for, so a tap does not snap it to a full
    /// lean and releasing does not snap it back level.
    /// </summary>
    private void UpdateBankPose()
    {
        if (_bankFrames == null || _bankFrames.Length == 0)
        {
            return;
        }

        _bank = Mathf.MoveTowards(_bank, _horizontal, _bankResponse * Time.deltaTime);

        // -1 (full left) .. +1 (full right) maps onto the frame list, first frame to last.
        var normalised = (_bank + 1f) * 0.5f;
        var index = Mathf.Clamp(Mathf.RoundToInt(normalised * (_bankFrames.Length - 1)),
                                0, _bankFrames.Length - 1);

        _spriteRenderer.sprite = _bankFrames[index];
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.PlayerAlive)
        {
            return;
        }

        var position = _rigidbody2D.position;
        position.x = Mathf.Clamp(position.x + _horizontal * _moveSpeed * Time.fixedDeltaTime, _minX, _maxX);

        _rigidbody2D.MovePosition(position);
    }

    /// <summary>
    /// Puts the ship back at its starting point. The transform and the Rigidbody2D hold separate
    /// poses and Physics2D does not sync them automatically, so both are set.
    /// </summary>
    public void RespawnPlayer()
    {
        transform.position = _spawnPosition;
        _rigidbody2D.position = _spawnPosition;
        _rigidbody2D.linearVelocity = Vector2.zero;

        _horizontal = 0f;
        _bank = 0f;
        UpdateBankPose();

        _spriteRenderer.enabled = true;
    }

    public void HidePlayer()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
        _spriteRenderer.enabled = false;
    }

    /// <summary>
    /// The lane the ship may move along. Invisible at runtime, so without a Gizmo it cannot be
    /// checked in the Scene view.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        var halfScreenWidth = mainCamera.orthographicSize * mainCamera.aspect;
        var y = transform.position.y;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(-halfScreenWidth, y, 0f), new Vector3(halfScreenWidth, y, 0f));
    }
}
