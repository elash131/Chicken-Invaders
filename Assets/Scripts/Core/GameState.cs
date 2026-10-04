public enum GameState
{
    Menu,
    WaveIntro,
    Playing,
    Respawning,
    BossFight,
    Paused,
    GameOver,
    Victory,
    // The flock reached the ship's line: a short, uncontrollable lead-in to Game Over.
    Breakthrough
}
