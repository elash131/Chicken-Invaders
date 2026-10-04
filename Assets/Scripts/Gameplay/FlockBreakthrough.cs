using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The flock's final charge when it reaches the ship's line: every chicken dives onto the ship at
/// once, slightly staggered and speeding up, so the loss is seen rather than just announced.
/// WaveManager starts it; GameManager decides when the ship is hit.
/// </summary>
public sealed class FlockBreakthrough
{
    private sealed class Charge
    {
        public Chicken Chicken;
        public Vector2 Start;
        public Vector2 Control;
        public Vector2 End;
        public Vector2 LastPosition;
        public float Delay;
    }

    // The latest a chicken starts its dive, so the flock arrives as a wave rather than in one block.
    private const float MaxStagger = 0.2f;

    private readonly List<Charge> _charges = new();
    private readonly float _duration;
    private float _elapsed;

    public FlockBreakthrough(float duration)
    {
        _duration = duration;
    }

    public void Begin(IEnumerable<Chicken> flock, Vector2 ship)
    {
        _charges.Clear();
        _elapsed = 0f;
        foreach (var chicken in flock)
        {
            if (chicken == null) continue;
            Vector2 start = chicken.transform.position;
            // Each lands on a slightly different spot around the ship, so they pile in rather than stack.
            var end = ship + new Vector2(Random.Range(-0.7f, 0.7f), Random.Range(-0.2f, 0.4f));
            _charges.Add(new Charge
            {
                Chicken = chicken,
                Start = start,
                Control = new Vector2(start.x, Mathf.Lerp(start.y, end.y, 0.4f)),
                End = end,
                LastPosition = start,
                Delay = Random.Range(0f, MaxStagger)
            });
        }
    }

    public void Clear() => _charges.Clear();

    public void Tick(float deltaTime)
    {
        _elapsed += deltaTime;
        var flightTime = Mathf.Max(0.05f, _duration - MaxStagger);
        foreach (var charge in _charges)
        {
            if (charge.Chicken == null) continue;
            var progress = Mathf.Clamp01((_elapsed - charge.Delay) / flightTime);
            // Squared, so each chicken speeds up into the ship like a dive rather than drifting down.
            var position = Bezier.Quadratic(charge.Start, charge.Control, charge.End, progress * progress);
            var velocity = position - charge.LastPosition;
            if (velocity.sqrMagnitude > 0.0001f)
                charge.Chicken.SetTilt(Vector2.SignedAngle(Vector2.down, velocity) * 0.5f);
            charge.Chicken.MoveTo(position);
            charge.LastPosition = position;
        }
    }
}
