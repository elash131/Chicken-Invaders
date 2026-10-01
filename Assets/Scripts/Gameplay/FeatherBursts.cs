using UnityEngine;

/// <summary>
/// Feather puffs for kills and boss hits. One world-space particle system emits every burst at the
/// requested point, so it is its own pool: no objects are created or destroyed during play.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public sealed class FeatherBursts : Singleton<FeatherBursts>
{
    [SerializeField] private WaveManager _waves;
    [SerializeField, Min(1)] private int _chickenFeathers = 12;

    private ParticleSystem _particles;

    /// <summary>Puffs feathers at a point, if the effect exists. Safe to call from anywhere.</summary>
    public static void Emit(Vector2 position, int count)
    {
        if (HasInstance) Instance.EmitAt(position, count);
    }

    protected override void Awake()
    {
        base.Awake();
        if (!enabled) return;
        _particles = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        if (_waves != null) _waves.OnChickenKilled += EmitChickenFeathers;
    }

    private void EmitChickenFeathers(Vector2 position) => EmitAt(position, _chickenFeathers);

    private void EmitAt(Vector2 position, int count)
    {
        // Shape and start values still apply; only the origin moves to the hit.
        var emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            applyShapeToPosition = true
        };
        _particles.Emit(emitParams, count);
    }

    protected override void OnDestroy()
    {
        if (_waves != null) _waves.OnChickenKilled -= EmitChickenFeathers;
        base.OnDestroy();
    }
}
