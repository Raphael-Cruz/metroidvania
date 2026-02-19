using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class RespawnController : MonoBehaviour
{
    public static RespawnController instance;
    public static bool respawningFromDeath = false;
    private Animator anim;

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

    private void FindPlayerAnimator()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null && pm.anim != null)
            anim = pm.anim;
        else
            anim = player.GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"<color=yellow>OnSceneLoaded: isHandlingRespawn={isHandlingRespawn}, this==instance:{this == instance}</color>");
        if (!isHandlingRespawn)
        {
            StartCoroutine(HandleSceneTransition());
            return;
        }

        if (this == instance)
        {
            respawningFromDeath = false;
            StartCoroutine(HandleRespawnAfterDeath());
        }
    }

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

        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame(respawnSceneName, respawnPoint);

        if (EnemyStatusManager.instance != null)
            EnemyStatusManager.instance.ResetDefeatedEnemies();

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

        yield return new WaitForSeconds(1);

        if (SceneFader.instance != null)
            yield return StartCoroutine(SceneFader.instance.FadeToBlack(1f));

        yield return new WaitForSeconds(waitToRespawn);

        StartCoroutine(SceneFader.instance.FadeIn());

        if (string.IsNullOrEmpty(respawnSceneName))
            respawnSceneName = SceneManager.GetActiveScene().name;

        respawningFromDeath = true;
        SceneManager.LoadScene(respawnSceneName);
    }

    private IEnumerator HandleSceneTransition()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        FindPlayerAnimator();
    }

    private IEnumerator HandleRespawnAfterDeath()
{
    yield return null; 
    yield return new WaitForEndOfFrame();
    
    // Use PlayerMovement.instance instead of FindWithTag to avoid stale/wrong references
    PlayerMovement pm = PlayerMovement.instance;
    if (pm == null)
    {
        Debug.LogError("[RESPAWN] PlayerMovement.instance is null!");
        isHandlingRespawn = false;
        yield break;
    }

    GameObject player = pm.gameObject;
    Rigidbody2D rb = pm.theRB;
    
   

    FindPlayerAnimator();

    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    player.SetActive(true);

    Collider2D col = player.GetComponent<Collider2D>();
    float halfHeight = col != null ? col.bounds.extents.y : 1.2f;

    Vector3 targetPos = respawnPoint;
    RaycastHit2D hit = Physics2D.Raycast(respawnPoint + Vector3.up * 2f, Vector2.down, 10f, LayerMask.GetMask("Ground"));
    if (hit.collider != null)
        targetPos = new Vector3(respawnPoint.x, hit.point.y + halfHeight, respawnPoint.z);

    player.transform.position = targetPos;

    var vcam = FindObjectOfType<CinemachineVirtualCamera>();
    if (vcam != null)
    {
        vcam.OnTargetObjectWarped(player.transform, targetPos - player.transform.position);
        vcam.Follow = player.transform;
    }

    pm.SetInitialDirection(savedFacingX);
    pm.canMove = false;

    yield return new WaitForSeconds(0.1f);
    if (anim != null)
    {
        anim.enabled = true;
        anim.SetTrigger("Awake");
    }
    

    // Wait for Awake animation with safety timeout
    float timeout = 5f;
    while (timeout > 0)
    {
        timeout -= Time.unscaledDeltaTime;
        bool inAwake = anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("Awake");
        bool finished = inAwake && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
        if (finished) break;
        yield return null;
    }

    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.simulated = true;
    }

    pm.isRespawning = false;
    pm.canMove = true;
    Debug.Log($"<color=cyan>RESPAWN DONE: simulated={rb?.simulated}, constraints={rb?.constraints}, isRespawning={pm.isRespawning}, canMove={pm.canMove}</color>");

    HealthManager.instance?.FillHealth();
    if (PotionManager.instance != null)
        PotionManager.instance.currentPotion = PotionManager.instance.maxPotions;
       
    isHandlingRespawn = false; 
}
}