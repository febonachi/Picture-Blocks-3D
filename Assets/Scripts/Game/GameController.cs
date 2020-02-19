using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController instance;

    #region editor
    [SerializeField] private int levelToLoad = 0;

    [Header("Debug Settings (Set to <False> in release version)")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool firstLaunch = false;

    [Space][SerializeField] private Transform managersHolder = default;
    #endregion

    #region public properties
    public GameSettings gameSettings => settings;
    public UIManager uiManager => this["ui"] as UIManager;
    public CameraManager cameraManager => this["camera"] as CameraManager;
    public Manager this[string id] => managers.FirstOrDefault(m => m.id == id);
    #endregion

    private GameSettings settings;

    private Manager[] managers;

    #region private
    private async void Awake() {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        settings = new GameSettings(debugMode, firstLaunch, levelToLoad, Resources.LoadAll<Sprite>("Pictures").Length);

        await loadManagers();

        print("All managers loaded");

        SceneManager.sceneLoaded += onSceneLoaded;
        SceneManager.LoadScene("Level");
    }

    private void Update() {
        if (gameSettings.gameStarted) return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) startLocalGame();
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) startLocalGame();
#endif
    }

    private async Task loadManagers() {
        managers = managersHolder.GetComponentsInChildren<Manager>();
        foreach (Manager manager in managers) manager.initialize();

        bool managersLoaded = false;
        while (!managersLoaded) {
            managersLoaded = managers.All(m => m.status == Manager.Status.Ready);

            await Task.Delay(25);
        }
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "main") return;

        uiManager.resetUIElements();
        uiManager.showMainMenu();

        cameraManager.zoom(1f, 1.25f);
    }

    private void startLocalGame() {
        cameraManager.zoomDefault(1f);

        settings.startLocalGame();

        uiManager.mainMenu.hide();

        FindObjectOfType<GameGridController>().activateGameGrid();
    }
    #endregion

    #region public
    public void vibrate() {
        if (settings.vibration) Handheld.Vibrate();
    }

    public void restartLevel() {
        settings.saveCurrentLevelIndex(settings.currentSavedLevel);

        SceneManager.LoadScene("Level");
    }

    public void nextLevel() {
        settings.newLevelPassed();

        int nextLevelIndex = settings.nextLevelIndex();
        settings.saveCurrentLevelIndex(nextLevelIndex);

        SceneManager.LoadScene("Level");
    }
    #endregion
}
