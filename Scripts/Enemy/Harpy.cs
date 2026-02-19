using System.Collections;
using UnityEngine;

public class Harpy : MonoBehaviour
{
    [Header("Detection")]
    public float chaseRange = 10f;
    public float attackRange = 3f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float dashSpeed = 14f;
    public float backstepForce = 5f;
    public float backstepDuration = 0.3f;
    public float idleCooldown = 2f;

    [Header("References")]
    public Transform player;
    public Animator anim;
    public SpriteRenderer spriteRenderer;

    private enum State { Idle, Chasing, Dashing, Backstep, Cooldown }
    private State currentState = State.Idle;

    private Vector2 dashDirection;
    private float backstepTimer;
    private float cooldownTimer;
    private bool hasDamagedThisAttack;
    private bool dashCoroutineRunning;

    void Start()
    {
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
        else
            Debug.LogError("Harpy: Player not found. Make sure Player tag is set.");
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeSelf) return;

        float dist = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                HandleIdle(dist);
                break;

            case State.Chasing:
                HandleChase(dist);
                break;

            case State.Dashing:
                HandleDash();
                break;

            case State.Backstep:
                HandleBackstep();
                break;

            case State.Cooldown:
                HandleCooldown(dist);
                break;
        }

        FlipSprite();
        LockZ();
    }

    // ── State Handlers ──────────────────────────────────────────

    private void HandleIdle(float dist)
    {
        anim.SetBool("isChasing", false);
        anim.SetBool("isAttacking", false);

        if (dist <= chaseRange)
            SetState(State.Chasing);
    }

    private void HandleChase(float dist)
    {
        anim.SetBool("isChasing", true);
        anim.SetBool("isAttacking", false);

        // Left chase range — go back to idle
        if (dist > chaseRange)
        {
            SetState(State.Idle);
            return;
        }

        // Entered attack range — start dash
        if (dist <= attackRange && !dashCoroutineRunning)
        {
            StartCoroutine(DashRoutine());
            return;
        }

        // Move toward player
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    private void HandleDash()
    {
        transform.position += (Vector3)(dashDirection * dashSpeed * Time.deltaTime);
    }

    private void HandleBackstep()
    {
        backstepTimer -= Time.deltaTime;
        transform.position += (Vector3)(-dashDirection * backstepForce * Time.deltaTime);

        if (backstepTimer <= 0f)
        {
            anim.SetBool("isAttacking", false);
            cooldownTimer = idleCooldown;
            SetState(State.Cooldown);
        }
    }

    private void HandleCooldown(float dist)
    {
        anim.SetBool("isChasing", false);
        anim.SetBool("isAttacking", false);

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            // Resume chasing if still in range, else go idle
            if (dist <= chaseRange)
                SetState(State.Chasing);
            else
                SetState(State.Idle);
        }
    }

    // ── Coroutines ───────────────────────────────────────────────

    private IEnumerator DashRoutine()
    {
        dashCoroutineRunning = true;

        // Lock in dash direction at moment of attack
        dashDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;

        anim.SetBool("isChasing", false);
        anim.SetBool("isAttacking", true);
        hasDamagedThisAttack = false;
        SetState(State.Dashing);

        yield return new WaitForSeconds(0.4f); // max dash time

        // If dash ended without hitting player, backstep anyway
        if (currentState == State.Dashing)
            StartBackstep();

        dashCoroutineRunning = false;
    }

    // ── Collision ────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState != State.Dashing) return;
        if (hasDamagedThisAttack) return;

        if (other.CompareTag("Player"))
        {
            hasDamagedThisAttack = true;

            HealthManager hm = other.GetComponent<HealthManager>();
            if (hm != null) hm.DamagePlayer(2);

            StartBackstep();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────

    private void StartBackstep()
    {
        backstepTimer = backstepDuration;
        SetState(State.Backstep);
    }

    private void SetState(State newState)
    {
        currentState = newState;
    }

private void FlipSprite()
{
    if (player == null) return;

    bool facingLeft = player.position.x < transform.position.x;

    Vector3 scale = transform.localScale;
    scale.x = facingLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
    transform.localScale = scale;
}

    private void LockZ()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
        transform.rotation = Quaternion.identity;
    }

    // ── Gizmos ───────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Chase range — yellow
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Attack (dash) range — red
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}