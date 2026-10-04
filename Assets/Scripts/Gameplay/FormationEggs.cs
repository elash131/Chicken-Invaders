using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Decides when the formation lays an egg and who lays it: only the lowest living chicken in each
/// column, never one that is diving, and never one so close directly above the ship that its egg
/// could not be dodged. Timing is random but keeps the wave's average rate per eligible chicken.
/// </summary>
public sealed class FormationEggs
{
    // How long to wait before checking again when no chicken may lay right now.
    private const float RetryDelay = 0.25f;

    private readonly List<Chicken> _eligible = new(8);
    private readonly GameBalanceConfig _balance;
    private readonly PlayerController _player;
    private readonly EggPool _eggs;
    private readonly ChickenDives _dives;
    private Chicken[] _lowestPerColumn = System.Array.Empty<Chicken>();
    private WaveConfig _wave;
    private float _nextEggTime;

    public FormationEggs(GameBalanceConfig balance, PlayerController player, EggPool eggs, ChickenDives dives)
    {
        _balance = balance;
        _player = player;
        _eggs = eggs;
        _dives = dives;
    }

    public void Reset(WaveConfig wave)
    {
        _wave = wave;
        _lowestPerColumn = wave != null ? new Chicken[wave.Columns] : System.Array.Empty<Chicken>();
        _eligible.Clear();
        _nextEggTime = 0f;
    }

    public void Register(Chicken chicken)
    {
        var current = _lowestPerColumn[chicken.Column];
        if (current == null || chicken.Row > current.Row) _lowestPerColumn[chicken.Column] = chicken;
    }

    /// <summary>When a column's lowest chicken dies, the next one up takes over laying.</summary>
    public void HandleRemoved(Chicken chicken, IEnumerable<Chicken> living)
    {
        var column = chicken.Column;
        if (column < 0 || column >= _lowestPerColumn.Length || _lowestPerColumn[column] != chicken) return;

        Chicken lowest = null;
        foreach (var candidate in living)
        {
            if (candidate != null && candidate.Column == column && (lowest == null || candidate.Row > lowest.Row))
                lowest = candidate;
        }
        _lowestPerColumn[column] = lowest;
    }

    public void Begin() => ScheduleNext(RefreshEligible());

    public void Tick()
    {
        if (_wave == null || Time.time < _nextEggTime) return;

        var count = RefreshEligible();
        if (count == 0)
        {
            _nextEggTime = Time.time + RetryDelay;
            return;
        }

        var layer = _eligible[Random.Range(0, count)];
        _eggs.Fire(layer.EggSpawnPosition, Vector2.down, _wave.EggSpeedMultiplier);
        AudioManager.Play(SoundEffect.EggLay);
        ScheduleNext(count);
    }

    private int RefreshEligible()
    {
        _eligible.Clear();
        Vector2 ship = _player.transform.position;
        foreach (var chicken in _lowestPerColumn)
        {
            // A diving chicken drops its own aimed egg; it does not also lay from the formation.
            if (chicken != null && !_dives.IsDiving(chicken) && IsFairToLay(chicken.transform.position, ship))
            {
                _eligible.Add(chicken);
            }
        }
        return _eligible.Count;
    }

    // Fair means dodgeable: high enough above the ship to react, or far enough to the side that the
    // egg falls past it. Only a low chicken right above the ship stays silent, so a low flock keeps
    // up the pressure without ever dropping an egg the player cannot avoid.
    private bool IsFairToLay(Vector2 chicken, Vector2 ship)
    {
        return chicken.y - ship.y >= _balance.EggSafetyDistance ||
               Mathf.Abs(chicken.x - ship.x) >= _balance.EggSideClearance;
    }

    private void ScheduleNext(int eligibleCount)
    {
        var combinedRate = _wave.EggRatePerShooter * eligibleCount;
        if (combinedRate <= 0f)
        {
            _nextEggTime = Time.time + RetryDelay;
            return;
        }

        // An exponential interval keeps the configured average rate without synchronised volleys.
        var sample = Mathf.Clamp(Random.value, 0.0001f, 0.9999f);
        _nextEggTime = Time.time - Mathf.Log(1f - sample) / combinedRate;
    }
}
