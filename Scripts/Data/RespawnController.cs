using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class RespawnController : MonoBehaviour
{
    public static RespawnController instance;

    [Header("Settings")]
    public float waitToRespawn = 2f;
    public GameObject deathEffect;

    private static Vector3 respawnPoint = Vector3.zero;
    private static string respawnSceneName = ""; 
    private static bool hasSetRealPoint = false;

    private float savedFacingX = 1f;
    private bool isHandlingRespawn = false;

    [HideInInspector]
    public bool isTransitioningBetweenRooms = false;

    public bool disableRespawn = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        { 
            Destroy(gameObject); 
        }
    }

    // --- NEW: Proper Event Registration ---
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    // ---------------------------------------

    public void SetRespawnPoint(Vector3 point) 
    {
        respawnPoint = point;
        respawnSceneName = SceneManager.GetActiveScene().name;
        hasSetRealPoint = true; 
        Debug.Log($"<color=green>STATUE REGISTERED:</color> {respawnPoint} in {respawnSceneName}");
    }

    public void SetRespawnPointIfEmpty(Vector3 point)
    {
        if (hasSetRealPoint) return; 

        respawnPoint = point;
        respawnSceneName = SceneManager.GetActiveScene().name;
    }

    public Vector3 GetRespawnPoint() => respawnPoint;

    public void Respawn()
    {
         if (disableRespawn) return;
         
        if (isHandlingRespawn) return;
        StartCoroutine(RespawnCo());
    }

private IEnumerator RespawnCo()
{
    isHandlingRespawn = true;

    // AUTO-SAVE BEFORE RELOADING
    if (SaveManager.instance != null)
    {
        SaveManager.instance.SaveGame(respawnSceneName, respawnPoint);
    }

    // RESET ENEMIES 
    if (EnemyStatusManager.instance != null)
        EnemyStatusManager.instance.ResetDefeatedEnemies();

    // HIDE BOSS UI ON DEATH/RESPAWN
    if (BossHealthUI.instance != null)
        BossHealthUI.instance.gameObject.SetActive(false);

     
    GameObject player = GameObject.FindWithTag("Player");
    if (player != null)
    {
        savedFacingX = Mathf.Sign(player.transform.localScale.x);
        if (deathEffect != null)
            Instantiate(deathEffect, player.transform.position, Quaternion.identity);
        player.SetActive(false);
    }

    yield return new WaitForSeconds(waitToRespawn);
    
    // LOAD SCENE
    if (string.IsNullOrEmpty(respawnSceneName))
        respawnSceneName = SceneManager.GetActiveScene().name;

    SceneManager.LoadScene(respawnSceneName);
}
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Added a check to only run if this is actually the active singleton instance
        if (this == instance && isHandlingRespawn)
        {
            StartCoroutine(HandleRespawnAfterLoad());
        }
    }

    private IEnumerator HandleRespawnAfterLoad()
    {
        yield return null; 
        yield return new WaitForEndOfFrame();
        
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) 
        {
            isHandlingRespawn = false;
            yield break;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false; 
        }

        player.transform.position = respawnPoint;

        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.OnTargetObjectWarped(player.transform, respawnPoint - player.transform.position);
            vcam.Follow = player.transform;
        }

        player.GetComponent<PlayerMovement>()?.SetInitialDirection(savedFacingX);
        player.SetActive(true);
        if (rb != null) rb.simulated = true;

        HealthManager.instance?.FillHealth();
        if(PotionManager.instance != null){
 PotionManager.instance.currentPotion = PotionManager.instance.maxPotions;
        }
           
        isHandlingRespawn = false; 
    }
}