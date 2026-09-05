using System;

/// <summary>
/// Everything the rest of the game is allowed to know about the current run.
///
/// This interface is a readable contract, not just a technical one: it answers "what is the
/// GameManager responsible for exposing?" in one screen. If it ever starts growing in unrelated
/// directions, that is the signal that GameManager has taken on a job belonging in its own manager.
/// </summary>
public interface IGameManager
{
    int  Score       { get; }
    int  HighScore   { get; }
    int  Lives       { get; }
    bool GameOver    { get; }
    bool PlayerAlive { get; }

    event Action<int, int> OnScoreChanged;   // score, high score
    event Action<int>      OnLivesChanged;
    event Action           OnGameStarted;
    event Action           OnGameOver;
    event Action           OnPlayerDied;

    void StartGame();
    void RestartGame();
    void AddScore(int amount);
    void OnPlayerHit();
}
