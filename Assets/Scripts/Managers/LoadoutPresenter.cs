using TMPro;
using UnityEngine;

/// <summary>
/// Shows the gift loadout on the HUD: the gift weapon with its seconds left, and the shield.
/// Hidden while the ship has only its default gun. Listens to PlayerWeapons and owns no rules.
/// </summary>
public sealed class LoadoutPresenter : MonoBehaviour
{
    [SerializeField] private PlayerWeapons _weapons;
    [SerializeField] private TextMeshProUGUI _loadoutText;

    private int _shownSeconds = -1;

    private void Start()
    {
        if (_weapons == null || _loadoutText == null)
        {
            Debug.LogError("LoadoutPresenter needs the player's weapons and a text.", this);
            enabled = false;
            return;
        }

        _weapons.OnWeaponChanged += HandleWeaponChanged;
        _weapons.OnShieldChanged += HandleShieldChanged;
        Refresh();
    }

    // The countdown only changes once a second, so the text is rebuilt only then.
    private void Update()
    {
        if (CurrentSeconds() != _shownSeconds) Refresh();
    }

    // Changes whenever either countdown ticks over a whole second.
    private int CurrentSeconds() =>
        Mathf.CeilToInt(_weapons.TimeLeft) * 100 + Mathf.CeilToInt(_weapons.ShieldTimeLeft);

    private void HandleWeaponChanged(WeaponConfig weapon) => Refresh();
    private void HandleShieldChanged(bool hasShield) => Refresh();

    private void Refresh()
    {
        _shownSeconds = CurrentSeconds();
        var weapon = _weapons.HasGiftWeapon
            ? $"{_weapons.Current.DisplayName}  {Mathf.CeilToInt(_weapons.TimeLeft)}" : string.Empty;
        var shield = _weapons.HasShield ? $"SHIELD  {Mathf.CeilToInt(_weapons.ShieldTimeLeft)}" : string.Empty;
        var separator = weapon.Length > 0 && shield.Length > 0 ? "\n" : string.Empty;

        _loadoutText.text = weapon + separator + shield;
        _loadoutText.gameObject.SetActive(_loadoutText.text.Length > 0);
    }

    private void OnDestroy()
    {
        if (_weapons == null) return;
        _weapons.OnWeaponChanged -= HandleWeaponChanged;
        _weapons.OnShieldChanged -= HandleShieldChanged;
    }
}
