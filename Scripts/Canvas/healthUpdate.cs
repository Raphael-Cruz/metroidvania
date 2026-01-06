using UnityEngine;

public class HealthUpgradePickup : MonoBehaviour
{
    public GameObject pickUpEffectPrefab;  // Particle pickup effect to spawn on UPGRADE
 public GameObject loopingVFX;  // Drag the actual Looping particle object here (not the prefab)!
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        HealthManager manager = HealthManager.instance;

        if (manager == null)
        {
            Debug.LogWarning("HealthManager instance is NULL");
            return;
        }

        // Use the new Manager method to upgrade health!
        manager.UpgradeMaxHealth(1); 
        
        // Stop the Looping Visual Effect
        if (loopingVFX != null)
        {
            ParticleSystem ps = loopingVFX.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Stop emitting new particles, but allow the currently alive ones to fade out.
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                
                // Destroy the looping VFX GameObject after its particles have faded (the longest lifetime).
                // We use a fixed duration as a safe fallback if the self-destruct script isn't used. got this from internet, not sure if does means anything for real, whatever.
                Destroy(loopingVFX, ps.main.startLifetime.constantMax + 0.1f);
            }
        }

       // Play the one-time Pickup Effect (Explosion/Sparkle)
        if (pickUpEffectPrefab != null)
        {
            // Spawn the one-time effect (This should be ----->NON-LOOPING <----  and use the DestroyVFX script!)
            Instantiate(pickUpEffectPrefab, transform.position, Quaternion.identity);
        }

      
        // Destroy this pickup icon object
               Destroy(gameObject); 
    }
}