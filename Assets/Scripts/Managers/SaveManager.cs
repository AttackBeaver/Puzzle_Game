using UnityEngine;
using System.IO;

public class SaveManager
{
    private static string saveFileName = "player_progress.sav";

    private static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, saveFileName); }
    }

    public static void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"The game is saved in {SavePath}. Level: {data.currentLevel}, Seed: {data.levelSeed}");
    }

    public static GameData LoadGame()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log($"The game is loaded. Level: {data.currentLevel}, Seed: {data.levelSeed}");
            return data;
        }
        else
        {
            Debug.Log("No save file found. Creating a new game.");
            return null;
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("The save was deleted.");
        }
    }
}
