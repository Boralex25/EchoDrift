using UnityEngine;

public class TestDamage : MonoBehaviour
{
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] private float damageAmount = 0.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (playerHealth != null) playerHealth.TakeDamage(damageAmount);
            else Debug.LogError("PlayerHealth не найден!");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (playerHealth != null) playerHealth.Heal(0.5f);
        }
    }
}
