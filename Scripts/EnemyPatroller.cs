using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPoint;
    public float moveSpeed, waitAtPoints;
    private float waitCounter;
    public float jumpForce;

    public Rigidbody2D theRB;
    public Animator anim;
    



        // Start is called before the first frame update

    void Start()
    {
        waitCounter =  waitAtPoints;
        foreach(Transform pPoints in patrolPoints)
        {
            pPoints.SetParent(null);
        }
    }



    // Update is called once per frame
    void Update()
    {
            if(Mathf.Abs( transform.position.x - patrolPoints[currentPoint].position.x) >.2f) //se ele estiver a 0.2f de distgancia do pointo ele faz o que: continua andando
        {
            if(transform.position.x < patrolPoints[currentPoint].position.x) //compara a posicao atual do mob com a posicao do ponto atual, se for menor, esta a esquerda se for maior a direita
            {
                theRB.velocity = new Vector2(moveSpeed, theRB.velocity.y);
                transform.localScale = new Vector3(-1f,1f,1f);
            } else
            {
                theRB.velocity = new Vector2(-moveSpeed, theRB.velocity.y);
                transform.localScale = Vector3.one;
            }
              if (transform.position.y < patrolPoints[currentPoint].position.y - .5f && theRB.velocity.y < .1f){
        theRB.velocity = new Vector2(theRB.velocity.x, jumpForce);    
        }
        }
      
        
            else
            {
                  theRB.velocity = new Vector2(0f, theRB.velocity.y);
                  waitCounter-= Time.deltaTime;

                  if(waitCounter < 0)
            {
               waitCounter = waitAtPoints;
                currentPoint ++;

                if(currentPoint >= patrolPoints.Length)
                {
                    currentPoint = 0;
                }
             
            }
            }
            anim.SetFloat("speed", Mathf.Abs(theRB.velocity.x));
    }
    
}
