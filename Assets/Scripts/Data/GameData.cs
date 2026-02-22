using UnityEngine;

[System.Serializable]
public class GameData
{
    public int currentLevel;  // Текущий уровень
    public int levelSeed;      // Сид

    public GameData()
    {
        currentLevel = 1;
        levelSeed = 0;
    }
}
