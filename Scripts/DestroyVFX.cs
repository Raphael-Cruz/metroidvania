using UnityEngine;

public class DestroyVFX : MonoBehaviour
{
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        
        if (ps != null && !ps.main.loop)
        {
            // Destroy the GameObject after the particles have finished playing.
            // This calculation includes the main duration and the lifetime of the last emitted particles.
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(gameObject, duration);
        }
        else if (ps != null && ps.main.loop)
        {
          //If the particle system is set to loop, you must manually 
          // call ps.Stop() from an external script, or they will run indefinitely.
            Debug.LogWarning("Particle system on " + gameObject.name + " is looping and won't self-destruct properly.");
        }
        else
        {
            // Fallback for objects without a ParticleSystem component
            Destroy(gameObject, 5f);
        }
    }
}