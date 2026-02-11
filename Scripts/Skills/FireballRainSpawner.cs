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
