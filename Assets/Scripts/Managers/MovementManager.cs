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

        // Подстраховка: добавляем всех существ, которые уже есть в сцене (например, при первом запуске)
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
        if (allEntities.Contains(entity))
            allEntities.Remove(entity);
    }

    public bool AttemptMove(int dx, int dz)
    {
        if (dx == 0 && dz == 0) return false;

        // Словарь: существо → новая позиция (для тех, кто может двигаться)
        Dictionary<GridEntity, Vector2Int> moves = new Dictionary<GridEntity, Vector2Int>();

        // Шаг 1: проверяем возможность движения для каждого
        foreach (var entity in allEntities)
        {
            Vector2Int newPos = entity.gridPosition + new Vector2Int(dx, dz);

            // Проверка границ арены
            if (!levelGenerator.IsInArena(newPos.x, newPos.y))
                continue;

            // Проверка препятствий (камни, стены)
            if (levelGenerator.IsObstacleAt(newPos.x, newPos.y))
                continue;

            // Проверка занятости клетки другими существами (текущие позиции)
            bool occupied = false;
            foreach (var other in allEntities)
            {
                if (other == entity) continue;
                if (other.gridPosition == newPos)
                {
                    occupied = true;
                    break;
                }
            }
            if (occupied) continue;

            // Все проверки пройдены
            moves[entity] = newPos;
        }

        // Шаг 2: исключаем конфликты (два существа хотят в одну клетку)
        // Считаем, сколько существ хотят в каждую клетку
        Dictionary<Vector2Int, int> cellCount = new Dictionary<Vector2Int, int>();
        foreach (var pos in moves.Values)
        {
            if (cellCount.ContainsKey(pos))
                cellCount[pos]++;
            else
                cellCount[pos] = 1;
        }

        // Клетки, в которые хотят больше одного
        HashSet<Vector2Int> conflictCells = new HashSet<Vector2Int>();
        foreach (var kvp in cellCount)
        {
            if (kvp.Value > 1)
                conflictCells.Add(kvp.Key);
        }

        // Удаляем всех, кто целится в конфликтные клетки
        List<GridEntity> toRemove = new List<GridEntity>();
        foreach (var kvp in moves)
        {
            if (conflictCells.Contains(kvp.Value))
                toRemove.Add(kvp.Key);
        }
        foreach (var rem in toRemove)
        {
            moves.Remove(rem);
        }

        // Шаг 3: выполняем перемещение
        foreach (var kvp in moves)
        {
            kvp.Key.MoveTo(kvp.Value, levelGenerator.cellSize);
        }

        return moves.Count > 0;
    }
}
