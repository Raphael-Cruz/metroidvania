using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;
using UnityEngine.SceneManagement;

public class BreakableTilemapWall : MonoBehaviour
{
    public static event Action OnWallFullyBroken;

    [Header("Ground que será quebrado")]
    public Tilemap groundTilemap;

    [Header("Tilemaps de estágio")]
    public GameObject breakableTilemap;
    public GameObject afterBreakTilemap;

    [Header("Camera Confiner")]
    public CinemachineConfiner2D confiner;
    public Collider2D boundsAfterBreak;

    public int hitsPerStage = 4;

    public Transform fakeWallHitPrefab;
    public  GameObject wallExplosion;
    private int totalHits = 0;
    private BoundsInt area;

 void Start()
{
    area = GetTileArea();

    if (WorldState.CompletedEvents.Contains("Scene6WallBroken") && WorldState.CompletedEvents.Contains("Scene6WallExplosion"))
    {
        // parede já foi quebrada antes, restaura estado final
        if (breakableTilemap != null)
            breakableTilemap.SetActive(false);

            if (wallExplosion != null)
            {
               wallExplosion.SetActive(false);
            }

        if (afterBreakTilemap != null)
            afterBreakTilemap.SetActive(true);

        if (confiner != null && boundsAfterBreak != null)
        {
            boundsAfterBreak.gameObject.SetActive(true);
            confiner.m_BoundingShape2D = boundsAfterBreak;
            confiner.InvalidateCache();
        }

        Destroy(gameObject);
        return;
    }

    // estado inicial normal
    if (breakableTilemap != null)
        breakableTilemap.SetActive(false);

    if (afterBreakTilemap != null)
        afterBreakTilemap.SetActive(false);

    if (boundsAfterBreak != null)
        boundsAfterBreak.gameObject.SetActive(false);
}

    public void Hit(Vector3 hitPosition)
    {
        totalHits++;

        if (fakeWallHitPrefab != null)
        {
            Instantiate(fakeWallHitPrefab, hitPosition, Quaternion.identity);
        }

        // quebra ground + ativa breakable
        if (totalHits == hitsPerStage)
        {
            BreakGroundTiles();

            if (breakableTilemap != null)
                breakableTilemap.SetActive(true);
        }

        // remove breakable + ativa afterBreak + libera câmera
        else if (totalHits == hitsPerStage * 2)
        {
            if (breakableTilemap != null)
                breakableTilemap.SetActive(false);

            if (afterBreakTilemap != null)
                afterBreakTilemap.SetActive(true);

            if (confiner != null && boundsAfterBreak != null)
            {
                boundsAfterBreak.gameObject.SetActive(true);
               confiner.m_BoundingShape2D = boundsAfterBreak;

                confiner.InvalidateCache();
            }

            OnWallFullyBroken?.Invoke();

        

            SaveManager.instance.SaveGame(SceneManager.GetActiveScene().name, transform.position);
            WorldState.CompletedEvents.Add("Scene6WallBroken");
             WorldState.CompletedEvents.Add("Scene6WallExplosion");

            Destroy(gameObject);
            
            
        
            
        
        }
        
    }

    BoundsInt GetTileArea()
    {
        Collider2D col = GetComponent<Collider2D>();

        Vector3Int min = groundTilemap.WorldToCell(col.bounds.min);
        Vector3Int max = groundTilemap.WorldToCell(col.bounds.max);

        return new BoundsInt(
            min.x,
            min.y,
            0,
            max.x - min.x + 1,
            max.y - min.y + 1,
            1
        );
    }

    void BreakGroundTiles()
    {
        foreach (var pos in area.allPositionsWithin)
        {
            if (groundTilemap.GetTile(pos) != null)
                groundTilemap.SetTile(pos, null);
        }
    }
}