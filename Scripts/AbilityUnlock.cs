using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUnlock : MonoBehaviour
{
    public bool unlockDoubleJump;
    public bool unlockDash;
    public bool unlockSuperJump;
    public bool unlockMissile;

    public GameObject pickUpEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Must be player
        if (!other.CompareTag("Player"))
            return;

        // Get player abilities safely
        PlayerAbilityTracker player = other.GetComponent<PlayerAbilityTracker>();

        // If not found on this object, check parents
        if (player == null)
            player = other.GetComponentInParent<PlayerAbilityTracker>();

        // Safety check - prevents errors
        if (player == null)
            return;

        // --- Unlock abilities ---
        if (unlockDoubleJump)
            player.canDoubleJump = true;

        if (unlockDash)
            player.canDash = true;

        if (unlockSuperJump)
            player.canSuperJump = true;

        if (unlockMissile)
            player.canMissile = true;

        // Spawn pickup effect
        if (pickUpEffect != null)
            Instantiate(pickUpEffect, transform.position, transform.rotation);

        // Destroy this powerup
        Destroy(gameObject);
    }
}

