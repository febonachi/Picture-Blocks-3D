using Utils;
using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

[RequireComponent(typeof(ColorPicker))]
[RequireComponent(typeof(GridSelector))]
public class GameGridController : MonoBehaviour {
    #region editor
    [SerializeField] private Grid grid = default;
    [SerializeField] private RandomizedFloat bombsCount = 1f;
    [SerializeField] private float fillTimeout = 1f;

    [Space][Header("Picture frame")]
    [SerializeField] private PictureFrame pictureFrame = default;

    [Space][Header("GridCell prefabs")]
    [SerializeField] private HighlightedCell highlightedCellPrefab = default;
    [SerializeField] private DefaultCell defaultCellPrefab = default;
    [SerializeField] private BombCell bombCellPrefab = default;

    [Space][Header("Confetti settings")]
    [SerializeField] private ParticleSystem confettiPs = default;
    [SerializeField] private Material[] confettiMaterials = default;

    [Space]
    [Header("Plane border")]
    [SerializeField] private MeshRenderer planeBorders = default;
    #endregion

    private float elapsed = 0f;
    private int bombsSelected = 0;
    private bool canFillGrid = false;
    private int pixelizationCount = 0;
    private bool fillActionStarted = true;
    private float localFillTimeout = .2f;
    private Color filledCellColor = Color.white;

    private HighlightedCell currentHighlightedCell;

    private ColorPicker colorPicker;
    private GridSelector gridSelector;
    private ColorScheme colorScheme;
    private GameController gameController;

    private HashSet<GridCell> cellsToSelect = new HashSet<GridCell>();
    private List<List<GridCell>> totalSelectedCells = new List<List<GridCell>>();

    #region private
    private void Awake() {
        localFillTimeout = .1f;
        elapsed = fillTimeout;

        colorScheme = ColorScheme.instance;
        gameController = GameController.instance;
        colorPicker = GetComponent<ColorPicker>();

        gridSelector = new GridSelector(grid);
        gridSelector.bombSelected += makeBoom;
    }

    private void Start() => initializeGrid();

    private void Update() {
        if (!canFillGrid) return;

        if (Input.GetMouseButtonDown(0)) fillActionStarted = true;

        if (Input.GetMouseButton(0) && fillActionStarted) {
            elapsed += Time.deltaTime;
            if (elapsed >= localFillTimeout) {
                fillGrid();
                elapsed = 0f;

                // slow down fillTimeout when first cells are opened
                if (totalSelectedCells.Count < 2) localFillTimeout = .1f;
                else localFillTimeout = fillTimeout;
            }
        }

        if (Input.GetMouseButtonUp(0) && fillActionStarted && totalSelectedCells.Count != 0) {
            elapsed = localFillTimeout;

            // checkout fever
            if (totalSelectedCells.Count != 0) makeFever();

            // swap highlighted cell with default cell
            swapSelectedCellsToDefault(defaultCell => {
                defaultCell.setColor(filledCellColor, immediately: true);
                defaultCell.select(true);
            });

            StartCoroutine(_pixelizeFilledCell(totalSelectedCells.ToList()));

            cellsToSelect.Clear();
            totalSelectedCells.Clear();

            // find next outplaced cell and update highlightedCell
            currentHighlightedCell = placeHighlightedCell();
            if (currentHighlightedCell == null) calculateProgress();

            fillActionStarted = false;
        }
    }

    private void initializeGrid() {
        filledCellColor = colorScheme.randomCellColor;
        planeBorders.material.DOColor(colorScheme.randomBorderColor, .1f);

        Color randomEmptyCellColor = colorScheme.randomBottomCellColor;
        Array.ForEach(FindObjectsOfType<EmptyCell>(), cell => cell.setColor(randomEmptyCellColor));

        grid.spawnGridCells(defaultCellPrefab);

        // randomize bombs count
        if (gameController.gameSettings.currentSavedLevel < 3) {
            bombsCount = UnityEngine.Random.Range(0, 2);
        } else if (gameController.gameSettings.currentSavedLevel >= 3 && gameController.gameSettings.currentSavedLevel < 10) {
            bombsCount = UnityEngine.Random.Range(2, 4);
        } else if (gameController.gameSettings.currentSavedLevel >= 10 && gameController.gameSettings.currentSavedLevel < 16) {
            bombsCount = UnityEngine.Random.Range(4, 6);
        } else if (gameController.gameSettings.currentSavedLevel >= 16) {
            bombsCount = UnityEngine.Random.Range(5, 8);
        }

        for (int i = 0; i < bombsCount.value; i++) swapCell(grid.randomDefaultCell(), bombCellPrefab);
    }

    private T swapCell<T>(GridCell cellToSwap, T newCellPrefab) where T : GridCell {
        if (cellToSwap == null) return null;

        T cell = Instantiate(newCellPrefab, cellToSwap.transform.position, cellToSwap.transform.rotation, cellToSwap.transform.parent);
        grid.placeCell(cell, cellToSwap.index);

        Destroy(cellToSwap.gameObject);

        return cell;
    }

    private void swapSelectedCellsToDefault(Action<DefaultCell> initialize) {
        foreach (List<GridCell> cells in totalSelectedCells) {
            for (int i = 0; i < cells.Count; i++) {
                GridCell cell = cells[i];
                if (cell.GetType() != typeof(DefaultCell)) {
                    cells[i] = swapCell(cell, defaultCellPrefab);
                    initialize(cells[i] as DefaultCell);
                }
            }
        }
    }

    private HighlightedCell placeHighlightedCell() {
        GridCell outplacedCell = grid.outplacedCell();
        if (outplacedCell == null) return null;

        HighlightedCell highlightedCell = swapCell(outplacedCell, highlightedCellPrefab);
        highlightedCell.setColor(filledCellColor, immediately: true);

        cellsToSelect.Add(highlightedCell);

        return highlightedCell;
    }

    private void makeBoom(GridCell cell) {
        canFillGrid = false;
        fillActionStarted = false;

        bombsSelected++;

        gameController.vibrate();
        gameController.cameraManager.shakeOnce(7.5f, 2.5f, damping: true);

        swapSelectedCellsToDefault(defaultCell => {
            defaultCell.setColor(filledCellColor, immediately: true);
            defaultCell.select(true);
        });
        StartCoroutine(_explodeFilledCells(totalSelectedCells.ToList(), bombsSelected >= 2)); // after 2 bombs -> restart
    }

    private void makeFever() {
        foreach (GridCell cell in totalSelectedCells.Last()) {
            List<GridCell> bombCells = grid.getAllNeighbors(cell).Where(c => c.GetType() == typeof(BombCell)).ToList();

            if (bombCells == null) return;

            foreach (GridCell neighborCell in bombCells) {
                List<GridCell> emptyCells = new List<GridCell>();
                foreach (GridCell emptyCell in grid.getAllNeighbors(neighborCell)) {
                    emptyCell.select(true);
                    emptyCell.setColor(filledCellColor);

                    emptyCells.Add(emptyCell);
                }
                totalSelectedCells.Add(emptyCells);
            }
        }
    }

    private void calculateProgress(){
        canFillGrid = false;

        gameController.cameraManager.zoom(2f, 1.25f);

        float progress = grid.totalCells.Count(cell => cell.selected);
        progress = progress.map(0, grid.size.x * grid.size.y, 0f, 100f);

        gameController.uiManager.gameMenu.show();
        bool levelPassed = progress >= 90f;
        gameController.uiManager.gameMenu.progressBar.setProgress(progress, levelPassed);

        if(levelPassed) StartCoroutine(_showPictureFrame());
        else StartCoroutine(_showRestartLevelMenu(2f));
    }   

    private IEnumerator _showRestartLevelMenu(float delay) {
        yield return new WaitForSeconds(delay);

        gameController.uiManager.showRestartLevelMenu();
    }

    private IEnumerator _showPictureFrame() {
        yield return new WaitUntil(() => pixelizationCount == 0); // wait for all cells ready   

        planeBorders.material.DOColor(colorScheme.randomBorderColor, 1f);

        foreach (GridCell cell in grid.totalCells.OrderBy(rnd => UnityEngine.Random.value)) {
            if (cell is DefaultCell) {
                DefaultCell defaultCell = cell as DefaultCell;
                foreach (Transform cellPart in defaultCell.meshTransforms) {
                    cellPart.DOMove(pictureFrame.transform.position, UnityEngine.Random.Range(.5f, 1f)).SetDelay(UnityEngine.Random.Range(.5f, 2f)).OnStart(() => {
                        cellPart.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCirc);
                    });
                }
            } else Destroy(cell.gameObject);
        }

        yield return new WaitForSeconds(.5f);

        // confetti
        for (int i = 0; i < 10; i++) {
            ParticleSystem confetti = Instantiate(confettiPs, grid.randomCell().transform.position + (Vector3.up * 15f), Quaternion.identity);
            ParticleSystemRenderer confettiRenderer = confetti.GetComponent<ParticleSystemRenderer>();
            confettiRenderer.material = confettiMaterials[UnityEngine.Random.Range(0, confettiMaterials.Length)];
            confetti.Play();
        }

        pictureFrame.setSprite(colorPicker.sprite);

        yield return new WaitForSeconds(4f);

        gameController.uiManager.showEndLevelMenu();
    }

    private IEnumerator _explodeFilledCells(List<List<GridCell>> selectedCells, bool stop) {
        selectedCells.Reverse();
        foreach (List<GridCell> cells in selectedCells) {
            foreach (GridCell cell in cells) cell.select(false);
            yield return new WaitForSeconds(.025f);
        }

        cellsToSelect.Clear();
        totalSelectedCells.Clear();

        if(!stop){
            yield return new WaitForSeconds(localFillTimeout);
            currentHighlightedCell = placeHighlightedCell();
            canFillGrid = true;
        }else StartCoroutine(_showRestartLevelMenu(localFillTimeout));
    }

    private IEnumerator _pixelizeFilledCell(List<List<GridCell>> selectedCells) {
        pixelizationCount++;
        foreach (List<GridCell> cellsRange in selectedCells) {
            foreach (GridCell cell in cellsRange) {
                DefaultCell defaultCell = cell as DefaultCell;
                List<Color> defaultCellPartColors = new List<Color>();
                foreach (Transform defaultCellPart in defaultCell.meshTransforms) {
                    defaultCellPart.DORotate(Vector3.up * 90f * (Utility.maybe ? -1f : 1f), .5f);
                    Color averageColor = colorPicker.getPixelAverageColor(defaultCellPart, Vector3.down);
                    defaultCellPartColors.Add(averageColor);
                }
                defaultCell?.setColor(defaultCellPartColors);
                cell.transform.DOLocalJump(cell.transform.localPosition, 2f, 1, .5f).SetEase(Ease.Linear);
            }

            yield return new WaitForSeconds(.05f);
        }
        pixelizationCount--;
    }

    private void fillGrid() {
        totalSelectedCells.Add(cellsToSelect.ToList());
        foreach (GridCell cell in cellsToSelect.ToList()) {
            List<GridCell> cellNeighbors = gridSelector.select(cell, filledCellColor);

            if (!canFillGrid) break;

            cellsToSelect.UnionWith(cellNeighbors);
            cellsToSelect.Remove(cell);
        }
    }
    #endregion

    #region public
    public async void activateGameGrid() {
        currentHighlightedCell = placeHighlightedCell();

        await Task.Delay(1000);

        canFillGrid = true;
    }
    #endregion
}
