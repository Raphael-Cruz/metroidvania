using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode saveKey = KeyCode.E;
    
    [Header("Visuals")]
    private Animator anim;
    private bool playerInRange = false;
    private bool isAlreadySaved = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(saveKey) && !isAlreadySaved)
        {
            PerformSaveProcess();
        }
    }

    private void PerformSaveProcess()
    {
        isAlreadySaved = true;

        // 1. Play the Animation
        if (anim != null) anim.SetTrigger("StartSave");

        // 2. Show UI Notification (NEW)
        if (UIController.instance != null)
        {
            UIController.instance.ShowSaveMessage();
        }

        // 3. Refresh Health
        HealthManager.instance?.FillHealth();

        // 4. Enemy Respawn Logic
        if (EnemyStatusManager.instance != null)
        {
            EnemyStatusManager.instance.ResetDefeatedEnemies();
        }

        // 5. Update Respawn Point
        if (RespawnController.instance != null)
        {
            RespawnController.instance.SetRespawnPoint(transform.position);
        }

        // 6. Hard Save to Disk
        if (SaveManager.instance != null)
        {
            SaveManager.instance.SaveGame(SceneManager.GetActiveScene().name, transform.position);
        }

        Debug.Log("Progress Saved: Health Refilled, Enemies Reset, UI Triggered.");
    }
}