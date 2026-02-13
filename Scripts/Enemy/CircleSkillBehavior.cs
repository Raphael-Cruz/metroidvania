using System.Collections;
using UnityEngine;

public class CircleSkillBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float castingDuration = 1.2f; 
    [SerializeField] private float destroyAnimationDuration = 0.5f;
    
    [Header("Collision Settings")]
    [SerializeField] private LayerMask groundLayer; // Layer for ground (to ignore)
    [SerializeField] private bool debugCollisions = true;
    
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D col;
    private Vector2 moveDirection;
    private bool isFired = false;
    private bool isDestroying = false;
    private GameObject caster;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        
        if (rb != null) 
        {
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        // Ensure the collider is set to Trigger
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        // Reset animator state when object is reactivated (if using object pooling)
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
        
        // Reset state
        isFired = false;
        isDestroying = false;
    }

    public void Initialize(GameObject casterObject, Vector3 targetPosition)
    {
        caster = casterObject;
        
        // Calculate initial direction toward target
        moveDirection = (targetPosition - transform.position).normalized;
        
        // Ignore collision with caster
        if (caster != null)
        {
            Collider2D casterCollider = caster.GetComponent<Collider2D>();
            if (casterCollider != null && col != null)
            {
                Physics2D.IgnoreCollision(col, casterCollider, true);
            }
        }
        
        StartCoroutine(SkillSequence());
    }

    IEnumerator SkillSequence()
    {
        // CASTING PHASE - projectile stays in place
        isFired = false;
        
        if (animator != null) 
        {
            animator.Play("Casting", 0, 0f); // Force play from start
        }

        yield return new WaitForSeconds(castingDuration);

        // FIRING PHASE - projectile starts moving
        isFired = true;
        
        if (animator != null) 
        {
            animator.Play("Fired", 0, 0f);
        }

        // Safety destruction after 5 seconds (in case it flies off screen)
        Destroy(gameObject, 5f); 
    }

    void FixedUpdate() 
    {
        // Freeze during boss transitions
        if (BossBattleState.IsInTransition) 
        {
            rb.velocity = Vector2.zero;
            return;
        }
        
        // Rotate projectile to face movement direction
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Move projectile if fired and not destroying
        if (isFired && !isDestroying)
        {
            Vector2 nextPos = (Vector2)transform.position + (moveDirection * moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(nextPos);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject obj)
    {
        if (isDestroying) return;

        // Ignore the caster
        if (obj == caster) 
        {
            if (debugCollisions) Debug.Log($"[CircleSkill] Ignored collision with caster: {obj.name}");
            return;
        }

        // Check if the object is on the ground layer (ignore it)
        if (IsOnLayer(obj, groundLayer))
        {
            if (debugCollisions) Debug.Log($"[CircleSkill] Passing through ground: {obj.name}");
            return;
        }

        // IGNORE specific tags that should NOT destroy the projectile
        if (obj.CompareTag("Ground") || 
            obj.CompareTag("Bullet") || 
            obj.CompareTag("Enemy") ||
            obj.CompareTag("Trigger")) 
        {
            if (debugCollisions) Debug.Log($"[CircleSkill] Ignored tag: {obj.tag} on {obj.name}");
            return;
        }

        // At this point, we hit something that SHOULD destroy the projectile
        // This includes: Player, Walls, Obstacles, etc.
        if (debugCollisions) Debug.Log($"[CircleSkill] Valid collision with {obj.name} (Tag: {obj.tag})");
        
        StartCoroutine(DestroySequence());
    }

    bool IsOnLayer(GameObject obj, LayerMask layer)
    {
        return ((1 << obj.layer) & layer) != 0;
    }

    IEnumerator DestroySequence()
    {
        isDestroying = true;
        isFired = false;
        
        // Stop movement
        rb.velocity = Vector2.zero;
        
        // Play destruction animation
        if (animator != null) 
        {
            animator.Play("Destroying", 0, 0f);
        }
        
        // Disable collider so we don't trigger more collisions
        if (col != null)
        {
            col.enabled = false;
        }
        
        yield return new WaitForSeconds(destroyAnimationDuration);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Clean up ignore collision when destroyed
        if (caster != null && col != null)
        {
            Collider2D casterCollider = caster.GetComponent<Collider2D>();
            if (casterCollider != null)
            {
                Physics2D.IgnoreCollision(col, casterCollider, false);
            }
        }
    }
}