using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Основной скрипт управления для игрока.
/// </summary>
public class Player : MonoBehaviour
{
    [Header("PC Input")]
    [SerializeField] private KeyCode keyOne;
    [SerializeField] private KeyCode keyTwo;
    [SerializeField] private Vector3 moveDirection; // Например, (0,0,1)

    [Header("Mobile Input")]
    [SerializeField] private float swipeThreshold = 50f;   // Порог для распознавания свайпа (в пикселях)
    [SerializeField] private float moveSpeed = 5f;           // Скорость движения
    [SerializeField] private float collisionDistance = 0.6f; // Дистанция для проверки столкновения
    [SerializeField] private float maxMoveDuration = 2f;     // Максимальное время движения, если препятствие не обнаружено

    private Vector2 touchStartPos; // Начальная позиция касания (или мыши)
    private bool isSwiping;        // Флаг начала свайпа
    private bool isMobileMoving = false; // Флаг, что движение уже запущено
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    [System.Obsolete]
    private void FixedUpdate()
    {
        if (!Application.isMobilePlatform && !Application.isEditor)
        {
            // ПК-управление: движение по удержанию клавиши
            if (Input.GetKey(keyOne))
            {
                rb.velocity = moveDirection * moveSpeed;
            }
            else if (Input.GetKey(keyTwo))
            {
                rb.velocity = -moveDirection * moveSpeed;
            }
            else if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            // Если запускаем в Editor или на мобильном устройстве, используем обработку свайпов (с мышью в Editor)
            HandleTouchInput();
        }
    }

    [System.Obsolete]
    private void HandleTouchInput()
    {
        // В редакторе симулируем касание с помощью мыши
        if (Application.isEditor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                isSwiping = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (isSwiping)
                {
                    Vector2 mouseDelta = (Vector2)Input.mousePosition - touchStartPos;
                    if (mouseDelta.magnitude > swipeThreshold)
                    {
                        Vector3 swipeDir = DetermineSwipeDirection(mouseDelta);
                        StartCoroutine(MobileMoveCoroutine(swipeDir));
                    }
                }
                isSwiping = false;
            }
        }
        else if (Input.touchCount > 0) // На мобильном устройстве
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isSwiping = true;
                    break;
                case TouchPhase.Ended:
                    if (isSwiping)
                    {
                        Vector2 touchDelta = touch.position - touchStartPos;
                        if (touchDelta.magnitude > swipeThreshold)
                        {
                            Vector3 swipeDir = DetermineSwipeDirection(touchDelta);
                            StartCoroutine(MobileMoveCoroutine(swipeDir));
                        }
                    }
                    isSwiping = false;
                    break;
                case TouchPhase.Canceled:
                    isSwiping = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Определяет направление свайпа для вида сверху.
    /// </summary>
    private Vector3 DetermineSwipeDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return (delta.x > 0) ? Vector3.right : Vector3.left;
        }
        else
        {
            return (delta.y > 0) ? Vector3.forward : Vector3.back;
        }
    }

    /// <summary>
    /// Корутина для мобильного движения.
    /// Объект движется в указанном направлении до столкновения с препятствием или истечения maxMoveDuration.
    /// </summary>
    [System.Obsolete]
    private IEnumerator MobileMoveCoroutine(Vector3 direction)
    {
        isMobileMoving = true;
        rb.velocity = direction * moveSpeed;
        float timer = 0f;
        while (timer < maxMoveDuration)
        {
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, collisionDistance))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    rb.velocity = Vector3.zero;
                    break;
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
        rb.velocity = Vector3.zero;
        isMobileMoving = false;
    }

    /// <summary>
    /// Метод срабатывания перехода на новый уровень,
    /// при пересечении тега "Finish".
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что коллайдер принадлежит игроку и столкновение произошло с объектом с тегом "Finish"
        if (CompareTag("Player") && other.CompareTag("Finish"))
        {
            // Получаем имя текущей сцены
            string currentSceneName = SceneManager.GetActiveScene().name;
            // Извлекаем номер текущего уровня из имени сцены
            int currentLevelNumber = ExtractLevelNumber(currentSceneName);
            // Вычисляем номер следующего уровня
            int nextLevelNumber = currentLevelNumber + 1;

            // Сохраняем прогресс с номером следующего уровня
            Progress_Saver.SaveProgress(nextLevelNumber);

            // Формируем имя следующей сцены по схеме "Level_{номер}"
            string nextSceneName = "Level_" + nextLevelNumber;
            // Загружаем следующую сцену по имени
            SceneManager.LoadScene(nextSceneName);
        }
    }

    /// <summary>
    /// Извлекает номер уровня из имени сцены, которая должна иметь формат "Level_X", где X – число.
    /// Если парсинг не удался, возвращает 1.
    /// </summary>
    /// <param name="sceneName">Имя сцены</param>
    /// <returns>Номер уровня</returns>
    private int ExtractLevelNumber(string sceneName)
    {
        // Разбиваем имя сцены по символу '_'
        string[] parts = sceneName.Split('_');
        if (parts.Length > 1 && int.TryParse(parts[1], out int levelNum))
        {
            return levelNum;
        }
        return 1; // Если формат не соответствует, возвращаем 1
    }
}
