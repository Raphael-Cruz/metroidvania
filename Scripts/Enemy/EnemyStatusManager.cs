using UnityEngine;
using System.Collections.Generic;

public class EnemyStatusManager : MonoBehaviour
{
    public static EnemyStatusManager instance;

    // Guardamos os nomes/IDs dos inimigos mortos aqui
    public HashSet<string> defeatedEnemies = new HashSet<string>();

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

    public void MarkAsDefeated(string enemyID)
    {
        if (!defeatedEnemies.Contains(enemyID))
        {
            defeatedEnemies.Add(enemyID);
        }
    }

    public void ResetDefeatedEnemies()
{
    defeatedEnemies.Clear();
}
}