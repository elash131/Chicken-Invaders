using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// The How To Play screen. Controls and rules are authored text; the food ladder and the gift list
/// are built from the real Food, Gifts and weapon configs, so the screen always matches the game.
/// </summary>
public sealed class HowToPlayPresenter : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private FoodConfig _food;
    [SerializeField] private GiftConfig _gifts;
    [SerializeField] private Sprite _shieldIcon;

    [Header("Layout")]
    [Tooltip("Inactive row: an icon Image with a label text as its child. Cloned once per entry.")]
    [SerializeField] private Image _rowTemplate;
    [SerializeField] private RectTransform _foodRows;
    [SerializeField] private RectTransform _giftRows;
    [SerializeField, Min(1f)] private float _rowHeight = 44f;
    [SerializeField] private Button _backButton;

    private InputAction _close;
    private bool _built;

    public event Action OnClosed;
    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        _close = InputSystem.actions.FindActionMap(Constants.PlayerActionMap, true)
            .FindAction(Constants.PauseAction, true);
        _backButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        if (!_built) BuildRows();
        gameObject.SetActive(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_backButton.gameObject);
    }

    public void Close()
    {
        if (!IsOpen) return;
        AudioManager.Play(SoundEffect.UiClick);
        gameObject.SetActive(false);
        OnClosed?.Invoke();
    }

    private void Update()
    {
        if (_close.WasPressedThisFrame()) Close();
    }

    private void BuildRows()
    {
        _built = true;
        var tiers = _food.Tiers;
        for (var i = 0; i < tiers.Length; i++)
        {
            AddRow(_foodRows, i, tiers[i].Sprite, $"x{tiers[i].MinStreak}+   {tiers[i].Points}");
        }

        var row = 0;
        AddRow(_giftRows, row++, _gifts.Frames[0], $"CATCH A GIFT  ({_gifts.WeaponDuration:0} S)");
        foreach (var weapon in _gifts.Weapons)
        {
            AddRow(_giftRows, row++, weapon.Frames[0], $"{weapon.DisplayName}  -  {weapon.Description}");
        }
        AddRow(_giftRows, row, _shieldIcon, $"SHIELD  -  BLOCKS ONE HIT ({_gifts.ShieldDuration:0} S)");
    }

    private void AddRow(RectTransform container, int index, Sprite icon, string label)
    {
        var row = Instantiate(_rowTemplate, container);
        row.gameObject.SetActive(true);
        row.sprite = icon;
        row.rectTransform.anchoredPosition = new Vector2(0f, -index * _rowHeight);
        row.GetComponentInChildren<TextMeshProUGUI>().text = label;
    }

    private void OnDestroy()
    {
        if (_backButton != null) _backButton.onClick.RemoveListener(Close);
    }
}
