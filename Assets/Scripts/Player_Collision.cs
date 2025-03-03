using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Collision : MonoBehaviour
{
    [SerializeField] private Sprite[] deathSprites; // Массив спрайтов для анимации смерти
    [SerializeField] private AudioClip deathSound;  // Звуковой клип для проигрыша
    [SerializeField] private float frameDuration = 0.1f; // Длительность одного кадра анимации
    [SerializeField] private string deathAnimationTrigger = "Death"; // Триггер для анимации (опционально)

    private SpriteRenderer spriteRenderer; // Компонент для отображения спрайтов
    private AudioSource audioSource;       // Компонент для воспроизведения звука
    private Animator animator;             // Компонент для управления анимацией (опционально)

    private void Start()
    {
        // Получаем необходимые компоненты
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        animator = GetComponent<Animator>(); // Опционально, если используете Animator
    }

    // Метод для 2D-столкновений
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            PlayDeathAnimationAndSound();
        }
    }

    // Метод для 3D-столкновений (если используете 3D)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            PlayDeathAnimationAndSound();
        }
    }

    // Метод для проигрывания анимации и звука
    private void PlayDeathAnimationAndSound()
    {
        // Отключаем дальнейшие столкновения (опционально)
        // GetComponent<Collider2D>().enabled = false; // Для 2D
        // GetComponent<Collider>().enabled = false; // Раскомментируйте для 3D

        // Проигрываем звук, если он есть
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Если есть массив спрайтов, запускаем анимацию
        if (deathSprites != null && deathSprites.Length > 0)
        {
            StartCoroutine(PlayDeathAnimation());
        }
        else if (animator != null)
        {
            // Если массив спрайтов не задан, используем Animator (опционально)
            animator.SetTrigger(deathAnimationTrigger);
            Invoke("RestartScene", GetAnimationLength());
        }
        else
        {
            // Если нет ни анимации, ни звука, сразу перезапускаем сцену
            RestartScene();
        }
    }

    // Корутина для проигрывания анимации из массива спрайтов
    private System.Collections.IEnumerator PlayDeathAnimation()
    {
        // Проигрываем анимацию смерти
        foreach (Sprite sprite in deathSprites)
        {
            spriteRenderer.sprite = sprite;
            yield return new WaitForSeconds(frameDuration);
        }

        // Останавливаемся на последнем кадре (если есть спрайты)
        if (deathSprites.Length > 0)
        {
            spriteRenderer.sprite = deathSprites[deathSprites.Length - 1];
        }

        // Сразу перезапускаем сцену
        RestartScene();
    }

    // Метод для получения длительности анимации из Animator (опционально)
    private float GetAnimationLength()
    {
        if (animator == null) return 0f;
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "DeathAnimation") // Замените на имя вашего клипа
            {
                return clip.length;
            }
        }
        return 0f;
    }

    // Метод для перезапуска сцены
    private void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}