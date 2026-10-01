using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Brief boss camera cues; gameplay always sees the unmodified camera.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public sealed class BossCameraFeedback : MonoBehaviour
{
    private Camera _camera;
    private GameManager _game;
    private GameBalanceConfig _balance;
    private GameBalanceConfig.CameraCue _cue;
    private Vector3 _restPosition;
    private float _restSize;
    private float _elapsed;
    private bool _playing;
    private bool _rendering;
    private bool _subscribed;
    private bool _bossEntered;
    private bool _enraged;

    public void Initialize(GameManager game, GameBalanceConfig balance)
    {
        if (_game != null) return;
        _camera = GetComponent<Camera>();
        if (game == null || balance == null || !_camera.orthographic)
        {
            Debug.LogError("BossCameraFeedback needs the game, balance and an orthographic camera.", this);
            return;
        }

        _game = game;
        _balance = balance;
        if (isActiveAndEnabled) Subscribe();
    }

    private void OnEnable()
    {
        if (_game != null) Subscribe();
    }

    private void Subscribe()
    {
        if (_subscribed) return;
        _subscribed = true;
        _game.OnGameStarted += ResetRun;
        _game.OnStateChanged += HandleStateChanged;
        RenderPipelineManager.beginCameraRendering += BeginRendering;
        RenderPipelineManager.endCameraRendering += EndRendering;
        HandleStateChanged(_game.State);
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Menu)
        {
            ResetRun();
        }
        else if (state == GameState.BossFight && !_bossEntered)
        {
            // Returning from pause or respawn must not replay the entrance.
            _bossEntered = true;
            Play(_balance.BossEntranceCamera);
        }
        else if (state == GameState.Victory && _bossEntered)
        {
            Play(_balance.BossDefeatCamera);
        }
        else if (state == GameState.GameOver || state == GameState.WaveIntro)
        {
            StopFeedback();
        }
    }

    /// <summary>Called once when Mother Hen crosses her half-health threshold.</summary>
    public void PlayEnrage()
    {
        if (!_subscribed || !_bossEntered || _enraged || !_game.CanEnemiesAct) return;
        _enraged = true;
        Play(_balance.BossEnrageCamera);
    }

    private void Play(GameBalanceConfig.CameraCue cue)
    {
        StopFeedback();
        _cue = cue;
        _playing = cue.Duration > 0f;
    }

    private void Update()
    {
        if (!_playing || _game == null || _game.State == GameState.Paused) return;
        _elapsed += Time.deltaTime;
        if (_elapsed >= _cue.Duration) StopFeedback();
    }

    private void BeginRendering(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera != _camera || _rendering || !_playing ||
            _game == null || _game.State == GameState.Paused) return;

        // URP invokes this before camera data/culling, then EndRendering after drawing.
        // Update/physics, egg-floor checks and viewport bounds never see this offset.
        _restPosition = _camera.transform.position;
        _restSize = _camera.orthographicSize;
        _rendering = true;

        var progress = Mathf.Clamp01(_elapsed / _cue.Duration);
        var envelope = progress < 0.2f
            ? Mathf.SmoothStep(0f, 1f, progress / 0.2f)
            : 1f - Mathf.SmoothStep(0f, 1f, (progress - 0.2f) / 0.8f);
        var extraSize = _restSize * _cue.ZoomOutFraction;
        // Shake stays inside the extra view margin, so edge hazards are not cropped.
        var amplitude = Mathf.Min(_cue.ShakeDistance,
            extraSize * Mathf.Min(1f, _camera.aspect) * 0.8f) * envelope;
        var offset = new Vector3(Mathf.Sin(_elapsed * 47f), Mathf.Sin(_elapsed * 61f), 0f) * amplitude;
        _camera.orthographicSize = _restSize + extraSize * envelope;
        _camera.transform.position = _restPosition + offset;
    }

    private void EndRendering(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera == _camera) RestoreCamera();
    }

    private void RestoreCamera()
    {
        if (!_rendering) return;
        if (_camera != null)
        {
            _camera.transform.position = _restPosition;
            _camera.orthographicSize = _restSize;
        }
        _rendering = false;
    }

    private void StopFeedback()
    {
        RestoreCamera();
        _playing = false;
        _elapsed = 0f;
    }

    private void ResetRun()
    {
        StopFeedback();
        _bossEntered = false;
        _enraged = false;
    }

    private void OnDisable()
    {
        StopFeedback();
        if (!_subscribed) return;
        RenderPipelineManager.beginCameraRendering -= BeginRendering;
        RenderPipelineManager.endCameraRendering -= EndRendering;
        if (_game != null)
        {
            _game.OnGameStarted -= ResetRun;
            _game.OnStateChanged -= HandleStateChanged;
        }
        _subscribed = false;
    }
}
