using UnityEngine;

public class MissileUpgrade : MonoBehaviour
{
    [Header("Settings")]
    public GameObject pickUpEffectPrefab;   // Particle effect to spawn on UPGRADE
    public GameObject loopingVFX;           // Drag the actual Looping particle object here!

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Check if the object is the player
        if (!other.CompareTag("Player"))
            return;

        // 2. Get the tracker from the player object that touched the trigger
        PlayerAbilityTracker abilities = other.GetComponent<PlayerAbilityTracker>();
        
        // Safety check if component is on a parent object
        if (abilities == null)
            abilities = other.GetComponentInParent<PlayerAbilityTracker>();

        if (abilities != null)
        {
            Debug.Log("Missile Ability Unlocked!");
            abilities.canMissile = true;
        }

        // 3. Stop the Looping Visual Effect
        if (loopingVFX != null)
        {
            ParticleSystem ps = loopingVFX.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(loopingVFX, ps.main.startLifetime.constantMax + 0.1f);
            }
        }

        // 4. Play the one-time Pickup Effect
        if (pickUpEffectPrefab != null)
        {
            Instantiate(pickUpEffectPrefab, transform.position, Quaternion.identity);
        }

        // 5. Destroy this pickup icon object
        Destroy(gameObject); 

        
    }

}