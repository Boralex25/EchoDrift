using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShadowHerald : MonoBehaviour
{
    private enum State
    {
        Idle,
        Chase
    }

    [SerializeField] private float patrolRange = 3.0f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float detectionRange = 5.0f;
    [SerializeField] private float hoverHeight = 1.0f;
    [SerializeField] private float chaseSpeed = 3.0f;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float lightRaius = 4.5f;
    [SerializeField] private float maxExposureTime = 2.0f;
    [SerializeField] private Transform exposureBar;
    [SerializeField] private int contactDmg = 1;
    [SerializeField] private float knockbackForce = 6f;

    private State currentState;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 patrolStartPos;
    private float currentExposureTime = 0f;
    private bool isInLight = false;
    private Vector3 originalBarScale;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        patrolStartPos = transform.position;
        if(exposureBar != null)
        {
            originalBarScale = exposureBar.localScale;
            exposureBar.gameObject.SetActive(false);
        }

        ChangeState(State.Idle);
    }

    private void Update()
    {
        EvaluateStateTransitions();
        CheckLightExposure();
    }

    private void FixedUpdate()
    {
        switch(currentState)
        {
            case State.Idle:
                HandleIdlemovement();
                break;
            case State.Chase:
                HandleChaseMovement();
                break;
        }

        MaintainHoverHeight();
        ApplySpriteFlip();
    }

    private void EvaluateStateTransitions()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= detectionRange && currentState != State.Chase) ChangeState(State.Chase);
        else if (distanceToPlayer > detectionRange && currentState != State.Idle) ChangeState(State.Idle);
    }



    private void ChangeState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        Debug.Log($"СМЕНА СОСТОЯНИЯ {gameObject.name}: {currentState}");
    }


    // ЛОГИКА ПОВЕДЕНИЯ И ПЕРЕДВИЖЕНИЯ

    private void HandleIdlemovement()
    {
        float offset = Mathf.PingPong(Time.time * patrolSpeed, patrolRange * 2) - patrolRange;
        Vector2 targetPos = new Vector2(patrolStartPos.x + offset, transform.position.y);
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * patrolSpeed;
    }

    private void HandleChaseMovement()
    {
        if(playerTarget == null) return;

        Vector2 targetPos = new Vector2(playerTarget.position.x, playerTarget.position.y + hoverHeight);
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, direction * chaseSpeed, 0.1f);
    }

    private void MaintainHoverHeight()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 3f);

        if(hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            float targetY = hit.point.y + hoverHeight;
            float heightError = targetY - transform.position.y;
            float yVelocity = heightError * 3f;     // жесткость удержания
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, yVelocity);
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.MoveTowards(rb.linearVelocity.y, -0.5f, Time.fixedDeltaTime));
        }
    }

    private void ApplySpriteFlip()
    {
        if(spriteRenderer == null) return;

        if(Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            float targetScaleX = Mathf.Sign(rb.linearVelocity.x) * Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(targetScaleX, transform.localScale.y, 1f);
        }
    }


    // ВЗАИМОДЕЙСТВИЕ СО СВЕТОМ

    private void CheckLightExposure()
    {
        if (playerTarget == null) return;

        float distance = Vector2.Distance(transform.position, playerTarget.position);
        bool inRadius = distance <= lightRaius;

        if (inRadius && playerTarget.GetComponent<Light2D>().enabled)
        {
            currentExposureTime += Time.deltaTime;
            if (!isInLight) OnEnterLight();
        }
        else
        {
            currentExposureTime -= Time.deltaTime * 2.5f;
            if (isInLight || !playerTarget.GetComponent<Light2D>().enabled) OnExitLight();
        }

        currentExposureTime = Mathf.Clamp(currentExposureTime, 0f, maxExposureTime);

        UpdateExposureBar();

        if(currentExposureTime >= maxExposureTime)
        {
            Die();
        }
    }

    private void OnEnterLight()
    {
        isInLight = true;
        Debug.Log("Враг на свету. Запуск таймера");
        exposureBar.gameObject.SetActive(true);
    }

    private void OnExitLight()
    {
        isInLight = false;
        Debug.Log("Враг вышел из света. Сброс таймера");
        exposureBar.gameObject.SetActive(false);
        currentExposureTime = 0;
    }

    private void UpdateExposureBar()
    {
        if(exposureBar == null) return;

        float progress = currentExposureTime / maxExposureTime;

        exposureBar.localScale = new Vector3(originalBarScale.x * progress, originalBarScale.y, originalBarScale.z);
    }


    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"Exposure Bar заполнен! Враг [{gameObject.name}] погибает");

        rb.simulated = false;
        if(TryGetComponent<BoxCollider2D>(out var coll)) coll.enabled = false;

        // анимация смерти

        Destroy(gameObject, 0.5f);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

            playerHealth?.TakeDamage(contactDmg);

            if(playerController != null)
            {
                Debug.Log("СРАБАТЫВАНИЕ КОЛЛИЗИИ!");
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                playerController.ApplyKnockback(knockbackDir, knockbackForce);
            }
        }
    }

}
