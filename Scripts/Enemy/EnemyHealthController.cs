using UnityEngine;
using UnityEngine.SceneManagement;
public class EnemyHealthController : MonoBehaviour
{
    public int totalhealth = 3;
    public GameObject deathEffect;

    [Header("Persistence")]
    public string enemyID; 
    public bool isPermanentEnemy = false; 

    private void Start()
    {
        // Check if this enemy is already marked as permanently dead in our WorldState
        if (!string.IsNullOrEmpty(enemyID) && WorldState.PermanentDeadEnemies.Contains(enemyID))
        {
            Destroy(gameObject);
            return;
        }
    }

    public void DamageEnemy(int damageAmount)
    {
        totalhealth -= damageAmount;
        if (totalhealth <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        //Register permanent death if applicable
        if (isPermanentEnemy && !string.IsNullOrEmpty(enemyID))
        {
            WorldState.PermanentDeadEnemies.Add(enemyID);
            
            // save the game the INSTANT a boss dies:
             SaveManager.instance.SaveGame(SceneManager.GetActiveScene().name, transform.position);
           
        }

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}