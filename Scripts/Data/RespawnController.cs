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

    // Static variables persist across scene changes and reloads
    private static Vector3 respawnPoint = Vector3.zero;
    private static string respawnSceneName = ""; 
    private static bool hasSetRealPoint = false;

    private float savedFacingX = 1f;
    private bool isHandlingRespawn = false;

    [HideInInspector]
    public bool isTransitioningBetweenRooms = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else { Destroy(gameObject); }
    }

    // Call this ONLY from SavePoint.cs or when Loading a Save File
    public void SetRespawnPoint(Vector3 point) 
    {
        respawnPoint = point;
        respawnSceneName = SceneManager.GetActiveScene().name;
        hasSetRealPoint = true; 
        Debug.Log($"<color=green>STATUE REGISTERED:</color> {respawnPoint} in {respawnSceneName}");
    }

    // Call this from PlayerMovement Start()
    public void SetRespawnPointIfEmpty(Vector3 point)
    {
        // If we have a Statue or Save File loaded, ignore the scene's default start
        if (hasSetRealPoint) return; 

        respawnPoint = point;
        respawnSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("<color=yellow>Default Start Point Set (No Statue yet)</color>");
    }

    public void Respawn()
    {
        if (isHandlingRespawn) return;
        StartCoroutine(RespawnCo());
    }

    private IEnumerator RespawnCo()
    {
        isHandlingRespawn = true;
        
        // Reset enemies so they respawn when the player wakes up
        if (EnemyStatusManager.instance != null)
            EnemyStatusManager.instance.ResetDefeatedEnemies();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            savedFacingX = Mathf.Sign(player.transform.localScale.x);
            if (deathEffect != null)
                Instantiate(deathEffect, player.transform.position, Quaternion.identity);
            player.SetActive(false);
        }

        yield return new WaitForSeconds(waitToRespawn);
        
        // Always load the scene where the STATUE is located
        SceneManager.LoadScene(respawnSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isHandlingRespawn)
        {
            StartCoroutine(HandleRespawnAfterLoad());
        }
    }

    private IEnumerator HandleRespawnAfterLoad()
    {
        // Wait for scene initialization
        yield return null; 
        yield return new WaitForEndOfFrame();
        
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false; 
        }

        // Teleport to the saved statue location
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
        isHandlingRespawn = false; 
    }
}