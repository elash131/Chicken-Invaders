using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Audio", menuName = "Chicken Invaders/Audio")]
public sealed class AudioConfig : ScriptableObject
{
    [Serializable]
    public sealed class SoundEntry
    {
        [SerializeField] private SoundEffect _effect;
        [Tooltip("One is picked at random each time, so repeated sounds do not feel identical.")]
        [SerializeField] private AudioClip[] _clips;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(0f, 0.3f)] private float _pitchVariation = 0.05f;
        [Tooltip("Ignores repeats closer than this, so a burst of eggs does not stack into noise.")]
        [SerializeField, Min(0f)] private float _minInterval = 0.03f;

        public SoundEffect Effect => _effect;
        public AudioClip[] Clips => _clips;
        public float Volume => _volume;
        public float PitchVariation => _pitchVariation;
        public float MinInterval => _minInterval;
    }

    [Header("Music")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _gameMusic;
    [SerializeField] private AudioClip _bossMusic;
    [SerializeField] private AudioClip _victoryMusic;
    [SerializeField] private AudioClip _victoryJingle;
    [SerializeField] private AudioClip _gameOverJingle;
    [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.5f;
    [SerializeField, Min(0f)] private float _fadeDuration = 0.6f;

    [Header("Effects")]
    [SerializeField] private SoundEntry[] _effects = Array.Empty<SoundEntry>();

    [Header("Feel")]
    [Tooltip("Low-pass cutoff while paused: the music sounds muffled instead of stopping.")]
    [SerializeField, Min(10f)] private float _pausedCutoff = 700f;
    [Tooltip("Music volume multiplier right after a big moment, such as Mother Hen's final blast.")]
    [SerializeField, Range(0f, 1f)] private float _duckVolume = 0.25f;
    [SerializeField, Min(0f)] private float _duckRecovery = 1.6f;

    public AudioClip MenuMusic => _menuMusic;
    public AudioClip GameMusic => _gameMusic;
    public AudioClip BossMusic => _bossMusic;
    public AudioClip VictoryMusic => _victoryMusic;
    public AudioClip VictoryJingle => _victoryJingle;
    public AudioClip GameOverJingle => _gameOverJingle;
    public float MusicVolume => _musicVolume;
    public float FadeDuration => _fadeDuration;
    public SoundEntry[] Effects => _effects;
    public float PausedCutoff => _pausedCutoff;
    public float DuckVolume => _duckVolume;
    public float DuckRecovery => _duckRecovery;
}
