using UnityEngine;
using System.Collections;

public class FireballRainSpawner : MonoBehaviour
{
    [Header("Fireball")]
    [SerializeField] private GameObject fireballPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRate = 0.2f;
    [SerializeField] private int fireballsPerWave = 20;

    [Header("Area (Local Space)")]
    [SerializeField] private float spawnWidth = 10f;
    [SerializeField] private float spawnHeight = 0f;
    
    // Dynamic parameters
    private float currentSpeed = -1f; // -1 means use prefab default

    private Coroutine rainRoutine;
public bool IsRaining { get; private set; }

    private void Start()
    {
        
    }

    public void StartFireballRain()
    {
        if (rainRoutine != null)
            StopCoroutine(rainRoutine);

        rainRoutine = StartCoroutine(SpawnRain());
    }

    public void StopRain()
    {
        if (rainRoutine != null)
            StopCoroutine(rainRoutine);
        
        IsRaining = false;

        // Destroy all existing rain fireballs (since they are children)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetPhase2Parameters(float newSpeed, int newCount, float newRate)
    {
        Debug.Log($"FireballRainSpawner: Phase 2 params set! Speed: {newSpeed}, Count: {newCount}, Rate: {newRate}");
        currentSpeed = newSpeed;
        fireballsPerWave = newCount;
        spawnRate = newRate;
    }

    private IEnumerator SpawnRain()
    {
        IsRaining = true;

        for (int i = 0; i < fireballsPerWave; i++)
        {
            Vector3 localPos = new Vector3(
                Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f),
                spawnHeight,
                0f
            );

            GameObject fireball = Instantiate(fireballPrefab, transform);
              fireball.SetActive(true);
            fireball.transform.localPosition = localPos;
            
            // Set speed if overridden
            if (currentSpeed > 0)
            {
                // Check for FireballRain component (this is the correct component for rain fireballs)
                var rainScript = fireball.GetComponent<FireballRain>();
                if (rainScript != null)
                {
                    rainScript.SetSpeed(currentSpeed);
                    Debug.Log($"Set FireballRain speed to {currentSpeed}");
                }
                // Fallback check for FireBall just in case prefab was swapped
                else 
                {
                    var fbScript = fireball.GetComponent<FireBall>();
                    if (fbScript != null)
                    {
                        fbScript.SetSpeed(currentSpeed);
                        Debug.Log($"Set FireBall speed to {currentSpeed}");
                    }
                    else
                    {
                        Debug.LogWarning($"FireballRainSpawner: No FireballRain or FireBall script found on spawned object {fireball.name}");
                    }
                }
            }
          
            yield return new WaitForSeconds(spawnRate);
        }
         IsRaining = false;
        rainRoutine = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 center = transform.position + new Vector3(0f, spawnHeight, 0f);
        Gizmos.DrawWireCube(center, new Vector3(spawnWidth, 0.2f, 1f));
    }
#endif
}
