using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;


public class script : MonoBehaviour
{
    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Haha");
    }

    // Update is called once per frame
    void Update()
    {
        if(Math.Abs(rb.transform.position.y + 3.52) < 0.1)
        {
            Debug.Log("Block Reached at: " + Time.time);
        }
    }
}
