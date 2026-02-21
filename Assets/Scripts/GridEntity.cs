using UnityEngine;

public class GridEntity : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool isPlayer; // для идентификации (опционально)

    public void SetGridPosition(int x, int z)
    {
        gridPosition = new Vector2Int(x, z);
    }

    // Вызывается MovementManager для синхронного перемещения
    public void MoveTo(Vector2Int newPos, float cellSize)
    {
        gridPosition = newPos;
        transform.position = new Vector3(newPos.x * cellSize, transform.position.y, newPos.y * cellSize);
    }
}
