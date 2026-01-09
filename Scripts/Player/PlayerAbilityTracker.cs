using UnityEngine;

public class PlayerAbilityTracker : MonoBehaviour
{
    public bool canDoubleJump;
    public bool canDash;
    public bool canSuperJump;
    public bool canHook;
    public bool canMissile;

    private void Start()
    {
        WorldState.ApplyAbilitiesToPlayer(this);
    }
}