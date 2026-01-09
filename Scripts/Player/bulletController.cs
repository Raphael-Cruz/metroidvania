using UnityEngine;

public class bulletController : MonoBehaviour
{
    public float bulletSpeed = 10f;
    public Rigidbody2D rb2D;
    public Vector2 moveDir;
    public GameObject impactEffect;
    public int damageAmount = 5;

    private void Awake()
    {
        if (rb2D == null)
            rb2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        rb2D.velocity = moveDir * bulletSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealthController enemy =
                other.GetComponent<EnemyHealthController>() ??
                other.GetComponentInParent<EnemyHealthController>();

            if (enemy != null)
                enemy.DamageEnemy(damageAmount);
        }

        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
