using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Galaga-style dive bombers. A chosen chicken wobbles as a warning, swoops down through the ship's
/// lane dropping one aimed egg, falls off the bottom, re-enters from the top and flies back to its
/// slot. WaveManager decides when a dive starts; this class only flies it.
/// </summary>
public sealed class ChickenDives
{
    private enum Stage
    {
        Warning,
        Swoop,
        Return
    }

    private sealed class Dive
    {
        public Chicken Chicken;
        public Stage Stage;
        public float Elapsed;
        public Vector2 Start;
        public Vector2 Control;
        public Vector2 End;
        public Vector2 LastPosition;
        public bool EggDropped;
    }

    private const float EggDropProgress = 0.35f;

    private readonly List<Dive> _dives = new();
    private readonly GameBalanceConfig _balance;
    private readonly Camera _camera;
    private readonly PlayerController _player;
    private readonly EggPool _eggs;
    private readonly Func<Chicken, Vector2> _slotPosition;

    public int Count => _dives.Count;

    public ChickenDives(GameBalanceConfig balance, Camera camera, PlayerController player, EggPool eggs,
        Func<Chicken, Vector2> slotPosition)
    {
        _balance = balance;
        _camera = camera;
        _player = player;
        _eggs = eggs;
        _slotPosition = slotPosition;
    }

    public bool IsDiving(Chicken chicken)
    {
        foreach (var dive in _dives)
        {
            if (dive.Chicken == chicken) return true;
        }
        return false;
    }

    public void Start(Chicken chicken)
    {
        if (chicken == null || IsDiving(chicken)) return;
        _dives.Add(new Dive { Chicken = chicken, Stage = Stage.Warning, LastPosition = _slotPosition(chicken) });
        AudioManager.Play(SoundEffect.ChickenDive, 1.25f);
    }

    public void Remove(Chicken chicken)
    {
        for (var i = _dives.Count - 1; i >= 0; i--)
        {
            if (_dives[i].Chicken == chicken) _dives.RemoveAt(i);
        }
    }

    public void Clear() => _dives.Clear();

    /// <summary>Flies every dive one step. Returns true if a diving chicken touched the ship.</summary>
    public bool Tick(float deltaTime, float speed)
    {
        var hitShip = false;
        for (var i = _dives.Count - 1; i >= 0; i--)
        {
            var dive = _dives[i];
            if (dive.Chicken == null)
            {
                _dives.RemoveAt(i);
                continue;
            }

            dive.Elapsed += deltaTime * speed;
            switch (dive.Stage)
            {
                case Stage.Warning:
                    UpdateWarning(dive);
                    break;
                case Stage.Swoop:
                    UpdateSwoop(dive);
                    hitShip |= TouchesShip(dive.Chicken);
                    break;
                case Stage.Return:
                    if (UpdateReturn(dive)) _dives.RemoveAt(i);
                    break;
            }
        }
        return hitShip;
    }

    private void UpdateWarning(Dive dive)
    {
        var slot = _slotPosition(dive.Chicken);
        dive.Chicken.MoveTo(slot);
        dive.Chicken.SetTilt(Mathf.Sin(dive.Elapsed * 45f) * 14f);
        dive.LastPosition = slot;
        if (dive.Elapsed < _balance.DiveWarning) return;

        // Curve out to one side first, then cut back through the ship's current position and on
        // past the bottom of the screen, so the player has to move rather than just wait.
        var floor = _camera.ViewportToWorldPoint(Vector3.zero).y;
        var side = slot.x > _player.transform.position.x ? 1f : -1f;
        dive.Start = slot;
        dive.Control = new Vector2(slot.x + side * _balance.DiveSwing, slot.y - 1.5f);
        dive.End = new Vector2(_player.transform.position.x, floor - 1.5f);
        dive.Stage = Stage.Swoop;
        dive.Elapsed = 0f;
    }

    private void UpdateSwoop(Dive dive)
    {
        var progress = dive.Elapsed / _balance.DiveSwoopDuration;
        var position = Bezier(dive.Start, dive.Control, dive.End, Mathf.Clamp01(progress));
        FlyTo(dive, position);

        if (!dive.EggDropped && progress >= EggDropProgress)
        {
            dive.EggDropped = true;
            var aim = ((Vector2)_player.transform.position - position).normalized;
            _eggs.Fire(position, aim.y < -0.3f ? aim : Vector2.down);
        }

        if (progress < 1f) return;

        // Off the bottom: come back in from above the slot.
        var top = _camera.ViewportToWorldPoint(Vector3.up).y;
        dive.Start = new Vector2(_slotPosition(dive.Chicken).x, top + 1.5f);
        dive.LastPosition = dive.Start;
        dive.Stage = Stage.Return;
        dive.Elapsed = 0f;
    }

    private bool UpdateReturn(Dive dive)
    {
        // The slot keeps moving with the formation, so the target is read every step.
        var progress = Mathf.Clamp01(dive.Elapsed / _balance.DiveReturnDuration);
        var slot = _slotPosition(dive.Chicken);
        FlyTo(dive, Vector2.Lerp(dive.Start, slot, Mathf.SmoothStep(0f, 1f, progress)));
        if (progress < 1f) return false;

        dive.Chicken.SetTilt(0f);
        return true;
    }

    private static void FlyTo(Dive dive, Vector2 position)
    {
        var velocity = position - dive.LastPosition;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            // Lean into the direction of flight: straight down is upright.
            dive.Chicken.SetTilt(Vector2.SignedAngle(Vector2.down, velocity) * 0.5f);
        }
        dive.Chicken.MoveTo(position);
        dive.LastPosition = position;
    }

    private bool TouchesShip(Chicken chicken)
    {
        return _player.IsVisible && chicken.Bounds.Intersects(_player.CatchBounds);
    }

    private static Vector2 Bezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        var remaining = 1f - t;
        return remaining * remaining * start + 2f * remaining * t * control + t * t * end;
    }
}
