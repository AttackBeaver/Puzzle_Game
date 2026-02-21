using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Префабы")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject stonePrefab;
    public GameObject playerPrefab;
    public GameObject slimePrefab;
    public GameObject skeletonPrefab;
    public GameObject finishPrefab;
    public GameObject pointerPrefab;

    [Header("Параметры арены")]
    public int gridRadius = 5;          // Радиус ромба (количество клеток от центра)
    public float cellSize = 2f;          // Расстояние между центрами ячеек

    [Header("Настройки врагов")]
    public int slimeCountEarly = 2;      // для уровней 1-10
    public int slimeCountLate = 2;       // для уровней 11-20
    public int skeletonCountLate = 2;
    public int levelThreshold = 10;

    private System.Random random;
    private Transform levelParent;
    private Vector3 centerPosition = Vector3.zero;

    // Сетка препятствий: true - камень, false - пусто
    private bool[,] stones;
    private Vector2Int playerPos;
    private Vector2Int exitPos;

    public void GenerateLevel(int seed, int currentLevel)
    {
        random = new System.Random(seed);

        // Очистка предыдущего уровня
        if (levelParent != null)
            Destroy(levelParent.gameObject);
        levelParent = new GameObject("Level").transform;

        // Пол
        CreateFloor();

        // Выбор позиций игрока и выхода
        ChoosePlayerAndExit();

        // Инициализация сетки камней (все пусто)
        int size = 2 * gridRadius + 1;
        stones = new bool[size, size];

        // Генерация камней с проверкой проходимости
        GenerateStones();

        // Размещение стен по периметру (кроме клетки выхода)
        CreatePerimeterWalls();
        //CreateSolidPerimeter();

        // Размещение камней
        PlaceStones();

        // Игрок
        PlacePlayer();

        // Выход
        PlaceExit();

        // Враги
        PlaceEnemies(currentLevel);

        // Указатель на выход
        PlacePointer();
    }

    private bool IsInArena(int x, int z) => Mathf.Abs(x) + Mathf.Abs(z) <= gridRadius;

    private Vector3 GridToWorld(int x, int z, float yOffset = 0.5f)
    {
        return new Vector3(x * cellSize, yOffset, z * cellSize);
    }

    private void CreateFloor()
    {
        float diameter = gridRadius * cellSize * 2;
        // Предполагаем, что Plane по умолчанию 10x10
        float scale = diameter / 10f;
        GameObject floor = Instantiate(floorPrefab, centerPosition, Quaternion.identity, levelParent);
        floor.transform.localScale = new Vector3(scale, 1, scale);
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

        // Игрок — случайная внутренняя клетка
        playerPos = innerCells[random.Next(innerCells.Count)];

        // Выход — случайная граничная клетка (может совпасть с игроком? но игрок внутри, так что ок)
        exitPos = borderCells[random.Next(borderCells.Count)];
    }

    private void GenerateStones()
    {
        // Список кандидатов (все внутренние клетки, кроме игрока и выхода)
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

        Shuffle(candidates);

        foreach (var cell in candidates)
        {
            int ix = cell.x + gridRadius;
            int iz = cell.y + gridRadius;
            stones[ix, iz] = true; // временно ставим камень

            if (!IsPathExists())
            {
                stones[ix, iz] = false; // убираем, если заблокировало путь
            }
        }
    }

    private bool IsPathExists()
    {
        // BFS от playerPos до exitPos
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
                // Граничные клетки проходимы только если это выход
                if (Mathf.Abs(nx) + Mathf.Abs(nz) == gridRadius && next != exitPos) continue;
                // Проверка камня
                if (stones[nx + gridRadius, nz + gridRadius]) continue;

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
        Instantiate(playerPrefab, pos, Quaternion.identity, levelParent);
    }

    private void PlaceExit()
    {
        Vector3 pos = GridToWorld(exitPos.x, exitPos.y);
        Instantiate(finishPrefab, pos, Quaternion.identity, levelParent);
    }

    private void PlaceEnemies(int currentLevel)
    {
        // Собираем все пустые клетки (не стена, не игрок, не выход)
        List<Vector2Int> empty = new List<Vector2Int>();
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                if (!IsInArena(x, z)) continue;
                if (Mathf.Abs(x) + Mathf.Abs(z) == gridRadius) continue; // граница не используется
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
        for (int i = 0; i < slimeCount; i++)
        {
            Vector3 pos = GridToWorld(empty[index].x, empty[index].y, 0.5f);
            Instantiate(slimePrefab, pos, Quaternion.identity, levelParent);
            index++;
        }
        for (int i = 0; i < skeletonCount; i++)
        {
            Vector3 pos = GridToWorld(empty[index].x, empty[index].y, 0.5f);
            Instantiate(slimePrefab, pos, Quaternion.identity, levelParent);
            index++;
        }
    }

    private void PlacePointer()
    {
        // Ставим указатель в центр арены и поворачиваем в сторону выхода
        Vector3 pos = centerPosition;
        GameObject pointer = Instantiate(pointerPrefab, pos, Quaternion.identity, levelParent);
        Vector3 exitWorld = GridToWorld(exitPos.x, exitPos.y);
        Vector3 direction = (exitWorld - pos).normalized;
        pointer.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void CreateSolidPerimeter()
    {
        // Словарь для хранения уже обработанных рёбер (чтобы не ставить две стены на одно ребро)
        HashSet<(int, int, int, int)> edges = new HashSet<(int, int, int, int)>();

        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                if (!IsInArena(x, z) || Mathf.Abs(x) + Mathf.Abs(z) != gridRadius)
                    continue;
                if (x == exitPos.x && z == exitPos.y) continue;

                // Проверяем четырех соседей (dx, dz) = (1,0), (-1,0), (0,1), (0,-1)
                int[] dx = { 1, -1, 0, 0 };
                int[] dz = { 0, 0, 1, -1 };
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int nz = z + dz[i];
                    if (!IsInArena(nx, nz)) continue; // сосед вне арены

                    // Упорядочиваем координаты ребра, чтобы избежать дублирования
                    int ax = Mathf.Min(x, nx);
                    int az = Mathf.Min(z, nz);
                    int bx = Mathf.Max(x, nx);
                    int bz = Mathf.Max(z, nz);
                    var edge = (ax, az, bx, bz);
                    if (edges.Contains(edge)) continue;
                    edges.Add(edge);

                    // Если сосед тоже на периметре, то ребро внутреннее, стена не нужна
                    if (Mathf.Abs(nx) + Mathf.Abs(nz) == gridRadius)
                        continue;

                    // Ребро ведёт наружу арены – ставим стену
                    // Позиция стены – середина ребра
                    Vector3 pos = new Vector3((x + nx) * 0.5f * cellSize, 0.5f, (z + nz) * 0.5f * cellSize);
                    // Поворот стены: если dx != 0, то стена вдоль Z, иначе вдоль X
                    Quaternion rot = (dx[i] != 0) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                    Instantiate(wallPrefab, pos, rot, levelParent);
                }
            }
        }
    }
}
