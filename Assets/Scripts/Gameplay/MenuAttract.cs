using UnityEngine;

/// <summary>
/// The menu's living background - a 2D take on an attract mode. A small flock of decorative
/// chickens drifts and swoops behind the title, and now and then Mother Hen glides past in the
/// distance. Decoration only: no colliders, no eggs, no score. Hidden the moment a run starts.
/// </summary>
public sealed class MenuAttract : MonoBehaviour
{
    private sealed class Flyer
    {
        public Transform Transform;
        public float Direction;
        public float Speed;
        public float BaseY;
        public float Phase;
        public float Swoop;
        public float RespawnAt;
        public bool Flying;
    }

    [Header("Flock")]
    [Tooltip("A chicken with only its visuals - no Chicken script, physics or collider.")]
    [SerializeField] private Transform _chickenPrefab;
    [Tooltip("Body sprites picked at random, so the flock is mixed like the waves.")]
    [SerializeField] private Sprite[] _bodyVariants;
    [SerializeField, Min(1)] private int _flockSize = 7;
    [SerializeField] private Vector2 _speedRange = new(1f, 2.4f);
    [SerializeField] private Vector2 _scaleRange = new(0.55f, 0.95f);

    [Header("Mother Hen")]
    [SerializeField] private Transform _hen;
    [SerializeField, Min(1f)] private float _henInterval = 14f;
    [SerializeField, Min(0.1f)] private float _henSpeed = 1.1f;

    private Flyer[] _flock = System.Array.Empty<Flyer>();
    private IGameManager _game;
    private Camera _camera;
    private float _henDirection;
    private float _nextHenAt;
    private bool _showing;

    private void Start()
    {
        _game = GameManager.Instance;
        _camera = Camera.main;
        if (_game == null || _camera == null || _chickenPrefab == null)
        {
            Debug.LogError("MenuAttract needs the game, a camera and the decorative chicken prefab.", this);
            enabled = false;
            return;
        }

        // Created once at load and reused every time the menu opens.
        _flock = new Flyer[_flockSize];
        for (var i = 0; i < _flockSize; i++)
        {
            var chicken = Instantiate(_chickenPrefab, transform);
            chicken.localScale = Vector3.one * Random.Range(_scaleRange.x, _scaleRange.y);
            // The body is the only part that differs between chicken colours.
            var body = chicken.Find("Body");
            if (body != null && _bodyVariants.Length > 0)
                body.GetComponent<SpriteRenderer>().sprite = _bodyVariants[Random.Range(0, _bodyVariants.Length)];
            _flock[i] = new Flyer { Transform = chicken };
        }

        _game.OnStateChanged += HandleStateChanged;
        Show(_game.State == GameState.Menu);
    }

    private void HandleStateChanged(GameState state) => Show(state == GameState.Menu);

    private void Show(bool show)
    {
        _showing = show;
        foreach (var flyer in _flock)
        {
            flyer.Flying = false;
            flyer.Transform.gameObject.SetActive(false);
            // Staggered entries, so the flock trickles in rather than arriving as a wall.
            flyer.RespawnAt = Time.unscaledTime + Random.Range(0f, 4f);
        }

        if (_hen != null) _hen.gameObject.SetActive(false);
        _nextHenAt = Time.unscaledTime + _henInterval * 0.5f;
    }

    // Unscaled time: the menu is never paused, and returning from a paused run restores timeScale anyway.
    private void Update()
    {
        if (!_showing) return;

        var now = Time.unscaledTime;
        var dt = Time.unscaledDeltaTime;
        var left = _camera.ViewportToWorldPoint(Vector3.zero).x - 1.5f;
        var right = _camera.ViewportToWorldPoint(Vector3.right).x + 1.5f;

        foreach (var flyer in _flock)
        {
            if (!flyer.Flying)
            {
                if (now >= flyer.RespawnAt) Launch(flyer, left, right);
                continue;
            }

            var position = flyer.Transform.position;
            position.x += flyer.Direction * flyer.Speed * dt;
            var wave = Mathf.Sin(now * 1.6f + flyer.Phase);
            position.y = flyer.BaseY + wave * flyer.Swoop;
            flyer.Transform.position = position;
            flyer.Transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Cos(now * 1.6f + flyer.Phase) * flyer.Swoop * -6f);

            if (position.x < left || position.x > right)
            {
                flyer.Flying = false;
                flyer.Transform.gameObject.SetActive(false);
                flyer.RespawnAt = now + Random.Range(0.5f, 3f);
            }
        }

        UpdateHen(now, dt, left, right);
    }

    private void Launch(Flyer flyer, float left, float right)
    {
        flyer.Direction = Random.value < 0.5f ? 1f : -1f;
        flyer.Speed = Random.Range(_speedRange.x, _speedRange.y);
        flyer.BaseY = _camera.ViewportToWorldPoint(new Vector3(0f, Random.Range(0.35f, 0.85f), 0f)).y;
        flyer.Phase = Random.Range(0f, Mathf.PI * 2f);
        // Most glide gently; now and then one makes a deep swoop.
        flyer.Swoop = Random.value < 0.25f ? Random.Range(1.2f, 2f) : Random.Range(0.2f, 0.6f);
        flyer.Transform.position = new Vector3(flyer.Direction > 0f ? left : right, flyer.BaseY, 0f);
        flyer.Transform.gameObject.SetActive(true);
        flyer.Flying = true;
    }

    private void UpdateHen(float now, float dt, float left, float right)
    {
        if (_hen == null) return;

        if (!_hen.gameObject.activeSelf)
        {
            if (now < _nextHenAt) return;
            _henDirection = Random.value < 0.5f ? 1f : -1f;
            var y = _camera.ViewportToWorldPoint(new Vector3(0f, 0.78f, 0f)).y;
            _hen.position = new Vector3(_henDirection > 0f ? left - 1.5f : right + 1.5f, y, 0f);
            _hen.gameObject.SetActive(true);
            return;
        }

        var position = _hen.position;
        position.x += _henDirection * _henSpeed * dt;
        position.y += Mathf.Sin(now * 0.9f) * 0.15f * dt;
        _hen.position = position;
        if (position.x < left - 2f || position.x > right + 2f)
        {
            _hen.gameObject.SetActive(false);
            _nextHenAt = now + _henInterval;
        }
    }

    private void OnDestroy()
    {
        if (_game != null) _game.OnStateChanged -= HandleStateChanged;
    }
}
