using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float missileSpeed = 10f;
     public float missileDelay = 0.2f;
    public float rotateSpeed = 500f; // Speed of the homing turn
    public Rigidbody2D rb2D;

    [Header("Homing Settings")]
    public bool isHoming = true;
    public float detectionRadius = 10f;
    public LayerMask enemyLayer;
    private Transform target;

    [Header("Impact Settings")]
    public GameObject impactEffect;
    public int damageAmount = 5;
    public Vector2 moveDir; // Initial direction set by the launcher

    

    [Header("Launch Settings")]
    public float launchDelay = 0.2f; 
    private float launchTimer;
   




    void Start()
    {
        launchTimer = launchDelay; // Initialize timer
        FindTarget();
    }

    void FixedUpdate()
    {
        // 1. Handle the Delay Timer
        if (launchTimer > 0)
        {
            launchTimer -= Time.fixedDeltaTime;
            rb2D.velocity = Vector2.zero; // Keep it still while "locking on"
            return; // Exit FixedUpdate early so it doesn't move yet
        }

        // 2. Normal Homing/Movement Logic
        if (isHoming && target != null)
        {
            Vector2 direction = (Vector2)target.position - rb2D.position;
            direction.Normalize();

            float rotateAmount = Vector3.Cross(direction, transform.right).z;
            rb2D.angularVelocity = -rotateAmount * rotateSpeed;

            rb2D.velocity = transform.right * missileSpeed;
        }
        else
        {
            rb2D.velocity = transform.right * missileSpeed;
            
            if (isHoming && Time.frameCount % 10 == 0) 
            {
                FindTarget();
            }
        }
    }

    void FindTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider2D enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        target = nearestEnemy;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyHealthController>();
            if (enemy != null)
            {
                enemy.DamageEnemy(damageAmount);
            }
        }

        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    // Visualizes the detection range in the Unity Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}




