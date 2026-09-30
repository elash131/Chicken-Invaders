using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "Chicken Invaders/Wave")]
public sealed class WaveConfig : ScriptableObject
{
    [SerializeField, Min(1)] private int _rows = 2;
    [SerializeField, Min(1)] private int _columns = 5;
    [Tooltip("Prefab variant index for each row. Values repeat when the wave has more rows.")]
    [SerializeField] private int[] _rowVariants = { 0 };

    public int Rows => _rows;
    public int Columns => _columns;

    public int GetVariantForRow(int row)
    {
        if (_rowVariants == null || _rowVariants.Length == 0)
        {
            return 0;
        }

        return _rowVariants[Mathf.Abs(row) % _rowVariants.Length];
    }
}
