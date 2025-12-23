using UnityEngine;

public class PotionCapacityPickup : MonoBehaviour
{
    [Header("Settings")]
    public GameObject pickUpEffectPrefab;   // Particle effect to spawn on UPGRADE
    public GameObject loopingVFX;           // Drag the actual Looping particle object here!

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PotionManager manager = PotionManager.instance;
        if (manager == null) return;

        // 1. Apply Upgrade Logic...
        if(manager.maxPotions < 5)
        {
            manager.maxPotions++;
            manager.currentPotion = manager.maxPotions;
        }
        else
        {
            manager.currentPotion = manager.maxPotions;
        }

       
        
        // 2. Stop the Looping Visual Effect
        if (loopingVFX != null)
        {
            ParticleSystem ps = loopingVFX.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Stop emitting new particles, but allow the currently alive ones to fade out.
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                
                // Destroy the looping VFX GameObject after its particles have faded (the longest lifetime).
                // We use a fixed duration as a safe fallback if the self-destruct script isn't used.
                Destroy(loopingVFX, ps.main.startLifetime.constantMax + 0.1f);
            }
        }

        // 3. Play the one-time Pickup Effect (Explosion/Sparkle)
        if (pickUpEffectPrefab != null)
        {
            // Spawn the one-time effect (This should be NON-LOOPING and use the DestroyVFX script!)
            Instantiate(pickUpEffectPrefab, transform.position, Quaternion.identity);
        }

        // --- FIX ENDS HERE ---

        // 4. Destroy this pickup icon object
        // NOTE: The loopingVFX object is destroyed separately above.
        Destroy(gameObject); 
    }
}