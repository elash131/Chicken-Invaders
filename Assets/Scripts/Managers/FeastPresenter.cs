using TMPro;
using UnityEngine;

/// <summary>
/// Shows the Feast Streak: a pulsing streak counter and "+points" that float up from each caught
/// piece of food. Listens to FoodManager and owns no rules.
/// </summary>
public sealed class FeastPresenter : MonoBehaviour
{
    private sealed class Popup
    {
        public TextMeshProUGUI Text;
        public Vector3 WorldPosition;
        public float StartedAt = float.NegativeInfinity;
    }

    [SerializeField] private FoodManager _food;
    [SerializeField] private TextMeshProUGUI _streakText;
    [Tooltip("Inactive text that is cloned for the floating points.")]
    [SerializeField] private TextMeshProUGUI _popupTemplate;
    [SerializeField, Min(1)] private int _popupCount = 8;
    [SerializeField, Min(0.1f)] private float _popupDuration = 0.8f;
    [SerializeField, Min(0f)] private float _popupRise = 1.2f;
    [Tooltip("The counter only appears once the streak reaches this, so single kills stay quiet.")]
    [SerializeField, Min(1)] private int _minVisibleStreak = 2;

    private Popup[] _popups = System.Array.Empty<Popup>();
    private Camera _camera;
    private int _nextPopup;
    private float _streakPunch;

    private void Start()
    {
        _camera = Camera.main;
        if (_food == null || _streakText == null || _popupTemplate == null || _camera == null)
        {
            Debug.LogError("FeastPresenter needs the FoodManager, its texts and a camera.", this);
            enabled = false;
            return;
        }

        // Created once and reused, so catching food never instantiates UI during play.
        _popups = new Popup[_popupCount];
        for (var i = 0; i < _popupCount; i++)
        {
            var text = Instantiate(_popupTemplate, _popupTemplate.transform.parent);
            text.gameObject.SetActive(false);
            _popups[i] = new Popup { Text = text };
        }

        _food.OnStreakChanged += ShowStreak;
        _food.OnFoodCaught += ShowPopup;
        ShowStreak(_food.Streak);
    }

    private void ShowStreak(int streak)
    {
        var visible = streak >= _minVisibleStreak;
        _streakText.gameObject.SetActive(visible);
        if (!visible) return;
        _streakText.text = $"STREAK  x{streak}";
        _streakPunch = 1f;
    }

    private void ShowPopup(Vector2 worldPosition, int points, bool herring)
    {
        var popup = _popups[_nextPopup];
        _nextPopup = (_nextPopup + 1) % _popups.Length;
        popup.WorldPosition = worldPosition;
        popup.StartedAt = Time.time;
        popup.Text.text = herring ? "RED HERRING!" : $"+{points}";
        popup.Text.gameObject.SetActive(true);
    }

    private void Update()
    {
        _streakPunch = Mathf.MoveTowards(_streakPunch, 0f, Time.deltaTime * 4f);
        _streakText.rectTransform.localScale = Vector3.one * (1f + 0.3f * _streakPunch);

        foreach (var popup in _popups)
        {
            if (!popup.Text.gameObject.activeSelf) continue;
            var progress = (Time.time - popup.StartedAt) / _popupDuration;
            if (progress >= 1f)
            {
                popup.Text.gameObject.SetActive(false);
                continue;
            }

            // Follows the world point, so it stays on the food's spot inside the letterboxed view.
            var world = popup.WorldPosition + Vector3.up * (_popupRise * progress);
            popup.Text.rectTransform.position = _camera.WorldToScreenPoint(world);
            popup.Text.alpha = 1f - progress * progress;
        }
    }

    private void OnDestroy()
    {
        if (_food == null) return;
        _food.OnStreakChanged -= ShowStreak;
        _food.OnFoodCaught -= ShowPopup;
    }
}
