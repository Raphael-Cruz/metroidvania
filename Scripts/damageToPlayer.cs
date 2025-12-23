using UnityEngine;

public class damageToPlayer : MonoBehaviour
{
    public int damageAmount = 1;

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Check global invincibility
        if (HealthManager.instance != null && HealthManager.instance.isInvincible)
            return;

        if (other.gameObject.CompareTag("Player"))
        {
            DealDamage();
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            Debug.Log("hit the water");
            DealDamage();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check global invincibility
        if (HealthManager.instance != null && HealthManager.instance.isInvincible)
            return;

        if (other.CompareTag("Player"))
        {
            DealDamage();
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            Debug.Log("hit the water");
            DealDamage();
        }
    }

    void DealDamage()
    {
        if (HealthManager.instance == null)
        {
            Debug.LogWarning("Tried to damage player but HealthManager instance is NULL");
            return;
        }

        HealthManager.instance.DamagePlayer(damageAmount);
    }
}
