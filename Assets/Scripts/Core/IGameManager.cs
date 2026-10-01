using System;

/// <summary>
/// The run events other systems may listen to. Listeners only present state; they never change it.
/// </summary>
public interface IGameManagerEvents
{
    event Action<GameState> OnStateChanged;
    event Action<int, int>  OnScoreChanged;        // score, high score
    event Action<int>       OnLivesChanged;
    event Action<int, int>  OnBossHealthChanged;   // current, maximum
    event Action            OnGameStarted;
    event Action            OnGameOver;
    event Action            OnPlayerDied;
}

/// <summary>
/// Everything the rest of the game is allowed to know about, and ask of, the current run.
/// If it starts growing in unrelated directions, that job belongs in its own manager.
/// </summary>
public interface IGameManager : IGameManagerEvents
{
    GameState State { get; }
    int  Score             { get; }
    int  HighScore         { get; }
    int  Lives             { get; }
    int  CurrentWaveNumber { get; }
    bool GameOver          { get; }
    bool PlayerAlive       { get; }
    bool CanControlPlayer  { get; }
    bool CanMovePlayer     { get; }
    bool CanEnemiesAct     { get; }
    bool CanDamageEnemies  { get; }
    bool CanDamagePlayer   { get; }
    bool CanRestart        { get; }

    // Commands from the menu and input
    void StartGame();
    void RestartGame();
    void PauseGame();
    void ResumeGame();
    void ReturnToMenu();

    // Reports from gameplay systems; GameManager decides what each one means for the run
    void AddScore(int amount);
    void ReportPlayerHit();
    void ReportFormationReady();
    void ReportWaveCleared();
    void ReportLoseLineCrossed();
    void ReportBossHealth(int current, int maximum);
    void ReportBossDefeated();
}
