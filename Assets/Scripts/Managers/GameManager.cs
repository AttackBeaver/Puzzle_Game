using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public LevelGenerator levelGenerator;

    [Header("Data")]
    public GameData currentGameData;

    [Header("Settings")]
    public int firstLevel = 1;

    [System.Obsolete]
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeGame();
    }

    [System.Obsolete]
    private void InitializeGame()
    {
        GameData loadedData = SaveManager.LoadGame();

        if (loadedData != null)
        {
            currentGameData = loadedData;
            Debug.Log("Continue the game. Load the level.");
            GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
        }
        else
        {
            currentGameData = new GameData();
            currentGameData.levelSeed = System.DateTime.Now.GetHashCode();
            Debug.Log("New game. Generating a level.");
            GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
        }
    }

    [System.Obsolete]
    public void LevelCompleted()
    {
        currentGameData.currentLevel++;
        currentGameData.levelSeed = System.DateTime.Now.GetHashCode();
        SaveManager.SaveGame(currentGameData);
        GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
        FindObjectOfType<UIManager>()?.UpdateLevelText(currentGameData.currentLevel);
    }

    [System.Obsolete]
    public void RestartLevel()
    {
        currentGameData.levelSeed = System.DateTime.Now.GetHashCode();
        GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
    }

    [System.Obsolete]
    private void GenerateLevel(int seed, int level)
    {
        levelGenerator.GenerateLevel(seed, level);
        if (MovementManager.Instance != null)
            MovementManager.Instance.RefreshEntities();
    }

    public void ResetProgress()
    {
        SaveManager.DeleteSave();
        currentGameData = new GameData
        {
            levelSeed = System.DateTime.Now.GetHashCode() // случайный сид
        };
    }

    [System.Obsolete]
    public void PlayerDied()
    {
        GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
    }

    [System.Obsolete]
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [System.Obsolete]
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [System.Obsolete]
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level")
        {
            GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
            StartCoroutine(DelayedRefreshEntities());
        }
    }

    [System.Obsolete]
    private System.Collections.IEnumerator DelayedRefreshEntities()
    {
        yield return null; // ждём один кадр
        if (MovementManager.Instance != null)
            MovementManager.Instance.RefreshEntities();
    }

    private void OnApplicationQuit()
    {
        SaveManager.SaveGame(currentGameData);
    }
}
