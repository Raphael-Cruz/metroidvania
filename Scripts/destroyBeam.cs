using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class destroyBeam : MonoBehaviour
{
    public float lifetime = 0.15f;
    void Start() => Destroy(gameObject, lifetime);
}