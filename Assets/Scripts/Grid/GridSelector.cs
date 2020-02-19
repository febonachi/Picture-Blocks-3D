using System;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public class GridSelector {
    private Grid grid;

    #region public properties
    public Action<GridCell> bombSelected;
    #endregion

    #region private
    private void animate(GridCell cell, Color color){ // animate only DefaultCell
        cell.setColor(color, immediately: true);

        DefaultCell defaultCell = cell as DefaultCell;

        Vector3 cachedPosition = defaultCell.platform.localPosition;
        Vector3 cachedRotation = defaultCell.platform.eulerAngles;

        if(cell.previousSelectedCell.index.x != cell.index.x) {
            float sign = Mathf.Sign(cell.index.x - cell.previousSelectedCell.index.x);
            defaultCell.platform.Translate(-.5f * sign, .5f, 0f, Space.World);
            defaultCell.platform.Rotate(Vector3.forward * 90f * sign);
        }

        if(cell.previousSelectedCell.index.y != cell.index.y) {
            float sign = Mathf.Sign(cell.previousSelectedCell.index.y - cell.index.y);
            defaultCell.platform.Translate(0f, .5f, .5f * sign, Space.World);
            defaultCell.platform.Rotate(Vector3.right * 90f * sign);
        }

        defaultCell.platform.DOLocalMove(cachedPosition, .5f).SetEase(Ease.OutCubic);
        defaultCell.platform.DORotate(cachedRotation, .75f).SetEase(Ease.OutCubic);
    } 
    #endregion

    #region public
    public GridSelector(Grid grid) => this.grid = grid;

    public List<GridCell> select(GridCell cell, Color color){
        cell.select(true);

        bool animateCell = false;

        switch(cell){
            case DefaultCell defaultCell: {
                animateCell = true;
                break;
            }
            case HighlightedCell highlightedCell:{
                break;
            }
            case BombCell bombCell: {
                animateCell = false;
                bombSelected?.Invoke(cell);
                break;
            }
        }

        List<GridCell> selectedCells = grid.getHVNeighbors(cell);
        selectedCells.ForEach(c => c.setPreviousSelectedCell(cell));

        if(animateCell) animate(cell, color);

        return selectedCells;
    }
    #endregion
}
