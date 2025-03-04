using UnityEngine;
using UnityEngine.SceneManagement;

public class Progress_Saver : MonoBehaviour
{
    // Ключ для сохранения номера уровня в PlayerPrefs
    private const string SavedLevelKey = "SavedLevel";

    /// <summary>
    /// Сохраняет номер текущей сцены (уровня) в PlayerPrefs.
    /// Вызывайте этот метод, когда переходите на новый уровень.
    /// </summary>
    public static void SaveProgress(int levelIndex)
    {
        PlayerPrefs.SetInt(SavedLevelKey, levelIndex);
        PlayerPrefs.Save();
        Debug.Log("Progress saved: Level " + levelIndex);
    }

    /// <summary>
    /// Загружает уровень, сохранённый в PlayerPrefs.
    /// Если прогресс не найден, выводит предупреждение.
    /// </summary>
    public static int LoadProgress()
    {
        return PlayerPrefs.HasKey(SavedLevelKey) ? PlayerPrefs.GetInt(SavedLevelKey) : 0;
    }
}