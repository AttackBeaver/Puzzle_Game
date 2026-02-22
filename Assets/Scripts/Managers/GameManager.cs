using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Ссылка на самого себя (простой синглтон для удобства доступа из других скриптов)
    public static GameManager Instance { get; private set; }
    public LevelGenerator levelGenerator; // назначить в инспекторе

    [Header("Данные")]
    public GameData currentGameData; // Текущие данные игры в оперативной памяти

    [Header("Настройки")]
    public int firstLevel = 1; // Для наглядности

    [System.Obsolete]
    private void Awake()
    {
        // Простая реализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не удалять при загрузке сцен (если они будут)
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
        // 1. Пытаемся загрузить сохранение
        GameData loadedData = SaveManager.LoadGame();

        if (loadedData != null)
        {
            // Сохранение найдено - продолжаем игру
            currentGameData = loadedData;
            Debug.Log("Продолжаем игру. Загружаем уровень...");
            // Здесь будет вызов генерации уровня с currentGameData.levelSeed
            GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
        }
        else
        {
            // Сохранения нет - создаем новую игру
            currentGameData = new GameData();
            // Генерируем сид. Например, на основе системного времени.
            currentGameData.levelSeed = System.DateTime.Now.GetHashCode();
            Debug.Log("Новая игра. Генерируем уровень...");
            GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
        }

        // После генерации уровня нужно обновить текст в UI с номером уровня.
        // Этим займется UIManager, который мы создадим позже.
    }

    /// <summary>
    /// Вызывать, когда игрок прошел уровень
    /// </summary>
    [System.Obsolete]
    public void LevelCompleted()
    {
        currentGameData.currentLevel++;
        currentGameData.levelSeed = System.DateTime.Now.GetHashCode();
        SaveManager.SaveGame(currentGameData);
        GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
        // Обновить UI
        FindObjectOfType<UIManager>()?.UpdateLevelText(currentGameData.currentLevel);
    }

    /// <summary>
    /// Вызывать, когда игрок нажал кнопку "Перезапустить"
    /// </summary>
    [System.Obsolete]
    public void RestartLevel()
    {
        currentGameData.levelSeed = System.DateTime.Now.GetHashCode();
        GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
        // if (uiManager != null)
        //     uiManager.UpdateLevelText(currentGameData.currentLevel);
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
        SaveManager.DeleteSave();                 // удаляем файл сохранения
        currentGameData = new GameData();         // новые данные (уровень 1, сид 0)
        currentGameData.levelSeed = System.DateTime.Now.GetHashCode(); // случайный сид
        // Не генерируем уровень сразу, он сгенерируется при загрузке сцены "Level"
    }

    [System.Obsolete]
    public void PlayerDied()
    {
        // Перезапускаем уровень с тем же сидом (игрок начинает заново этот же уровень)
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
        }
    }
}
