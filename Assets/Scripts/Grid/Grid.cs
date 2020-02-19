using Utils;
using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using static UnityEngine.Random;

public class Grid : MonoBehaviour {
    public enum GridSelectionType {Spiral, Neighbors, Wave};

    #region editor
    [SerializeField] private bool placeOnAwake = true;
    [SerializeField] private GridCell prefab = default;
    [SerializeField] private Transform cellHolder = default;
    [SerializeField] private float spacing = .2f;
    [SerializeField] private float scale = 1f;
    [SerializeField] private Vector2Int dimension = new Vector2Int(2, 2);
    #endregion

    #region public properties
    public Vector2Int size => dimension;
    public List<GridCell> totalCells => cells.Cast<GridCell>().ToList();
    #endregion

    // private Vector3Int[] neighborIndexes => new Vector3Int[]{
    //     new Vector3Int(-1, 0, 1), new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1),
    //     new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0),
    //     new Vector3Int(-1, 0, -1), new Vector3Int(0, 0, -1), new Vector3Int(1, 0, -1)
    // };

    private bool initialized = false;
    private Vector3 pivotOffset = Vector3.zero;

    private GridCell[,] cells;

    #region private
    private void Awake() => initialize();

    private void initialize(){
        if(cellHolder == null) cellHolder = transform;
        
        cells = new GridCell[dimension.x, dimension.y];

        Vector3 dimensionOffset = new Vector3((dimension.x * (scale + spacing)) / 2f, scale / 2f, (dimension.y * (scale + spacing)) / 2f);
        Vector3 scaleOffset = new Vector3(scale + spacing, scale, scale + spacing) / 2f;
        pivotOffset = dimensionOffset - scaleOffset;

        initialized = true;
    }

    private void instantiatePrefabs() {
        for (int i = 0; i < dimension.x; i++) {
            for (int j = 0; j < dimension.y; j++) {
                instantiatePrefab(i, j);
            }
        }
    }

    private void instantiatePrefab(int i, int j) {
        if(prefab == null || cells[i, j] != null) return;
        
        placeCell(Instantiate(prefab), new Vector2Int(i, j), resetParent: true);
    }

    private bool isValidCellIndex(Vector2Int index) => isValidCellIndex(index.x, index.y);

    private bool isValidCellIndex(int x, int z) => ((x >= 0 && x < dimension.x) && (z >= 0 && z < dimension.y));

    private void OnDrawGizmos() {
        Vector3 dimensionOffset = new Vector3((dimension.x * (scale + spacing)) / 2f,
                                              scale / 2f,
                                              (dimension.y * (scale + spacing)) / 2f);
        Vector3 scaleOffset = new Vector3(scale + spacing, scale, scale + spacing) / 2f;
        pivotOffset = dimensionOffset - scaleOffset;

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(dimension.x * (scale + spacing), scale + scale / 2f, dimension.y * (scale + spacing)));

        Color yellowColor = Color.yellow;
        yellowColor.a = .5f;
        Gizmos.color = yellowColor;
        for (int i = 0; i < dimension.x; i++) {
            for (int j = 0; j < dimension.y; j++) {
                Gizmos.DrawWireCube(transform.position + new Vector3((i * (scale + spacing)), 
                                                                     0f, 
                                                                     (j * (scale + spacing))) - pivotOffset, Vector3.one * scale);
            }
        }                                                           
    }
    #endregion

    #region public
    public void spawnGridCells(GridCell cellPrefab = null){
        if(cellPrefab != null) prefab = cellPrefab;

        if(!initialized) initialize();

        instantiatePrefabs();
    }
    
    public void clear(){
        cellHolder.Cast<Transform>().ToList().ForEach(t => {
            if (Application.isPlaying) Destroy(t.gameObject);
            else DestroyImmediate(t.gameObject);
        });

        if(initialized) Array.Clear(cells, 0, cells.Length);
    }

    public void placeCell(GridCell cell, Vector2Int index, bool resetParent = false){
        if(!initialized) return;

        if(resetParent) cell.transform.parent = cellHolder;

        cell.transform.localScale = new Vector3(cell.transform.localScale.x * scale, 
                                                cell.transform.localScale.y * scale,
                                                cell.transform.localScale.z * scale);
        cell.transform.position = transform.position + new Vector3((index.x * (scale + spacing)), 0f, (index.y * (scale + spacing))) - pivotOffset;
        
        cells[index.x, index.y] = cell;

        cell.updateIndexCallback(this, index);
    }

    public GridCell outplacedCell(int range = 1) {
        return cells.Cast<GridCell>().Where(cell => cell.GetType() == typeof(DefaultCell) && !cell.selected).OrderBy(rnd => UnityEngine.Random.value).FirstOrDefault(cell => {
            for(int i = range; i > 0; i--){
                List<GridCell> neighbors = getAllNeighbors(cell, i, includeSelected: true);
                if(neighbors.Count(c => c.selected) > 1 ||
                   neighbors.Any(c => c.GetType() == typeof(BombCell))) return false; 
            }
            return true;
        });
    }

    public GridCell randomDefaultCell() => cells.Cast<GridCell>().Where(cell => cell.GetType() == typeof(DefaultCell) && !cell.selected).OrderBy(rnd => UnityEngine.Random.value).FirstOrDefault();

    public GridCell randomCell() => cells[Range(0, dimension.x), Range(0, dimension.y)];

    public Vector2Int worldPositionToGridIndex(Vector3 position){
        Vector3 pivot = pivotOffset;
        float stepDelta = scale + spacing;
        Vector2Int cellIndex = new Vector2Int(dimension.x, dimension.y);

        while(pivot.x >= position.x) {
            cellIndex.x--;
            pivot.x -= stepDelta;
        }

        while(pivot.z >= position.z) {
            cellIndex.y--;
            pivot.z -= stepDelta;
        }

        return cellIndex;
    }

    public List<GridCell> getNeighbors(GridCell cell, int range, params Vector2Int[] indexes){
        List<GridCell> selectedCells = new List<GridCell>();

        Vector2Int cellIndex = cell.index;
        for(int i = 1; i < range + 1; i++){
            foreach(Vector2Int index in indexes) {
                Vector2Int rangeIndex = cellIndex + (index * i);
                if(isValidCellIndex(rangeIndex)){
                    GridCell neighborCell = cells[rangeIndex.x, rangeIndex.y];
                    if(!neighborCell.selected) selectedCells.Add(neighborCell);
                }
            }
        }

        return selectedCells.OrderBy(rnd => UnityEngine.Random.value).ToList();
    }

    public List<GridCell> getVerticalNeighbors(GridCell cell, int range = 1) => getNeighbors(cell, range, new Vector2Int(0, 1), new Vector2Int(0, -1));

    public List<GridCell> getHorizontalNeighbors(GridCell cell, int range = 1) => getNeighbors(cell, range, new Vector2Int(1, 0), new Vector2Int(-1, 0));

    public List<GridCell> getHVNeighbors(GridCell cell, int range = 1) => getVerticalNeighbors(cell, range).Concat(getHorizontalNeighbors(cell, range)).ToList();

    public List<GridCell> getAllNeighbors(GridCell cell, int range = 1, bool includeSelected = false) {
        List<GridCell> selectedCells = new List<GridCell>();

        Vector2Int cellIndex = cell.index;
        for(int x = -range; x < range + 1; x++) {
            for(int z = -range; z < range + 1; z++) {
                Vector2Int rangeIndex = cellIndex + new Vector2Int(x, z);
                if (isValidCellIndex(rangeIndex)) {
                    GridCell neighborCell = cells[rangeIndex.x, rangeIndex.y];
                    if (!neighborCell.selected) selectedCells.Add(neighborCell);
                    else if (includeSelected) selectedCells.Add(neighborCell);
                }
            }
        }

        return selectedCells;
    }
    #endregion
}
