using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Special Effect
/// Floats this object up and down based on your parameters
/// </summary>
public class sinFloat : MonoBehaviour
{

    [Range(0, 1)]
    public float FloatStrength;
    float origionalY;
  

    // Use this for initialization
    void OnEnable()
    {
        origionalY = transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x, origionalY + (Mathf.Sin(Time.time) * FloatStrength), transform.position.z);
    }

 
}
