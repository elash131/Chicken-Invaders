using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Plays music and sound effects. Music follows GameManager's run state; gameplay code asks for
/// one-shots by name with <see cref="Play"/>. Like the UI it only listens and owns no rules.
/// </summary>
public sealed class AudioManager : Singleton<AudioManager>
{
    [Header("Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioLowPassFilter _musicFilter;
    [Tooltip("Template for the effect voices. It is cloned so each voice can have its own pitch.")]
    [SerializeField] private AudioSource _effectSource;
    [SerializeField, Min(1)] private int _voiceCount = 8;

    [Header("Configuration")]
    [SerializeField] private AudioConfig _config;

    private const float OpenCutoff = 22000f;

    private readonly Dictionary<SoundEffect, AudioConfig.SoundEntry> _effectLookup = new();
    private readonly Dictionary<SoundEffect, float> _lastPlayedAt = new();
    private AudioSource[] _voices = Array.Empty<AudioSource>();
    private int _nextVoice;
    private IGameManager _game;
    private InputAction _mute;
    private GameState _previousState = GameState.Menu;
    private Coroutine _musicRoutine;
    private Coroutine _cueRoutine;
    private float _duck = 1f;
    private float _fade = 1f;
    private bool _muted;

    /// <summary>Plays a one-shot if an AudioManager exists. Safe to call from anywhere.</summary>
    public static void Play(SoundEffect effect, float pitch = 1f)
    {
        if (HasInstance) Instance.PlayEffect(effect, pitch);
    }

    protected override void Awake()
    {
        base.Awake();
        if (!enabled) return;

        if (_config != null)
        {
            foreach (var entry in _config.Effects) _effectLookup[entry.Effect] = entry;
        }
        CreateVoices();

        _muted = PlayerPrefs.GetInt(Constants.MutedKey, 0) == 1;
        AudioListener.volume = _muted ? 0f : 1f;
        var map = InputSystem.actions.FindActionMap(Constants.PlayerActionMap, true);
        _mute = map.FindAction(Constants.MuteAction, true);
    }

    private void CreateVoices()
    {
        if (_effectSource == null) return;
        if (_effectSource.gameObject == gameObject)
        {
            // Cloning a component clones its whole GameObject, which would copy this manager too.
            Debug.LogError("The effect AudioSource must sit on its own child object.", this);
            _voices = new[] { _effectSource };
            return;
        }

        _voices = new AudioSource[_voiceCount];
        _voices[0] = _effectSource;
        for (var i = 1; i < _voiceCount; i++)
        {
            _voices[i] = Instantiate(_effectSource, _effectSource.transform.parent);
            _voices[i].name = $"{_effectSource.name} {i + 1}";
        }
    }

    private void Start()
    {
        _game = GameManager.Instance;
        if (_game == null || _config == null || _musicSource == null || _effectSource == null)
        {
            Debug.LogError("AudioManager needs a GameManager, its audio config and its music and effect sources.", this);
            enabled = false;
            return;
        }

        _game.OnStateChanged += HandleStateChanged;
        _game.OnPlayerDied += HandlePlayerDied;
        SetMusicFilter(OpenCutoff);
        PlayMusic(_config.MenuMusic);
    }

    private void Update()
    {
        if (_mute.WasPressedThisFrame()) ToggleMute();

        // Unscaled time, because pause sets timeScale to zero and the music keeps playing.
        _duck = Mathf.MoveTowards(_duck, 1f, Time.unscaledDeltaTime / Mathf.Max(0.01f, _config.DuckRecovery));
        _musicSource.volume = _config.MusicVolume * _fade * _duck;
    }

    public void ToggleMute()
    {
        _muted = !_muted;
        AudioListener.volume = _muted ? 0f : 1f;
        PlayerPrefs.SetInt(Constants.MutedKey, _muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>Briefly lowers the music so a big moment cuts through it.</summary>
    public static void DuckMusic()
    {
        if (HasInstance && Instance._config != null) Instance._duck = Instance._config.DuckVolume;
    }

    private void PlayEffect(SoundEffect effect, float pitch)
    {
        if (_voices.Length == 0 || !_effectLookup.TryGetValue(effect, out var entry) ||
            entry.Clips == null || entry.Clips.Length == 0) return;

        var now = Time.unscaledTime;
        if (_lastPlayedAt.TryGetValue(effect, out var last) && now - last < entry.MinInterval) return;
        _lastPlayedAt[effect] = now;

        var clip = entry.Clips[UnityEngine.Random.Range(0, entry.Clips.Length)];
        if (clip == null) return;

        // Pitch belongs to the AudioSource, so each sound gets the next voice in turn.
        var voice = _voices[_nextVoice];
        _nextVoice = (_nextVoice + 1) % _voices.Length;
        voice.pitch = pitch + UnityEngine.Random.Range(-entry.PitchVariation, entry.PitchVariation);
        voice.PlayOneShot(clip, entry.Volume);
    }

    private void HandleStateChanged(GameState state)
    {
        var previous = _previousState;
        _previousState = state;

        if (previous == GameState.Paused) SetPaused(false);

        switch (state)
        {
            case GameState.Menu:
                PlayMusic(_config.MenuMusic);
                break;
            case GameState.WaveIntro:
                StartCue(WaveIntroCue(previous == GameState.Playing));
                break;
            case GameState.Playing:
            case GameState.BossFight:
                if (previous == GameState.Respawning) Play(SoundEffect.Respawn);
                if (state == GameState.BossFight) PlayMusic(_config.BossMusic);
                break;
            case GameState.Paused:
                SetPaused(true);
                break;
            case GameState.GameOver:
                // After the jingle the calm menu track returns, so the "one more flight?" screen is not silent.
                StartCue(ResultCue(_config.GameOverJingle, _config.MenuMusic));
                break;
            case GameState.Victory:
                StartCue(ResultCue(_config.VictoryJingle, _config.VictoryMusic));
                break;
        }
    }

    private void HandlePlayerDied() => Play(SoundEffect.PlayerExplode);

    private IEnumerator WaveIntroCue(bool waveCleared)
    {
        if (waveCleared)
        {
            Play(SoundEffect.WaveClear);
            yield return new WaitForSeconds(0.6f);
        }

        if (_game.IsBossStage)
        {
            // Silence before the boss makes her alarm and her own track land harder.
            PlayMusic(null);
            Play(SoundEffect.BossAlarm);
        }
        else
        {
            PlayMusic(_config.GameMusic);
            Play(SoundEffect.WaveStart);
        }
        _cueRoutine = null;
    }

    private IEnumerator ResultCue(AudioClip jingle, AudioClip followUp)
    {
        PlayMusic(null);
        var wait = 0f;
        if (jingle != null)
        {
            _voices[0].pitch = 1f;
            _voices[0].PlayOneShot(jingle);
            wait = jingle.length;
        }
        yield return new WaitForSecondsRealtime(wait + 0.3f);
        if (followUp != null) PlayMusic(followUp);
        _cueRoutine = null;
    }

    private void StartCue(IEnumerator cue)
    {
        if (_cueRoutine != null) StopCoroutine(_cueRoutine);
        _cueRoutine = StartCoroutine(cue);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (_musicSource.clip == clip && (clip == null || _musicSource.isPlaying)) return;
        if (_musicRoutine != null) StopCoroutine(_musicRoutine);
        _musicRoutine = StartCoroutine(CrossFade(clip));
    }

    // Fades out, swaps the clip, fades back in - the CanvasGroup fade idea, applied to music.
    private IEnumerator CrossFade(AudioClip clip)
    {
        if (_musicSource.isPlaying)
        {
            for (var t = 0f; t < 1f; t += Time.unscaledDeltaTime / Mathf.Max(0.01f, _config.FadeDuration))
            {
                _fade = Mathf.Min(_fade, 1f - t);
                yield return null;
            }
        }

        _fade = 0f;
        _musicSource.Stop();
        _musicSource.clip = clip;
        if (clip == null)
        {
            _musicRoutine = null;
            yield break;
        }

        _musicSource.loop = true;
        _musicSource.Play();
        for (var t = 0f; t < 1f; t += Time.unscaledDeltaTime / Mathf.Max(0.01f, _config.FadeDuration))
        {
            _fade = t;
            yield return null;
        }
        _fade = 1f;
        _musicRoutine = null;
    }

    private void SetPaused(bool paused)
    {
        SetMusicFilter(paused ? _config.PausedCutoff : OpenCutoff);
        foreach (var voice in _voices)
        {
            if (paused) voice.Pause();
            else voice.UnPause();
        }
    }

    private void SetMusicFilter(float cutoff)
    {
        if (_musicFilter != null) _musicFilter.cutoffFrequency = cutoff;
    }

    protected override void OnDestroy()
    {
        if (_game != null)
        {
            _game.OnStateChanged -= HandleStateChanged;
            _game.OnPlayerDied -= HandlePlayerDied;
        }
        base.OnDestroy();
    }
}
