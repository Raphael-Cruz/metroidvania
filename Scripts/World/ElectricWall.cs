using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricWall : MonoBehaviour


{
    [Header("Configurações do Efeito")]
    public GameObject vfxEletricidadePrefab; // Arraste seu Prefab de VFX aqui
    public float offsetDaParede = 0.05f;    // Evita que o efeito "entre" na parede

    private void OnTriggerEnter2D(Collider2D other) // Use OnTriggerEnter2D se for 2D
{
    if (other.CompareTag("Bullet") || other.CompareTag("Player"))
    {
        Debug.Log("Bateu em 2D: " + other.gameObject.name);
        // Cria o efeito na posição exata onde o centro do tiro está agora
        Instantiate(vfxEletricidadePrefab, other.transform.position, Quaternion.identity);
        
        Debug.Log("Trigger detectado em: " + other.gameObject.name);
    }
}
}