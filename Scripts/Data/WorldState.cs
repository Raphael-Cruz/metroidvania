using System.Collections.Generic;
using UnityEngine;

public static class WorldState
{
    
    public static HashSet<string> CollectedRelics = new HashSet<string>();
    public static HashSet<string> UnlockedSkills = new HashSet<string>();
    public static HashSet<string> PermanentDeadEnemies = new HashSet<string>();

    public static void ApplyAbilitiesToPlayer(PlayerAbilityTracker player)
    {
 // RELIC-BASED ABILITIES
        player.canDoubleJump = CollectedRelics.Contains("DoubleJump");
        player.canDash       = CollectedRelics.Contains("Dash");
        player.canSuperJump  = CollectedRelics.Contains("SuperJump");
        player.canHook       = CollectedRelics.Contains("Hook");
     

        // SKILL-BASED ABILITIES
        player.canMissile    = UnlockedSkills.Contains("Missile");
        player.canShield     = UnlockedSkills.Contains("Shield");

        Debug.Log(
            $"[WorldState] Abilities → " +
            $"DJ:{player.canDoubleJump}, " +
            $"Dash:{player.canDash}, " +
            $"SJ:{player.canSuperJump}, " +
            $"Hook:{player.canHook}, " +
            $"Missile:{player.canMissile}"+
            $"Shield:{player.canShield}"
        );

        if (player.canMissile)
        {
                  Debug.Log("Missile Registered"        );  
        }
          if (player.canShield)
        {
                  Debug.Log("Shield Registered"        );  
        }
    }


    public static void LoadFromData(GameData data)
    {
        CollectedRelics = new HashSet<string>(data.collectedRelics);
        UnlockedSkills  = new HashSet<string>(data.collectedSkills);
        PermanentDeadEnemies = new HashSet<string>(data.permanentDeadEnemies);
    }
}
