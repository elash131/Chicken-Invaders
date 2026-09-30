using UnityEngine;

/// <summary>Observes player-death events and plays presentation without owning damage rules.</summary>
public sealed class PlayerDeathPresenter : MonoBehaviour
{
    private GameManager _game;
    private PlayerController _player;
    private PlayerExplosionEffect _effect;

    public void Initialize(
        GameManager game,
        PlayerController player,
        PlayerExplosionEffect effectPrefab)
    {
        if (_game != null) return;
        if (game == null || player == null || effectPrefab == null)
        {
            Debug.LogError("PlayerDeathPresenter needs the game, player and explosion prefab.", this);
            return;
        }

        _game = game;
        _player = player;
        _effect = Instantiate(effectPrefab);
        _effect.name = "PlayerExplosionEffect";
        _effect.Stop();

        _game.OnPlayerDied += PlayExplosion;
        _game.OnGameStarted += StopExplosion;
        _game.OnStateChanged += HandleStateChanged;
    }

    private void PlayExplosion()
    {
        if (_effect != null && _player != null) _effect.Play(_player.transform.position);
    }

    private void StopExplosion()
    {
        if (_effect != null) _effect.Stop();
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Menu) StopExplosion();
    }

    private void OnDestroy()
    {
        if (_game != null)
        {
            _game.OnPlayerDied -= PlayExplosion;
            _game.OnGameStarted -= StopExplosion;
            _game.OnStateChanged -= HandleStateChanged;
        }

        if (_effect != null) Destroy(_effect.gameObject);
    }
}
