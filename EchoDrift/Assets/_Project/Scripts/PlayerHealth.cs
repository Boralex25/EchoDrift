using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 12;
    [SerializeField] private float invincibilityDuration = 1f;

    private int currentHealth;
    private bool isInvicibility = false;
    private SpriteRenderer spriteRenderer;

    public UnityEvent<int, int> onHealthChanged;
    public UnityEvent onDeath;

    void Awake()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) Debug.LogWarning("Компонент SpriteRenderer не найден у " + gameObject.name);
    }

    void Start()
    {
        if(UIManager.Instance != null)
        {
            UIManager.Instance.InitializeHealth(maxHealth);
            onHealthChanged.AddListener(UIManager.Instance.UpdateHealth);
            Debug.Log("Подписка выполнена!");
        }
        else
        {
            Debug.LogError("UIManager.Instance не найден!");
        }
    }

    public void TakeDamage(float dmgInHearts)
    {
        Debug.Log($"Получен урон {dmgInHearts} сердец. Здоровье было: {currentHealth / 4}"); // DEBUG

        if (isInvicibility) return;

        int dmgInUnits = Mathf.RoundToInt(dmgInHearts * 4f);
        currentHealth = Mathf.Max(0, currentHealth -  dmgInUnits);

        Debug.Log($"Здоровье стало: {(float)currentHealth / 4}");  // DEBUG

        if(currentHealth <= 0)
        {
            Die();
            return;
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);
        StartInvicibility();
    }

    private void StartInvicibility()
    {
        if (!gameObject.activeInHierarchy) return; // проверка, выключен ли объект в Hierarchy
        StartCoroutine(InvicibilityCoroutine());
    }

    private IEnumerator InvicibilityCoroutine()
    {
        isInvicibility = true;

        StartCoroutine(BlinkEffect());

        yield return new WaitForSeconds(invincibilityDuration);

        isInvicibility = false;

        // выключиить визуальный эффект
    }

    private IEnumerator BlinkEffect()
    {
        float blinkDuration = invincibilityDuration;
        float blinkInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;

        }

        spriteRenderer.enabled = true;
    }

    private void Die()
    {
        onDeath?.Invoke();
    }

    public void Heal(float amntInHearts)
    {
        int healInUnits = Mathf.RoundToInt(amntInHearts * 4);
        currentHealth = Mathf.Min(maxHealth, currentHealth + healInUnits);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"Здоровье после лечения: {(float)currentHealth / 4}");   // DEBUG
    }

    public void IncreaseMaxHealth(int increaseInUnits)
    {
        maxHealth += increaseInUnits;
        currentHealth += increaseInUnits;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
