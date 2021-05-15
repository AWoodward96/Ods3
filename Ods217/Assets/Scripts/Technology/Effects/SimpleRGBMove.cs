using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleRGBMove : MonoBehaviour {

    public Vector3 Move;
    Rigidbody rgb;

    private void Start()
    {
        rgb = GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, -GlobalConstants.Gravity, 0);
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if(rgb.velocity.magnitude < Move.magnitude)
            rgb.AddForce(Move);
    }
}
