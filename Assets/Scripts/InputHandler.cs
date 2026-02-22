using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Swipe Settings")]
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float maxSwipeTime = 0.5f;

    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isSwiping = false;

    [System.Obsolete]
    private void Update()
    {
        if (Application.isEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.OSXPlayer ||
            Application.platform == RuntimePlatform.LinuxPlayer)
        {
            HandleKeyboardInput();
        }
        else
        {
            HandleTouchInput();
        }
    }

    [System.Obsolete]
    private void HandleKeyboardInput()
    {
        int dx = 0, dz = 0;

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
            MovementManager.Instance?.AttemptMove(dx, dz);
        }
    }

    [System.Obsolete]
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
                            float angle = Vector2.SignedAngle(Vector2.up, direction);

                            int dx = 0, dz = 0;

                            if (angle > -45 && angle <= 45)         // вверх
                                dz = 1;
                            else if (angle > -135 && angle <= -45) // вправо
                                dx = 1;
                            else if (angle > 135 || angle <= -135)  // вниз
                                dz = -1;
                            else if (angle > 45 && angle <= 135)  // влево
                                dx = -1;

                            MovementManager.Instance?.AttemptMove(dx, dz);
                        }
                    }
                    isSwiping = false;
                }
                break;
        }
    }
}
