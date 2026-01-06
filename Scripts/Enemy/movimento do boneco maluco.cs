using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movimentodobonecomaluco : MonoBehaviour
{
    public float rangeToStartChase;
    public bool isChasing;
    public float moveSpeed, turnSpeed;
    public Transform player; // This holds the player's transform
    public Animator anim;


    void Start()
    {
        // 
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
        }
        else
        {
            Debug.LogError("Enemy AI cannot find the Player. Check the Player Tag.");
        }
    }

    void Update()
    {
        //  null check on 'player' in case the player hasn't loaded yet or was destroyed
        if (player == null || !player.gameObject.activeSelf) return;

    float dist = Vector3.Distance(transform.position, player.position);

    // Enter chase mode
    if (!isChasing && dist < rangeToStartChase)
    {
        isChasing = true;
        anim.SetBool("isChasing", isChasing);
    }

    // Only rotate if enemy is chasing OR player is inside range
    if (isChasing || dist < rangeToStartChase)
    {
        RotateTowardsPlayer();
    }

    // Only move if chasing
    if (isChasing)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
            
        );
         
    }
}

private void RotateTowardsPlayer()
{
    Vector3 direction = player.position - transform.position; 

    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; //this function give you the angle in degrees of a 2d vector
    Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward); // quaternion is th way unity handles rotation, this way the mob is rotating toward the player.



    transform.rotation = Quaternion.Slerp( //this is gonna handle how fast the mob is gonna turn around
        transform.rotation,
        targetRot,
        turnSpeed * Time.deltaTime
    );
}
}











