using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistenceObject : MonoBehaviour
{
    [Header("ID MUST MATCH SkillData.skillID OR RelicData.relicID")]
    public string uniqueID;

    public enum ObjectType { Relic, Skill, PermanentEnemy }
    public ObjectType type;

    private void Start()
    {
        if (string.IsNullOrEmpty(uniqueID)) return;

        bool shouldDestroy =
            (type == ObjectType.Relic && WorldState.CollectedRelics.Contains(uniqueID)) ||
            (type == ObjectType.Skill && WorldState.UnlockedSkills.Contains(uniqueID)) ||
            (type == ObjectType.PermanentEnemy && WorldState.PermanentDeadEnemies.Contains(uniqueID));

        if (shouldDestroy)
            Destroy(gameObject);
    }

    public void MarkAsCollected()
    {
        switch (type)
        {
            case ObjectType.Relic:
                WorldState.CollectedRelics.Add(uniqueID);
                break;

            case ObjectType.Skill:
                WorldState.UnlockedSkills.Add(uniqueID);
                break;

            case ObjectType.PermanentEnemy:
                WorldState.PermanentDeadEnemies.Add(uniqueID);
                break;
        }

        // 🔒 HARD SAVE IMMEDIATELY
        if (SaveManager.instance != null)
        {
            SaveManager.instance.SaveGame(
                SceneManager.GetActiveScene().name,
                GameObject.FindWithTag("Player").transform.position
            );
        }
    }
}
