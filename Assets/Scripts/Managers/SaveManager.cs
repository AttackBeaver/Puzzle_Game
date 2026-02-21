using UnityEngine;
using System.IO;

public class SaveManager
{
    // Имя файла сохранения
    private static string saveFileName = "player_progress.sav";

    // Путь к файлу на устройстве (персистентный путь)
    private static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, saveFileName); }
    }

    /// <summary>
    /// Сохранить игру
    /// </summary>
    /// <param name="data">Данные для сохранения</param>
    public static void SaveGame(GameData data)
    {
        // 1. Преобразуем объект в JSON строку
        string json = JsonUtility.ToJson(data, true); // true - делает красивый отступ, удобно читать
        // 2. Записываем строку в файл
        File.WriteAllText(SavePath, json);

        Debug.Log($"Игра сохранена в {SavePath}. Уровень: {data.currentLevel}, Сид: {data.levelSeed}");
    }

    /// <summary>
    /// Загрузить игру
    /// </summary>
    /// <returns>Объект GameData, или null, если сохранения нет</returns>
    public static GameData LoadGame()
    {
        if (File.Exists(SavePath))
        {
            // 1. Читаем JSON из файла
            string json = File.ReadAllText(SavePath);
            // 2. Преобразуем JSON обратно в объект
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log($"Игра загружена. Уровень: {data.currentLevel}, Сид: {data.levelSeed}");
            return data;
        }
        else
        {
            Debug.Log("Сохранение не найдено. Создаем новую игру.");
            return null; // Сохранения нет
        }
    }

    /// <summary>
    /// Сброс прогресса (для отладки или кнопки "Новая игра")
    /// </summary>
    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Сохранение удалено.");
        }
    }
}
