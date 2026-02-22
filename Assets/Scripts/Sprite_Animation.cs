using System.Collections;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Run Animations")]
    [SerializeField] private Sprite[] runUp;
    [SerializeField] private Sprite[] runDown;
    [SerializeField] private Sprite[] runHorizontal;

    [Header("Idle Animations (After Movement)")]
    [SerializeField] private Sprite[] idleUp;
    [SerializeField] private Sprite[] idleDown;
    [SerializeField] private Sprite[] idleHorizontal;

    [Header("Initial Idle Animation (Before Movement)")]
    [SerializeField] private Sprite[] initialIdleAnimation;

    [Header("Animation Settings")]
    [SerializeField] private float frameRate = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody rb;

    private float timer;
    private int frameIndex;
    private Vector3 lastMoveDirection = Vector3.forward;
    private bool isMoving;
    private bool hasMoved = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
    }

    [System.Obsolete]
    private void Update()
    {
        Vector3 velocity = rb.velocity;
        isMoving = velocity.magnitude > 0.1f;

        if (isMoving)
        {
            hasMoved = true;
            Vector3 direction = velocity.normalized;
            if (direction != Vector3.zero)
                lastMoveDirection = direction;
        }

        UpdateAnimation();
    }

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

    private Sprite[] GetRunAnimation(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            spriteRenderer.flipX = direction.x < 0;
            return runHorizontal;
        }
        else
        {
            spriteRenderer.flipX = false;
            return (direction.z > 0) ? runUp : runDown;
        }
    }

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
