using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Everything that falls for the ship to catch. The Feast Streak: every chicken drops food, and the
/// faster the kills come the better the food. Now and then a kill also drops a gift holding a
/// weapon or a shield. Owns the pickup pool, the kill streak and catching. Points still go through
/// GameManager.AddScore and the loadout through PlayerWeapons, so their rules stay in one place.
/// </summary>
public sealed class PickupManager : MonoBehaviour
{
    [SerializeField] private FoodConfig _config;
    [SerializeField] private Pickup _foodPrefab;
    [SerializeField] private GiftConfig _gifts;

    private readonly List<Pickup> _scratch = new();

    private TrackedPool<Pickup> _pool;
    private IGameManager _game;
    private PlayerController _player;
    private Camera _camera;
    private WaveManager _waves;
    private PlayerWeapons _weapons;
    private int _streak;
    private int _killsSinceGift;
    private int _giftGuarantee;
    // A shuffled bag of gift contents: every item appears once before any repeats. Index
    // Weapons.Length stands for the shield.
    private readonly List<int> _giftBag = new();
    private float _lastKillAt = float.NegativeInfinity;

    public int Streak => _streak;
    public bool HasActiveFood => _pool != null && _pool.ActiveCount > 0;

    public event Action<int> OnStreakChanged;
    /// <summary>Where something was caught and what to show there, such as "+300" or "SHIELD".</summary>
    public event Action<Vector2, string> OnPickupCaught;

    private void Start()
    {
        _game = GameManager.Instance;
        _player = PlayerController.Instance;
        _weapons = _player != null ? _player.GetComponent<PlayerWeapons>() : null;
        _camera = Camera.main;
        if (_config == null || _foodPrefab == null || _game == null || _player == null || _camera == null ||
            _config.Tiers.Length == 0)
        {
            Debug.LogError("PickupManager needs its config, food prefab, the game, the player and a camera.", this);
            enabled = false;
            return;
        }

        _pool = new TrackedPool<Pickup>(() => Instantiate(_foodPrefab, transform), pickup => pickup.ResetForPool(),
            _config.PoolPrewarm, _config.PoolMaxRetained);

        _game.OnStateChanged += HandleStateChanged;
        _game.OnPlayerDied += ResetStreak;
        _game.OnGameStarted += ClearAll;
        _waves = GetComponent<WaveManager>();
        if (_waves != null) _waves.OnChickenKilled += RegisterKill;
    }

    private void Update()
    {
        if (_streak > 0 && Time.time - _lastKillAt > _config.StreakWindow) ResetStreak();
        CatchTouchingFood();
    }

    // Every chicken kill extends the streak and drops the food it earned.
    private void RegisterKill(Vector2 position)
    {
        if (_pool == null) return;
        _streak = Time.time - _lastKillAt <= _config.StreakWindow ? _streak + 1 : 1;
        _lastKillAt = Time.time;
        OnStreakChanged?.Invoke(_streak);
        Drop(position, TierForStreak(_streak), 1f);
        TryDropGift(position);
    }

    private void TryDropGift(Vector2 position)
    {
        if (_gifts == null || _weapons == null || IsGiftFalling()) return;
        _killsSinceGift++;
        if (_killsSinceGift < _giftGuarantee && UnityEngine.Random.value >= _gifts.DropChance) return;

        _killsSinceGift = 0;
        _giftGuarantee = _gifts.RollGuarantee();
        var contents = new PickupContents
        {
            Kind = PickupKind.Gift,
            Frames = _gifts.Frames,
            FrameRate = _gifts.FrameRate,
            Scale = _gifts.Scale
        };
        _pool.Get().Launch(this, _config, _camera, position, contents, 0.6f);
    }

    private bool IsGiftFalling()
    {
        foreach (var pickup in _pool.Active)
        {
            if (pickup.Contents.Kind == PickupKind.Gift) return true;
        }
        return false;
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
        var contents = new PickupContents
        {
            Kind = herring ? PickupKind.Herring : PickupKind.Food,
            Frames = new[] { herring ? _config.HerringSprite : tier.Sprite },
            Scale = 1f,
            Points = herring ? 0 : tier.Points,
            TierIndex = tierIndex
        };
        _pool.Get().Launch(this, _config, _camera, position, contents, popMultiplier);
    }

    private void CatchTouchingFood()
    {
        if (!HasActiveFood || !_game.CanMovePlayer || !_player.IsVisible) return;

        // The ship plus everything below it: food resting on the floor is shorter than the gap
        // under the ship, so the ship's own outline would pass over a burger without touching it.
        var catchArea = _player.CatchBounds;
        var floor = _camera.ViewportToWorldPoint(Vector3.zero).y;
        catchArea.SetMinMax(new Vector3(catchArea.min.x, floor, catchArea.min.z), catchArea.max);
        _scratch.Clear();
        foreach (var food in _pool.Active)
        {
            if (food.Bounds.Intersects(catchArea)) _scratch.Add(food);
        }

        foreach (var food in _scratch) Catch(food);
        _scratch.Clear();
    }

    private void Catch(Pickup pickup)
    {
        var position = (Vector2)pickup.transform.position;
        var contents = pickup.Contents;
        Release(pickup);

        switch (contents.Kind)
        {
            case PickupKind.Gift:
                OnPickupCaught?.Invoke(position, OpenGift());
                break;
            case PickupKind.Herring:
                AudioManager.Play(SoundEffect.FoodHerring);
                OnPickupCaught?.Invoke(position, "RED HERRING!");
                break;
            default:
                _game.AddScore(contents.Points);
                AudioManager.Play(SoundEffect.Food, 1f + contents.TierIndex * 0.06f);
                OnPickupCaught?.Invoke(position, $"+{contents.Points}");
                break;
        }
    }

    // A gift holds the next item from the bag: a weapon or the shield. Returns the name to show.
    private string OpenGift()
    {
        AudioManager.Play(SoundEffect.GiftCatch);
        var pick = DrawFromGiftBag();
        if (pick == _gifts.Weapons.Length)
        {
            _weapons.GiveShield(_gifts.ShieldDuration);
            return "SHIELD!";
        }

        var weapon = _gifts.Weapons[pick];
        _weapons.EquipGiftWeapon(weapon, _gifts.WeaponDuration);
        return weapon.DisplayName + "!";
    }

    private int DrawFromGiftBag()
    {
        if (_giftBag.Count == 0)
        {
            for (var i = 0; i <= _gifts.Weapons.Length; i++) _giftBag.Add(i);
        }

        var index = UnityEngine.Random.Range(0, _giftBag.Count);
        var pick = _giftBag[index];
        _giftBag.RemoveAt(index);
        return pick;
    }

    public void Release(Pickup food) => _pool?.Release(food);

    private void ResetStreak()
    {
        if (_streak == 0) return;
        _streak = 0;
        OnStreakChanged?.Invoke(0);
    }

    private void ClearAll()
    {
        ResetStreak();
        _killsSinceGift = 0;
        _giftBag.Clear();
        if (_gifts != null) _giftGuarantee = _gifts.RollGuarantee();
        _pool?.ReleaseAll();
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
        if (_waves != null) _waves.OnChickenKilled -= RegisterKill;
        _pool?.Dispose();
    }
}
