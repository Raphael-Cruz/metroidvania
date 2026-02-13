using UnityEngine;

public class BossSkillCaster : MonoBehaviour
{
    [Header("Skill Prefab")]
    [SerializeField] private GameObject circleSkillPrefab;
    
    [Header("Casting Settings")]
    [SerializeField] private Transform skillSpawnPoint; // Where the skill spawns from
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // NEW: Store the initial spawn offset when boss faces right
    private Vector3 initialSpawnOffset;
    private bool isInitialized = false;
    
    private void Start()
    {
        // Capture the initial spawn point offset
        // This should be set when boss is facing RIGHT
        if (skillSpawnPoint != null)
        {
            initialSpawnOffset = skillSpawnPoint.localPosition;
            isInitialized = true;
            
            if (debugMode)
                Debug.Log($"[BossSkillCaster] Initial spawn offset captured: {initialSpawnOffset}");
        }
    }
    
    public GameObject CastCircleSkill()
    {
        if (circleSkillPrefab == null)
        {
            if (debugMode) Debug.LogError("[BossSkillCaster] Circle skill prefab is null!");
            return null;
        }
    
        Vector3 spawnPosition = skillSpawnPoint != null ? skillSpawnPoint.position : transform.position;
        
        // Create the skill projectile
        GameObject skill = Instantiate(circleSkillPrefab, spawnPosition, Quaternion.identity);
        skill.SetActive(true);
        
        // Initialize the skill behavior
        CircleSkillBehavior behavior = skill.GetComponent<CircleSkillBehavior>();
        if (behavior != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector3 targetPos = player != null ? player.transform.position : transform.position + transform.right;
            
            behavior.Initialize(gameObject, targetPos);
            
            if (debugMode)
            {
                Debug.Log($"[BossSkillCaster] Cast circle skill");
                Debug.Log($"  Spawn World Pos: {spawnPosition}");
                Debug.Log($"  Target: {targetPos}");
                Debug.Log($"  Boss Scale X: {transform.localScale.x}");
            }
        }
        else
        {
            if (debugMode) Debug.LogError("[BossSkillCaster] CircleSkillBehavior not found on prefab!");
        }
        
        return skill;
    }

    // This method is called by CyberPig when it flips
    // But actually, we DON'T need to do anything here!
    // The spawn point is a CHILD of the boss, so when boss flips (scale.x *= -1),
    // the spawn point automatically flips with it in WORLD space
    public void FlipSpawnPoint(bool facingRight)
    {
        // The spawn point is a child transform
        // When parent (boss) flips its scale.x, child automatically flips in world space
        // So we actually DON'T need to change the local position!
        
        // Just for debugging, let's log what's happening:
        if (debugMode && skillSpawnPoint != null)
        {
            Debug.Log($"[BossSkillCaster] Flip called. Facing Right: {facingRight}");
            Debug.Log($"  Boss Scale X: {transform.localScale.x}");
            Debug.Log($"  Spawn Local Pos: {skillSpawnPoint.localPosition}");
            Debug.Log($"  Spawn World Pos: {skillSpawnPoint.position}");
        }
        
        // IMPORTANT: We should NOT change localPosition here
        // The local position should remain constant (e.g., always (1.5, 0.5, 0))
        // Unity's transform hierarchy handles the world position flip automatically
    }
    
    // Optional: Visualize spawn point in editor
    private void OnDrawGizmos()
    {
        if (skillSpawnPoint != null)
        {
            // Draw spawn point
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(skillSpawnPoint.position, 0.3f);
            
            // Draw arrow showing direction based on boss facing
            Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(skillSpawnPoint.position, direction * 1.0f);
            
            // Draw line from boss to spawn point
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, skillSpawnPoint.position);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (skillSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(skillSpawnPoint.position, 0.4f);
            
            // Show local offset as text would be nice, but we can't in Gizmos
            // So just draw a bigger highlight
        }
    }
}