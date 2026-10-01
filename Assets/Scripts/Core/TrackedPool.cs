using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Unity's ObjectPool plus a record of which objects are currently out. That record lets an owner
/// release everything at once (restart, end of wave) and refuse a second release of the same object,
/// which is how a hit and an expiry on the same frame stay harmless. Bullets, eggs and pickups each
/// own one of these instead of repeating the bookkeeping.
/// </summary>
public sealed class TrackedPool<T> where T : Component
{
    private readonly ObjectPool<T> _pool;
    private readonly HashSet<T> _active = new();
    private readonly List<T> _scratch = new();

    public int ActiveCount => _active.Count;
    public IReadOnlyCollection<T> Active => _active;

    /// <param name="create">Makes one new object, inactive.</param>
    /// <param name="resetForPool">Clears an object's state and deactivates it on its way back.</param>
    public TrackedPool(Func<T> create, Action<T> resetForPool, int prewarm, int maxRetained)
    {
        _pool = new ObjectPool<T>(
            create,
            item => _active.Add(item),
            item =>
            {
                _active.Remove(item);
                resetForPool(item);
            },
            item =>
            {
                if (item != null) UnityEngine.Object.Destroy(item.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: prewarm,
            maxSize: maxRetained);

        // Pay the Instantiate cost at load time rather than on the first shot.
        for (var i = 0; i < prewarm; i++) _scratch.Add(_pool.Get());
        foreach (var item in _scratch) _pool.Release(item);
        _scratch.Clear();
    }

    public T Get() => _pool.Get();

    public bool IsOut(T item) => item != null && _active.Contains(item);

    /// <summary>Returns an object; ignores anything that is not out, so a second release is harmless.</summary>
    public void Release(T item)
    {
        if (IsOut(item)) _pool.Release(item);
    }

    public void ReleaseAll()
    {
        // Copied first: releasing changes the set being walked.
        _scratch.Clear();
        _scratch.AddRange(_active);
        foreach (var item in _scratch) Release(item);
        _scratch.Clear();
    }

    public void Dispose()
    {
        ReleaseAll();
        _pool.Dispose();
    }
}
