using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movedopigboss : MonoBehaviour
{
    public float rangeToStartChase = 5f;
    public bool isChasing;
    public float moveSpeed = 3f;

    public float flipThreshold = 1.5f; // <-- added

    public Transform player;
    public Animator anim;

    private SpriteRenderer sr;

 void Start()
{
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
        if (!player || !player.gameObject.activeSelf)
            return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (!isChasing && dist < rangeToStartChase)
        {
            isChasing = true;
            anim.SetBool("isChasing", true);
        }

        if (isChasing)
        {
            Vector3 targetPos = new Vector3(player.position.x, transform.position.y, transform.position.z);

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            FlipTowardsPlayer();
        }
    }

    void FlipTowardsPlayer()
    {
        if (!player || !sr) return;

        float xDiff = player.position.x - transform.position.x;

        if (Mathf.Abs(xDiff) > flipThreshold)
            sr.flipX = xDiff < 0;
    }
}
