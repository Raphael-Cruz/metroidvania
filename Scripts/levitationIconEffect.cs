using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levitationIconEffect : MonoBehaviour
{
    public float floatAmplitude = 0.2f;   // how high it moves
    public float floatSpeed = 2f;         // how fast it moves
   

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // ONLY floating up/down — no rotation 
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = startPos + new Vector3(0, offset, 0);

    }
}   