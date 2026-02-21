using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Swipe Settings")]
    [SerializeField] private float swipeThreshold = 50f;     // минимальное расстояние свайпа в пикселях
    [SerializeField] private float maxSwipeTime = 0.5f;      // максимальное время свайпа

    [Header("Debug (PC)")]
    [SerializeField] private bool useDiagonalMapping = true; // для изометрии: вверх/вниз -> диагонали

    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isSwiping = false;

    private void Update()
    {
        // ПК-управление (клавиши)
        if (Application.isEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.OSXPlayer ||
            Application.platform == RuntimePlatform.LinuxPlayer)
        {
            HandleKeyboardInput();
        }
        else // Мобильные устройства
        {
            HandleTouchInput();
        }
    }

    private void HandleKeyboardInput()
    {
        int dx = 0, dz = 0;

        // WASD или стрелки
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            dz = 1;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            dz = -1;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            dx = -1;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            dx = 1;

        if (dx != 0 || dz != 0)
        {
            if (useDiagonalMapping)
            {
                // Преобразуем ортогональные направления в диагональные для изометрии
                // Здесь мы используем простое правило: W = (-1,1), S = (1,-1), A = (-1,-1), D = (1,1)
                // Это нужно настроить под твою камеру
                if (dx == 0 && dz == 1)      // W -> вверх по экрану
                    (dx, dz) = (-1, 1);
                else if (dx == 0 && dz == -1) // S -> вниз
                    (dx, dz) = (1, -1);
                else if (dx == -1 && dz == 0) // A -> влево
                    (dx, dz) = (-1, -1);
                else if (dx == 1 && dz == 0)  // D -> вправо
                    (dx, dz) = (1, 1);
            }

            MovementManager.Instance?.AttemptMove(dx, dz);
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartPos = touch.position;
                touchStartTime = Time.time;
                isSwiping = true;
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isSwiping)
                {
                    float swipeTime = Time.time - touchStartTime;
                    if (swipeTime <= maxSwipeTime)
                    {
                        Vector2 swipeDelta = touch.position - touchStartPos;
                        if (swipeDelta.magnitude >= swipeThreshold)
                        {
                            Vector2 direction = swipeDelta.normalized;
                            float angle = Vector2.SignedAngle(Vector2.up, direction); // угол от вертикали вверх

                            int dx = 0, dz = 0;

                            if (angle > -45 && angle <= 45)         // вверх экрана -> влево
                                (dx, dz) = (-1, -1);
                            else if (angle > 45 && angle <= 135)    // вправо экрана -> вниз
                                (dx, dz) = (1, -1);
                            else if (angle > 135 || angle <= -135)  // вниз экрана -> вправо
                                (dx, dz) = (1, 1);
                            else if (angle > -135 && angle <= -45)  // влево экрана -> вверх
                                (dx, dz) = (-1, 1);

                            MovementManager.Instance?.AttemptMove(dx, dz);
                        }
                    }
                    isSwiping = false;
                }
                break;
        }
    }
}
