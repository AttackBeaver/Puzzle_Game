using UnityEngine;
using System.Collections.Generic;

public class MovementManager : MonoBehaviour
{
    public static MovementManager Instance { get; private set; }

    private List<GridEntity> allEntities = new List<GridEntity>();
    private LevelGenerator levelGenerator;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [System.Obsolete]
    private void Start()
    {
        levelGenerator = FindObjectOfType<LevelGenerator>();
        if (levelGenerator == null)
            Debug.LogError("MovementManager: LevelGenerator not found!");

        // Подхватываем уже существующих существ (на случай, если генерация произошла раньше)
        var existing = FindObjectsOfType<GridEntity>();
        foreach (var e in existing)
        {
            if (!allEntities.Contains(e))
                allEntities.Add(e);
        }
    }

    public void RegisterEntity(GridEntity entity)
    {
        if (!allEntities.Contains(entity))
            allEntities.Add(entity);
    }

    public void UnregisterEntity(GridEntity entity)
    {
        allEntities.Remove(entity);
    }

    [System.Obsolete]
    public void RefreshEntities()
    {
        allEntities.Clear();
        allEntities.AddRange(FindObjectsOfType<GridEntity>());
    }

    [System.Obsolete]
    public bool AttemptMove(int dx, int dz)
    {
        if (dx == 0 && dz == 0) return false;

        // Словарь: существо → новая позиция (для тех, кто может двигаться)
        Dictionary<GridEntity, Vector2Int> moves = new Dictionary<GridEntity, Vector2Int>();

        // Шаг 1: определяем, кто может двигаться (проверяем только стены и границы)
        foreach (var entity in allEntities)
        {
            Vector2Int newPos = entity.gridPosition + new Vector2Int(dx, dz);

            // Проверка границ арены
            if (!levelGenerator.IsInArena(newPos.x, newPos.y))
                continue;

            // Проверка препятствий (камни, стены)
            if (levelGenerator.IsObstacleAt(newPos.x, newPos.y))
                continue;

            // Клетка свободна от препятствий – добавляем в кандидаты
            moves[entity] = newPos;
        }

        // Шаг 2: исключаем конфликты (два существа хотят в одну клетку)
        Dictionary<Vector2Int, int> cellCount = new Dictionary<Vector2Int, int>();
        foreach (var pos in moves.Values)
        {
            if (cellCount.ContainsKey(pos))
                cellCount[pos]++;
            else
                cellCount[pos] = 1;
        }

        HashSet<Vector2Int> conflictCells = new HashSet<Vector2Int>();
        foreach (var kvp in cellCount)
            if (kvp.Value > 1) conflictCells.Add(kvp.Key);

        // Удаляем всех, кто целится в конфликтные клетки
        List<GridEntity> toRemove = new List<GridEntity>();
        foreach (var kvp in moves)
            if (conflictCells.Contains(kvp.Value))
                toRemove.Add(kvp.Key);
        foreach (var rem in toRemove)
            moves.Remove(rem);

        // Шаг 3: проверка на смерть игрока
        GridEntity player = null;
        Vector2Int? playerNewPos = null;
        foreach (var kvp in moves)
        {
            if (kvp.Key.isPlayer)
            {
                player = kvp.Key;
                playerNewPos = kvp.Value;
                break;
            }
        }

        if (playerNewPos.HasValue)
        {
            // Проверяем, не совпадает ли новая позиция игрока с позицией любого другого существа
            foreach (var entity in allEntities)
            {
                if (entity == player) continue;
                Vector2Int otherPos = moves.ContainsKey(entity) ? moves[entity] : entity.gridPosition;
                if (otherPos == playerNewPos.Value)
                {
                    // Смерть!
                    PlayerDied();
                    return true; // движение отменяется
                }
            }
        }

        // Шаг 4: выполняем перемещение
        foreach (var kvp in moves)
            kvp.Key.MoveTo(kvp.Value, levelGenerator.cellSize);

        return moves.Count > 0;
    }

    [System.Obsolete]
    private void PlayerDied()
    {
        Debug.Log("Player died! Restarting level...");
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
    }
}
