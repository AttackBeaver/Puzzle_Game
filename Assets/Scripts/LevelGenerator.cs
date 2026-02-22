using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelDifficulty
{
    public int fromLevel;               // начальный уровень (включительно)
    public int toLevel;                 // конечный уровень (включительно)
    [Range(0f, 1f)] public float stoneDensity;
    public int slimeCount;
    public int skeletonCount;
}

public class LevelGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject stonePrefab;
    public GameObject playerPrefab;
    public GameObject slimePrefab;
    public GameObject skeletonPrefab;
    public GameObject finishPrefab;

    [Header("The size of the arena")]
    public int gridSizeX = 11;
    public int gridSizeZ = 11;

    [Header("Cell Parameters")]
    public float cellSize = 1f;

    [Header("Difficulty Progression")]
    public LevelDifficulty[] difficultyLevels;

    // Старые поля (используются, если difficultyLevels пуст)
    [Header("Fallback Settings (if no difficulty levels)")]
    public float stoneDensity = 0.4f;
    public int slimeCountEarly = 2;
    public int slimeCountLate = 2;
    public int skeletonCountLate = 2;
    public int levelThreshold = 10;

    private System.Random random;
    private Transform levelParent;
    private Vector3 centerPosition = Vector3.zero;

    private bool[,] stones;
    private Vector2Int playerPos;
    private Vector2Int exitPos;

    private int halfX, halfZ;

    public void GenerateLevel(int seed, int currentLevel)
    {
        halfX = gridSizeX / 2;
        halfZ = gridSizeZ / 2;

        random = new System.Random(seed);

        if (levelParent != null)
            Destroy(levelParent.gameObject);
        levelParent = new GameObject("Level").transform;

        float targetDensity = stoneDensity; // по умолчанию
        int slimeCount = slimeCountEarly;
        int skeletonCount = 0;

        if (difficultyLevels != null && difficultyLevels.Length > 0)
        {
            foreach (var d in difficultyLevels)
            {
                if (currentLevel >= d.fromLevel && currentLevel <= d.toLevel)
                {
                    targetDensity = d.stoneDensity;
                    slimeCount = d.slimeCount;
                    skeletonCount = d.skeletonCount;
                    break;
                }
            }
        }
        else
        {
            // Старая логика
            if (currentLevel <= levelThreshold)
            {
                slimeCount = slimeCountEarly;
                skeletonCount = 0;
            }
            else
            {
                slimeCount = slimeCountLate;
                skeletonCount = skeletonCountLate;
            }
            targetDensity = stoneDensity;
        }

        stones = new bool[gridSizeX, gridSizeZ];

        CreateFloor();
        ChoosePlayerAndExit();
        GenerateStones(targetDensity);               // плотность
        CreatePerimeterWalls();
        PlaceStones();
        PlacePlayer();
        PlaceExit();
        PlaceEnemies(slimeCount, skeletonCount);     // количество
    }

    public bool IsInArena(int x, int z)
    {
        return x >= -halfX && x <= halfX && z >= -halfZ && z <= halfZ;
    }

    private bool IsBorder(int x, int z)
    {
        return x == -halfX || x == halfX || z == -halfZ || z == halfZ;
    }

    private Vector3 GridToWorld(int x, int z, float yOffset = 0.5f)
    {
        return new Vector3(x * cellSize, yOffset, z * cellSize);
    }

    private void CreateFloor()
    {
        for (int x = -halfX; x <= halfX; x++)
        {
            for (int z = -halfZ; z <= halfZ; z++)
            {
                if (!IsInArena(x, z)) continue;
                Vector3 pos = GridToWorld(x, z, -0.05f);
                GameObject tile = Instantiate(floorPrefab, pos, Quaternion.identity, levelParent);
                tile.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
            }
        }
    }

    private void ChoosePlayerAndExit()
    {
        List<Vector2Int> innerCells = new List<Vector2Int>();
        List<Vector2Int> borderCells = new List<Vector2Int>();

        for (int x = -halfX; x <= halfX; x++)
        {
            for (int z = -halfZ; z <= halfZ; z++)
            {
                if (!IsInArena(x, z)) continue;
                if (IsBorder(x, z))
                    borderCells.Add(new Vector2Int(x, z));
                else
                    innerCells.Add(new Vector2Int(x, z));
            }
        }

        playerPos = innerCells[random.Next(innerCells.Count)];
        exitPos = borderCells[random.Next(borderCells.Count)];
    }

    private void GenerateStones(float density)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = -halfX; x <= halfX; x++)
        {
            for (int z = -halfZ; z <= halfZ; z++)
            {
                if (!IsInArena(x, z)) continue;
                if (IsBorder(x, z)) continue;
                if (x == playerPos.x && z == playerPos.y) continue;
                if (x == exitPos.x && z == exitPos.y) continue;
                candidates.Add(new Vector2Int(x, z));
            }
        }

        int stoneCount = Mathf.FloorToInt(candidates.Count * density);
        stoneCount = Mathf.Clamp(stoneCount, 0, candidates.Count);

        int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            stones = new bool[gridSizeX, gridSizeZ];
            Shuffle(candidates);
            for (int i = 0; i < stoneCount; i++)
            {
                Vector2Int cell = candidates[i];
                stones[cell.x + halfX, cell.y + halfZ] = true;
            }
            if (IsPathExists())
            {
                Debug.Log($"Generated for {attempt + 1} step(s)");
                return;
            }
        }

        Debug.LogWarning("Failed to create a passable maze, and the stones are not placed");
        stones = new bool[gridSizeX, gridSizeZ];
    }

    private bool IsPathExists()
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(playerPos);
        visited.Add(playerPos);

        int[] dx = { 1, -1, 0, 0 };
        int[] dz = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            if (cur == exitPos) return true;

            for (int i = 0; i < 4; i++)
            {
                int nx = cur.x + dx[i];
                int nz = cur.y + dz[i];
                Vector2Int next = new Vector2Int(nx, nz);

                if (!IsInArena(nx, nz)) continue;
                if (IsBorder(nx, nz) && (nx != exitPos.x || nz != exitPos.y)) continue;
                if (stones[nx + halfX, nz + halfZ]) continue;

                if (!visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return false;
    }

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void CreatePerimeterWalls()
    {
        for (int x = -halfX; x <= halfX; x++)
        {
            for (int z = -halfZ; z <= halfZ; z++)
            {
                if (!IsBorder(x, z)) continue;
                if (x == exitPos.x && z == exitPos.y) continue;

                Vector3 pos = GridToWorld(x, z, 0.5f);
                Quaternion rot = Quaternion.identity;

                if (x == -halfX)           // левая граница
                    rot = Quaternion.Euler(0, 90, 0);
                else if (x == halfX)        // правая граница
                    rot = Quaternion.Euler(0, 90, 0);
                else if (z == -halfZ)       // нижняя граница
                    rot = Quaternion.Euler(0, 0, 0);
                else if (z == halfZ)         // верхняя граница
                    rot = Quaternion.Euler(0, 0, 0);

                Instantiate(wallPrefab, pos, rot, levelParent);
            }
        }
    }

    private void PlaceStones()
    {
        for (int x = -halfX; x <= halfX; x++)
        {
            for (int z = -halfZ; z <= halfZ; z++)
            {
                if (!IsInArena(x, z)) continue;
                if (IsBorder(x, z)) continue;
                if (stones[x + halfX, z + halfZ])
                {
                    Vector3 pos = GridToWorld(x, z, 0.5f);
                    Instantiate(stonePrefab, pos, Quaternion.identity, levelParent);
                }
            }
        }
    }

    private void PlacePlayer()
    {
        Vector3 pos = GridToWorld(playerPos.x, playerPos.y, 0.5f);
        GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity, levelParent);
        GridEntity entity = player.GetComponent<GridEntity>();
        if (entity != null)
        {
            entity.SetGridPosition(playerPos.x, playerPos.y);
            entity.isPlayer = true;  // <-- помечаем как игрока
            MovementManager.Instance?.RegisterEntity(entity);
        }
    }

    private void PlaceExit()
    {
        Vector3 pos = GridToWorld(exitPos.x, exitPos.y, 0f);
        Instantiate(finishPrefab, pos, Quaternion.identity, levelParent);
    }

    private void PlaceEnemies(int slimeCount, int skeletonCount)
    {
        List<Vector2Int> empty = new List<Vector2Int>();
        for (int x = -halfX; x <= halfX; x++)
        {
            for (int z = -halfZ; z <= halfZ; z++)
            {
                if (!IsInArena(x, z)) continue;
                if (IsBorder(x, z)) continue;
                if (x == playerPos.x && z == playerPos.y) continue;
                if (x == exitPos.x && z == exitPos.y) continue;
                if (!stones[x + halfX, z + halfZ])
                    empty.Add(new Vector2Int(x, z));
            }
        }

        Shuffle(empty);

        int index = 0;
        for (int i = 0; i < slimeCount && index < empty.Count; i++, index++)
        {
            Vector3 pos = GridToWorld(empty[index].x, empty[index].y, 0.5f);
            GameObject slime = Instantiate(slimePrefab, pos, Quaternion.identity, levelParent);
            GridEntity entity = slime.GetComponent<GridEntity>();
            if (entity != null)
            {
                entity.SetGridPosition(empty[index].x, empty[index].y);
                MovementManager.Instance?.RegisterEntity(entity);
            }
        }
        for (int i = 0; i < skeletonCount && index < empty.Count; i++, index++)
        {
            Vector3 pos = GridToWorld(empty[index].x, empty[index].y, 0.5f);
            GameObject skeleton = Instantiate(skeletonPrefab, pos, Quaternion.identity, levelParent);
            GridEntity entity = skeleton.GetComponent<GridEntity>();
            if (entity != null)
            {
                entity.SetGridPosition(empty[index].x, empty[index].y);
                MovementManager.Instance?.RegisterEntity(entity);
            }
        }
    }

    public bool IsObstacleAt(int x, int z)
    {
        if (!IsInArena(x, z)) return true;
        if (IsBorder(x, z))
        {
            return !(x == exitPos.x && z == exitPos.y);
        }
        return stones[x + halfX, z + halfZ];
    }
}
