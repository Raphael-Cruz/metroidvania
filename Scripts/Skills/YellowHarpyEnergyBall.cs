using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YellowHarpyEnergyBall : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 2;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private GameObject ballExplosion;

    public bool hasCollided { get; private set; } = false;

    private Vector2 moveDirection;

    // Called by the boss when spawning
    public void Init(Vector2 targetPosition)
    {
        moveDirection = (targetPosition - (Vector2)transform.position).normalized;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
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
    if (hasCollided) return; // Evita múltiplas colisões

    if (other.CompareTag("Player") || other.CompareTag("Ground"))
    {
        other.GetComponent<HealthManager>()?.DamagePlayer(damage);
        hasCollided = true;
        Explode();
        return;
    }

    if (other.CompareTag("Ground"))
    {
        hasCollided = true;
        Explode();
    }
    
}

    private void Explode()
    {
        if(ballExplosion != null) 
            Instantiate(ballExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
