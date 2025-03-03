using System.Collections;
using UnityEngine;

/// <summary>
/// Контроллер анимации для главного персонажа.
/// Поддерживает беговую анимацию и анимацию покоя, при этом для горизонтальных движений используется один массив с зеркалированием.
/// Также реализована начальная idle-анимация, которая проигрывается до первого движения.
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Run Animations")]
    [SerializeField] private Sprite[] runUp;         // Кадры беговой анимации для движения вверх
    [SerializeField] private Sprite[] runDown;       // Кадры беговой анимации для движения вниз
    [SerializeField] private Sprite[] runHorizontal; // Один массив для движения влево/вправо (будет зеркалироваться)

    [Header("Idle Animations (After Movement)")]
    [SerializeField] private Sprite[] idleUp;         // Idle анимация, когда персонаж уже двигался и стоит, смотрящий вверх
    [SerializeField] private Sprite[] idleDown;       // Idle анимация, когда персонаж уже двигался и стоит, смотрящий вниз
    [SerializeField] private Sprite[] idleHorizontal; // Idle анимация для горизонтального состояния (после движения)

    [Header("Initial Idle Animation (Before Movement)")]
    [SerializeField] private Sprite[] initialIdleAnimation; // Idle анимация до первого движения

    [Header("Animation Settings")]
    [SerializeField] private float frameRate = 0.1f; // Время одного кадра анимации

    private SpriteRenderer spriteRenderer;
    private Rigidbody rb;

    private float timer;
    private int frameIndex;
    private Vector3 lastMoveDirection = Vector3.forward; // Начальное направление (можно задать любое)
    private bool isMoving;
    private bool hasMoved = false; // Флаг, указывающий, двигался ли персонаж хоть раз

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
    }

    [System.Obsolete]
    private void Update()
    {
        // Получаем скорость и определяем, движется ли персонаж
        Vector3 velocity = rb.velocity;
        isMoving = velocity.magnitude > 0.1f;

        // Если персонаж движется, обновляем направление и помечаем, что он двигался
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
    /// Обновляет анимацию персонажа в зависимости от его состояния:
    /// - Если персонаж движется, проигрывается беговая анимация.
    /// - Если персонаж стоит и еще не двигался, проигрывается начальная idle-анимация.
    /// - Если персонаж стоит, но уже двигался, проигрывается стандартная idle-анимация.
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
            // Если персонаж не двигался вообще, проигрываем начальную idle-анимацию
            if (!hasMoved && initialIdleAnimation != null && initialIdleAnimation.Length > 0)
            {
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
                // Если персонаж уже двигался, проигрываем стандартную idle-анимацию, соответствующую последнему направлению
                Sprite[] idleAnim = GetIdleAnimation(lastMoveDirection);
                timer += Time.deltaTime;
                if (timer >= frameRate)
                {
                    timer = 0f;
                    frameIndex = (frameIndex + 1) % idleAnim.Length;
                    spriteRenderer.sprite = idleAnim[frameIndex];
                }
            }
        }
    }

    /// <summary>
    /// Возвращает массив спрайтов беговой анимации в зависимости от направления движения.
    /// Для горизонтального направления используется массив runHorizontal с зеркалированием.
    /// </summary>
    private Sprite[] GetRunAnimation(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            // Горизонтальное движение: зеркалим спрайт, если движение влево (x < 0)
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
    /// Возвращает массив спрайтов idle-анимации в зависимости от направления движения.
    /// Для горизонтального направления используется массив idleHorizontal с зеркалированием.
    /// </summary>
    private Sprite[] GetIdleAnimation(Vector3 direction)
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
