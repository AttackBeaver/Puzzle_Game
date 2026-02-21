using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Ссылка на самого себя (простой синглтон для удобства доступа из других скриптов)
    public static GameManager Instance { get; private set; }
    public LevelGenerator levelGenerator; // назначить в инспекторе

    [Header("Данные")]
    public GameData currentGameData; // Текущие данные игры в оперативной памяти

    [Header("Настройки")]
    public int firstLevel = 1; // Для наглядности

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
    public void RestartLevel()
    {
        GenerateLevel(currentGameData.levelSeed, currentGameData.currentLevel);
    }

    private void GenerateLevel(int seed, int level)
    {
        levelGenerator.GenerateLevel(seed, level);
    }
}
