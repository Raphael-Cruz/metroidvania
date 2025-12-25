using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletController : MonoBehaviour
{

    public float bulletSpeed;
    
    public Rigidbody2D rb2D;

    public Vector2 moveDir;
    public GameObject impactEffect;

    public int damageAmount = 5;

    // Update is called once per frame
    void FixedUpdate()
    {
        rb2D.velocity = moveDir * bulletSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy")) //another way to write this: if (other.tag == "enemy") bus is less efficient.
        {
            other.GetComponent<EnemyHealthController>().DamageEnemy(damageAmount);    
        }

        if (impactEffect != null)
        {
        Instantiate(impactEffect, transform.position, Quaternion.identity);

       
    } Destroy(gameObject);}

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
