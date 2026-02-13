using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningGhoul : MonoBehaviour
{
    [Header("Configurações do Efeito")]
    public GameObject vfxExplosao; 
    public float offsetDaParede = 0.05f;   

    private void OnTriggerEnter2D(Collider2D other) 
{
    if (other.CompareTag("Bullet") || other.CompareTag("Player"))
    {
        Debug.Log("Bateu em 2D: " + other.gameObject.name);
        Instantiate(vfxExplosao, other.transform.position, Quaternion.identity);
        
        Debug.Log("Trigger detectado em: " + other.gameObject.name);
       
    }
}

    private void OnCollisionEnter2D(Collision2D other) 
{
    if (other.gameObject.CompareTag("Bullet") || other.gameObject.CompareTag("Player"))
    {
        Instantiate(vfxExplosao, other.transform.position, Quaternion.identity);
        EnemyHealthController.instance.DamageEnemy(1);
       
    }
}


}