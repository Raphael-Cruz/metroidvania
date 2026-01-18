using System.Collections.Generic;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float spawnRate = 0.05f; // How often to spawn a ghost while active

    private Queue<GhostSprite> pool;
    private float spawnTimer;

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        pool = new Queue<GhostSprite>();

        // Create a container to keep hierarchy clean
        GameObject container = new GameObject("GhostPool_" + gameObject.name);

        for (int i = 0; i < poolSize; i++)
        {
            if (ghostPrefab == null)
            {
                Debug.LogError("GhostTrail: Ghost Prefab is missing!");
                break;
            }

            GameObject obj = Instantiate(ghostPrefab, container.transform);
            GhostSprite ghost = obj.GetComponent<GhostSprite>();
            
            // If prefab doesn't have the script, add it
            if (ghost == null) ghost = obj.AddComponent<GhostSprite>();
            
            obj.SetActive(false);
            pool.Enqueue(ghost);
        }
    }

    /// <summary>
    /// Gets a ghost from the pool and activates it matching the target sprite.
    /// </summary>
    public void ShowGhost(SpriteRenderer targetSprite, Transform targetTransform)
    {
        if (pool == null || pool.Count == 0 || targetSprite == null) return;

        GhostSprite ghost = pool.Dequeue();
        
        ghost.Activate(targetSprite, targetTransform);
        
        // Put it back in the queue immediately (round-robin)
        pool.Enqueue(ghost); 
    }
    
    /// <summary>
    /// Helper to reliably spawn ghosts over time. Call this in Update or Coroutine.
    /// </summary>
    public void TrySpawnGhost(SpriteRenderer targetSprite, Transform targetTransform)
    {
         spawnTimer -= Time.deltaTime;
         if (spawnTimer <= 0)
         {
             ShowGhost(targetSprite, targetTransform);
             spawnTimer = spawnRate;
         }
    }
}
