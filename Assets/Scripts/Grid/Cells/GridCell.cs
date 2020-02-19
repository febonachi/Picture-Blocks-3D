using UnityEngine;

public abstract class GridCell : MonoBehaviour {
    #region public properties
    public bool selected { get; private set; }
    public Vector2Int index { get; private set; }
    public GridCell previousSelectedCell { get; private set; }
    #endregion

    protected Grid grid;

    #region protected
    protected void Awake() => setPreviousSelectedCell(this);
    #endregion

    #region public
    public void updateIndexCallback(Grid newGrid, Vector2Int newIndex) {
        grid = newGrid;
        index = newIndex;
    }

    public void setPreviousSelectedCell(GridCell cell) => previousSelectedCell = cell;
    #endregion

    #region public virtual
    public virtual void select(bool state) => selected = state;
    #endregion

    #region public abstract
    public abstract void setColor(Color color, bool immediately = false);
    #endregion
}
