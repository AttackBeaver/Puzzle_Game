using UnityEngine;

[System.Serializable]
public class GameData
{
    public int currentLevel;  // Текущий уровень игрока
    public int levelSeed;      // Сид (зерно) для генерации последнего уровня

    // Конструктор по умолчанию для нового сохранения
    public GameData()
    {
        currentLevel = 1;
        // Сид можно сгенерировать на основе времени, но пока оставим 0,
        // чтобы уровень при первом запуске был всегда одинаковый для тестов.
        levelSeed = 0;
    }
}
