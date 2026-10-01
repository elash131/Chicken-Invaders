using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Flies a new formation in: each chicken curves in from the side of the screen to its slot, one
/// after another. WaveManager owns the formation; this only plays its entrance.
/// </summary>
public sealed class FormationEntry
{
    private sealed class Flight
    {
        public Chicken Chicken;
        public Vector2 Start;
        public Vector2 Control;
        public float Delay;
        public float Elapsed;
        public bool Arrived;
    }

    private readonly List<Flight> _flights = new();
    private readonly float _duration;

    public FormationEntry(float duration)
    {
        _duration = duration;
    }

    public void Add(Chicken chicken, Vector2 start, Vector2 control, float delay)
    {
        _flights.Add(new Flight { Chicken = chicken, Start = start, Control = control, Delay = delay });
    }

    public void Clear() => _flights.Clear();

    /// <summary>Moves every flight one step. Returns true once all of them have reached their slots.</summary>
    public bool Tick(float deltaTime, Vector2 formationOrigin)
    {
        var allArrived = true;
        foreach (var flight in _flights)
        {
            // A chicken shot down mid-entry no longer needs to arrive.
            if (flight.Chicken == null || flight.Arrived) continue;

            flight.Elapsed += deltaTime;
            var progress = Mathf.Clamp01((flight.Elapsed - flight.Delay) / _duration);
            // The slot is read every step, because the formation can recentre on a window resize.
            var slot = formationOrigin + flight.Chicken.SlotOffset;
            if (progress >= 1f)
            {
                flight.Arrived = true;
                flight.Chicken.MoveTo(slot);
                continue;
            }

            allArrived = false;
            if (flight.Elapsed < flight.Delay) continue;
            flight.Chicken.MoveTo(Bezier.Quadratic(flight.Start, flight.Control, slot, Mathf.SmoothStep(0f, 1f, progress)));
        }
        return allArrived;
    }
}
