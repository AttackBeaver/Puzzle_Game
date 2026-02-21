using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Префабы")]
    public GameObject floorPrefab;          // плитка пола (куб с высотой 0.1)
    public GameObject wallPrefab;           // стена (куб 1x1x1)
    public GameObject stonePrefab;          // камень (куб 1x1x1)
    public GameObject playerPrefab;
    public GameObject slimePrefab;
    public GameObject skeletonPrefab;
    public GameObject finishPrefab;         // невидимый триггер
    public GameObject pointerPrefab;        // указатель

    [Header("Параметры арены")]
    public int gridRadius = 10;              // радиус ромба в клетках
    public float cellSize = 1f;              // размер клетки

    [Header("Настройки врагов")]
    public int slimeCountEarly = 2;
    public int slimeCountLate = 2;
    public int skeletonCountLate = 2;
    public int levelThreshold = 10;

    [Header("Плотность камней (0-1)")]
    [Range(0f, 1f)]
    public float stoneDensity = 0.4f;

    private System.Random random;
    private Transform levelParent;
    private Vector3 centerPosition = Vector3.zero;

    private bool[,] stones;
    private Vector2Int playerPos;
    private Vector2Int exitPos;

    public void GenerateLevel(int seed, int currentLevel)
    {
        random = new System.Random(seed);

        if (levelParent != null)
            Destroy(levelParent.gameObject);
        levelParent = new GameObject("Level").transform;

        CreateFloor();                // плиточный пол
        ChoosePlayerAndExit();        // выбор позиций игрока и выхода
        GenerateStones();             // новая генерация камней
        //CreateSolidPerimeter();       // сплошные стены по периметру
        CreatePerimeterWalls();
        PlaceStones();                // расстановка камней
        PlacePlayer();
        PlaceExit();
        PlaceEnemies(currentLevel);
        PlacePointerOutside();        // указатель за ареной
    }

    public bool IsInArena(int x, int z)
    {
        return Mathf.Abs(x) + Mathf.Abs(z) <= gridRadius;
    }

    public bool IsObstacleAt(int x, int z)
    {
        // Вне арены считаем препятствием
        if (!IsInArena(x, z)) return true;

        // Клетки периметра непроходимы, кроме выхода
        if (Mathf.Abs(x) + Mathf.Abs(z) == gridRadius)
        {
            return !(x == exitPos.x && z == exitPos.y);
        }

        // Внутренние клетки: проверяем камень
        return stones[x + gridRadius, z + gridRadius];
    }

    private Vector3 GridToWorld(int x, int z, float yOffset = 0.5f)
    {
        return new Vector3(x * cellSize, yOffset, z * cellSize);
    }

    private void CreateFloor()
    {
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                if (!IsInArena(x, z)) continue;
                Vector3 pos = GridToWorld(x, z, -0.05f); // чуть ниже, чтобы объекты стояли сверху
                GameObject tile = Instantiate(floorPrefab, pos, Quaternion.identity, levelParent);
                tile.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
            }
        }
    }

    private void ChoosePlayerAndExit()
    {
        List<Vector2Int> innerCells = new List<Vector2Int>();
        List<Vector2Int> borderCells = new List<Vector2Int>();

        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                if (!IsInArena(x, z)) continue;
                if (Mathf.Abs(x) + Mathf.Abs(z) == gridRadius)
                    borderCells.Add(new Vector2Int(x, z));
                else
                    innerCells.Add(new Vector2Int(x, z));
            }
        }

        playerPos = innerCells[random.Next(innerCells.Count)];
        exitPos = borderCells[random.Next(borderCells.Count)];
    }

    private void GenerateStones()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                if (!IsInArena(x, z)) continue;
                if (Mathf.Abs(x) + Mathf.Abs(z) == gridRadius) continue; // граница
                if (x == playerPos.x && z == playerPos.y) continue;
                if (x == exitPos.x && z == exitPos.y) continue;
                candidates.Add(new Vector2Int(x, z));
            }
        }

        int stoneCount = Mathf.FloorToInt(candidates.Count * stoneDensity);
        stoneCount = Mathf.Clamp(stoneCount, 0, candidates.Count);

        int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            stones = new bool[2 * gridRadius + 1, 2 * gridRadius + 1];
            Shuffle(candidates);
            for (int i = 0; i < stoneCount; i++)
            {
                Vector2Int cell = candidates[i];
                stones[cell.x + gridRadius, cell.y + gridRadius] = true;
            }
            if (IsPathExists())
            {
                Debug.Log($"Уровень сгенерирован за {attempt + 1} попыток");
                return;
            }
        }

        Debug.LogWarning("Не удалось создать проходимый лабиринт, камни не ставятся");
        stones = new bool[2 * gridRadius + 1, 2 * gridRadius + 1];
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
                if (Mathf.Abs(nx) + Mathf.Abs(nz) == gridRadius && next != exitPos) continue; // граница непроходима, кроме выхода
                if (stones[nx + gridRadius, nz + gridRadius]) continue; // камень

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

    // private void CreateSolidPerimeter()
    // {
    //     HashSet<(int, int, int, int)> edges = new HashSet<(int, int, int, int)>();

    //     for (int x = -gridRadius; x <= gridRadius; x++)
    //     {
    //         for (int z = -gridRadius; z <= gridRadius; z++)
    //         {
    //             if (!IsInArena(x, z) || Mathf.Abs(x) + Mathf.Abs(z) != gridRadius)
    //                 continue;
    //             if (x == exitPos.x && z == exitPos.y) continue;

    //             int[] dx = { 1, -1, 0, 0 };
    //             int[] dz = { 0, 0, 1, -1 };
    //             for (int i = 0; i < 4; i++)
    //             {
    //                 int nx = x + dx[i];
    //                 int nz = z + dz[i];
    //                 if (!IsInArena(nx, nz)) continue;

    //                 int ax = Mathf.Min(x, nx);
    //                 int az = Mathf.Min(z, nz);
    //                 int bx = Mathf.Max(x, nx);
    //                 int bz = Mathf.Max(z, nz);
    //                 var edge = (ax, az, bx, bz);
    //                 if (edges.Contains(edge)) continue;
    //                 edges.Add(edge);

    //                 if (Mathf.Abs(nx) + Mathf.Abs(nz) == gridRadius)
    //                     continue; // внутреннее ребро

    //                 Vector3 pos = new Vector3((x + nx) * 0.5f * cellSize, 0.5f, (z + nz) * 0.5f * cellSize);
    //                 Quaternion rot = (dx[i] != 0) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
    //                 Instantiate(wallPrefab, pos, rot, levelParent);
    //             }
    //         }
    //     }
    // }

    private void CreatePerimeterWalls()
    {
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                if (IsInArena(x, z) && Mathf.Abs(x) + Mathf.Abs(z) == gridRadius)
                {
                    if (x == exitPos.x && z == exitPos.y) continue;
                    Vector3 pos = GridToWorld(x, z, 0.5f);
                    Instantiate(wallPrefab, pos, Quaternion.identity, levelParent);
                }
            }
        }
    }

    private void PlaceStones()
    {
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                if (!IsInArena(x, z)) continue;
                if (Mathf.Abs(x) + Mathf.Abs(z) == gridRadius) continue;
                if (stones[x + gridRadius, z + gridRadius])
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
            MovementManager.Instance.RegisterEntity(entity);
        }
    }
    private void PlaceExit()
    {
        Vector3 pos = GridToWorld(exitPos.x, exitPos.y, 0f); // невидимый триггер можно на полу
        Instantiate(finishPrefab, pos, Quaternion.identity, levelParent);
    }

    private void PlaceEnemies(int currentLevel)
    {
        List<Vector2Int> empty = new List<Vector2Int>();
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                if (!IsInArena(x, z)) continue;
                if (Mathf.Abs(x) + Mathf.Abs(z) == gridRadius) continue;
                if (x == playerPos.x && z == playerPos.y) continue;
                if (x == exitPos.x && z == exitPos.y) continue;
                if (!stones[x + gridRadius, z + gridRadius])
                    empty.Add(new Vector2Int(x, z));
            }
        }

        Shuffle(empty);

        int slimeCount = (currentLevel <= levelThreshold) ? slimeCountEarly : slimeCountLate;
        int skeletonCount = (currentLevel <= levelThreshold) ? 0 : skeletonCountLate;

        int index = 0;
        for (int i = 0; i < slimeCount && index < empty.Count; i++, index++)
        {
            Vector3 pos = GridToWorld(empty[index].x, empty[index].y, 0.5f);
            GameObject slime = Instantiate(slimePrefab, pos, Quaternion.identity, levelParent);
            GridEntity entity = slime.GetComponent<GridEntity>();
            if (entity != null)
            {
                entity.SetGridPosition(empty[index].x, empty[index].y);
                MovementManager.Instance.RegisterEntity(entity);
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
                MovementManager.Instance.RegisterEntity(entity);
            }
        }
    }

    private void PlacePointerOutside()
    {
        Vector3 exitWorld = GridToWorld(exitPos.x, exitPos.y, 0f);
        Vector3 dirToExit = exitWorld.normalized;
        float distToExit = Vector3.Distance(centerPosition, exitWorld);
        float extraOffset = 1f;
        Vector3 pointerPos = centerPosition + dirToExit * (distToExit + extraOffset);
        pointerPos.y = 0f;

        Quaternion pointerRot = Quaternion.LookRotation(-dirToExit); // смотрит на арену
        Instantiate(pointerPrefab, pointerPos, pointerRot, levelParent);
    }
}
