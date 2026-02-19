using UnityEngine;
using UnityEngine.SceneManagement;
public class EnemyHealthController : MonoBehaviour
{
    public static EnemyHealthController instance { get; private set; }
    public int totalhealth = 3;
    public GameObject deathEffect;
    public bool isInvulnerable = false;

    [Header("Persistence")]
    public string enemyID; 
    public bool isPermanentEnemy = false;
    
    // Callback for custom death handling (e.g., playing death animation before destruction)
    // If set, this will be called instead of immediately destroying the object
    public System.Action<EnemyHealthController> onDeathCallback; 

    // Callback for when damage is taken
    public System.Action<int> onDamageCallback;

private void Awake()
    {
       
        if (instance == null) {
            instance = this;
        }
    }
    private void Start()
    {
        // Check if this enemy is already marked as permanently dead in our WorldState
        if (!string.IsNullOrEmpty(enemyID) && WorldState.PermanentDeadEnemies.Contains(enemyID) || EnemyStatusManager.instance.defeatedEnemies.Contains(enemyID))
        {
            Destroy(gameObject);
            return;
        }
    }

    public void DamageEnemy(int damageAmount)
    {
        if (isInvulnerable || BossBattleState.IsInTransition) 
    {
        Debug.Log("Boss is currently in transition - No damage!");
        return; 
    }
        totalhealth -= damageAmount;
        
        onDamageCallback?.Invoke(damageAmount);

        if (totalhealth <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        // If a custom death callback is set, use it instead of default destruction
        // This allows bosses to play death animations before being destroyed
        if (onDeathCallback != null)
        {
            onDeathCallback.Invoke(this);
            return;
        }
        
        // Default death behavior for regular enemies
        PerformDefaultDeath();
    }
    
    // Public method that can be called after custom death handling (e.g., after animation completes)
    public void PerformDefaultDeath()
    {
        //Register permanent death if applicable
        if (isPermanentEnemy && !string.IsNullOrEmpty(enemyID))
        {
            WorldState.PermanentDeadEnemies.Add(enemyID);
            
            // save the game the INSTANT a boss dies:
            SaveManager.instance.SaveGame(SceneManager.GetActiveScene().name, transform.position);
        }
        if(EnemyStatusManager.instance != null)
        {
            EnemyStatusManager.instance.MarkAsDefeated(enemyID);
        }
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}



public static class BossBattleState
{
    public static bool IsInTransition = false;
}