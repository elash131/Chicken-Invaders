using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// The Feast Streak: every chicken drops food, and the faster the kills come the better the food.
/// Owns the food pool, the kill streak and catching. Points still go through GameManager.AddScore,
/// so the score rules stay in one place.
/// </summary>
public sealed class FoodManager : MonoBehaviour
{
    [SerializeField] private FoodConfig _config;
    [SerializeField] private FoodPickup _foodPrefab;

    private readonly HashSet<FoodPickup> _activeFood = new();
    private readonly List<FoodPickup> _scratch = new();

    private ObjectPool<FoodPickup> _pool;
    private IGameManager _game;
    private PlayerController _player;
    private Camera _camera;
    private int _streak;
    private float _lastKillAt = float.NegativeInfinity;

    public int Streak => _streak;
    public bool HasActiveFood => _activeFood.Count > 0;

    public event Action<int> OnStreakChanged;
    /// <summary>World position, points and whether it was a red herring.</summary>
    public event Action<Vector2, int, bool> OnFoodCaught;

    private void Start()
    {
        _game = GameManager.Instance;
        _player = PlayerController.Instance;
        _camera = Camera.main;
        if (_config == null || _foodPrefab == null || _game == null || _player == null || _camera == null ||
            _config.Tiers.Length == 0)
        {
            Debug.LogError("FoodManager needs its config, food prefab, the game, the player and a camera.", this);
            enabled = false;
            return;
        }

        _pool = new ObjectPool<FoodPickup>(
            () => Instantiate(_foodPrefab, transform),
            food => _activeFood.Add(food),
            food => { _activeFood.Remove(food); food.ResetForPool(); },
            food => { if (food != null) Destroy(food.gameObject); },
            collectionCheck: true,
            defaultCapacity: _config.PoolPrewarm,
            maxSize: _config.PoolMaxRetained);
        Prewarm();

        _game.OnStateChanged += HandleStateChanged;
        _game.OnPlayerDied += ResetStreak;
        _game.OnGameStarted += ClearAll;
    }

    private void Prewarm()
    {
        _scratch.Clear();
        for (var i = 0; i < _config.PoolPrewarm; i++) _scratch.Add(_pool.Get());
        foreach (var food in _scratch) _pool.Release(food);
        _scratch.Clear();
    }

    private void Update()
    {
        if (_streak > 0 && Time.time - _lastKillAt > _config.StreakWindow) ResetStreak();
        CatchTouchingFood();
    }

    /// <summary>Called for every chicken kill: extends the streak and drops the food it earned.</summary>
    public void RegisterKill(Vector2 position)
    {
        if (_pool == null) return;
        _streak = Time.time - _lastKillAt <= _config.StreakWindow ? _streak + 1 : 1;
        _lastKillAt = Time.time;
        OnStreakChanged?.Invoke(_streak);
        Drop(position, TierForStreak(_streak), 1f);
    }

    /// <summary>One piece of Mother Hen's feast: always good food, thrown wide.</summary>
    public void DropFeastItem(Vector2 position)
    {
        if (_pool == null) return;
        var lowest = Mathf.Min(_config.FeastLowestTier, _config.Tiers.Length - 1);
        Drop(position, UnityEngine.Random.Range(lowest, _config.Tiers.Length), 1.6f);
    }

    private int TierForStreak(int streak)
    {
        var best = 0;
        for (var i = 0; i < _config.Tiers.Length; i++)
        {
            if (streak >= _config.Tiers[i].MinStreak) best = i;
        }
        return best;
    }

    private void Drop(Vector2 position, int tierIndex, float popMultiplier)
    {
        var herring = _config.HerringSprite != null && UnityEngine.Random.value < _config.HerringChance;
        var tier = _config.Tiers[tierIndex];
        _pool.Get().Launch(this, _config, _camera, position,
            herring ? _config.HerringSprite : tier.Sprite, herring ? 0 : tier.Points, tierIndex, herring,
            popMultiplier);
    }

    private void CatchTouchingFood()
    {
        if (_activeFood.Count == 0 || !_game.CanMovePlayer || !_player.IsVisible) return;

        // The ship plus everything below it: food resting on the floor is shorter than the gap
        // under the ship, so the ship's own outline would pass over a burger without touching it.
        var catchArea = _player.CatchBounds;
        var floor = _camera.ViewportToWorldPoint(Vector3.zero).y;
        catchArea.SetMinMax(new Vector3(catchArea.min.x, floor, catchArea.min.z), catchArea.max);
        _scratch.Clear();
        foreach (var food in _activeFood)
        {
            if (food.Bounds.Intersects(catchArea)) _scratch.Add(food);
        }

        foreach (var food in _scratch) Catch(food);
        _scratch.Clear();
    }

    private void Catch(FoodPickup food)
    {
        var position = (Vector2)food.transform.position;
        var points = food.Points;
        var herring = food.IsHerring;
        var pitch = 1f + food.TierIndex * 0.06f;
        Release(food);

        _game.AddScore(points);
        if (herring) AudioManager.Play(SoundEffect.FoodHerring);
        else AudioManager.Play(SoundEffect.Food, pitch);
        OnFoodCaught?.Invoke(position, points, herring);
    }

    public void Release(FoodPickup food)
    {
        if (_pool != null && food != null && _activeFood.Contains(food)) _pool.Release(food);
    }

    private void ResetStreak()
    {
        if (_streak == 0) return;
        _streak = 0;
        OnStreakChanged?.Invoke(0);
    }

    private void ClearAll()
    {
        ResetStreak();
        if (_pool == null) return;
        _scratch.Clear();
        _scratch.AddRange(_activeFood);
        foreach (var food in _scratch) _pool.Release(food);
        _scratch.Clear();
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Menu || state == GameState.GameOver || state == GameState.Victory) ClearAll();
    }

    private void OnDestroy()
    {
        if (_game != null)
        {
            _game.OnStateChanged -= HandleStateChanged;
            _game.OnPlayerDied -= ResetStreak;
            _game.OnGameStarted -= ClearAll;
        }
        _pool?.Dispose();
    }
}
