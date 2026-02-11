using UnityEngine;

public class Sunkidama : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private GameObject sunExplosion;

    private Vector2 moveDirection;

    // Called by the boss when spawning
    public void Init(Vector2 targetPosition)
    {
        moveDirection = (targetPosition - (Vector2)transform.position).normalized;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<HealthManager>()?.DamagePlayer(damage);
            Explode();
            return;
        }

        if (other.CompareTag("Ground"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        Instantiate(sunExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
