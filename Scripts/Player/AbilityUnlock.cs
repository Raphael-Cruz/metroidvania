using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class AbilityUnlock : MonoBehaviour
    {
        [Header("Abilities to Unlock")]
        public bool unlockDoubleJump;
        public bool unlockDash;
        public bool unlockSuperJump;
        public bool unlockMissile;

    [Header("Skill Data (For HUD)")]
    public SkillData skillToRegister; 

  public GameObject pickUpEffectPrefab;    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerAbilityTracker player = other.GetComponent<PlayerAbilityTracker>();
        if (player == null)
            player = other.GetComponentInParent<PlayerAbilityTracker>();

        if (player == null)
            return;

            //  Register skill by ID
        if (skillToRegister != null)
        {
            WorldState.UnlockedSkills.Add(skillToRegister.skillID);

            skillToRegister.isCollected = true;
            skillToRegister.currentQuantity = skillToRegister.maxQuantity;

            FindFirstObjectByType<SkillHUDManager>()?.AddSkill(skillToRegister);
        }

        //  Apply abilities
        WorldState.ApplyAbilitiesToPlayer(player);
   

        //  Save immediately
        SaveManager.instance?.SaveGame(
            SceneManager.GetActiveScene().name,
            other.transform.position
        );

        //  Mark persistent object
        GetComponent<PersistenceObject>()?.MarkAsCollected();
   if (pickUpEffectPrefab != null)
        {
            Instantiate(pickUpEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);}
}



   