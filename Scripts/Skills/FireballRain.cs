using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballRain : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 5f;

    [SerializeField] private HealthManager healthController;
    [SerializeField] private GameObject fireballExplosion;
    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
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
    Instantiate(fireballExplosion, transform.position, Quaternion.identity);
    Destroy(gameObject);
}
}