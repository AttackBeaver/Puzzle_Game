using System.Collections;
using UnityEngine;

/// <summary>
/// Контроллер анимации для главного персонажа.
/// Поддерживает беговую анимацию для движения вверх, вниз и горизонтально (один массив для влево/вправо, с зеркалированием),
/// а также анимацию покоя. Если персонаж ещё не двигался, проигрывается начальная анимация покоя.
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Idle Sprites")]
    [SerializeField] private Sprite idleUp;          // Спрайт покоя для движения вверх
    [SerializeField] private Sprite idleDown;        // Спрайт покоя для движения вниз
    [SerializeField] private Sprite idleHorizontal;  // Спрайт покоя для движения влево/вправо

    [Header("Run Animations")]
    [SerializeField] private Sprite[] runUp;         // Кадры беговой анимации для движения вверх
    [SerializeField] private Sprite[] runDown;       // Кадры беговой анимации для движения вниз
    [SerializeField] private Sprite[] runHorizontal; // Один массив кадров для движения влево/вправо

    [Header("Initial Idle Animation (Before Movement)")]
    [SerializeField] private Sprite[] initialIdleAnimation; // Анимация покоя до первого движения

    [Header("Animation Settings")]
    [SerializeField] private float frameRate = 0.1f; // Время одного кадра анимации

    private SpriteRenderer spriteRenderer;
    private Rigidbody rb;

    private float timer;
    private int frameIndex;
    private Vector3 lastMoveDirection = Vector3.forward; // Начальное направление (можно изменить)
    private bool isMoving;
    private bool hasMoved = false; // Флаг: был ли игрок хоть раз в движении

    private void Awake()
    {
        // Получаем необходимые компоненты
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
    }

    [System.Obsolete]
    private void Update()
    {
        // Определяем, движется ли персонаж
        Vector3 velocity = rb.velocity;
        isMoving = velocity.magnitude > 0.1f;

        // Если движется, обновляем направление и помечаем, что персонаж уже двигался
        if (isMoving)
        {
            hasMoved = true;
            Vector3 direction = velocity.normalized;
            if (direction != Vector3.zero)
                lastMoveDirection = direction;
        }

        UpdateAnimation();
    }

    /// <summary>
    /// Обновляет анимацию в зависимости от состояния персонажа.
    /// Если движется – проигрывается беговая анимация, иначе – спрайт покоя.
    /// Если персонаж ещё не двигался, проигрывается специальная анимация покоя.
    /// </summary>
    private void UpdateAnimation()
    {
        if (isMoving)
        {
            Sprite[] anim = GetRunAnimation(lastMoveDirection);
            timer += Time.deltaTime;
            if (timer >= frameRate)
            {
                timer = 0f;
                frameIndex = (frameIndex + 1) % anim.Length;
                spriteRenderer.sprite = anim[frameIndex];
            }
        }
        else
        {
            if (!hasMoved && initialIdleAnimation != null && initialIdleAnimation.Length > 0)
            {
                // Анимация покоя до первого движения
                timer += Time.deltaTime;
                if (timer >= frameRate)
                {
                    timer = 0f;
                    frameIndex = (frameIndex + 1) % initialIdleAnimation.Length;
                    spriteRenderer.sprite = initialIdleAnimation[frameIndex];
                }
            }
            else
            {
                // Статичный спрайт покоя после того, как персонаж уже двигался
                spriteRenderer.sprite = GetIdleSprite(lastMoveDirection);
                frameIndex = 0;
                timer = 0f;
            }
        }
    }

    /// <summary>
    /// Возвращает массив беговой анимации для заданного направления.
    /// Если горизонтальное движение, используется массив runHorizontal,
    /// и спрайт отзеркаливается в зависимости от направления по оси X.
    /// </summary>
    private Sprite[] GetRunAnimation(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            // Горизонтальное движение: используем runHorizontal и зеркалим спрайт
            spriteRenderer.flipX = direction.x < 0;
            return runHorizontal;
        }
        else
        {
            spriteRenderer.flipX = false;
            return (direction.z > 0) ? runUp : runDown;
        }
    }

    /// <summary>
    /// Возвращает спрайт покоя для заданного направления.
    /// Если горизонтальное движение, используется idleHorizontal с зеркалированием.
    /// </summary>
    private Sprite GetIdleSprite(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            spriteRenderer.flipX = direction.x < 0;
            return idleHorizontal;
        }
        else
        {
            spriteRenderer.flipX = false;
            return (direction.z > 0) ? idleUp : idleDown;
        }
    }
}
