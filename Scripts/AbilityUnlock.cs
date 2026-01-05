using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUnlock : MonoBehaviour
{
    [Header("Abilities to Unlock")]
    public bool unlockDoubleJump;
    public bool unlockDash;
    public bool unlockSuperJump;
    public bool unlockMissile;

    [Header("Skill Data (For HUD)")]
    public SkillData skillToRegister; // Drag your MissileData ScriptableObject here

    public GameObject pickUpEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerAbilityTracker player = other.GetComponent<PlayerAbilityTracker>();
        if (player == null)
            player = other.GetComponentInParent<PlayerAbilityTracker>();

        if (player == null)
            return;

        // --- Unlock logic ---
        if (unlockDoubleJump) player.canDoubleJump = true;
        if (unlockDash) player.canDash = true;
        if (unlockSuperJump) player.canSuperJump = true;
        
        if (unlockMissile)
        {
            player.canMissile = true;
            
            // Link to the HUD to make it appear
            SkillHUDManager hud = FindFirstObjectByType<SkillHUDManager>();
            if (hud != null && skillToRegister != null)
            {
                hud.AddSkill(skillToRegister);
            }
        }

  

           Destroy(gameObject); 
    }
}