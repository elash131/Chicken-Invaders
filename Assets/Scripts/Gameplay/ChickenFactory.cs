using UnityEngine;

/// <summary>
/// Creates configured chickens. It deliberately knows nothing about scoring or wave completion.
/// </summary>
public sealed class ChickenFactory : MonoBehaviour
{
    [SerializeField] private Chicken[] _chickenPrefabs;

    public Chicken Create(
        int variantIndex,
        WaveManager owner,
        int row,
        int column,
        Vector2 slotOffset,
        Vector2 spawnPosition)
    {
        if (_chickenPrefabs == null || _chickenPrefabs.Length == 0)
        {
            Debug.LogError("ChickenFactory has no chicken prefabs configured.", this);
            return null;
        }

        var prefab = _chickenPrefabs[Mathf.Abs(variantIndex) % _chickenPrefabs.Length];
        if (prefab == null)
        {
            Debug.LogError("ChickenFactory contains a missing chicken prefab.", this);
            return null;
        }

        var chicken = Instantiate(prefab, spawnPosition, Quaternion.identity);
        chicken.name = $"Chicken_R{row + 1}_C{column + 1}";
        chicken.Initialize(owner, row, column, slotOffset, spawnPosition);
        return chicken;
    }
}
