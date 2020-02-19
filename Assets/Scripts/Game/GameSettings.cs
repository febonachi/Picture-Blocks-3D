using Utils;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameSettings {
    #region public properties
    public bool sound => soundOn.toBool();
    public bool vibration => vibrationOn.toBool();
    public int currentSavedLevel => currentLevel;
    public int levelsPassed => totalLevelsPassed;
    public int levelsCount => totalLevelsCount;
    public bool gameStarted => localGameIsStarted;
    #endregion

    private bool debug = false;

    private int soundOn = 0;
    private int vibrationOn = 0;
    private int currentLevel = 0;
    private int totalLevelsPassed = 0;
    private int totalLevelsCount = 0;
    private bool localGameIsStarted = false;

    List<int> notPassedLevels = new List<int>();

    #region private
    private void initializeSavedSettings(int levelToLoad) {
        soundOn = PlayerPrefs.GetInt("soundOn");
        vibrationOn = PlayerPrefs.GetInt("vibrationOn");
        currentLevel = PlayerPrefs.GetInt("currentLevel");
        totalLevelsPassed = PlayerPrefs.GetInt("totalLevelsPassed");

        for (int i = 1; i < totalLevelsCount; i++) {
            if (!PlayerPrefs.GetInt($"Level_{i}").toBool()) notPassedLevels.Add(i); // this levels is not passed
        }

        localGameIsStarted = false;

        if (debug) {
            currentLevel = levelToLoad;
        } // dbg stuff
    }
    #endregion

    #region public
    public GameSettings(bool debugMode, bool firstLaunch, int levelToLoad, int levelsCount) {
        debug = debugMode;

        totalLevelsCount = levelsCount;

        if (!PlayerPrefs.GetInt("globalSettingsInitialized").toBool() || firstLaunch) initializeGlobalSettings();

        initializeSavedSettings(levelToLoad);
    }

    public void initializeGlobalSettings() {
        PlayerPrefs.SetInt("soundOn", true.toInt());
        PlayerPrefs.SetInt("vibrationOn", true.toInt());

        PlayerPrefs.SetInt("currentLevel", 1);
        PlayerPrefs.SetInt("totalLevelsPassed", 0);

        for(int i = 1; i < totalLevelsCount; i++) PlayerPrefs.SetInt($"Level_{i}", false.toInt());

        PlayerPrefs.SetInt("globalSettingsInitialized", true.toInt());        
    }

    public void saveCurrentLevelIndex(int value) {
        currentLevel = value;

        PlayerPrefs.SetInt("currentLevel", currentLevel);

        localGameIsStarted = false; // stop local game when new scene loaded
    }

    public void setSound(bool state){
        soundOn = state.toInt();

        PlayerPrefs.SetInt("soundOn", soundOn);
    }

    public void setVibration(bool state){
        vibrationOn = state.toInt();

        PlayerPrefs.SetInt("vibrationOn", vibrationOn);
    }

    public void startLocalGame() => localGameIsStarted = true;

    public void newLevelPassed() {
        totalLevelsPassed++;
        PlayerPrefs.SetInt("totalLevelsPassed", totalLevelsPassed);

        PlayerPrefs.SetInt($"Level_{currentLevel}", true.toInt());
        notPassedLevels.Remove(currentLevel);
    }

    public int nextLevelIndex() {
        int index = 1;
        if (notPassedLevels.Count != 0) {
            index = notPassedLevels.First();
        } else {
            int[] indexes = Enumerable.Range(1, totalLevelsCount - 1).Where(i => i != currentSavedLevel).ToArray();
            index = indexes[Random.Range(0, indexes.Length)];
        }
        return index;
    }
    #endregion
}
