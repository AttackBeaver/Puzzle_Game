using UnityEngine;
using System.Collections.Generic;

public class MovementManager : MonoBehaviour
{
    public static MovementManager Instance { get; private set; }

    private List<GridEntity> allEntities = new List<GridEntity>();
    private LevelGenerator levelGenerator;

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
        }
    }

    [System.Obsolete]
    private void Start()
    {
        levelGenerator = FindObjectOfType<LevelGenerator>();
        if (levelGenerator == null)
            Debug.LogError("MovementManager: LevelGenerator not found.");

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

        Dictionary<GridEntity, Vector2Int> moves = new Dictionary<GridEntity, Vector2Int>();

        foreach (var entity in allEntities)
        {
            Vector2Int newPos = entity.gridPosition + new Vector2Int(dx, dz);

            if (!levelGenerator.IsInArena(newPos.x, newPos.y))
                continue;

            if (levelGenerator.IsObstacleAt(newPos.x, newPos.y))
                continue;

            moves[entity] = newPos;
        }

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

        List<GridEntity> toRemove = new List<GridEntity>();
        foreach (var kvp in moves)
            if (conflictCells.Contains(kvp.Value))
                toRemove.Add(kvp.Key);
        foreach (var rem in toRemove)
            moves.Remove(rem);

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
            foreach (var entity in allEntities)
            {
                if (entity == player) continue;
                Vector2Int otherPos = moves.ContainsKey(entity) ? moves[entity] : entity.gridPosition;
                if (otherPos == playerNewPos.Value)
                {
                    PlayerDied();
                    return true;
                }
            }
        }

        foreach (var kvp in moves)
            kvp.Key.MoveTo(kvp.Value, levelGenerator.cellSize);

        return moves.Count > 0;
    }

    [System.Obsolete]
    private void PlayerDied()
    {
        Debug.Log("Player died. Restarting level.");
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
    }

    public void ClearEntities()
    {
        allEntities.Clear();
    }
}
