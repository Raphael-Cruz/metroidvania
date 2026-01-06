using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPoint : MonoBehaviour
{
    [Header("Visual")]
    public GameObject shineVisual;
    private EnergyBeamVisual beam;
    private Collider2D hookCollider;


    private void Awake()
    {
                hookCollider = GetComponent<Collider2D>();

        if (shineVisual != null)
            shineVisual.SetActive(false);
               
    }

private void OnTriggerEnter2D(Collider2D other)
{
   

    if (!other.CompareTag("Player"))
    {
      
        return;
    }

    var pull =
        other.GetComponent<PlayerChainPull>() ??
        other.GetComponentInParent<PlayerChainPull>() ??
        other.GetComponentInChildren<PlayerChainPull>();
    if (pull != null)
        pull.SetHook(this);



    if (EnergyBeamVisual.Instance != null)
        EnergyBeamVisual.Instance.SetHook(hookCollider);
}

private void OnTriggerExit2D(Collider2D other)
{
    if (!other.CompareTag("Player"))
        return;

    var pull =
        other.GetComponent<PlayerChainPull>() ??
        other.GetComponentInParent<PlayerChainPull>() ??
        other.GetComponentInChildren<PlayerChainPull>();

    if (pull != null)
        pull.ClearHook(this);


   
    if (EnergyBeamVisual.Instance != null)
        EnergyBeamVisual.Instance.ClearHook(hookCollider);
     

    if (shineVisual != null)
        shineVisual.SetActive(false);
}




}



    