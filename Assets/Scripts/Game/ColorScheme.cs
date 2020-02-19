using UnityEngine;

public class ColorScheme : MonoBehaviour {
    #region editor
    public Color[] cellColors = default;
    public Color[] bottomCellColors = default;
    public Color[] borderColors = default;
    #endregion

    public static ColorScheme instance;

    #region public properties
    public Color randomColor => new Color(Random.value, Random.value, Random.value, 1f);
    public Color randomCellColor => cellColors[Random.Range(0, cellColors.Length)];
    public Color randomBottomCellColor => bottomCellColors[Random.Range(0, bottomCellColors.Length)];
    public Color randomBorderColor => borderColors[Random.Range(0, borderColors.Length)];
    #endregion


    #region private
    private void Awake() {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    #endregion
}
