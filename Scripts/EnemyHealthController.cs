using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthController : MonoBehaviour
{
    public int totalhealth = 3;
    public GameObject deathEffect;

    [Header("Persistência")]
    [Tooltip("Dê um nome único para este inimigo (ex: Inimigo_Sala1_A)")]
    public string enemyID; 

    private void Start()
    {
        // Ao carregar a cena, pergunta ao Manager se este ID já morreu
        if (EnemyStatusManager.instance != null && EnemyStatusManager.instance.defeatedEnemies.Contains(enemyID))
        {
            gameObject.SetActive(false); // Desativa o inimigo antes de ele aparecer
        }
    }

    public void DamageEnemy(int damageAmount)
    {
        totalhealth -= damageAmount;

        if (totalhealth <= 0)
        {
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, transform.rotation);
            }

            // --- NOVO: Salva que este inimigo morreu ---
            if (EnemyStatusManager.instance != null && !string.IsNullOrEmpty(enemyID))
            {
                EnemyStatusManager.instance.MarkAsDefeated(enemyID);
            }

            Destroy(gameObject);
        }
    }
}